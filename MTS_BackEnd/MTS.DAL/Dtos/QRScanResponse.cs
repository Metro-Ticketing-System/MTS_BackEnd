using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTS.DAL.Dtos
{
    public class QRScanResponse
    {
        public int NumberOfTicket { get; set; } = 0;
        public string Message { get; set; }
    }
}
