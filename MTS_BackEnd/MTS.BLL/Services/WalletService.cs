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
		Task<WalletDto> GetWalletByUserIdAsync(Guid userId);
		Task<bool> ProcessTopUpCallbackAsync(string orderId, decimal amount);
		Task<bool> PurchaseTicketWithWalletAsync(Guid userId, int ticketId);
		Task<bool> AddToWalletAsync(Guid userId, decimal amount, TransactionType type, string description);
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
				UpdatedAt = DateTime.UtcNow
			};

			await _unitOfWork.GetRepository<Wallet>().AddAsync(newWallet);
			return await _unitOfWork.SaveAsync() > 0;
		}

		public async Task<WalletDto> GetWalletByUserIdAsync(Guid userId)
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

		public async Task<bool> ProcessTopUpCallbackAsync(string orderId, decimal amount)
		{
			var parts = orderId.Split('_');
			if (parts.Length < 2) return false;

			var userIdString = parts[1];
			if (!Guid.TryParse(userIdString, out Guid userId)) return false;

			return await AddToWalletAsync(userId, amount, TransactionType.TopUp, $"Top-up via VNPay. Order ID: {orderId}");
		}

		public async Task<bool> AddToWalletAsync(Guid userId, decimal amount, TransactionType type, string description)
		{
			var wallet = await _unitOfWork.GetRepository<Wallet>().GetByPropertyAsync(w => w.UserId == userId);
			if (wallet == null) return false;

			wallet.Balance += amount;
			wallet.UpdatedAt = DateTime.UtcNow;

			await _unitOfWork.GetRepository<Wallet>().UpdateAsync(wallet);

			var transaction = new WalletTransaction
			{
				WalletId = wallet.UserId,
				Amount = amount,
				Type = type,
				Status = TransactionStatus.Succeed,
				Description = description,
				CreatedAt = DateTime.UtcNow
			};
			await _unitOfWork.GetRepository<WalletTransaction>().AddAsync(transaction);

			// Save changes for both Wallet and WalletTransaction
			return await _unitOfWork.SaveAsync() > 1;
		}

		public async Task<bool> PurchaseTicketWithWalletAsync(Guid userId, int ticketId)
		{
			var wallet = await _unitOfWork.GetRepository<Wallet>().GetByPropertyAsync(w => w.UserId == userId);
			var ticket = await _unitOfWork.GetRepository<Ticket>().GetByPropertyAsync(t => t.Id == ticketId);

			if (wallet == null || ticket == null || ticket.isPaid) return false;
			if (wallet.Balance < ticket.TotalAmount) return false;

			wallet.Balance -= ticket.TotalAmount.Value;
			wallet.UpdatedAt = DateTime.UtcNow;
			await _unitOfWork.GetRepository<Wallet>().UpdateAsync(wallet);

			var transaction = new WalletTransaction
			{
				WalletId = wallet.UserId,
				Amount = -ticket.TotalAmount,
				Type = TransactionType.Purchase,
				Status = TransactionStatus.Succeed,
				Description = $"Ticket Purchase ID: {ticket.Id}",
				CreatedAt = DateTime.UtcNow
			};
			await _unitOfWork.GetRepository<WalletTransaction>().AddAsync(transaction);

			ticket.isPaid = true;
			ticket.Status = TicketStatus.UnUsed;
			ticket.PurchaseTime = DateTime.UtcNow;
			ticket.QRCode = _qrTokenGenerator.GenerateQRToken(ticket.Id, ticket.PassengerId);
			await _unitOfWork.GetRepository<Ticket>().UpdateAsync(ticket);

			return await _unitOfWork.SaveAsync() > 2;
		}
	}
}