using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTS.BLL.Services.VNPayService
{
    public class PaymentInformationModel
    {
        public int TicketID { get; set; }
        public decimal Amount { get; set; }
        public string? PassengerName { get; set; }
    }
}
