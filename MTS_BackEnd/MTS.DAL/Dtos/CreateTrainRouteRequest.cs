using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTS.DAL.Dtos
{
    public class CreateTrainRouteRequest
    {
        [Required(ErrorMessage = "PassengerId is required.")]
        public Guid UserId { get; set; } // FK to User
        public decimal Price { get; set; }
        public int StartTerminal { get; set; }
        public int EndTerminal { get; set; }
    }
}
