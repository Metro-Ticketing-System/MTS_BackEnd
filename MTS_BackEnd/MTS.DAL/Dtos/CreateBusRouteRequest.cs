using MTS.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTS.DAL.Dtos
{
    public class CreateBusRouteRequest
    {
        [Required(ErrorMessage = "PassengerId is required.")]
        public Guid PassengerId { get; set; } // FK to User
        [Required(ErrorMessage = "BusNumber is required.")]
        public string BusNumber { get; set; }

        public List<int> TerminalId { get; set; }
    }
}
