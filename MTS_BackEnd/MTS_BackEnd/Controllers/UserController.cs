using Microsoft.AspNetCore.Mvc;
using MTS.BLL;
using MTS.DAL.Dtos;

namespace MTS.BackEnd.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly IServiceProviders _serviceProviders;
		public UserController(IServiceProviders serviceProviders)
		{
			_serviceProviders = serviceProviders;
		}

		//[HttpPost("Register")]
		//public async Task<IActionResult> Register([FromBody] LoginRequestModelDto request)
		//{
		//	try
		//	{
		//		var registerResponse = await _serviceProviders.UserService.Register(request);
		//		if (registerResponse == null)
		//		{
		//			return new NotFoundObjectResult("User already exists!");
		//		}
		//		return new OkObjectResult(registerResponse);
		//	}
		//	catch (Exception ex)
		//	{
		//		return new BadRequestObjectResult(ex.Message);
		//	}
		//}

		[HttpPost("Login")]
		public async Task<IActionResult> Login([FromBody] LoginRequestModelDto request)
		{
			try
			{
				var loginResponse = await _serviceProviders.UserService.Login(request);
				if (loginResponse == null)
				{
					return new NotFoundObjectResult("Invalid login information!");
				}
				return new OkObjectResult(loginResponse);


			}
			catch (Exception ex)
			{
				return new BadRequestObjectResult(ex.Message);
			}

		}
	}
}
