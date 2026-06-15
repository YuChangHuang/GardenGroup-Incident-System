using GardenGroupIncidentSystem.Models;
using System;
using System.Collections.Generic;

namespace GardenGroupIncidentSystem.Services.Repositories
{
    public interface IEmployeeRepository
    {
        string GetNextEmployeeId();

        Employee CreateEmployee(Employee employee);
        void CreateEmployees(List<Employee> employees);

        List<Employee> GetAllEmployees();
        Employee GetEmployeeById(string employeeId);
        List<Employee> GetEmployeesByRole(Role role);
        List<Employee> GetEmployeesByIds(List<string> employeeIds);
        List<Employee> GetEmployeesByRoles(List<Role> roles);
        List<Employee> GetEmployeesByLocationAndRole(string location, Role role);
        List<Employee> SearchEmployeesByName(string searchTerm);

        bool EmployeeExists(string employeeId);
        long CountAllEmployees();

        Dictionary<string, int> GetEmployeeCountByRole();
        Dictionary<string, int> GetEmployeeCountByLocation();
        Employee.EmployeeStatistics GetEmployeeStatistics();

        void UpdateEmployee(string employeeId, string firstName, string lastName, string role, string? email, string? phone, string? location, string? hashedPassword = null);

        void DeleteEmployee(string employeeId);

        Employee? GetByLoginCredentials(string userName, string password);

        Employee? GetEmployeeByEmail(string email);

        Employee? GetEmployeeByResetToken(string token);

        void SavePasswordResetToken(string employeeId, string token, DateTime tokenExpiry);

        void UpdatePasswordAndClearToken(string employeeId, string hashedPassword);
    }
}