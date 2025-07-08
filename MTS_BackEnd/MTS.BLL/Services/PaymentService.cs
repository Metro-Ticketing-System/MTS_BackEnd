// MTS.BLL/Services/PaymentService.cs
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MTS.BLL.Services.VNPayService;

namespace MTS.BLL.Services
{
	public interface IPaymentService
	{
		string CreatePaymentUrl(PaymentInformationModel model, HttpContext context, string? orderPrefix);
		PaymentResponseModel PaymentExecute(IQueryCollection collections);
	}
	public class PaymentService : IPaymentService
	{
		private readonly IConfiguration _configuration;
		public PaymentService(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context, string? orderPrefix)
		{
			var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]!);
			var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
			var pay = new VnPayLibrary();
			var urlCallBack = _configuration["PaymentCallBack:ReturnUrl"];
			var createDate = timeNow.ToString("yyyyMMddHHmmss");

			string uniqueTxnRef;
			string orderInfo;

			if (!string.IsNullOrEmpty(orderPrefix) && orderPrefix.StartsWith("WALLET_"))
			{
				uniqueTxnRef = $"{orderPrefix}_{DateTime.Now:yyyyMMddHHmmss}";
				orderInfo = $"Top up wallet for {model.PassengerName}, Amount: {model.Amount}";
			}
			else
			{
				uniqueTxnRef = $"{model.TicketID}-{DateTime.Now:yyyyMMddHHmmss}";
				// Pass the CreateDate in the OrderInfo field to be retrieved later.
				orderInfo = createDate;
			}

			pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]!);
			pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]!);
			pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]!);
			pay.AddRequestData("vnp_Amount", ((int)model.Amount * 100).ToString());
			pay.AddRequestData("vnp_CreateDate", createDate);
			pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]!);
			pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
			pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]!);
			pay.AddRequestData("vnp_OrderInfo", orderInfo);
			pay.AddRequestData("vnp_OrderType", _configuration["Vnpay:OrderType"]!);
			pay.AddRequestData("vnp_ReturnUrl", urlCallBack!);
			pay.AddRequestData("vnp_TxnRef", uniqueTxnRef);

			var paymentUrl =
				pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"]!, _configuration["Vnpay:HashSecret"]!);

			return paymentUrl;
		}

		public PaymentResponseModel PaymentExecute(IQueryCollection collections)
		{
			var pay = new VnPayLibrary();
			var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]!);
			return response;
		}
	}
}