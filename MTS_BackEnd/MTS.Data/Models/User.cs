using System.ComponentModel.DataAnnotations.Schema;

namespace MTS.Data.Models
{
	public class User
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Id { get; set; }
		public string UserName { get; set; } = string.Empty;
		public string NormalizedUserName { get; set; } = string.Empty;
		public string PasswordHash { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string NormalizedEmail { get; set; } = string.Empty;
		public bool EmailConfirmed { get; set; }
		public string? EmailVerificationToken { get; set; } = string.Empty;
		public DateTime? EmailVerificationTokenExpiry { get; set; }
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public DateOnly DateOfBirth { get; set; }
		public bool IsActive { get; set; } = true;
		public DateTime CreatedAt { get; set; }
		public DateTime? DeletedAt { get; set; } = null;
		public bool IsStudent { get; set; } = false;
		public int RoleId { get; set; }
		public virtual Role Role { get; set; } = null!;

        // Navigation property
        public ICollection<Ticket> Tickets { get; set; }
    }
}
