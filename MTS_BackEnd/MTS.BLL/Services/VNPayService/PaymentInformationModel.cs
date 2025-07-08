namespace MTS.BLL.Services.VNPayService
{
    public class PaymentInformationModel
    {
        public int? TicketID { get; set; }
        public string? OrderId { get; set; }
		public decimal Amount { get; set; }
        public string? PassengerName { get; set; }
    }
}
