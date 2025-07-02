using MTS.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTS.Data.Models
{
    public class TrainRoute : BaseEntity
    {
        public decimal Price { get; set; }
        public int StartTerminal { get; set; }
        public int EndTerminal { get; set; }

        // Navigation properties
        public Terminal StartTerminalNavigation { get; set; }
        public Terminal EndTerminalNavigation { get; set; }

        public ICollection<Ticket> Tickets { get; set; }

    }
}
