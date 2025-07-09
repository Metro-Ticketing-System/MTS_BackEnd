using MTS.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace MTS.DAL.Dtos
{
    public class CreateTicketRequestDto
    {
        [Required(ErrorMessage = "PassengerId is required.")]
        public Guid PassengerId { get; set; } // FK to User
        public int TicketTypeId { get; set; } // FK to TicketType
        public decimal TotalAmount { get; set; }
        public int? TrainRouteId { get; set; } // FK to TrainRoute
        public int? NumberOfTicket { get; set; }
    }
}
