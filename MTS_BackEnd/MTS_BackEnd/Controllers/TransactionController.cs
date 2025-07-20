using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MTS.BLL;

namespace MTS.BackEnd.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TransactionController : ControllerBase
	{
		private readonly IServiceProviders _serviceProviders;

		public TransactionController(IServiceProviders serviceProviders)
		{
			_serviceProviders = serviceProviders;
		}

		[Authorize(Roles = "1")]
		[HttpGet("get-all")]
		public async Task<IActionResult> GetAllTransactions()
		{
			try
			{
				var transactions = await _serviceProviders.TransactionService.GetAllAsync();
				return Ok(transactions);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}
	}
}
