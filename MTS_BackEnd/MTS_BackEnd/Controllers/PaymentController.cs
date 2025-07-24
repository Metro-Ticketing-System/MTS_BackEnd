// MTS_BackEnd/Controllers/PaymentController.cs
using Microsoft.AspNetCore.Mvc;
using MTS.BLL;
using MTS.BLL.Services.QRService;
using MTS.BLL.Services.VNPayService;
using MTS.DAL.Dtos;
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
            if (response == null || response.OrderId == null)
            {
                return Redirect("http://103.252.93.73:5000/api/Payment/paymentCallback?transactionStatus=02");
            }

            bool isSuccess = response.VnPayResponseCode == "00";

            if (!isSuccess)
            {
                if (TryParseTicketId(response.OrderId, out int ticketId))
                {
                    await _serviceProviders.TicketService.DeleteTicket(ticketId);
                }

                if (response.OrderId.StartsWith("WALLET_"))
                {
                    await HandleWalletTopUp(response, isSuccess);
                }

                return Redirect($"http://103.252.93.73:5000/api/Payment/paymentCallback?transactionStatus={response.VnPayTransactionStatus}&orderId={response.OrderId}");
            }
            if (response.OrderId.StartsWith("WALLET_")) return await HandleWalletTopUp(response, isSuccess);
            else return await HandleTicketPurchase(response);
        }

        private async Task<IActionResult> HandleTicketPurchase(PaymentResponseModel response)
        {
            if (!TryParseTicketId(response.OrderId, out int ticketId))
            {
                return Redirect($"http://103.252.93.73:5000/api/Payment/paymentCallback?transactionStatus={response.VnPayTransactionStatus}&orderId={response.OrderId}");
            }

            var ticket = await _serviceProviders.TicketService.GetTicketById(ticketId);
            if (ticket == null)
            {
                return Redirect($"http://103.252.93.73:5000/api/Payment/paymentCallback?transactionStatus={response.VnPayTransactionStatus}&orderId={response.OrderId}");
            }


            ticket.Status = Data.Enums.TicketStatus.UnUsed;
            ticket.IsPaid = true;
            ticket.QRCode = _qRTokenGeneratorService.GenerateQRToken(ticket.TicketId, ticket.PassengerId);
            ticket.PurchaseTime = DateTime.Now;

            // --- SAVE VNPay Details for Refund ---
            ticket.TxnRef = response.OrderId; // Save the original merchant transaction reference
            ticket.VnPayTransactionNo = response.TransactionId;
            ticket.VnPayTransactionDate = response.PayDate;

            var ticketDto = new TicketDto
            {
                TicketId = ticket.TicketId,
                PassengerId = ticket.PassengerId,
                PassengerName = ticket.PassengerName,
                TicketTypeId = ticket.TicketTypeId,
                TotalAmount = ticket.TotalAmount,
                ValidTo = ticket.ValidTo,
                PurchaseTime = ticket.PurchaseTime,
                TrainRouteId = ticket.TrainRouteId,
                QRCode = ticket.QRCode,
                Status = ticket.Status,
                NumberOfTicket = ticket.NumberOfTicket,
                isPaid = ticket.IsPaid,
                TxnRef = ticket.TxnRef,
                VnPayTransactionDate = ticket.VnPayTransactionDate,
                VnPayTransactionNo = ticket.VnPayTransactionNo
            };

            var result = await _serviceProviders.TicketService.UpdateTicket(ticketDto);
            if (result.IsSuccess)
            {
                return Redirect($"http://103.252.93.73:5000/api/Payment/paymentCallback?transactionStatus={response.VnPayTransactionStatus}&vnPayTransactionNo={ticket.VnPayTransactionNo}&totalAmountn={ticket.TotalAmount}");
            }
            return Redirect($"http://103.252.93.73:5000/api/Payment/paymentCallback?status=02&orderId={response.OrderId}");
        }

        private async Task<IActionResult> HandleWalletTopUp(PaymentResponseModel response, bool isSuccess)
        {
            if (!decimal.TryParse(response.Amount, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amountValue))
                return Content("Payment failed: Invalid amount.");
            var actualAmount = amountValue / 100;
            var success = await _serviceProviders.WalletService.ProcessTopUpCallbackAsync(response.OrderId, actualAmount, isSuccess);
            if (success && isSuccess) return Content("Wallet top-up successful!");
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