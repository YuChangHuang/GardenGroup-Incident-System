using GardenGroupIncidentSystem.Models;
using GardenGroupIncidentSystem.Services.Repositories;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace GardenGroupIncidentSystem.Services
{
    public class EmployeeService
    {
        private readonly IEmployeeRepository _repository;

        public EmployeeService(IEmployeeRepository repository)
        {
            _repository = repository;
        }

        public Employee CreateEmployee(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            if (string.IsNullOrEmpty(employee.Password))
                throw new ArgumentException("Password is required");

            employee.Password = HashPassword(employee.Password);
            return _repository.CreateEmployee(employee);
        }

        public void CreateEmployees(List<Employee> employees)
        {
            if (employees == null || employees.Count == 0)
                throw new ArgumentException("Employee list cannot be empty");

            foreach (var emp in employees)
            {
                if (!string.IsNullOrEmpty(emp.Password))
                {
                    emp.Password = HashPassword(emp.Password);
                }
            }

            _repository.CreateEmployees(employees);
        }

        public List<Employee> GetAllEmployees()
        {
            return _repository.GetAllEmployees();
        }

        public Employee GetEmployeeById(string employeeId)
        {
            return _repository.GetEmployeeById(employeeId);
        }

        public List<Employee> GetEmployeesByRole(Role role)
        {
            return _repository.GetEmployeesByRole(role);
        }

        public List<Employee> GetEmployeesByIds(List<string> employeeIds)
        {
            return _repository.GetEmployeesByIds(employeeIds);
        }

        public List<Employee> GetEmployeesByRoles(List<Role> roles)
        {
            return _repository.GetEmployeesByRoles(roles);
        }

        public List<Employee> GetEmployeesByLocationAndRole(string location, Role role)
        {
            return _repository.GetEmployeesByLocationAndRole(location, role);
        }

        public List<Employee> SearchEmployeesByName(string searchTerm)
        {
            return _repository.SearchEmployeesByName(searchTerm);
        }

        public Dictionary<string, int> GetEmployeeCountByRole()
        {
            return _repository.GetEmployeeCountByRole();
        }

        public Dictionary<string, int> GetEmployeeCountByLocation()
        {
            return _repository.GetEmployeeCountByLocation();
        }

        public Employee.EmployeeStatistics GetEmployeeStatistics()
        {
            return _repository.GetEmployeeStatistics();
        }

        public bool EmployeeExists(string employeeId)
        {
            return _repository.EmployeeExists(employeeId);
        }

        public long CountAllEmployees()
        {
            return _repository.CountAllEmployees();
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        public void UpdateEmployee(string employeeId, string firstName, string lastName, string role, string? email, string? phone, string? location, string? newPassword = null)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
                throw new ArgumentException("employeeId is required");

            string? hashed = null;
            if (!string.IsNullOrWhiteSpace(newPassword))
                hashed = HashPassword(newPassword);

            _repository.UpdateEmployee(employeeId, firstName, lastName, role, email, phone, location, hashed);
        }

        public void DeleteEmployee(string employeeId)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
                throw new ArgumentException("employeeId is required");
            _repository.DeleteEmployee(employeeId);
        }


        public Employee? GetByLoginCredentials(string userName, string password)
        {
            try
            {
                return _repository.GetByLoginCredentials(userName, HashPassword(password));
            }
            catch
            {
                throw new Exception("An error occurred while retrieving employee by login credentials.");
            }
        }
    }
}