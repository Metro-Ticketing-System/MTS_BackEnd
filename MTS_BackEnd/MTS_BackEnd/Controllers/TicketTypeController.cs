using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MTS.BLL;

namespace MTS.BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketTypeController : ControllerBase
    {
        private readonly IServiceProviders _serviceProviders;

        public TicketTypeController(IServiceProviders serviceProviders)
        {
            _serviceProviders = serviceProviders;
        }

        [HttpGet("List")]
        public async Task<IActionResult> GetListTicketType()
        {
            if (_serviceProviders?.TicketTypeService == null)
            {
                return StatusCode(500, "Service is not available.");
            }

            var result = await _serviceProviders.TicketTypeService.GetListTicketType();
            return Ok(result);
        }

    }
}
