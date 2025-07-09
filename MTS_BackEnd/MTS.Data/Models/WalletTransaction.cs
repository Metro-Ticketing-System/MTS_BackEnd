using MTS.Data.Base;
using MTS.Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MTS.Data.Models
{
	public class WalletTransaction
	{
		public int Id { get; set; }
		public Guid WalletId { get; set; }
		public decimal? Amount { get; set; }
		public TransactionType Type { get; set; }
		public TransactionStatus Status { get; set; }
		public string? Description { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		[ForeignKey("WalletId")]
		public virtual Wallet Wallet { get; set; } = null!;
	}
}
