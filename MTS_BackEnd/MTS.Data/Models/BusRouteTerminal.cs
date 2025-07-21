namespace MTS.Data.Models
{
    public class BusRouteTerminal
    {
        public int BusRouteId { get; set; }
        public BusRoute BusRoute { get; set; }

        public int TerminalId { get; set; }
        public Terminal Terminal { get; set; }
    }

}
