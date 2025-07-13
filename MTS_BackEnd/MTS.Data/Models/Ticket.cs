using MTS.Data.Base;
using MTS.Data.Enums;

namespace MTS.Data.Models
{
    public class Ticket : BaseEntity
    {
        public Guid PassengerId { get; set; } // FK to User
        public int TicketTypeId { get; set; } // FK to TicketType
        public decimal TotalAmount { get; set; }
        public DateTime ValidTo { get; set; }
        public DateTime? PurchaseTime { get; set; }
        public int? TrainRouteId { get; set; } // FK to TrainRoute
        public string? QRCode { get; set; }
        public TicketStatus Status { get; set; }
        public int? NumberOfTicket { get; set; }
        public bool isPaid { get; set; } = false;

		// --- Fields for VNPay ---
		public string? TxnRef { get; set; } // The original vnp_TxnRef from payment
		public string? VnPayTransactionNo { get; set; } // The vnp_TransactionNo from VNPay
		public string? VnPayTransactionDate { get; set; } // Storing as yyyyMMddHHmmss

		// Navigation properties
		public TicketType TicketType { get; set; }
        public TrainRoute? TrainRoute { get; set; }
        public User Passenger { get; set; }

    }
}
