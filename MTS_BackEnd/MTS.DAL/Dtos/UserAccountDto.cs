using MTS.Data.Models;

namespace MTS.DAL.Dtos
{
	public class UserAccountDto
	{
		public Guid Id { get; set; }
		public string UserName { get; set; } = string.Empty;
		public string NormalizedUserName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string NormalizedEmail { get; set; } = string.Empty;
		public bool EmailConfirmed { get; set; }
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public DateOnly DateOfBirth { get; set; }
		public bool IsActive { get; set; } = true;
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public DateTime? DeletedAt { get; set; } = null;
		public bool IsStudent { get; set; } = false;
		public bool IsRevolutionaryContributor { get; set; } = false;
		public int RoleId { get; set; }

		public static UserAccountDto FromModelToDto(User user)
		{
			return new UserAccountDto
			{
				Id = user.Id,
				UserName = user.UserName,
				NormalizedUserName = user.NormalizedUserName,
				Email = user.Email,
				NormalizedEmail = user.NormalizedEmail,
				EmailConfirmed = user.EmailConfirmed,
				FirstName = user.FirstName,
				LastName = user.LastName,
				DateOfBirth = user.DateOfBirth,
				IsActive = user.IsActive,
				CreatedAt = user.CreatedAt,
				UpdatedAt = user.UpdatedAt,
				DeletedAt = user.DeletedAt,
				IsStudent = user.IsStudent,
				IsRevolutionaryContributor = user.IsRevolutionaryContributor,
				RoleId = user.RoleId
			};
		}
	}
}
