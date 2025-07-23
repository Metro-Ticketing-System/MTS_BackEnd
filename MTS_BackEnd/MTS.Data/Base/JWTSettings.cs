namespace MTS.Data.Base
{
	public class JWTSettings
	{
		public string? SecretKey { get; set; }

		public string? Issuer { get; set; } // thong tin nha phat hanh

		public string? Audience { get; set; } // doi tuong token cap

		public int AccessTokenExpirationDays { get; set; } // thoi gian ton tai cua access token

		public int RefreshTokenExpirationMonths { get; set; } // thoi gian ton tai cua refresh token
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

			if (AccessTokenExpirationDays <= 0)
			{
				throw new ArgumentException("AccessTokenExpirationMinutes must be greater than 0.");
			}

			if (RefreshTokenExpirationMonths <= 0)
			{
				throw new ArgumentException("RefreshTokenExpirationDays must be greater than 0.");
			}

			return true;
		}
	}
}
