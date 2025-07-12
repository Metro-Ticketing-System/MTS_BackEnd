using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTS.DAL.Dtos
{
    public class PushTokenDto
    {
        public Guid UserId { get; set; }
        public string ExpoPushToken { get; set; }
    }


}
