using System.ComponentModel.DataAnnotations;

namespace MTS.DAL.Dtos
{
	public class CreateRefundRequestDto
	{
		[Required]
		public int TicketId { get; set; }
		[Required]
		[StringLength(500, MinimumLength = 10)]
		public string Reason { get; set; } = string.Empty;
	}
}
