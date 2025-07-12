using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTS.DAL.Dtos
{
    public class GetTrainRouteResponse
    {
        public int TrainRouteId { get; set; }
        public decimal Price { get; set; }
        public int StartTerminal { get; set; }
        public int EndTerminal { get; set; }
        public string StartTerminalName { get; set; }
        public string EndTerminalName { get; set; }
    }
}
