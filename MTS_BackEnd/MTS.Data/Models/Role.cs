using MTS.Data.Base;

namespace MTS.Data.Models
{
	public class Role : BaseEntity
	{
		public string Name { get; set; } = string.Empty;
		public string NormalizedName { get; set; } = string.Empty;
		public virtual ICollection<User> Users { get; set; } = new List<User>();
	}
}
