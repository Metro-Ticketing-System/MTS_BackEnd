using MTS.BLL.Services.QRService;
using MTS.DAL.Dtos;
using MTS.DAL.Repositories;
using MTS.Data.Enums;
using MTS.Data.Models;

namespace MTS.BLL.Services
{
	public interface IWalletService
	{
		Task<bool> CreateWalletAsync(Guid userId);
		Task<WalletDto?> GetWalletByUserIdAsync(Guid userId);
		Task<bool> ProcessTopUpCallbackAsync(string orderId, decimal amount, bool isSuccess);
		Task<bool> PurchaseTicketWithWalletAsync(Guid userId, int ticketId);
		Task<bool> AddToWalletAsync(Guid userId, decimal amount, TransactionType type, string description);
		Task<List<TransactionDto>> GetTransactionAsync(Guid userId);
	}

	public class WalletService : IWalletService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly QRTokenGeneratorService _qrTokenGenerator;

		public WalletService(IUnitOfWork unitOfWork, QRTokenGeneratorService qrTokenGenerator)
		{
			_unitOfWork = unitOfWork;
			_qrTokenGenerator = qrTokenGenerator;
		}

		public async Task<bool> CreateWalletAsync(Guid userId)
		{
			var existingWallet = await _unitOfWork.GetRepository<Wallet>().GetByPropertyAsync(w => w.UserId == userId);
			if (existingWallet != null)
			{
				return false;
			}

			var newWallet = new Wallet
			{
				UserId = userId,
				Balance = 0,
				CreatedAt = DateTime.UtcNow,
			};

			await _unitOfWork.GetRepository<Wallet>().AddAsync(newWallet);
			return true;
		}

		public async Task<WalletDto?> GetWalletByUserIdAsync(Guid userId)
		{
			var wallet = await _unitOfWork.GetRepository<Wallet>()
				.GetByPropertyAsync(
					w => w.UserId == userId,
					includeProperties: "Transactions"
				);

			if (wallet == null) return null;

			return new WalletDto
			{
				UserId = wallet.UserId,
				Balance = wallet.Balance,
				UpdatedAt = wallet.UpdatedAt,
				Transactions = wallet.Transactions
									 .OrderByDescending(t => t.CreatedAt)
									 .Select(t => new WalletTransactionDto
									 {
										 Id = t.Id,
										 Amount = t.Amount,
										 Type = t.Type,
										 Status = t.Status,
										 Description = t.Description,
										 CreatedAt = t.CreatedAt
									 }).ToList()
			};
		}

		public async Task<bool> ProcessTopUpCallbackAsync(string orderId, decimal amount, bool isSuccess)
		{
			var parts = orderId.Split('_');
			if (parts.Length < 2) return false;

			var userIdString = parts[1];
			if (!Guid.TryParse(userIdString, out Guid userId)) return false;

			if (isSuccess)
			{
				//var success = await AddToWalletAsync(userId, amount, TransactionType.TopUp, $"Top-up via VNPay. Order ID: {orderId}");
				//if (!success) return false;
				await AddToWalletAsync(userId, amount, TransactionType.TopUp, $"Top-up via VNPay. Order ID: {orderId}");
			}
			else
			{
				await CreateTransactionAsync(userId, amount, TransactionType.TopUp, TransactionStatus.Failed, $"Failed Top-up via VNPay. Order ID: {orderId}");
			}
			
			return await _unitOfWork.SaveAsync() > 0;
		}

		public async Task<bool> AddToWalletAsync(Guid userId, decimal amount, TransactionType type, string description)
		{
			var wallet = await _unitOfWork.GetRepository<Wallet>().GetByPropertyAsync(w => w.UserId == userId);
			//bool isNew = wallet == null;
			//if (isNew)
			//{
			//	wallet = new Wallet
			//	{
			//		UserId = userId,
			//		Balance = 0,
			//		CreatedAt = DateTime.UtcNow,
			//	};
			//	await _unitOfWork.GetRepository<Wallet>().AddAsync(wallet);
			//}

			wallet.Balance += amount;
			wallet.UpdatedAt = DateTime.UtcNow;

			//if(!isNew)
			//{
				await _unitOfWork.GetRepository<Wallet>().UpdateAsync(wallet);
			//}

			await CreateTransactionAsync(wallet.UserId, amount, type, TransactionStatus.Succeed, description);
			
			return true;
		}

		public async Task<bool> PurchaseTicketWithWalletAsync(Guid userId, int ticketId)
		{
			var wallet = await _unitOfWork.GetRepository<Wallet>().GetByPropertyAsync(w => w.UserId == userId);
			var ticket = await _unitOfWork.GetRepository<Ticket>().GetByPropertyAsync(t => t.Id == ticketId);

			if (wallet == null || ticket == null || ticket.isPaid) return false;
			if (wallet.Balance < ticket.TotalAmount)
			{
				await CreateTransactionAsync(wallet.UserId, -ticket.TotalAmount, TransactionType.Purchase, TransactionStatus.Failed, $"Ticket Purchase ID: {ticket.Id} - Insufficient funds");
				await _unitOfWork.SaveAsync();
				return false;
			}

			wallet.Balance -= ticket.TotalAmount;
			wallet.UpdatedAt = DateTime.UtcNow;
			await _unitOfWork.GetRepository<Wallet>().UpdateAsync(wallet);

			await CreateTransactionAsync(wallet.UserId, -ticket.TotalAmount, TransactionType.Purchase, TransactionStatus.Succeed, $"Ticket Purchase ID: {ticket.Id}");

			ticket.isPaid = true;
			ticket.Status = TicketStatus.UnUsed;
			ticket.PurchaseTime = DateTime.UtcNow;
			ticket.QRCode = _qrTokenGenerator.GenerateQRToken(ticket.Id, ticket.PassengerId);
			await _unitOfWork.GetRepository<Ticket>().UpdateAsync(ticket);

			return await _unitOfWork.SaveAsync() > 2;
		}

		private async Task CreateTransactionAsync(Guid userId, decimal amount, TransactionType type, TransactionStatus status, string description)
		{
			var transaction = new WalletTransaction
			{
				WalletId = userId,
				Amount = amount,
				Type = type,
				Status = status,
				Description = description,
				CreatedAt = DateTime.UtcNow
			};
			await _unitOfWork.GetRepository<WalletTransaction>().AddAsync(transaction);
		}

		public async Task<List<TransactionDto>> GetTransactionAsync(Guid userId)
		{
			var transactions = await _unitOfWork.GetRepository<WalletTransaction>().GetAllByPropertyAsync(u => u.WalletId == userId);
			if (transactions == null || !transactions.Any())
			{
				return new List<TransactionDto>();
			}
			return transactions.Select(t => new TransactionDto
			{
				Id = t.Id,
				Amount = t.Amount,
				Type = t.Type,
				Status = t.Status,
				Description = t.Description,
				CreatedAt = t.CreatedAt
			}).ToList();
		}
	}
}