using MTS.Data.Enums;
using MTS.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace MTS.DAL.Dtos
{
	public class CreatePriorityApplicationDto
	{
		[Required]
		public Guid PassengerId { get; set; }
		[Required]
		public PriorityType Type { get; set; }
	}
}
