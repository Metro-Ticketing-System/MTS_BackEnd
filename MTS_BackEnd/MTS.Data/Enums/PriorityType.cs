using System.ComponentModel.DataAnnotations;

namespace MTS.Data.Enums
{
	public enum PriorityType
	{
		[Display(Name = "Học sinh")]
		Student = 0,
		[Display(Name = "Người có công với cách mạng")]
		RevolutionaryContributor = 1
	}
}
