// MTS.BLL/Services/RefundService.cs
using MTS.BLL.Services.VNPayService;
using MTS.DAL.Dtos;
using MTS.DAL.Repositories;
using MTS.Data.Enums;
using MTS.Data.Models;

namespace MTS.BLL.Services
{
	public interface IRefundService
	{
		Task<(bool Success, string Message)> CreateRefundRequestAsync(Guid passengerId, CreateRefundRequestDto requestDto);
		Task<List<RefundRequestDto>> GetPendingRefundsAsync();
		Task<(bool Success, string? Message)> ProcessRefundRequestAsync(int requestId, Guid adminId, ProcessRefundRequestDto requestDto);
	}

	public class RefundService : IRefundService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IVNPayRefundGatewayService _refundGateway;
		private readonly IWalletService _walletService;

		public RefundService(IUnitOfWork unitOfWork, IVNPayRefundGatewayService refundGateway, IWalletService walletService)
		{
			_unitOfWork = unitOfWork;
			_refundGateway = refundGateway;
			_walletService = walletService;
		}

		public async Task<(bool Success, string Message)> CreateRefundRequestAsync(Guid passengerId, CreateRefundRequestDto requestDto)
		{
			var ticket = await _unitOfWork.GetRepository<Ticket>().GetByPropertyAsync(t => t.Id == requestDto.TicketId && t.PassengerId == passengerId);
			if (ticket == null)
				return (false, "Ticket not found or does not belong to the user.");

			if (ticket.Status != TicketStatus.UnUsed)
				return (false, "Ticket has already been used and cannot be refunded.");

			if (ticket.PurchaseTime == null || ticket.PurchaseTime.Value.AddHours(24) < DateTime.UtcNow)
				return (false, "The 24-hour window for a refund has passed.");

			var existingRequest = await _unitOfWork.GetRepository<RefundRequestApplication>().GetByPropertyAsync(r => r.TicketId == requestDto.TicketId);
			if (existingRequest != null)
				return (false, "A refund request for this ticket has already been submitted.");

			var passenger = await _unitOfWork.GetRepository<User>().GetByPropertyAsync(u => u.Id == passengerId && u.IsActive == true);
			if (passenger == null)
				return (false, "Passenger account not found or is inactive.");

			var refundRequest = new RefundRequestApplication
			{
				TicketId = requestDto.TicketId,
				PassengerId = passengerId,
				Reason = requestDto.Reason,
				Status = ApplicationStatus.Pending,
				RequestedAt = DateTime.UtcNow,
				CreatedTime = DateTime.UtcNow,
				CreatedBy = passenger.UserName,
			};

			await _unitOfWork.GetRepository<RefundRequestApplication>().AddAsync(refundRequest);
			var success = await _unitOfWork.SaveAsync() > 0;

			if (success)
			{
				return (true, "Refund request created successfully.");
			}
			else
			{
				return (false, "Failed to save the refund request due to a database error.");
			}
		}

		public async Task<List<RefundRequestDto>> GetPendingRefundsAsync()
		{
			var requests = await _unitOfWork.GetRepository<RefundRequestApplication>().GetAllByPropertyAsync(
				r => r.Status == ApplicationStatus.Pending,
				includeProperties: "Passenger,Ticket,Passenger,Admin");

			return requests.Select(r => new RefundRequestDto
			{
				Id = r.Id,
				TicketId = r.TicketId,
				PassengerName = $"{r.Passenger.FirstName} {r.Passenger.LastName}",
				TicketAmount = r.Ticket.TotalAmount,
				Reason = r.Reason,
				Status = r.Status,
				RequestedAt = r.RequestedAt,
				AdminName = r.Admin != null ? $"{r.Admin.FirstName} {r.Admin.LastName}" : null,
				ProcessedAt = r.ProcessedAt,
				AdminNotes = r.AdminNotes
			}).ToList();
		}

		public async Task<(bool Success, string? Message)> ProcessRefundRequestAsync(int requestId, Guid adminId, ProcessRefundRequestDto requestDto)
		{
			var refundRequest = await _unitOfWork.GetRepository<RefundRequestApplication>().GetByPropertyAsync(r => r.Id == requestId, includeProperties: "Ticket,Passenger");
			if (refundRequest == null || refundRequest.Status != ApplicationStatus.Pending)
				return (false, "Request not found or already processed.");

			var admin = await _unitOfWork.GetRepository<User>().GetByPropertyAsync(u => u.Id == adminId);
			if (admin == null) return (false, "Processing user not found.");

			var ticket = refundRequest.Ticket;
			if (ticket.Status != TicketStatus.UnUsed)
			{
				return (false, "Ticket has been used and can no longer be refunded.");
			}

			if (ticket.PurchaseTime == null || refundRequest.RequestedAt > ticket.PurchaseTime.Value.AddHours(24))
			{
				return (false, "Refund request was made outside the 24-hour window.");
			}

			refundRequest.AdminId = adminId;
			refundRequest.AdminNotes = requestDto.AdminNotes;
			refundRequest.ProcessedAt = DateTime.UtcNow;

			if (requestDto.Status == ApplicationStatus.Rejected)
			{
				refundRequest.Status = ApplicationStatus.Rejected;
				await _unitOfWork.GetRepository<RefundRequestApplication>().UpdateAsync(refundRequest);
				await _unitOfWork.SaveAsync();
				return (true, "Refund rejected successfully.");
			}

			// Check if it was a VNPay transaction
			if (!string.IsNullOrEmpty(ticket.TxnRef) && !string.IsNullOrEmpty(ticket.VnPayTransactionNo) && !string.IsNullOrEmpty(ticket.VnPayTransactionDate))
			{
				long totalAmountInCents = (long)(ticket.TotalAmount * 100);
				long refundAmountInCents = totalAmountInCents * 9 / 10; // Use integer arithmetic to avoid precision issues

				// Determine transaction type: 02 for full refund, 03 for partial
				string transactionType = (refundAmountInCents < totalAmountInCents) ? "03" : "02";

				var refundResponse = await _refundGateway.SendRefundRequestAsync(
					refundAmountInCents,
					transactionType,
					ticket.TxnRef,
					ticket.VnPayTransactionNo,
					ticket.VnPayTransactionDate,
					admin.UserName
				);

				if (refundResponse.vnp_ResponseCode == "00")
				{
					refundRequest.Status = ApplicationStatus.Approved;
					ticket.Status = TicketStatus.Refunded;
					await _unitOfWork.GetRepository<Ticket>().UpdateAsync(ticket);
				}
				else
				{
					refundRequest.Status = ApplicationStatus.Rejected;
					refundRequest.AdminNotes += $" | VNPay Refund Failed: ({refundResponse.vnp_ResponseCode}) {refundResponse.vnp_Message}";
				}
			}
			else // Wallet transaction
			{
				var refundAmount = ticket.TotalAmount * 0.9m; // 90% refund for wallet transactions too
				var walletRefundSuccess = await _walletService.AddToWalletAsync(
					refundRequest.PassengerId,
					refundAmount,
					TransactionType.Refund,
					$"Refund for ticket {ticket.Id}"
				);

				if (walletRefundSuccess)
				{
					refundRequest.Status = ApplicationStatus.Approved;
					ticket.Status = TicketStatus.Refunded;
					await _unitOfWork.GetRepository<Ticket>().UpdateAsync(ticket);
				}
				else
				{
					refundRequest.Status = ApplicationStatus.Rejected;
					refundRequest.AdminNotes += " | Wallet refund failed (unknown error).";
				}
			}

			await _unitOfWork.GetRepository<RefundRequestApplication>().UpdateAsync(refundRequest);
			await _unitOfWork.SaveAsync();

			return refundRequest.Status == ApplicationStatus.Approved
				? (true, "Refund processed successfully.")
				: (false, refundRequest.AdminNotes);
		}
	}
}