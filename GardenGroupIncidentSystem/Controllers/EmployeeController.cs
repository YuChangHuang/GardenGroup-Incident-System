using Microsoft.AspNetCore.Mvc;
using GardenGroupIncidentSystem.Services;
using GardenGroupIncidentSystem.Models;
using System;
using System.Linq;

namespace GardenGroupIncidentSystem.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly EmployeeService _employeeService;

        public EmployeeController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public IActionResult Index()
        {
            try
            {
                var employees = _employeeService.GetAllEmployees();
                ViewBag.TotalCount = employees.Count;

                var regularCount = _employeeService.GetEmployeesByRole(Role.regular).Count;
                var serviceDeskCount = employees.Count(e =>
                    e.EmployeeRole == Role.servicedesk_1 ||
                    e.EmployeeRole == Role.servicedesk_2 ||
                    e.EmployeeRole == Role.servicedesk_3);

                ViewBag.RegularCount = regularCount;
                ViewBag.ServiceDeskCount = serviceDeskCount;

                return View(employees);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading employees: {ex.Message}";
                return View(new System.Collections.Generic.List<Employee>());
            }
        }

        //[HttpGet]
        //public IActionResult Login()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public IActionResult Login(LoginModel loginModel)
        //{
        //    try
        //    {
        //        Employee? employee = _employeeService.GetByLoginCredentials(loginModel.UserName, loginModel.Password);
        //        if (employee == null)
        //        {
        //            //bad login
        //            ViewBag.ErrorMessage = "Invalid username or password!";
        //            return View(loginModel);
        //        }
        //        else
        //        {
        //            //remember logged in user
        //            HttpContext.Session.SetObject("LoggedInUser", employee);
        //            return RedirectToAction("Index", "Home");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //handle exception
        //        TempData["ErrorMessage"] = ex.Message;
        //        return View(loginModel);
        //    }
        //}

        // Logout - Clears session and redirects to Login page
        //[HttpGet]
        //public IActionResult Logout()
        //{
        //    // Get username before clearing session
        //    var user = HttpContext.Session.GetObject<Employee>("LoggedInUser");
        //    var username = user?.Name?.FirstName ?? "User";

        //    // Clear the session (log out the user)
        //    HttpContext.Session.Clear();

        //    // Show success message on the LOGIN page
        //    TempData["Success"] = $"Goodbye, {username}! You have been logged out successfully.";

        //    // Redirect to LOGIN page (not Index)
        //    return RedirectToAction("Login", "Employee");
        //}

        [HttpPost]
        public IActionResult Create(string firstName, string lastName,
            string email, string phone, string location, string role, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
                    string.IsNullOrEmpty(password))
                {
                    TempData["Error"] = "First Name, Last Name, and Password are required!";
                    return RedirectToAction("Index");
                }

                // Parse role enum
                if (!Enum.TryParse<Role>(role, out Role employeeRole))
                {
                    TempData["Error"] = "Invalid role selected!";
                    return RedirectToAction("Index");
                }

                // Create employee with nested objects
                var employee = new Employee
                {
                    Password = password,
                    EmployeeRole = employeeRole,
                    Name = new Employee.Names(firstName, lastName),
                    ContactDetails = new Employee.ContactDetail(email, location, phone)
                };

                var created = _employeeService.CreateEmployee(employee);
                TempData["Success"] = $"Employee {created.Id} created successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to create employee: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        public IActionResult Details(string id)
        {
            try
            {
                var employee = _employeeService.GetEmployeeById(id);

                if (employee == null)
                {
                    TempData["Error"] = $"Employee {id} not found!";
                    return RedirectToAction("Index");
                }

                return View(employee);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading employee: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult Search(string searchTerm, string roleFilter)
        {
            try
            {
                var employees = _employeeService.GetAllEmployees();

                if (!string.IsNullOrEmpty(roleFilter) && roleFilter != "all")
                {
                    if (Enum.TryParse<Role>(roleFilter, out Role role))
                    {
                        employees = employees.Where(e => e.EmployeeRole == role).ToList();
                    }
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    employees = employees.Where(e =>
                        e.Id.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (e.Name != null && e.Name.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (e.Name != null && e.Name.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (e.ContactDetails != null && e.ContactDetails.EmailAddress != null &&
                         e.ContactDetails.EmailAddress.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                ViewBag.SearchTerm = searchTerm;
                ViewBag.RoleFilter = roleFilter;
                ViewBag.TotalCount = employees.Count;

                var regularCount = employees.Count(e => e.EmployeeRole == Role.regular);
                var serviceDeskCount = employees.Count(e =>
                    e.EmployeeRole == Role.servicedesk_1 ||
                    e.EmployeeRole == Role.servicedesk_2 ||
                    e.EmployeeRole == Role.servicedesk_3);

                ViewBag.RegularCount = regularCount;
                ViewBag.ServiceDeskCount = serviceDeskCount;

                return View("Index", employees);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Search failed: {ex.Message}";
                return View("Index", new System.Collections.Generic.List<Employee>());
            }
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var emp = _employeeService.GetEmployeeById(id);
            return View(emp);
        }

        [HttpPost]
        public IActionResult Edit(string id, string firstName, string lastName, string role, string? emailAddress, string? phoneNumber, string? location, string? newPassword)
        {
            _employeeService.UpdateEmployee(id, firstName, lastName, role, emailAddress, phoneNumber, location, newPassword);
            TempData["Success"] = "Employee updated.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(string id)
        {
            var emp = _employeeService.GetEmployeeById(id);
            return View(emp);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        public IActionResult DeleteConfirmed(string id)
        {
            _employeeService.DeleteEmployee(id);
            TempData["Success"] = "Employee deleted.";
            return RedirectToAction("Index");
        }


    }
}