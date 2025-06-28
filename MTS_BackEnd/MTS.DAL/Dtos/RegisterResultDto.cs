using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTS.DAL.Dtos
{
	public class RegisterResultDto
	{
		public bool IsSuccess { get; set; }
		public string Email { get; set; } = string.Empty;
		public string VerificationToken { get; set; } = string.Empty;
	}
}
