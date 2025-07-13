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

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var registerResult = await _serviceProviders.UserService.Register(request);
			if (!registerResult.IsSuccess)
				return Conflict("User already exists or email not exist!");

			var verificationLink = $"https://HCMCMTS.com/user/verify-email?token={registerResult.VerificationToken}&email={Uri.EscapeDataString(registerResult.Email!)}";

			await Task.Run(async () =>
			{
				await _emailSender.SendEmailAsync(registerResult.Email, "Email Verification",
					$"Please verify your email: <a href='{verificationLink}'>Verify</a>");
			});

			return Ok("Registration successful. Please check your email to verify your account.");
		}

		[HttpPost("verify-email")]
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

		[HttpPost("resend-verification-email")]
		public async Task<IActionResult> ResendVerificationEmail([FromQuery][Required] string email)
		{
			if (string.IsNullOrEmpty(email))
			{
				return BadRequest("Email is required.");
			}

			var resendResult = await _serviceProviders.UserService.ResendVerificationEmailAsync(email);
			if (!resendResult.IsSuccess)
			{
				return BadRequest("Could not resend verification email. The email may not be registered or is already verified.");
			}

			var verificationLink = $"https://HCMCMTS.com/user/verify-email?token={resendResult.VerificationToken}&email={Uri.EscapeDataString(resendResult.Email!)}";

			await Task.Run(async () =>
			{
				await _emailSender.SendEmailAsync(resendResult.Email, "Email Verification",
					$"Please verify your email: <a href='{verificationLink}'>Verify</a>");
			});

			return Ok("Verification email sent successfully. Please check your email.");
		}

		[HttpPost("login")]
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

		[HttpPost("forgot-password")]
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

		[HttpPost("resend-password-reset-email")]
		public async Task<IActionResult> ResendPasswordResetEmail([FromQuery][Required] string email)
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

		[HttpPost("reset-password")]
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
		[HttpPost("create-staff-account")]
		public async Task<IActionResult> CreateStaffAccount([FromBody] StaffAccountDto staffAccountDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			if (staffAccountDto == null) return BadRequest("Staff account data is required!");
			var result = await _serviceProviders.UserService.CreateStaffAccount(staffAccountDto);
			if (result < 0) return Conflict("User already exists!");
			return CreatedAtAction(nameof(result), null, "Staff account created successfully!");
		}

		[Authorize]
		[HttpPost("get-user-profile")]
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

		[Authorize]
		[HttpPut("update-profile")]
		public async Task<IActionResult> UpdateProfile([FromBody] UserProfileDto userProfileDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			if (userProfileDto == null) return BadRequest("User profile data is required!");
			var userId = User.FindFirstValue("id");
			if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authenticated!");
			userProfileDto.Id = Guid.Parse(userId);
			var result = await _serviceProviders.UserService.UpdateProfile(userProfileDto);
			if (!result) return BadRequest("Failed to update profile!");
			return NoContent();
		}

		[Authorize(Roles = "1")]
		[HttpPatch("set-user-account-isActive-status")]
		public async Task<IActionResult> SetUserAccountIsActiveStatus([FromQuery] Guid userId, [FromQuery] bool result)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			if (userId == Guid.Empty) return BadRequest("User ID is required!");
			var isSuccess = await _serviceProviders.UserService.SetUserAccountIsActiveStatus(userId, result);
			if (!isSuccess) return NotFound("User not found or update failed!");
			return NoContent();
		}

		[Authorize(Roles = "1")]
		[HttpGet("get-all-user-accounts")]
		public async Task<IActionResult> GetAllUserAccounts()
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var userAccounts = await _serviceProviders.UserService.GetAllUsers();
			if (userAccounts == null || !userAccounts.Any()) return NotFound("No user accounts found!");
			return Ok(userAccounts);
		}

        [HttpPost("SavePushToken")]
        public async Task<IActionResult> SavePushToken([FromBody] PushTokenDto pushTokenDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _serviceProviders.UserService.SavePushToken(pushTokenDto);

            if (!success)
            {
                return BadRequest(new { Message = "Failed to save push token." });
            }

            return Ok(new { Message = "Push token saved successfully." });
        }
    }
}
