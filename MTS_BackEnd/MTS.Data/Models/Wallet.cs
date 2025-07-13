using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MTS.Data.Models
{
	public class Wallet
	{
		[Key]
		[ForeignKey("User")]
		public Guid UserId { get; set; }
		public decimal Balance { get; set; } = 0;
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public virtual User User { get; set; } = null!;
		public virtual ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
	}
}
