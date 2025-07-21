using System.ComponentModel.DataAnnotations;

namespace MTS.DAL.Dtos
{
    public class CreateBusRouteRequest
    {
        [Required(ErrorMessage = "PassengerId is required.")]
        public Guid PassengerId { get; set; } // FK to User
        [Required(ErrorMessage = "BusNumber is required.")]
        public string BusNumber { get; set; }

        public List<int> TerminalId { get; set; }
    }
}
