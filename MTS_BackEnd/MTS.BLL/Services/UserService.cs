using Microsoft.AspNetCore.Identity;
using MTS.DAL.Dtos;
using MTS.DAL.Libraries;
using MTS.DAL.Repositories;
using MTS.Data.Base;
using MTS.Data.Models;

namespace MTS.BLL.Services
{
	public interface IUserService
	{
		Task<LoginResponseModelDto?> Login(LoginRequestModelDto loginRequest);
		Task<RegisterResultDto> Register(RegisterRequestDto registerRequest);
		Task<bool> VerifyEmail(string email, string token);
		Task<PasswordResetRequestResultDto> RequestPasswordReset(string email);
		Task<bool> ResetPassword(string token, string newPassword);
	}
	public class UserService : IUserService
	{
		private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();
		private readonly IUnitOfWork _unitOfWork;
		private readonly JWTSettings _jwtSettings;
		private IGenericRepository<User> _userRepo;
		public UserService()
		{

		}

		public UserService(IUnitOfWork unitOfWork, JWTSettings jwtSettings)
		{
			_unitOfWork = unitOfWork;
			_jwtSettings = jwtSettings;
			_userRepo = _unitOfWork.GetRepository<User>();
		}

		public async Task<LoginResponseModelDto?> Login(LoginRequestModelDto loginRequest)
		{
			try
			{
				var account = await _userRepo.GetByPropertyAsync(u => u.UserName == loginRequest.UserName && u.IsActive && u.EmailConfirmed);
				var checkPassword = VerifyPassword(account, loginRequest.Password);
				if (account == null || !checkPassword) return null;
				LoginResponseModelDto token = await Authentication.CreateToken(account!, account.RoleId!, _jwtSettings);
				return token;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error during login: {ex.Message}");
				return null;
			}
		}

		public async Task<RegisterResultDto> Register(RegisterRequestDto registerRequest)
		{
			try
			{
				var userByUsername = await _userRepo.GetByPropertyAsync(u => u.UserName == registerRequest.UserName);
				var userByEmail = await _userRepo.GetByPropertyAsync(u => u.Email == registerRequest.Email);
				if (userByUsername != null || userByEmail != null)
				{
					return new RegisterResultDto { IsSuccess = false };
				}

				var passwordHash = _passwordHasher.HashPassword(new User(), registerRequest.Password);
				var verificationToken = Guid.NewGuid().ToString();
				var user = new User
				{
					UserName = registerRequest.UserName,
					FirstName = registerRequest.FirstName,
					LastName = registerRequest.LastName,
					NormalizedUserName = registerRequest.UserName.ToUpperInvariant(),
					Email = registerRequest.Email,
					NormalizedEmail = registerRequest.Email.ToUpperInvariant(),
					PasswordHash = passwordHash,
					RoleId = 3,
					CreatedAt = DateTime.Now,
					IsActive = false,
					EmailConfirmed = false,
					EmailVerificationToken = verificationToken,
					EmailVerificationTokenExpiry = DateTime.Now.AddMinutes(5),
				};

				await _userRepo.AddAsync(user);
				var succeedCount = await _unitOfWork.SaveAsync();
				if (succeedCount > 0)
				{
					return new RegisterResultDto
					{
						IsSuccess = true,
						Email = user.Email,
						VerificationToken = verificationToken
					};
				}
				return new RegisterResultDto { IsSuccess = false };
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error during registration: {ex.Message}");
				throw;
			}
		}

		public async Task<PasswordResetRequestResultDto> RequestPasswordReset(string email)
		{
			try
			{
				var user = await _userRepo.GetByPropertyAsync(u => u.Email == email && u.IsActive && u.EmailConfirmed);

				if (user == null) return new PasswordResetRequestResultDto { IsSucceed = false };

				user.PasswordResetToken = Guid.NewGuid().ToString();
				user.PasswordResetTokenExpiry = DateTime.Now.AddMinutes(5);
				await _userRepo.UpdateAsync(user);
				var result = await _unitOfWork.SaveAsync();
				if (result > 0) return new PasswordResetRequestResultDto
				{
					IsSucceed = true,
					Email = user.Email,
					PasswordResetToken = user.PasswordResetToken,
				};
				return new PasswordResetRequestResultDto { IsSucceed = false };
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error during password reset request: {ex.Message}");
				return new PasswordResetRequestResultDto { IsSucceed = false };
			}
		}

		public async Task<bool> ResetPassword(string token, string newPassword)
		{
			try
			{
				var user = await _userRepo.GetByPropertyAsync(u => u.PasswordResetToken == token && u.PasswordResetTokenExpiry > DateTime.Now);
				if (user == null) return false;

				user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
				user.PasswordResetToken = null;
				user.PasswordResetTokenExpiry = null;
				await _userRepo.UpdateAsync(user);
				var result = await _unitOfWork.SaveAsync();
				if (result > 0)
				{
					await _unitOfWork.SaveAsync();
					return true;
				}
				return false;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error during password reset: {ex.Message}");
				return false;
			}
		}

		public async Task<bool> VerifyEmail(string email, string token)
		{
			try
			{
				var user = await _userRepo.GetByPropertyAsync(u => u.Email == email && u.EmailVerificationToken == token && u.EmailVerificationTokenExpiry > DateTime.Now);
				if (user == null) return false;
				user.EmailConfirmed = true;
				user.EmailVerificationToken = null;
				user.EmailVerificationTokenExpiry = null;
				user.IsActive = true;
				await _userRepo.UpdateAsync(user);
				var result = await _unitOfWork.SaveAsync();
				if (result > 0)
				{
					return true;
				}
				return false;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error during email verification: {ex.Message}");
				return false;
			}
		}

		private bool VerifyPassword(User user, string password)
		{
			return _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Success;
		}
	}
}
