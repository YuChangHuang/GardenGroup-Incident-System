using Microsoft.AspNetCore.Mvc;
using GardenGroupIncidentSystem.Models;
using GardenGroupIncidentSystem.Services;
using System.Threading.Tasks;

namespace GardenGroupIncidentSystem.Controllers
{
// PasswordResetController - Forget Password feature by YuChangHuang.
    public class PasswordResetController : Controller
    {
        private readonly PasswordResetService _passwordResetService;

        public PasswordResetController(PasswordResetService passwordResetService)
        {
            _passwordResetService = passwordResetService;
        }


        // GET:
        // Displays the forgot password form
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        // POST:
        // Processes the forgot password request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _passwordResetService.InitiatePasswordResetAsync(model.Email);

                if (result.Success)
                {
                    TempData["Success"] = "If an account with that email exists, a password reset link has been sent to your email address.";
                    return RedirectToAction("ForgotPassword");
                }
                else
                {
                    string errorMessage = string.IsNullOrWhiteSpace(result.ErrorMessage)
                        ? "An error occurred while processing your request. Please try again later."
                        : result.ErrorMessage;

                    TempData["Error"] = errorMessage;
                    return View(model);
                }
            }
            catch
            {
                TempData["Error"] = "An error occurred while processing your request. Please try again later.";
                return View(model);
            }
        }


        // GET:
        // Displays the reset password form
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Invalid reset token.";
                return RedirectToAction("ForgotPassword");
            }

            // Validate token
            if (!_passwordResetService.ValidateResetToken(token))
            {
                TempData["Error"] = "Invalid or expired reset token. Please request a new password reset.";
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordModel
            {
                Token = token
            };

            return View(model);
        }

        //POST:
        //Processes the password reset
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                bool success = _passwordResetService.ResetPassword(model.Token, model.NewPassword);

                if (success)
                {
                    TempData["Success"] = "Your password has been reset successfully. You can now login with your new password.";
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    TempData["Error"] = "Invalid or expired reset token. Please request a new password reset.";
                    return RedirectToAction("ForgotPassword");
                }
            }
            catch
            {
                TempData["Error"] = "An error occurred while resetting your password. Please try again.";
                return View(model);
            }
        }
    }
}

