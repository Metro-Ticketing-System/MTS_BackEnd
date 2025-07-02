using System.ComponentModel.DataAnnotations;

namespace MTS.DAL.Dtos
{
	public class PasswordResetRequestDto
	{
		[Required(ErrorMessage = "Password is required.")]
		public string Password { get; set; } = string.Empty;
		[Required(ErrorMessage = "Confirm Password is required.")]
		public string ConfirmPassword { get; set; } = string.Empty;
	}
}
