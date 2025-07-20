using MTS.DAL.Dtos;
using MTS.DAL.Repositories;
using MTS.Data.Models;

namespace MTS.BLL.Services
{
	public interface ITransactionService
	{
		Task<List<TransactionDto>> GetAllAsync();
	}
	public class TransactionService : ITransactionService
	{
		private readonly IUnitOfWork _unitOfWork;
		public TransactionService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<List<TransactionDto>> GetAllAsync()
		{
			var transactions = await _unitOfWork.GetRepository<WalletTransaction>()
				.GetAllByPropertyAsync(includeProperties: "Wallet.User");
			if (transactions == null || !transactions.Any()) return new List<TransactionDto>();
			return transactions.Select(t => new TransactionDto
			{
				Id = t.Id,
				Amount = t.Amount,
				Type = t.Type,
				Description = t.Description,
				CreatedAt = t.CreatedAt,
				UserId = t.Wallet.User.Id,
				UserName = t.Wallet.User.UserName ?? "Unknown",
				FirstName = t.Wallet.User.FirstName ?? "Unknown",
				LastName = t.Wallet.User.LastName ?? "Unknown",
				Email = t.Wallet.User.Email ?? "Unknown",
				DateOfBirth = t.Wallet.User.DateOfBirth,
				IsStudent = t.Wallet.User.IsStudent,
				IsRevolutionaryContributor = t.Wallet.User.IsRevolutionaryContributor,
				Status = t.Status,
			}).ToList();
		}
	}
}
