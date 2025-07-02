using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTS.DAL.Dtos
{
    public class CreateTicketResponseDto
    {
        public bool IsSuccess { get; set; }
        public int TicketId { get; set; }
    }
}
