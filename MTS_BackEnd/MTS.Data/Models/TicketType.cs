using MTS.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTS.Data.Models
{
    public class TicketType : BaseEntity
    {
        public string TicketTypeName { get; set; }
        public decimal Price { get; set; }

        // Navigation property
        public ICollection<Ticket> Tickets { get; set; }

    }
}
