namespace MTS.Data.Base
{
	public class BaseEntity
	{
		public int Id { get; set; }
		public string? CreatedBy { get; set; }
		public string? UpdatedBy { get; set; }
		//public string? DeletedBy { get; set; }
		public bool IsDeleted { get; set; } = false;	
		public DateTime CreatedTime { get; set; }
		public DateTime? LastUpdatedTime { get; set; }
		//public DateTime? DeletedTime { get; set; }
	}
}
