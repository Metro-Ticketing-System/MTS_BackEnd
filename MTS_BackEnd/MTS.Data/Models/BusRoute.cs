using MTS.Data.Base;

namespace MTS.Data.Models
{
    public class BusRoute : BaseEntity
    {
        public string BusNumber { get; set; }

        public ICollection<BusRouteTerminal> BusRouteTerminals { get; set; }

    }
}
