using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTS.DAL.Dtos
{
    public class TicketTypeDto
    {
        public int TicketTypeId { get; set; }
        public string TicketTypeName { get; set; }
        public decimal Price { get; set; }
    }
}
