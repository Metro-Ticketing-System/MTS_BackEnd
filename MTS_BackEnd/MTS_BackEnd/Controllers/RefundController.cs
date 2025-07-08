using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MTS.BLL;
using MTS.DAL.Dtos;
using System.Security.Claims;

namespace MTS.BackEnd.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class RefundController : ControllerBase
	{
		private readonly IServiceProviders _serviceProviders;

		public RefundController(IServiceProviders serviceProviders)
		{
			_serviceProviders = serviceProviders;
		}

		[HttpPost("request")]
		public async Task<IActionResult> RequestRefund([FromBody] CreateRefundRequestDto requestDto)
		{
			var userId = User.FindFirstValue("id");
			if (string.IsNullOrEmpty(userId)) return Unauthorized();

			var success = await _serviceProviders.RefundService.CreateRefundRequestAsync(Guid.Parse(userId), requestDto);

			if (!success)
			{
				return BadRequest("Could not create refund request. The ticket may not be eligible for a refund.");
			}

			return Ok("Refund request submitted successfully.");
		}

		[HttpGet("pending-requests")]
		[Authorize(Roles = "1")] // Admin only
		public async Task<IActionResult> GetPendingRefunds()
		{
			var requests = await _serviceProviders.RefundService.GetPendingRefundsAsync();
			return Ok(requests);
		}

		[HttpPatch("process/{requestId}")]
		[Authorize(Roles = "1")] // Admin only
		public async Task<IActionResult> ProcessRefund(int requestId, [FromBody] ProcessRefundRequestDto requestDto)
		{
			var adminId = User.FindFirstValue("id");
			if (string.IsNullOrEmpty(adminId)) return Unauthorized();

			var success = await _serviceProviders.RefundService.ProcessRefundRequestAsync(requestId, Guid.Parse(adminId), requestDto);

			if (string.IsNullOrEmpty(success))
			{
				return BadRequest("Failed to process the refund request. The request may have already been processed or an error occurred.");
			}

			return Ok("Refund request processed successfully.");
		}
	}
}