using MTS.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTS.DAL.Dtos
{
    public class TicketDto
    {
        public int TicketId { get; set; }
        public Guid PassengerId { get; set; } // FK to User
        public int TicketTypeId { get; set; } // FK to TicketType
        public decimal TotalAmount { get; set; }
        public DateTime ValidTo { get; set; }
        public DateTime? PurchaseTime { get; set; }
        public int TrainRouteId { get; set; } // FK to TrainRoute
        public string? QRCode { get; set; }
        public TicketStatus Status { get; set; }
        public int NumberOfTicket { get; set; }
        public string? PassengerName { get; set; }
        public bool isPaid { get; set; }
    }
}
