using System;
using System.Security.Cryptography;
using System.Text;

namespace GardenGroupIncidentSystem.Services
{
    // PasswordResetTokenService for Forget Password (YuChangHuang).
    public class PasswordResetTokenService
    {
        private const int TokenExpirationHours = 1; // Token expires in 1 hour

        // Generates a secure random token for password reset.
        public string GenerateResetToken()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[32];
                rng.GetBytes(bytes);
                var base64 = Convert.ToBase64String(bytes)
                    .Replace("+", "-")
                    .Replace("/", "_")
                    .Replace("=", "");
                // Return a URL-safe token (at least 32 characters)
                return base64.Length > 32 ? base64.Substring(0, 32) : base64;
            }
        }

        // Returns when the token will expire.
        public DateTime GetTokenExpiration()
        {
            return DateTime.UtcNow.AddHours(TokenExpirationHours);
        }

        // Checks whether a token expired.
        public bool IsTokenValid(DateTime? tokenExpiry)
        {
            if (tokenExpiry == null)
                return false;

            return tokenExpiry.Value > DateTime.UtcNow;
        }

        // Confirms provided token matches stored token and is not expired.
        public bool ValidateToken(string providedToken, string storedToken, DateTime? tokenExpiry)
        {
            if (string.IsNullOrEmpty(providedToken) || string.IsNullOrEmpty(storedToken))
                return false;

            if (!IsTokenValid(tokenExpiry))
                return false;

            // Constant-time comparison to prevent timing attacks.
            return SecureCompare(providedToken, storedToken);
        }

        // Performs constant-time string comparison to prevent timing attacks.
        private bool SecureCompare(string a, string b)
        {
            if (a.Length != b.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }

            return result == 0;
        }
    }
}

