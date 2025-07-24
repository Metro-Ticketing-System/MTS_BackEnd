using MTS.Data.Base;
using MTS.Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MTS.Data.Models
{
	public class RefundRequestApplication : BaseEntity
	{
		public int TicketId { get; set; }
		public Guid PassengerId { get; set; }
		public string Reason { get; set; } = string.Empty;
		public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
		public DateTime RequestedAt { get; set; } = DateTime.Now;
		public Guid? AdminId { get; set; }
		public DateTime? ProcessedAt { get; set; }
		public string? AdminNotes { get; set; }

		[ForeignKey(nameof(TicketId))]
		public virtual Ticket Ticket { get; set; } = null!;
		[ForeignKey(nameof(PassengerId))]
		public virtual User Passenger { get; set; } = null!;
		[ForeignKey(nameof(AdminId))]
		public virtual User? Admin { get; set; }
	}
}
