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
	}
	public class UserService : IUserService
	{
		private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();
		private readonly IUnitOfWork _unitOfWork;
		private readonly JWTSettings _jwtSettings;

		public UserService()
		{

		}

		public UserService(IUnitOfWork unitOfWork, JWTSettings jwtSettings)
		{
			_unitOfWork = unitOfWork;
			_jwtSettings = jwtSettings;
		}

		public async Task<LoginResponseModelDto?> Login(LoginRequestModelDto loginRequest)
		{
			try
			{
				var account = await _unitOfWork.GetRepository<User>().GetByPropertyAsync(u => u.UserName == loginRequest.UserName);
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

		private bool VerifyPassword(User user, string password)
		{
			return _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Success;
		}
	}
}
