// MTS.BLL/Services/VNPayService/VNPayRefundGatewayService.cs
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MTS.BLL.Services.VNPayService
{
	public class VNPayRefundResponse
	{
		public string vnp_ResponseId { get; set; }
		public string vnp_Command { get; set; }
		public string vnp_ResponseCode { get; set; }
		public string vnp_Message { get; set; }
		public string vnp_TmnCode { get; set; }
		public string vnp_TxnRef { get; set; }
		public string vnp_Amount { get; set; }
		public string vnp_OrderInfo { get; set; }
		public string vnp_BankCode { get; set; }
		public string vnp_PayDate { get; set; }
		public string vnp_TransactionNo { get; set; }
		public string vnp_TransactionType { get; set; }
		public string vnp_TransactionStatus { get; set; }
		public string vnp_SecureHash { get; set; }
	}

	public interface IVNPayRefundGatewayService
	{
		Task<VNPayRefundResponse> SendRefundRequestAsync(long amount, string transactionType, string originalTxnRef, string vnpTransactionNo, string transDate, string user);
	}

	public class VNPayRefundGatewayService : IVNPayRefundGatewayService
	{
		private readonly IConfiguration _configuration;
		private readonly HttpClient _httpClient;

		public VNPayRefundGatewayService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
		{
			_configuration = configuration;
			_httpClient = httpClientFactory.CreateClient("VNPayRefund");
		}

		public async Task<VNPayRefundResponse> SendRefundRequestAsync(long amount, string transactionType, string originalTxnRef, string vnpTransactionNo, string transDate, string user)
		{
			var apiUrl = "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction";
			var tmnCode = _configuration["Vnpay:TmnCode"];
			var hashSecret = _configuration["Vnpay:HashSecret"];

			var requestId = DateTime.Now.ToString("yyyyMMddHHmmssfff");
			var requestTime = DateTime.Now.ToString("yyyyMMddHHmmss");
			var vnpTransactionNoForRequest = string.IsNullOrEmpty(vnpTransactionNo) ? "0" : vnpTransactionNo;


			var data = new SortedDictionary<string, string>(new VnPayCompare())
			{
				{"vnp_RequestId", requestId},
				{"vnp_Version", "2.1.0"},
				{"vnp_Command", "refund"},
				{"vnp_TmnCode", tmnCode},
				{"vnp_TransactionType", transactionType},
				{"vnp_TxnRef", originalTxnRef},
				{"vnp_Amount", amount.ToString()},
				{"vnp_OrderInfo", $"Hoan tien cho don hang {originalTxnRef}"},
				{"vnp_TransactionNo", vnpTransactionNoForRequest},
				{"vnp_TransactionDate", transDate},
				{"vnp_CreateBy", user},
				{"vnp_CreateDate", requestTime},
				{"vnp_IpAddr", "127.0.0.1"}
			};

			var dataToHash = new StringBuilder();
			dataToHash.Append(data["vnp_RequestId"]);
			dataToHash.Append("|" + data["vnp_Version"]);
			dataToHash.Append("|" + data["vnp_Command"]);
			dataToHash.Append("|" + data["vnp_TmnCode"]);
			dataToHash.Append("|" + data["vnp_TransactionType"]);
			dataToHash.Append("|" + data["vnp_TxnRef"]);
			dataToHash.Append("|" + data["vnp_Amount"]);
			dataToHash.Append("|" + data["vnp_TransactionNo"]);
			dataToHash.Append("|" + data["vnp_TransactionDate"]);
			dataToHash.Append("|" + data["vnp_CreateBy"]);
			dataToHash.Append("|" + data["vnp_CreateDate"]);
			dataToHash.Append("|" + data["vnp_IpAddr"]);
			dataToHash.Append("|" + data["vnp_OrderInfo"]);

			var secureHash = HmacSha512(hashSecret, dataToHash.ToString());
			data.Add("vnp_SecureHash", secureHash);

			try
			{
				var response = await _httpClient.PostAsJsonAsync(apiUrl, data);
				if (response.IsSuccessStatusCode)
				{
					var responseString = await response.Content.ReadAsStringAsync();
					return JsonSerializer.Deserialize<VNPayRefundResponse>(responseString) ?? new VNPayRefundResponse { vnp_ResponseCode = "99", vnp_Message = "Failed to parse response." };
				}
				var errorContent = await response.Content.ReadAsStringAsync();
				return new VNPayRefundResponse { vnp_ResponseCode = "99", vnp_Message = $"Request failed: {response.ReasonPhrase} - {errorContent}" };
			}
			catch (Exception ex)
			{
				return new VNPayRefundResponse { vnp_ResponseCode = "99", vnp_Message = $"Exception: {ex.Message}" };
			}
		}

		private string HmacSha512(string key, string inputData)
		{
			var hash = new StringBuilder();
			var keyBytes = Encoding.UTF8.GetBytes(key);
			var inputBytes = Encoding.UTF8.GetBytes(inputData);
			using (var hmac = new HMACSHA512(keyBytes))
			{
				var hashValue = hmac.ComputeHash(inputBytes);
				foreach (var theByte in hashValue)
				{
					hash.Append(theByte.ToString("x2"));
				}
			}
			return hash.ToString();
		}
	}
}