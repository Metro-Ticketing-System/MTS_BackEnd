using Microsoft.IdentityModel.Tokens;
using MTS.DAL.Dtos;
using MTS.Data.Base;
using MTS.Data.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MTS.DAL.Libraries
{
	public static class Authentication
	{
		public static async Task<LoginResponseModelDto> CreateToken(User user, int? role, JWTSettings jwtSettings, bool isRefresh = false)
		{
			// Tạo ra các claims
			DateTime now = DateTime.Now;

			// Danh sách các claims chung cho cả Access Token và Refresh Token
			List<Claim> claims = new List<Claim>
				{
					new Claim("id", user!.Id.ToString()),
					new Claim("role", role.ToString()),
					new Claim("email",user.Email),
				};

			// đăng kí khóa bảo mật
			SymmetricSecurityKey? key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey ?? string.Empty));
			SigningCredentials? creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

			// Generate access token
			DateTime dateTimeAccessExpr = now.AddMinutes(jwtSettings.AccessTokenExpirationMinutes);
			claims.Add(new Claim("token_type", "access"));
			JwtSecurityToken accessToken = new JwtSecurityToken(
				claims: claims,
				issuer: jwtSettings.Issuer,
				audience: jwtSettings.Audience,
				expires: dateTimeAccessExpr,
				signingCredentials: creds
			);

			string refreshTokenString = string.Empty;
			string accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);

			if (isRefresh == false)
			{
				// tạo ra refresh Token
				DateTime datetimeRefrestExpr = now.AddDays(jwtSettings.RefreshTokenExpirationDays);

				claims.Remove(claims.First(c => c.Type == "token_type"));
				claims.Add(new Claim("token_type", "refresh"));

				JwtSecurityToken? refreshToken = new JwtSecurityToken(
					claims: claims,
					issuer: jwtSettings.Issuer,
					audience: jwtSettings.Audience,
					expires: datetimeRefrestExpr,
					signingCredentials: creds
				);

				refreshTokenString = new JwtSecurityTokenHandler().WriteToken(refreshToken);
			}

			return new LoginResponseModelDto
			{
				Id = user.Id,
				Role = user.RoleId,
				Username = user.UserName,
				Token = accessTokenString,
			};
		}
		public static Guid GetUserId(this ClaimsPrincipal user)
		{
			var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == "id");
			return Guid.TryParse(userIdClaim?.Value, out Guid userId) ? userId : Guid.Empty;
		}
		public static string GetUserEmail(this ClaimsPrincipal user)
		{
			return user.Claims.FirstOrDefault(x =>
				x.Type.Equals(ClaimTypes.Email) ||
				x.Type.Equals("email") ||
				x.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"))
				?.Value ?? string.Empty;
		}
	}
}
