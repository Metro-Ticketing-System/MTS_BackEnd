﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MTS.BLL;
using MTS.BLL.Services.QRService;
using MTS.DAL.Dtos;
using MTS.Data.Models;
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
            if(result == null || !result.IsSuccess)
            {
                return BadRequest(result ?? null);
            }    
            return Ok(result);
        }

		[HttpPost("PayWithWallet/{ticketId}")]
		[Authorize]
		public async Task<IActionResult> PayTicketWithWallet([FromRoute] int ticketId)
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketById(int id)
        {
            if (_serviceProviders?.TicketService == null)
                return StatusCode(500, "Service is not available.");

            var result = await _serviceProviders.TicketService.GetTicketById(id);

            if (result == null)
                return NotFound($"Ticket with ID {id} not found.");

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            if (_serviceProviders?.TicketService == null)
                return StatusCode(500, "Service is not available.");

            var success = await _serviceProviders.TicketService.DeleteTicket(id);

            if (!success)
                return NotFound($"Ticket with ID {id} not found or could not be deleted.");

            return Ok(new { Message = "Ticket deleted successfully." });
        }

        [HttpPut("Update")]
        public async Task<IActionResult> UpdateTicket([FromBody] TicketDto ticket)
        {
            if (_serviceProviders?.TicketService == null)
                return StatusCode(500, "Service is not available.");

            var result = await _serviceProviders.TicketService.UpdateTicket(ticket);

            if (result == null || !result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("CreatePriority")]
        public async Task<IActionResult> CreatePriorityTicket([FromBody] CreatePriorityTicketRequestDto request)
        {
            if (_serviceProviders?.TicketService == null)
                return StatusCode(500, "Service is not available.");

            var result = await _serviceProviders.TicketService.CreatePriorityTicket(request);

            if (result == null || !result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("ListByUser/{userId}")]
        public async Task<IActionResult> GetListTicket(Guid userId)
        {
            if (_serviceProviders?.TicketService == null)
                return StatusCode(500, "Service is not available.");

            var result = await _serviceProviders.TicketService.GetListTicket(userId);
            return Ok(result);
        }

        [HttpPost("Disable/{id}")]
        public async Task<IActionResult> DisableTicket(int id)
        {
            if (_serviceProviders?.TicketService == null)
                return StatusCode(500, "Service is not available.");

            var result = await _serviceProviders.TicketService.DisableTicket(id);

            if (result == null || !result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("Activate/{id}")]
        public async Task<IActionResult> ActiveTicket(int id)
        {
            if (_serviceProviders?.TicketService == null)
                return StatusCode(500, "Service is not available.");

            var result = await _serviceProviders.TicketService.ActiveTicket(id);

            if (result == null || !result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("generate-qr")]
        public async Task<IActionResult> GetDynamicQrCode(int id)
        {
            var ticket = await _serviceProviders.TicketService.GetTicketById(id);
            if (ticket == null)
                return NotFound();
            var user = await _serviceProviders.UserService.GetUserAsync(ticket.PassengerId);
            if (user == null)
                return NotFound();

            var token = await _serviceProviders.TicketService.GenerateQRToken(ticket.PassengerId, ticket.TicketId);

            return Ok(new { qrToken = token });
        }

        [HttpPost("QRScan")]
        public async Task<IActionResult> QRScan([FromBody] QRScanRequest request)
        {
            if (_serviceProviders?.TicketService == null)
            {
                return StatusCode(500, "Service is not available.");
            }

            var result = await _serviceProviders.TicketService.QRScan(request);

            if (result.NumberOfTicket == 0)
            {
                return BadRequest(result);
            }
            // Gửi push notification tới user
            await _serviceProviders.TicketService.SendPushNotification(result.UserId, result.TicketId);
            return Ok(result);
        }

        [HttpPatch("CheckExpire/{ticketId}")]
        public async Task<IActionResult> CheckTicketExpire(int ticketId)
        {
            if (_serviceProviders?.TicketService == null)
            {
                return StatusCode(500, "Service is not available.");
            }

            var result = await _serviceProviders.TicketService.CheckTicketExpire(ticketId);

            if (result == null)
            {
                return NotFound($"Ticket with ID {ticketId} not found or expired.");
            }

            return Ok(result);
        }

        [HttpPatch("CheckExpireByUserId")]
        public async Task<IActionResult> CheckExpireByUser()
        {
            if (_serviceProviders?.TicketService == null)
            {
                return StatusCode(500, "Service is not available.");
            }

            var userId = User.FindFirstValue("id");
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authenticated!");
            var userIdGuid = Guid.Parse(userId);

            var result = await _serviceProviders.TicketService.CheckTicketExpireByUser(userIdGuid);

            if (result == null)
            {
                return BadRequest("Error");
            }

            return Ok(result);
        }

        [HttpGet("GetAllTicket")]
        public async Task<IActionResult> GetAllTicket()
        {
            if (_serviceProviders?.TicketService == null)
                return StatusCode(500, "Service is not available.");

            var result = await _serviceProviders.TicketService.GetLAllTicket();
            return Ok(result);
        }
    }
}
