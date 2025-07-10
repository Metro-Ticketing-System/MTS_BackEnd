using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTS.DAL.Dtos
{
    public class QRScanRequest
    {
        [Required]
        public string QRToken { get; set; }
        [Required]
        public int TerminalId { get; set; }
        [Required]
        public bool isOut { get; set; }
    }
}
