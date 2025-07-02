using MTS.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace MTS.DAL.Dtos
{
    public class CreateTicketRequestDto
    {
        [Required(ErrorMessage = "PassengerId is required.")]
        public Guid PassengerId { get; set; } // FK to User
        [Required(ErrorMessage = "TicketTypeId is required.")]
        public int TicketTypeId { get; set; } // FK to TicketType
        [Required(ErrorMessage = "TotalAmount is required.")]
        public decimal TotalAmount { get; set; }
        [Required(ErrorMessage = "TrainRouteId is required.")]
        public int TrainRouteId { get; set; } // FK to TrainRoute
        [Required(ErrorMessage = "NumberOfTicket is required.")]
        public int NumberOfTicket { get; set; }
    }
}
