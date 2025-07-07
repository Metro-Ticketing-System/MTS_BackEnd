using System.ComponentModel.DataAnnotations;

namespace MTS.DAL.Dtos
{
	public class RegisterRequestDto
	{
		[Required(ErrorMessage = "Username is required!")]
		public string UserName { get; set; } = string.Empty;
		[Required(ErrorMessage = "First name is required!")]
		public string FirstName { get; set; } = string.Empty;
		[Required(ErrorMessage = "Last name is required!")]
		public string LastName { get; set; } = string.Empty;
		[Required(ErrorMessage = "Password must not be empty!")]
		public string Password { get; set; } = string.Empty;
		[Required(ErrorMessage = "Email must not be empty!")]
		public string Email { get; set; } = string.Empty;
	}
}
