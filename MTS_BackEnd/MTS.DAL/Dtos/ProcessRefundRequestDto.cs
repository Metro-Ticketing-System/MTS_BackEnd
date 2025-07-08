using MTS.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace MTS.DAL.Dtos
{
	public class ProcessRefundRequestDto
	{
		[Required]
		public ApplicationStatus Status { get; set; }
		public string? AdminNotes { get; set; }
	}
}
