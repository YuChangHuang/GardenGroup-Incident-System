using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace GardenGroupIncidentSystem.Services
{
    // EmailService for Forget Password (created by YuChangHuang).
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpServer = _configuration["Email:SmtpServer"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["Email:SmtpUsername"] ?? "";
            _smtpPassword = _configuration["Email:SmtpPassword"] ?? "";
            _fromEmail = _configuration["Email:FromEmail"] ?? "";
            _fromName = _configuration["Email:FromName"] ?? "Garden Group Incident System";
        }

        // Checks if email is properly configured.
        public bool IsEmailConfigured()
        {
            return !string.IsNullOrWhiteSpace(_smtpUsername) &&
                   !string.IsNullOrWhiteSpace(_smtpPassword) &&
                   !string.IsNullOrWhiteSpace(_fromEmail);
        }

        // Sends a password reset email to the user.
        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string toName, string resetToken, string resetLink)
        {
            try
            {
                if (!IsEmailConfigured())
                    return false;

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_fromName, _fromEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = "Password Reset Request - Garden Group Incident System";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                                <h2 style='color: #667eea;'>Password Reset Request</h2>
                                <p>Hello {toName},</p>
                                <p>We received a request to reset your password for your Garden Group Incident System account.</p>
                                <p>Click the button below to reset your password:</p>
                                <div style='text-align: center; margin: 30px 0;'>
                                    <a href='{resetLink}' style='background-color: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>Reset Password</a>
                                </div>
                                <p>Or copy and paste this link into your browser:</p>
                                <p style='word-break: break-all; color: #667eea;'>{resetLink}</p>
                                <p>This link will expire in 1 hour for security reasons.</p>
                                <p>If you did not request a password reset, please ignore this email and your password will remain unchanged.</p>
                                <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'/>
                                <p style='color: #666; font-size: 12px;'>This is an automated message. Please do not reply to this email.</p>
                            </div>
                        </body>
                        </html>",
                    
                    TextBody = $@"
                            Password Reset Request

                            Hello {toName},

                            We received a request to reset your password for your Garden Group Incident System account.

                            Click the following link to reset your password:
                            {resetLink}

                            This link will expire in 1 hour for security reasons.

                            If you did not request a password reset, please ignore this email and your password will remain unchanged.

                            This is an automated message. Please do not reply to this email."
                };

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending password reset email: {ex.Message}");
                return false;
            }
        }
    }
}

