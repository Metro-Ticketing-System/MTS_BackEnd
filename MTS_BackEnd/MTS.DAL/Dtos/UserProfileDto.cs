namespace MTS.DAL.Dtos
{
	public class UserProfileDto
	{
		public string UserName { get; set; } = string.Empty;
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;	
		public DateOnly DateOfBirth { get; set; }
		public bool IsStudent { get; set; } = false;
		public bool IsRevolutionaryContributor { get; set; } = false;

	}
}
