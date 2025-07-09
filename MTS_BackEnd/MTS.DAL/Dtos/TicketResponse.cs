using MTS.Data.Enums;

namespace MTS.DAL.Dtos
{
    public class TicketResponse
    {
        public int TicketId { get; set; }
        public Guid PassengerId { get; set; } // FK to User
        public string? PassengerName { get; set; }
        public string Email { get; set; }
        public int TicketTypeId { get; set; } // FK to TicketType
        public string TicketTypeName { get; set; }
        public decimal? TotalAmount { get; set; }
        public DateTime ValidTo { get; set; }
        public DateTime? PurchaseTime { get; set; }
        public int? TrainRouteId { get; set; } // FK to TrainRoute
        public decimal? TrainRoutePrice { get; set; }
        public int? StartTerminal { get; set; }
        public int? EndTerminal { get; set; }
        public string? QRCode { get; set; }
        public TicketStatus Status { get; set; }
        public int? NumberOfTicket { get; set; }
        public bool isPaid { get; set; }
        public string? TxnRef { get; set; }
        public string? VnPayTransactionNo { get; set; }
        public string? VnPayTransactionDate { get; set; }
    }
}
