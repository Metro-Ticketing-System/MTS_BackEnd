namespace MTS.Data.Base
{
	public class JWTSettings
	{
		public string? SecretKey { get; set; }

		public string? Issuer { get; set; } // thong tin nha phat hanh

		public string? Audience { get; set; } // doi tuong token cap

		public int AccessTokenExpirationMinutes { get; set; } // thoi gian ton tai cua access token

		public int RefreshTokenExpirationDays { get; set; } // thoi gian ton tai cua refresh token
		public bool IsValid()
		{
			if (string.IsNullOrEmpty(SecretKey))
			{
				throw new ArgumentException("SecretKey cannot be null or empty.");
			}

			if (string.IsNullOrEmpty(Issuer))
			{
				throw new ArgumentException("Issuer cannot be null or empty.");
			}

			if (string.IsNullOrEmpty(Audience))
			{
				throw new ArgumentException("Audience cannot be null or empty.");
			}

			if (AccessTokenExpirationMinutes <= 0)
			{
				throw new ArgumentException("AccessTokenExpirationMinutes must be greater than 0.");
			}

			if (RefreshTokenExpirationDays <= 0)
			{
				throw new ArgumentException("RefreshTokenExpirationDays must be greater than 0.");
			}

			return true;
		}
	}
}
