﻿using Microsoft.AspNetCore.Authorization;
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

			var (success, message) = await _serviceProviders.RefundService.CreateRefundRequestAsync(Guid.Parse(userId), requestDto);

			if (!success)
			{
				return BadRequest(message);
			}

			return Ok(message);
		}

		[HttpGet("requests-for-admin")]
		[Authorize(Roles = "1")] // Admin only
		public async Task<IActionResult> GetAllRefund()
		{
			var requests = await _serviceProviders.RefundService.GetAllAsync();
			return Ok(requests);
		}

		[HttpGet("requests-for-passenger")]
		[Authorize(Roles = "3")] // Passenger only
		public async Task<IActionResult> GetAllRefundForPassenger()
		{
			var userId = User.FindFirstValue("id");
			if (string.IsNullOrEmpty(userId)) return Unauthorized();
			var requests = await _serviceProviders.RefundService.GetAllForPassengerAsync(Guid.Parse(userId));
			return Ok(requests);
		}

		[HttpPatch("process/{requestId}")]
		[Authorize(Roles = "1")] // Admin only
		public async Task<IActionResult> ProcessRefund(int requestId, [FromBody] ProcessRefundRequestDto requestDto)
		{
			var adminId = User.FindFirstValue("id");
			if (string.IsNullOrEmpty(adminId)) return Unauthorized();

			var (success, message) = await _serviceProviders.RefundService.ProcessRefundRequestAsync(requestId, Guid.Parse(adminId), requestDto);

			if (!success)
			{
				return BadRequest(message);
			}

			return Ok(message);
		}
	}
}