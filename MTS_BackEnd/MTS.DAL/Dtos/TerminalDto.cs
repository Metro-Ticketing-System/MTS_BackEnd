using MTS.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTS.DAL.Dtos
{
    public class TerminalDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public List<TrainRoute> StartRoutes { get; set; }
        public List<TrainRoute> EndRoutes { get; set; }
        public List<BusRoute> BusRoutes { get; set; }
    }
}
