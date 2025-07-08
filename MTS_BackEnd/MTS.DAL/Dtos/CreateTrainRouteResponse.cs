using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTS.DAL.Dtos
{
    public class CreateTrainRouteResponse
    {
        public bool IsSuccess { get; set; } = false;
        public int Id { get; set; }
    }
}
