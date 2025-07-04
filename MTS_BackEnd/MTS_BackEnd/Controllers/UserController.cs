using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using MTS.BLL;
using MTS.DAL.Dtos;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

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
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

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

		[HttpPost("VerifyEmail")]
		public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var result = await _serviceProviders.UserService.VerifyEmail(email, token);
			if (result)
				return Ok("Email verified successfully.");

			return BadRequest("Invalid verification link or email already verified.");
		}

		[HttpPost("Login")]
		public async Task<IActionResult> Login([FromBody] LoginRequestModelDto request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var loginResponse = await _serviceProviders.UserService.Login(request);
			if (loginResponse == null)
				return Unauthorized("Invalid login information!");

			return Ok(loginResponse);
		}

		[HttpPost("ForgotPassword")]
		public async Task<IActionResult> ForgotPassword([FromQuery][Required] string email)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			if (string.IsNullOrEmpty(email)) return BadRequest("Email must not be empty!");
			var result = await _serviceProviders.UserService.RequestPasswordReset(email);
			if (!result.IsSucceed) return NotFound("Email is not registered!");

			var resetLink = $"https://HCMCMTS.com/user/reset-password?token={result.PasswordResetToken}&email={Uri.EscapeDataString(email)}";
			await Task.Run(async () =>
			{
				await _emailSender.SendEmailAsync(email, "Password Reset Request",
					$"Please click to reset your password: <a href='{resetLink}'>Reset Password</a>");
			});
			return Ok("Password reset link has been sent to your email.");
		}

		[HttpPost("ResetPassword")]
		public async Task<IActionResult> ResetPassword([FromQuery][Required] string token, [FromBody] PasswordResetRequestDto dto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			if (string.IsNullOrEmpty(token)) return BadRequest("Token is required!");
			if (string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.ConfirmPassword)) return BadRequest("Password & cofirmPassword is required!");
			if (!dto.Password.Equals(dto.ConfirmPassword)) return BadRequest("Password do not match!");
			var result = await _serviceProviders.UserService.ResetPassword(token, dto.Password);
			if (!result) return BadRequest("Failed to reset password!");
			return Ok("Password has been reset successfully! You can now log in with your new password.");
		}

		[Authorize(Roles = "1")]
		[HttpPost("CreateStaffAccount")]
		public async Task<IActionResult> CreateStaffAccount([FromBody] StaffAccountDto staffAccountDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			if (staffAccountDto == null) return BadRequest("Staff account data is required!");
			var result = await _serviceProviders.UserService.CreateStaffAccount(staffAccountDto);
			if (result < 0) return Conflict("User already exists!");
			return Ok("Staff account created successfully!");
		}

		[Authorize]
		[HttpPost("GetUserProfile")]
		public async Task<IActionResult> GetUserProfile()
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var userId = User.FindFirstValue("id");
			if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authenticated!");
			var userProfile = await _serviceProviders.UserService.GetUserProfile(Guid.Parse(userId));
			if (userProfile == null) return NotFound("User profile not found!");
			return Ok(userProfile);
		}
	}
}
