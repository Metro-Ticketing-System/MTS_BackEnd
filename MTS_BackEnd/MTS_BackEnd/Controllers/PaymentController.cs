// MTS_BackEnd/Controllers/PaymentController.cs
using Microsoft.AspNetCore.Mvc;
using MTS.BLL;
using MTS.BLL.Services.QRService;
using MTS.BLL.Services.VNPayService;
using System.Globalization;

namespace MTS.BackEnd.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PaymentController : ControllerBase
	{
		private readonly IServiceProviders _serviceProviders;
		private readonly QRTokenGeneratorService _qRTokenGeneratorService;
		public PaymentController(IServiceProviders serviceProviders, QRTokenGeneratorService qRTokenGeneratorService)
		{
			_serviceProviders = serviceProviders;
			_qRTokenGeneratorService = qRTokenGeneratorService;
		}

		[HttpPost("create-paymentUrl")]
		public async Task<IActionResult> CreatePaymentUrl(int ticketId)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);
			var ticket = await _serviceProviders.TicketService.GetTicketById(ticketId);
			if (ticket == null) return NotFound("Invalid ticketId");
			var model = new PaymentInformationModel()
			{
				TicketID = ticket.TicketId,
				Amount = ticket.TotalAmount,
				PassengerName = ticket.PassengerName,
			};
			var url = _serviceProviders.PaymentService.CreatePaymentUrl(model, HttpContext, ticketId.ToString());
			return Ok(new { PaymentUrl = url });
		}

		[HttpGet("paymentCallback")]
		public async Task<IActionResult> PaymentCallback()
		{
			var response = _serviceProviders.PaymentService.PaymentExecute(Request.Query);
			if (response == null || response.OrderId == null) return Content("Payment failed: Invalid response.");
			if (response.VnPayResponseCode != "00")
			{
				if (TryParseTicketId(response.OrderId, out int ticketId)) await _serviceProviders.TicketService.DeleteTicket(ticketId);
				return Content($"Payment failed. Status code: {response.VnPayResponseCode}");
			}
			if (response.OrderId.StartsWith("WALLET_")) return await HandleWalletTopUp(response);
			else return await HandleTicketPurchase(response);
		}

		private async Task<IActionResult> HandleTicketPurchase(PaymentResponseModel response)
		{
			if (!TryParseTicketId(response.OrderId, out int ticketId)) return Content("Payment failed: Could not parse ticket info.");
			var ticket = await _serviceProviders.TicketService.GetTicketById(ticketId);
			if (ticket == null) return Content("Payment failed: Ticket not found.");

			ticket.Status = Data.Enums.TicketStatus.UnUsed;
			ticket.isPaid = true;
			ticket.QRCode = _qRTokenGeneratorService.GenerateQRToken(ticket.TicketId, ticket.PassengerId);
			ticket.PurchaseTime = DateTime.UtcNow;

			// --- SAVE VNPay Details for Refund ---
			ticket.TxnRef = response.OrderId; // Save the original merchant transaction reference
			ticket.VnPayTransactionNo = response.TransactionId;
			ticket.VnPayTransactionDate = response.PayDate;

			var result = await _serviceProviders.TicketService.UpdateTicket(ticket);
			if (result.IsSuccess) return Content("Ticket purchase successful!");
			return Content("Payment failed: Could not update ticket.");
		}

		private async Task<IActionResult> HandleWalletTopUp(PaymentResponseModel response)
		{
			if (!decimal.TryParse(response.Amount, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amountValue))
				return Content("Payment failed: Invalid amount.");
			var actualAmount = amountValue / 100;
			var success = await _serviceProviders.WalletService.ProcessTopUpCallbackAsync(response.OrderId, actualAmount);
			if (success) return Content("Wallet top-up successful!");
			return Content("Wallet top-up failed.");
		}

		private bool TryParseTicketId(string orderId, out int ticketId)
		{
			ticketId = 0;
			var txnRefParts = orderId.Split('-');
			return txnRefParts.Length > 0 && int.TryParse(txnRefParts[0], out ticketId);
		}
	}
}