using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTS.DAL.Dtos
{
    public class CreateTerminalRequest
    {
        public Guid UserId { get; set; } // FK to User
        public string Name { get; set; }
        public string Location { get; set; }
    }
}
