using MTS.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTS.Data.Models
{
    public class BusRoute : BaseEntity
    {
        public string BusNumber { get; set; }

        public ICollection<Terminal> Terminals { get; set; }

    }
}
