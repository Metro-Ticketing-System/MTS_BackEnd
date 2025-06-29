namespace MTS.DAL.Dtos
{
	public class PasswordResetRequestResultDto
	{
		public bool IsSucceed { get; set; }
		public string? Email { get; set; }
		public string? PasswordResetToken { get; set; }
	}
}
