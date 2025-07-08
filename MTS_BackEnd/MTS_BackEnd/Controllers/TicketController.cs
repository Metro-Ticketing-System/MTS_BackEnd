using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MTS.BLL;
using MTS.DAL.Dtos;
using System.Security.Claims;

namespace MTS.BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly IServiceProviders _serviceProviders;

        public TicketController(IServiceProviders serviceProviders)
        {
            _serviceProviders = serviceProviders;
        }

        [HttpPost("CreateTicket")]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequestDto request)
        {
            if (_serviceProviders?.TicketService == null)
            {
                return StatusCode(500, "Service is not available.");
            }

            var result = await _serviceProviders.TicketService.CreateTicket(request);
            if(!result.IsSuccess)
            {
                return BadRequest(result);
            }    
            return Ok(result);
        }

		[HttpPost("PayWithWallet")]
		[Authorize]
		public async Task<IActionResult> PayTicketWithWallet([FromBody] int ticketId)
		{
			var userId = User.FindFirstValue("id");
			if (string.IsNullOrEmpty(userId)) return Unauthorized();

			var success = await _serviceProviders.WalletService.PurchaseTicketWithWalletAsync(Guid.Parse(userId), ticketId);

			if (!success)
			{
				return BadRequest("Payment failed. Please check your wallet balance or ticket status.");
			}

			return Ok("Ticket purchased successfully with wallet.");
		}
	}
}
