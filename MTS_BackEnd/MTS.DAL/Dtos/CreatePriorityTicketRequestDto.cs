using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTS.DAL.Dtos
{
    public class CreatePriorityTicketRequestDto
    {
        [Required(ErrorMessage = "PassengerId is required.")]
        public Guid PassengerId { get; set; } // FK to User
        [Required(ErrorMessage = "TicketTypeId is required.")]
        public int TicketTypeId { get; set; } // FK to TicketType
        [Required(ErrorMessage = "TrainRouteId is required.")]
        public int TrainRouteId { get; set; } // FK to TrainRoute
    }
}
