using Microsoft.AspNetCore.Mvc;
using MTS.BLL;
using MTS.DAL.Dtos;

namespace MTS.BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusRoutesController : ControllerBase
    {
        private readonly IServiceProviders _serviceProviders;

        public BusRoutesController(IServiceProviders serviceProviders)
        {
            _serviceProviders = serviceProviders;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (_serviceProviders?.BusRouteService == null)
            {
                return StatusCode(500, "Service is not available.");
            }

            var result = await _serviceProviders.BusRouteService.GetById(id);

            if (result == null)
            {
                return NotFound($"Bus route with ID {id} not found.");
            }

            return Ok(result);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateBusRoute([FromBody] CreateBusRouteRequest request)
        {
            if (_serviceProviders?.BusRouteService == null)
            {
                return StatusCode(500, "Service is not available.");
            }

            var result = await _serviceProviders.BusRouteService.CreateBusRoute(request);

            if (result == null || !result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("Update")]
        public async Task<IActionResult> UpdateBusRoute([FromBody] BusRouteDto request)
        {
            if (_serviceProviders?.BusRouteService == null)
            {
                return StatusCode(500, "Service is not available.");
            }

            var result = await _serviceProviders.BusRouteService.UpdateBusRoute(request);

            if (result == null || !result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBusRoute(int id)
        {
            if (_serviceProviders?.BusRouteService == null)
            {
                return StatusCode(500, "Service is not available.");
            }

            var result = await _serviceProviders.BusRouteService.DeleteBusRoute(id);

            if (!result)
            {
                return NotFound($"Bus route with ID {id} not found or could not be deleted.");
            }

            return Ok(new { Message = "Bus route deleted successfully." });
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllBusRoutes()
        {
            if (_serviceProviders?.BusRouteService == null)
            {
                return StatusCode(500, "Service is not available.");
            }
            var result = await _serviceProviders.BusRouteService.GetAll();
            if (result == null || !result.Any())
            {
                return NotFound("No bus route found.");
            }
            return Ok(result);
        }
    }
}
