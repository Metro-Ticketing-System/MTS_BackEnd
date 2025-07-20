using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MTS.BLL;
using MTS.DAL.Dtos;

namespace MTS.BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TerminalController : ControllerBase
    {
        private readonly IServiceProviders _serviceProviders;

        public TerminalController(IServiceProviders serviceProviders)
        {
            _serviceProviders = serviceProviders;
        }

        [HttpGet("{terminalId}/bus-routes")]
        public async Task<IActionResult> GetBusRoutes(int terminalId)
        {
            if (_serviceProviders?.TerminalService == null)
            {
                return StatusCode(500, "Service is not available.");
            }

            var result = await _serviceProviders.TerminalService.GetBusRoutes(terminalId);
            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllTerminals()
        {
            if (_serviceProviders?.TerminalService == null)
            {
                return StatusCode(500, "Service is not available.");
            }

            var result = await _serviceProviders.TerminalService.GetAllTerminals();
            return Ok(result);
        }

        [HttpGet("{terminalId}")]
        public async Task<IActionResult> GetTerminalById(int terminalId)
        {
            if (_serviceProviders?.TerminalService == null)
            {
                return StatusCode(500, "Service is not available.");
            }

            var result = await _serviceProviders.TerminalService.GetTerminalById(terminalId);

            if (result == null)
            {
                return NotFound($"Terminal with ID {terminalId} not found.");
            }

            return Ok(result);
        }

        [HttpPost("create-terminal")]
        public async Task<IActionResult> CreateTerminal([FromBody] CreateTerminalRequest request)
        {
            if (_serviceProviders?.TerminalService == null)
            {
                return StatusCode(500, "Service is not available.");
            }
            if (request == null || string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Location))
            {
                return BadRequest("Invalid terminal data.");
            }
            var result = await _serviceProviders.TerminalService.CreateTerminal(request);
            if (result == null)
            {
                return StatusCode(500, "Failed to create terminal.");
            }
            return Ok(result);
		}
	}
}
