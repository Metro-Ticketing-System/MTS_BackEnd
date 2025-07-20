using MTS.Data.Enums;

namespace MTS.DAL.Dtos
{
	public class TransactionDto
	{
		public int Id { get; set; }
		public decimal? Amount { get; set; }
		public TransactionType Type { get; set; }
		public TransactionStatus Status { get; set; }
		public string? Description { get; set; }
		public DateTime CreatedAt { get; set; }
		public Guid UserId { get; set; } = Guid.Empty;
		public string UserName { get; set; } = string.Empty;
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public DateOnly DateOfBirth { get; set; }
		public bool IsStudent { get; set; } = false;
		public bool IsRevolutionaryContributor { get; set; } = false;
	}
}
