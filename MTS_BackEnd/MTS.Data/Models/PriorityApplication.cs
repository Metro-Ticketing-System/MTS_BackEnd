using MTS.Data.Base;
using MTS.Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MTS.Data.Models
{
	public class PriorityApplication : BaseEntity
	{
		public PriorityType Type { get; set; }
		public string FrontIdCardImageUrl { get; set; } = string.Empty;
		public string BackIdCardImageUrl { get; set; } = string.Empty;
		public string? StudentCardImageUrl { get; set; }
		public string? RevolutionaryContributorImageUrl { get; set; }
		public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
		public string? Note { get; set; }
		public Guid PassengerId { get; set; }
		[ForeignKey(nameof(PassengerId))]
		public User Passenger { get; set; } = null!;
		public Guid? AdminId { get; set; }

		[ForeignKey(nameof(AdminId))]
		public User? Admin { get; set; }

	}
}
