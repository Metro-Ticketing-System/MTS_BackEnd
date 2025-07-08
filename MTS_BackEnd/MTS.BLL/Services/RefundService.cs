// MTS.BLL/Services/RefundService.cs
using MTS.DAL.Dtos;
using MTS.DAL.Repositories;
using MTS.Data.Enums;
using MTS.Data.Models;

namespace MTS.BLL.Services
{
	public interface IRefundService
	{
		Task<bool> CreateRefundRequestAsync(Guid passengerId, CreateRefundRequestDto requestDto);
		Task<List<RefundRequestDto>> GetPendingRefundsAsync();
		Task<string?> ProcessRefundRequestAsync(int requestId, Guid adminId, ProcessRefundRequestDto requestDto);
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

		public async Task<bool> CreateRefundRequestAsync(Guid passengerId, CreateRefundRequestDto requestDto)
		{
			var ticket = await _unitOfWork.GetRepository<Ticket>().GetByPropertyAsync(t => t.Id == requestDto.TicketId && t.PassengerId == passengerId);
			if (ticket == null || ticket.Status != TicketStatus.UnUsed || ticket.PurchaseTime == null || ticket.PurchaseTime.Value.AddHours(24) < DateTime.UtcNow)
				return false;

			var existingRequest = await _unitOfWork.GetRepository<RefundRequestApplication>().GetByPropertyAsync(r => r.TicketId == requestDto.TicketId);
			if (existingRequest != null) return false;

			var passenger = await _unitOfWork.GetRepository<User>().GetByPropertyAsync(u => u.Id == passengerId && u.IsActive == true);	
			if (passenger == null) return false;
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
			return await _unitOfWork.SaveAsync() > 0;
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

		public async Task<string?> ProcessRefundRequestAsync(int requestId, Guid adminId, ProcessRefundRequestDto requestDto)
		{
			var refundRequest = await _unitOfWork.GetRepository<RefundRequestApplication>().GetByPropertyAsync(r => r.Id == requestId, includeProperties: "Ticket,Passenger");
			if (refundRequest == null || refundRequest.Status != ApplicationStatus.Pending)
				return "Request not found or already processed.";

			var admin = await _unitOfWork.GetRepository<User>().GetByPropertyAsync(u => u.Id == adminId);
			if (admin == null) return "Processing user not found.";

			refundRequest.AdminId = adminId;
			refundRequest.AdminNotes = requestDto.AdminNotes;
			refundRequest.ProcessedAt = DateTime.UtcNow;

			if (requestDto.Status == ApplicationStatus.Rejected)
			{
				refundRequest.Status = ApplicationStatus.Rejected;
				await _unitOfWork.GetRepository<RefundRequestApplication>().UpdateAsync(refundRequest);
				await _unitOfWork.SaveAsync();
				return "Refund rejected successfully.";
			}

			var ticket = refundRequest.Ticket;
			decimal refundAmount = ticket.TotalAmount * 0.9m;

			// Check if it was a VNPay transaction
			if (!string.IsNullOrEmpty(ticket.TxnRef) && !string.IsNullOrEmpty(ticket.VnPayTransactionNo) && !string.IsNullOrEmpty(ticket.VnPayTransactionDate))
			{
				long refundAmountInCents = (long)(refundAmount * 100);

				var refundResponse = await _refundGateway.SendRefundRequestAsync(
					refundAmountInCents,
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
					refundRequest.AdminNotes += " | Wallet refund failed.";
				}
			}

			await _unitOfWork.GetRepository<RefundRequestApplication>().UpdateAsync(refundRequest);
			await _unitOfWork.SaveAsync();

			return refundRequest.Status == ApplicationStatus.Approved 
				? "Refund processed successfully." 
				: refundRequest.AdminNotes;
		}
	}
}