namespace MTS.DAL.Dtos
{
    public class BusRouteDto
    {
        public int BusRouteId { get; set; }
        public string BusNumber { get; set; }

        public List<int> TerminalId { get; set; }
    }
}
