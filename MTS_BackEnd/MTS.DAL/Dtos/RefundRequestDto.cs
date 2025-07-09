using MTS.Data.Enums;

namespace MTS.DAL.Dtos
{
	public class RefundRequestDto
	{
		public int Id { get; set; }
		public int TicketId { get; set; }
		public string PassengerName { get; set; }
		public decimal? TicketAmount { get; set; }
		public string Reason { get; set; }
		public ApplicationStatus Status { get; set; }
		public DateTime RequestedAt { get; set; }
		public string? AdminName { get; set; }
		public DateTime? ProcessedAt { get; set; }
		public string? AdminNotes { get; set; }
	}
}
