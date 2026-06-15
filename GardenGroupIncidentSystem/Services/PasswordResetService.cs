using System;
using System.Threading.Tasks;
using GardenGroupIncidentSystem.Models;
using GardenGroupIncidentSystem.Services.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace GardenGroupIncidentSystem.Services
{
    // PasswordResetService for Forget Password feature (by YuChangHuang).
    public class PasswordResetService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly EmailService _emailService;
        private readonly PasswordResetTokenService _tokenService;
        private readonly EmployeeService _employeeService;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PasswordResetService(
            IEmployeeRepository employeeRepository,
            EmailService emailService,
            PasswordResetTokenService tokenService,
            EmployeeService employeeService,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _employeeRepository = employeeRepository;
            _emailService = emailService;
            _tokenService = tokenService;
            _employeeService = employeeService;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        // Generates a token and sends reset email.
        public async Task<(bool Success, string ErrorMessage)> InitiatePasswordResetAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email address is required.");

            if (!_emailService.IsEmailConfigured())
                return (false, "Email service is not configured. Please contact administrator.");

            var employee = _employeeRepository.GetEmployeeByEmail(email);
            if (employee == null)
                return (true, "");

            string resetToken = _tokenService.GenerateResetToken();
            DateTime tokenExpiry = _tokenService.GetTokenExpiration();

            try
            {
                _employeeRepository.SavePasswordResetToken(employee.Id, resetToken, tokenExpiry);
            }
            catch (Exception)
            {
                return (false, "Could not save reset token. Please try again later.");
            }

            string resetLink = GenerateResetLink(resetToken);
            string recipientEmail = employee.ContactDetails?.EmailAddress ?? email;
            string userName = $"{employee.Name?.FirstName} {employee.Name?.LastName}".Trim();

            if (string.IsNullOrWhiteSpace(userName))
                userName = employee.Id;

            bool emailSent = await _emailService.SendPasswordResetEmailAsync(
                recipientEmail,
                userName,
                resetToken,
                resetLink);

            if (!emailSent)
                return (false, "Failed to send password reset email. Please check email settings.");

            return (true, "");
        }

        // Resets password when token is valid.
        public bool ResetPassword(string token, string newPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newPassword))
                    return false;

                // Find employee by token
                var employee = _employeeRepository.GetEmployeeByResetToken(token);
                if (employee == null)
                    return false;

                // Validate token
                if (!_tokenService.ValidateToken(token, employee.PasswordResetToken, employee.PasswordResetTokenExpiry))
                    return false;

                // Hash new password
                string hashedPassword = HashPassword(newPassword);

                // Update password and clear reset token
                _employeeRepository.UpdatePasswordAndClearToken(employee.Id, hashedPassword);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting password: {ex.Message}");
                return false;
            }
        }

        // Checks whether a reset token is still valid.
        public bool ValidateResetToken(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return false;

                var employee = _employeeRepository.GetEmployeeByResetToken(token);
                if (employee == null)
                    return false;

                return _tokenService.ValidateToken(token, employee.PasswordResetToken, employee.PasswordResetTokenExpiry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating token: {ex.Message}");
                return false;
            }
        }

        // Builds reset link URL for email.
        private string GenerateResetLink(string token)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
            {
                // Fallback if HttpContext is not available
                string baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:5001";
                return $"{baseUrl}/PasswordReset/ResetPassword?token={Uri.EscapeDataString(token)}";
            }

            string scheme = request.Scheme;
            string host = request.Host.Value;
            string path = "/PasswordReset/ResetPassword";
            string query = $"?token={Uri.EscapeDataString(token)}";

            return $"{scheme}://{host}{path}{query}";
        }

        // Hash password with SHA256.
        private string HashPassword(string password)
        {
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                System.Text.StringBuilder builder = new System.Text.StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}

