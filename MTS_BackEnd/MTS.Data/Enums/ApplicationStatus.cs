using System.ComponentModel.DataAnnotations;

namespace MTS.Data.Enums
{
	public enum ApplicationStatus
	{
		[Display(Name = "Đang chờ duyệt")]
		Pending = 0,
		[Display(Name = "Đã phê duyệt")]
		Approved = 1,
		[Display(Name = "Bị từ chối")]
		Rejected = 2
	}
}
