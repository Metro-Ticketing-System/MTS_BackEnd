namespace MTS.DAL.Dtos
{
	public class RegisterResultDto
	{
		public bool IsSuccess { get; set; }
		public string Email { get; set; } = string.Empty;
		public string VerificationToken { get; set; } = string.Empty;
	}
}
