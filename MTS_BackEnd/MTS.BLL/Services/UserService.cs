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
        Task<int> CreateStaffAccount(StaffAccountDto staffAccountDto);
        Task<UserProfileDto?> GetUserProfile(Guid userId);
        Task<bool> UpdateProfile(UserProfileDto userProfileDto);
        Task<bool> SetUserAccountIsActiveStatus(Guid userId, bool result);
        Task<List<UserAccountDto>> GetAllUsers();
        Task<UserAccountDto?> GetUserAsync(Guid id);
        public Task<RegisterResultDto> ResendVerificationEmailAsync(string email);

        public Task<bool> SavePushToken(PushTokenDto pushTokenDto);
    }
    public class UserService : IUserService
    {
        private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();
        private readonly IUnitOfWork _unitOfWork;
        private readonly JWTSettings _jwtSettings;
        private readonly IWalletService _walletService;
        private readonly IEmailValidationService _emailValidationService;
        private IGenericRepository<User> _userRepo;
        
        public UserService(IUnitOfWork unitOfWork, JWTSettings jwtSettings, IWalletService walletService, IEmailValidationService emailValidationService)
        {
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtSettings;
            _walletService = walletService;
            _emailValidationService = emailValidationService;
            _userRepo = _unitOfWork.GetRepository<User>();
        }

        public async Task<int> CreateStaffAccount(StaffAccountDto staffAccountDto)
        {
            try
            {
                var existingUser = await _userRepo.GetByPropertyAsync(u => u.UserName == staffAccountDto.UserName || u.Email == staffAccountDto.Email);
                if (existingUser != null) return -1;

                var newAccount = new User
                {
                    UserName = staffAccountDto.UserName,
                    FirstName = staffAccountDto.FirstName,
                    LastName = staffAccountDto.LastName,
                    NormalizedUserName = staffAccountDto.UserName.ToUpperInvariant(),
                    Email = staffAccountDto.Email,
                    NormalizedEmail = staffAccountDto.Email.ToUpperInvariant(),
                    PasswordHash = _passwordHasher.HashPassword(new User(), staffAccountDto.Password),
                    RoleId = 2,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepo.AddAsync(newAccount);
                var result = await _unitOfWork.SaveAsync();
                if (result > 0) return 1;
                return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating staff account: {ex.Message}");
                return -1;
            }
        }

        public async Task<UserProfileDto?> GetUserProfile(Guid userId)
        {
            try
            {
                var account = await _userRepo.GetByPropertyAsync(u => u.Id == userId && u.IsActive == true);
                if (account == null) return null;
                return new UserProfileDto
                {
                    Id = account.Id,
                    UserName = account.UserName,
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                    Email = account.Email,
                    DateOfBirth = account.DateOfBirth,
                    IsStudent = account.IsStudent,
                    IsRevolutionaryContributor = account.IsRevolutionaryContributor
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user profile: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> SetUserAccountIsActiveStatus(Guid userId, bool result)
        {
            try
            {
                var account = await _userRepo.GetByPropertyAsync(u => u.Id == userId);
                if (account == null) return false;
                account.IsActive = result;
                if (result == false)
                {
                    account.DeletedAt = DateTime.UtcNow;
				}
                account.DeletedAt = null; // Reset DeletedAt if activating the account
                account.UpdatedAt = DateTime.UtcNow;
                await _userRepo.UpdateAsync(account);
                var finalResult = await _unitOfWork.SaveAsync();
                if (finalResult > 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inactivating account: {ex.Message}");
                return false;
            }
        }

        public async Task<LoginResponseModelDto?> Login(LoginRequestModelDto loginRequest)
        {
            try
            {
                var account = await _userRepo.GetByPropertyAsync(u => u.UserName == loginRequest.UserName && u.IsActive);
                if (account == null) return null;
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
                var isEmailValid = await _emailValidationService.IsEmailValidAsync(registerRequest.Email);
                if (!isEmailValid)
                {
                    return new RegisterResultDto { IsSuccess = false }; // Consider adding a specific error message
                }

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
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    EmailConfirmed = true,
                    //EmailVerificationToken = verificationToken,
                    //EmailVerificationTokenExpiry = DateTime.UtcNow.AddMinutes(5),
                };

                await _userRepo.AddAsync(user);
                await _walletService.CreateWalletAsync(user.Id);
                var succeedCount = await _unitOfWork.SaveAsync();
                if (succeedCount > 1)
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
                user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(5);
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
                var user = await _userRepo.GetByPropertyAsync(u => u.PasswordResetToken == token && u.PasswordResetTokenExpiry > DateTime.UtcNow);
                if (user == null) return false;

                user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
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
                Console.WriteLine($"Error during password reset: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateProfile(UserProfileDto userProfileDto)
        {
            try
            {
                var user = await _userRepo.GetByPropertyAsync(u => u.Id == userProfileDto.Id && u.IsActive);
                if (user == null) return false;
                user.FirstName = userProfileDto.FirstName;
                user.LastName = userProfileDto.LastName;
                user.DateOfBirth = userProfileDto.DateOfBirth;
                user.UpdatedAt = DateTime.UtcNow;
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
                Console.WriteLine($"Error updating profile: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> VerifyEmail(string email, string token)
        {
            try
            {
                var user = await _userRepo.GetByPropertyAsync(u => u.Email == email && u.EmailVerificationToken == token && u.EmailVerificationTokenExpiry > DateTime.UtcNow);
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

        public async Task<List<UserAccountDto>> GetAllUsers()
        {
            try
            {
                var users = await _userRepo.GetAllByPropertyAsync();
                if (users == null || !users.Any()) return new List<UserAccountDto>();
                return users.Select(UserAccountDto.FromModelToDto).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving all users: {ex.Message}");
                return new List<UserAccountDto>();
            }
        }

        public async Task<UserAccountDto?> GetUserAsync(Guid id)
        {
            try
            {
                var user = await _userRepo.GetByPropertyAsync(u => u.Id == id && u.IsActive == true);
                if (user == null) return null;
                return UserAccountDto.FromModelToDto(user);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user: {ex.Message}");
                return null;
            }
        }

        public async Task<RegisterResultDto> ResendVerificationEmailAsync(string email)
        {
            try
            {
                var user = await _userRepo.GetByPropertyAsync(u => u.Email == email);
                if (user == null || user.EmailConfirmed)
                {
                    // User not found or email already confirmed
                    return new RegisterResultDto { IsSuccess = false };
                }

                var verificationToken = Guid.NewGuid().ToString();
                user.EmailVerificationToken = verificationToken;
                user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddMinutes(5);

                await _userRepo.UpdateAsync(user);
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
                Console.WriteLine($"Error during resending verification email: {ex.Message}");
                return new RegisterResultDto { IsSuccess = false };
            }
        }

        public async Task<bool> SavePushToken(PushTokenDto pushTokenDto)
        {
            try
            {
                var user = await _userRepo.GetByPropertyAsync(u => u.Id == pushTokenDto.UserId);
                if (user == null) return false;

                user.ExpoPushToken = pushTokenDto.ExpoPushToken;
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
                Console.WriteLine($"Error during login: {ex.Message}");
                return false;
            }
        }
    }
}
