using System.ComponentModel.DataAnnotations;

namespace MTS.DAL.Dtos
{
	public class TopUpRequestDto
	{
		[Required]
		[Range(10000, 10000000, ErrorMessage = "Amount must be between 10,000 and 10,000,000.")]
		public decimal Amount { get; set; }
	}
}
