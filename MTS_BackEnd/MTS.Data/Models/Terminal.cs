using MTS.Data.Base;

namespace MTS.Data.Models
{
    public class Terminal : BaseEntity
    {
        public string Name { get; set; }
        public string Location { get; set; }

        // Navigation properties
        public ICollection<TrainRoute> StartRoutes { get; set; }
        public ICollection<TrainRoute> EndRoutes { get; set; }
        public ICollection<BusRouteTerminal> BusRouteTerminals { get; set; }
    }
}
