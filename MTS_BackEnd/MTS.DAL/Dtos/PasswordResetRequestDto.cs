namespace MTS.DAL.Dtos
{
	public class PasswordResetRequestDto
	{
		public string Password { get; set; } = string.Empty;
		public string ConfirmPassword { get; set; } = string.Empty;
	}
}
