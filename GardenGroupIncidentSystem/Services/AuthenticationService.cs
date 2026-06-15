using GardenGroupIncidentSystem.Models;
using Microsoft.AspNetCore.Http;

namespace GardenGroupIncidentSystem.Services
{
    public class AuthenticationService
    {
        private readonly EmployeeService _employeeService;

        public AuthenticationService(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public Employee? Authenticate(string userName, string password)
        {
            return _employeeService.GetByLoginCredentials(userName, password);
        }

        // Get current logged-in user from session
        public Employee? GetLoggedInUser(HttpContext context)
        {
            return context.Session.GetObject<Employee>("LoggedInUser");
        }

        // Check if user is authenticated
        public bool IsAuthenticated(HttpContext context)
        {
            return GetLoggedInUser(context) != null;
        }

        // Check if logged-in user is a regular employee
        public bool IsRegularUser(HttpContext context)
        {
            var user = GetLoggedInUser(context);
            return user != null && user.EmployeeRole == Role.regular;
        }
        // Determine post-login redirect controller based on role
        public string GetPostLoginRedirectController(Role role)
        {
            return role == Role.regular ? "Dashboard" : "Home";
        }

    }
}
