using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MTS.BLL;
using MTS.BLL.Services.QRService;
using MTS.DAL.Dtos;
using System.Security.Claims;

namespace MTS.BackEnd.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class WalletController : Controller
	{
		private readonly IServiceProviders _serviceProviders;
		public WalletController(IServiceProviders serviceProviders)
		{
			_serviceProviders = serviceProviders;
		}

		[HttpGet("my-wallet")]
		public async Task<IActionResult> GetMyWallet()
		{
			var userId = User.FindFirstValue("id");
			if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authenticated!");

			var wallet = await _serviceProviders.WalletService.GetWalletByUserIdAsync(Guid.Parse(userId));
			if (wallet == null)
			{
				return NotFound("Wallet not found for this user.");
			}
			return Ok(wallet);
		}

		[HttpPost("create-topup-url")]
		public async Task<IActionResult> CreateTopUpUrl([FromBody] TopUpRequestDto topUpRequest)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var userId = User.FindFirstValue("id");
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized();
			}

			var user = await _serviceProviders.UserService.GetUserAsync(Guid.Parse(userId));
			if (user == null)
			{
				return NotFound("User not found.");
			}

			var paymentModel = new BLL.Services.VNPayService.PaymentInformationModel
			{
				Amount = topUpRequest.Amount,
				PassengerName = user.LastName + " " + user.FirstName,
				OrderId = $"WALLET_{userId}_TOPUP",
			};

			// The order prefix helps differentiate a wallet top-up from a ticket purchase
			var url = _serviceProviders.PaymentService.CreatePaymentUrl(paymentModel, HttpContext, paymentModel.OrderId);

			return Ok(new { TopUpUrl = url });
		}

		[HttpGet("transaction-history")]	
		public async Task<IActionResult> GetTransactionHistory()
		{
			var userId = User.FindFirstValue("id");
			if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authenticated!");
			var transactions = await _serviceProviders.WalletService.GetTransactionAsync(Guid.Parse(userId));
			if (transactions == null || !transactions.Any())
			{
				return NotFound("No transaction history found for this user.");
			}
			return Ok(transactions);
		}
	}
}
