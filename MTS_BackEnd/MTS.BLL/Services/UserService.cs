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
	}
	public class UserService : IUserService
	{
		private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();
		private readonly IUnitOfWork _unitOfWork;
		private readonly JWTSettings _jwtSettings;
		private IGenericRepository<User> userRepo;
		public UserService()
		{

		}

		public UserService(IUnitOfWork unitOfWork, JWTSettings jwtSettings)
		{
			_unitOfWork = unitOfWork;
			_jwtSettings = jwtSettings;
			userRepo = _unitOfWork.GetRepository<User>();
		}

		public async Task<LoginResponseModelDto?> Login(LoginRequestModelDto loginRequest)
		{
			try
			{
				var account = await userRepo.GetByPropertyAsync(u => u.UserName == loginRequest.UserName && u.IsActive && u.EmailConfirmed);
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
				var userByUsername = await userRepo.GetByPropertyAsync(u => u.UserName == registerRequest.UserName);
				var userByEmail = await userRepo.GetByPropertyAsync(u => u.Email == registerRequest.Email);
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

				await userRepo.AddAsync(user);
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

		public async Task<bool> VerifyEmail(string email, string token)
		{
			try
			{
				var user = await userRepo.GetByPropertyAsync(u => u.Email == email && u.EmailVerificationToken == token && u.EmailVerificationTokenExpiry > DateTime.UtcNow);
				if (user == null) return false;
				user.EmailConfirmed = true;
				user.EmailVerificationToken = null;
				user.EmailVerificationTokenExpiry = null;
				user.IsActive = true;
				await userRepo.UpdateAsync(user);
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
