using MTS.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTS.Data.Models
{
    public class Terminal : BaseEntity
    {
        public string Name { get; set; }
        public string Location { get; set; }

        // Navigation properties
        public ICollection<TrainRoute> StartRoutes { get; set; }
        public ICollection<TrainRoute> EndRoutes { get; set; }
        public ICollection<BusRoute> BusRoutes { get; set; }
    }
}
