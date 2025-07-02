using Microsoft.AspNetCore.Identity.UI.Services;
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
		private readonly IEmailSender _emailSender;

		public UserController(IServiceProviders serviceProviders, IEmailSender emailSender)
		{
			_serviceProviders = serviceProviders;
			_emailSender = emailSender;
		}

		[HttpPost("Register")]
		public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
		{
			var registerResult = await _serviceProviders.UserService.Register(request);
			if (!registerResult.IsSuccess)
				return Conflict("User already exists!");

			var verificationLink = $"https://HCMCMTS.com/user/verify-email?token={registerResult.VerificationToken}&email={Uri.EscapeDataString(registerResult.Email!)}";

			await Task.Run(async () =>
			{
				await _emailSender.SendEmailAsync(registerResult.Email, "Email Verification",
					$"Please verify your email: <a href='{verificationLink}'>Verify</a>");
			});

			return Ok("Registration successful. Please check your email to verify your account.");
		}

		[HttpGet("VerifyEmail")]
		public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
		{
			var result = await _serviceProviders.UserService.VerifyEmail(email, token);
			if (result)
				return Ok("Email verified successfully.");

			return BadRequest("Invalid verification link or email already verified.");
		}

		[HttpPost("Login")]
		public async Task<IActionResult> Login([FromBody] LoginRequestModelDto request)
		{
			var loginResponse = await _serviceProviders.UserService.Login(request);
			if (loginResponse == null)
				return Unauthorized("Invalid login information!");

			return Ok(loginResponse);
		}
	}
}
