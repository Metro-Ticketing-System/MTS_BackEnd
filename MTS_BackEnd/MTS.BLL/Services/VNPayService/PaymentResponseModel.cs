namespace MTS.BLL.Services.VNPayService
{
    public class PaymentResponseModel
    {
        public string? OrderDescription { get; set; } // This will hold the transaction date
        public string? TransactionId { get; set; } // This will hold vnp_TransactionNo
        public string? OrderId { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentId { get; set; }
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? VnPayResponseCode { get; set; }
        public string? Amount { get; set; }
        public string? PayDate { get; set; } // Add this property to store the payment date from VNPay
        public string? VnPayTransactionStatus { get; set; }
    }
}
