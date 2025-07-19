using MTS.Data.Enums;

namespace MTS.DAL.Dtos
{
	public class PriorityApplicationDto
	{
		public int Id { get;set; }
		public PriorityType Type { get; set; }
		public string FrontIdCardImageUrl { get; set; } = string.Empty;
		public string BackIdCardImageUrl { get; set; } = string.Empty;
		public string? StudentCardImageUrl { get; set; }
		public string? RevolutionaryContributorImageUrl { get; set; }
		public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
		public string PassengerName { get; set; } = string.Empty;
		public string? AdminName { get; set; }
		public DateTime CreatedTime { get; set; }
		public DateTime? LastUpdatedTime { get; set; }
		public string? CreatedBy { get; set; }
		public string? UpdatedBy { get; set; }
		public string? Note { get; set; }
		public UserAccountDto User { get; set; } = new UserAccountDto();
	}
}
