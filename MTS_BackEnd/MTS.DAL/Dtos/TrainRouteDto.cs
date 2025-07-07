using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTS.DAL.Dtos
{
    public class TrainRouteDto
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int StartTerminal { get; set; }
        public int EndTerminal { get; set; }
    }
}
