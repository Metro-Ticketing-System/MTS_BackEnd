using System.ComponentModel.DataAnnotations;

namespace MTS.DAL.Dtos
{
	public class StaffAccountDto
	{
		[Required(ErrorMessage = "Username is required.")]
		public string UserName { get; set; } = string.Empty;
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		[Required(ErrorMessage = "Email is required.")]
		public string Email { get; set; } = string.Empty;
		[Required(ErrorMessage = "Password is required.")]
		public string Password { get; set; } = string.Empty;
	}
}
