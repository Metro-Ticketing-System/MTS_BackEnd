using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MTS.BLL;
using MTS.BLL.Services.QRService;
using MTS.BLL.Services.VNPayService;

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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ticket = await _serviceProviders.TicketService.GetTicketById(ticketId);
            if (ticket == null)
                return NotFound("Invalid ticketId");

            var model = new PaymentInformationModel()
            {
                TicketID = ticket.TicketId,
                Amount = ticket.TotalAmount,
                PassengerName = ticket.PassengerName,
            };

            var url = _serviceProviders.PaymentService.CreatePaymentUrl(model, HttpContext);

            return Ok(new { PaymentUrl = url });
        }

        [HttpGet("paymentCallback")]
        public async Task<IActionResult> PaymentCallback()
        {
            var response = _serviceProviders.PaymentService.PaymentExecute(Request.Query);

            if (response == null || response.OrderId == null)
                // return Redirect("http://localhost:5173/paymentfailed");
                return NotFound("NOT FOUND!");

            if (!TryParseBookingId(response.OrderId, out int ticketId))
                // return Redirect("http://localhost:5173/paymentfailed");
                return NotFound("NOT FOUND!");

            var ticket = await _serviceProviders.TicketService.GetTicketById(ticketId);
            if (ticket == null)
            {
                // return Redirect("http://localhost:5173/paymentfailed");
                return NotFound("NOT FOUND!");
            }

            if (response.VnPayResponseCode != "00")
            {
                _serviceProviders.TicketService.DeleteTicket(ticket.TicketId);
                // return Redirect("http://localhost:5173/paymentfailed");
                return NotFound("NOT FOUND!");
            }

            ticket.Status = Data.Enums.TicketStatus.UnUsed;
            ticket.isPaid = true;
            ticket.QRCode = _qRTokenGeneratorService.GenerateQRToken(ticket.TicketId, ticket.PassengerId);
            ticket.PurchaseTime = DateTime.UtcNow;
            var result = await _serviceProviders.TicketService.UpdateTicket(ticket);
            if(result.IsSuccess)
            {
                //return Redirect($"http://localhost:5173/paymentsuccess/{ticketId}");
                return Ok("COMPLETE");
            }
            return NotFound("NOT FOUND!");
        }

        private bool TryParseBookingId(string orderId, out int ticketId)
        {
            ticketId = 0;
            var txnRefParts = orderId.Split('-');
            return txnRefParts.Length > 0 && int.TryParse(txnRefParts[0], out ticketId);
        }
    }
}
