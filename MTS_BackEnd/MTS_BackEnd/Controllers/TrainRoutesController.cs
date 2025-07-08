using Microsoft.AspNetCore.Mvc;
using MTS.BLL;
using MTS.DAL.Dtos;

namespace MTS.BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainRoutesController : ControllerBase
    {
        private readonly IServiceProviders _serviceProviders;

        public TrainRoutesController(IServiceProviders serviceProviders)
        {
            _serviceProviders = serviceProviders;
        }

        [HttpPost("GetTrainRoute")]
        public async Task<IActionResult> GetTrainRoute([FromBody] GetTrainRouteRequest request)
        {
            if (_serviceProviders?.TrainRouteService == null)
            {
                return StatusCode(500, "Service is not available.");
            }

            var result = await _serviceProviders.TrainRouteService.GetTrainRoute(request);
            return Ok(result);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateTrainRoute([FromBody] CreateTrainRouteRequest request)
        {
            if (_serviceProviders?.TrainRouteService == null)
            {
                return StatusCode(500, "Service is not available.");
            }

            var result = await _serviceProviders.TrainRouteService.CreateTrainRoute(request);

            if (result == null || !result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("Update")]
        public async Task<IActionResult> UpdateTrainRoute([FromBody] TrainRouteDto request)
        {
            if (_serviceProviders?.TrainRouteService == null)
            {
                return StatusCode(500, "Service is not available.");
            }

            var result = await _serviceProviders.TrainRouteService.UpdateTrainRoute(request);

            if (result == null || !result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrainRoute(int id)
        {
            if (_serviceProviders?.TrainRouteService == null)
            {
                return StatusCode(500, "Service is not available.");
            }

            var result = await _serviceProviders.TrainRouteService.DeleteTrainRoute(id);

            if (!result)
            {
                return NotFound($"Train route with ID {id} not found or could not be deleted.");
            }

            return Ok(new { Message = "Train route deleted successfully." });
        }
    }
}
