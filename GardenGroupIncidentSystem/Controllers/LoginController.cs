using GardenGroupIncidentSystem.Models;
using GardenGroupIncidentSystem.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GardenGroupIncidentSystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly AuthenticationService _authService;

        public LoginController(AuthenticationService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginModel loginModel)
        {
            try
            {
                var employee = _authService.Authenticate(loginModel.UserName, loginModel.Password);

                if (employee == null)
                {
                    ViewBag.ErrorMessage = "Invalid username or password!";
                    return View("Index", loginModel);
                }

                HttpContext.Session.SetObject("LoggedInUser", employee);

                // Get redirection controller based on role
                string redirectController = _authService.GetPostLoginRedirectController(employee.EmployeeRole);

                return RedirectToAction("Index", redirectController);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Index", loginModel);
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            var user = HttpContext.Session.GetObject<Employee>("LoggedInUser");
            var username = user?.Name?.FirstName ?? "User";

            HttpContext.Session.Remove("LoggedInUser");

            TempData["Success"] = $"Goodbye, {username}! You have been logged out successfully.";

            return RedirectToAction("Index", "Login");
        }
    }
}
