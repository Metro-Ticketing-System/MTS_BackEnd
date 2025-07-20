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
	}
}
