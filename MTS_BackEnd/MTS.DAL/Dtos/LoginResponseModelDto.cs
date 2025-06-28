namespace MTS.DAL.Dtos
{
	public class LoginResponseModelDto
	{
		public Guid Id { get; set; }
		public string Username { get; set; } = string.Empty;
		public int Role { get; set; }
		public string Token { get; set; } = string.Empty;
	}
}
