using MongoDB.Driver;
using GardenGroupIncidentSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GardenGroupIncidentSystem.Services.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IMongoCollection<Employee> _employees;

        public EmployeeRepository(IMongoDatabase db)
        {
            _employees = db.GetCollection<Employee>("Employee");
        }

        public string GetNextEmployeeId()
        {
            var allEmployees = _employees.Find(_ => true).ToList();
            int maxNumber = 0;

            foreach (var emp in allEmployees)
            {
                if (emp.Id.StartsWith("E") && int.TryParse(emp.Id.Substring(1), out int num))
                {
                    if (num > maxNumber)
                        maxNumber = num;
                }
            }

            return $"E{(maxNumber + 1):D4}";
        }

        public Employee CreateEmployee(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            if (string.IsNullOrEmpty(employee.Id))
            {
                employee.Id = GetNextEmployeeId();
            }
            else
            {
                if (EmployeeExists(employee.Id))
                    throw new InvalidOperationException($"Employee {employee.Id} already exists");
            }

            _employees.InsertOne(employee);
            return employee;
        }

        public void CreateEmployees(List<Employee> employees)
        {
            if (employees == null || employees.Count == 0)
                throw new ArgumentException("Employee list cannot be empty");

            foreach (var emp in employees)
            {
                if (string.IsNullOrEmpty(emp.Id))
                {
                    emp.Id = GetNextEmployeeId();
                }
            }

            _employees.InsertMany(employees);
        }

        public List<Employee> GetAllEmployees()
        {
            return _employees.Find(emp => true).ToList();
        }

        public Employee GetEmployeeById(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
                return null;

            return _employees.Find(emp => emp.Id == employeeId).FirstOrDefault();
        }

        public List<Employee> GetEmployeesByRole(Role role)
        {
            return _employees.Find(emp => emp.EmployeeRole == role).ToList();
        }

        public List<Employee> GetEmployeesByIds(List<string> employeeIds)
        {
            if (employeeIds == null || employeeIds.Count == 0)
                return new List<Employee>();

            var filter = Builders<Employee>.Filter.In(emp => emp.Id, employeeIds);
            return _employees.Find(filter).ToList();
        }

        public List<Employee> GetEmployeesByRoles(List<Role> roles)
        {
            if (roles == null || roles.Count == 0)
                return new List<Employee>();

            var filter = Builders<Employee>.Filter.In(emp => emp.EmployeeRole, roles);
            return _employees.Find(filter).ToList();
        }

        public List<Employee> GetEmployeesByLocationAndRole(string location, Role role)
        {
            var filterBuilder = Builders<Employee>.Filter;
            var filters = new List<FilterDefinition<Employee>>();

            if (!string.IsNullOrEmpty(location))
                filters.Add(filterBuilder.Eq("ContactDetails.Location", location));

            filters.Add(filterBuilder.Eq(emp => emp.EmployeeRole, role));

            if (filters.Count == 0)
                return new List<Employee>();

            var combinedFilter = filterBuilder.And(filters);
            return _employees.Find(combinedFilter).ToList();
        }

        public List<Employee> SearchEmployeesByName(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return new List<Employee>();

            var filter = Builders<Employee>.Filter.Or(
                Builders<Employee>.Filter.Regex("Name.FirstName",
                    new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Employee>.Filter.Regex("Name.LastName",
                    new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );

            return _employees.Find(filter).ToList();
        }

        public Dictionary<string, int> GetEmployeeCountByRole()
        {
            var pipeline = _employees.Aggregate()
                .Group(
                    emp => emp.EmployeeRole,
                    group => new
                    {
                        Role = group.Key,
                        Count = group.Count()
                    }
                )
                .ToList();

            return pipeline.ToDictionary(x => x.Role.ToString(), x => x.Count);
        }

        public Dictionary<string, int> GetEmployeeCountByLocation()
        {
            var pipeline = _employees.Aggregate()
                .Group(
                    emp => emp.ContactDetails.Location,
                    group => new
                    {
                        Location = group.Key,
                        Count = group.Count()
                    }
                )
                .ToList();

            return pipeline.ToDictionary(x => x.Location ?? "Unknown", x => x.Count);
        }

        public Employee.EmployeeStatistics GetEmployeeStatistics()
        {
            var total = _employees.CountDocuments(emp => true);
            var byRole = GetEmployeeCountByRole();
            var byLocation = GetEmployeeCountByLocation();

            return new Employee.EmployeeStatistics
            {
                TotalEmployees = (int)total,
                ByRole = byRole,
                ByLocation = byLocation
            };
        }

        public bool EmployeeExists(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
                return false;

            return _employees.CountDocuments(emp => emp.Id == employeeId) > 0;
        }

        public long CountAllEmployees()
        {
            return _employees.CountDocuments(emp => true);
        }

        public void UpdateEmployee(string employeeId, string firstName, string lastName, string role, string? email, string? phone, string? location, string? hashedPassword = null)
        {
            if (!Enum.TryParse(role, true, out Role roleEnum))
                throw new ArgumentException("Invalid role", nameof(role));

            List<UpdateDefinition<Employee>> updates = new List<UpdateDefinition<Employee>>
            {
                Builders<Employee>.Update.Set("Name.FirstName", firstName),
                Builders<Employee>.Update.Set("Name.LastName",  lastName),
                Builders<Employee>.Update.Set(employee => employee.EmployeeRole, roleEnum),
                Builders<Employee>.Update.Set("ContactDetails.EmailAddress", email ?? string.Empty),
                Builders<Employee>.Update.Set("ContactDetails.PhoneNumber",  phone ?? string.Empty),
                Builders<Employee>.Update.Set("ContactDetails.Location",     location ?? string.Empty)
            };

            if (!string.IsNullOrWhiteSpace(hashedPassword))
                updates.Add(Builders<Employee>.Update.Set(e => e.Password, hashedPassword));

            var result = _employees.UpdateOne(e => e.Id == employeeId,
                                         Builders<Employee>.Update.Combine(updates));
            if (result.MatchedCount == 0)
                throw new KeyNotFoundException($"Employee {employeeId} not found");
        }

        public void DeleteEmployee(string employeeId)
        {
            var result = _employees.DeleteOne(e => e.Id == employeeId);
            if (result.DeletedCount == 0)
                throw new KeyNotFoundException($"Employee {employeeId} not found");
        }

        public Employee? GetByLoginCredentials(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return null;
            return _employees.Find(emp => emp.Name.FirstName == userName && emp.Password == password).FirstOrDefault();
        }

        public Employee? GetEmployeeByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            var filter = Builders<Employee>.Filter.Eq("ContactDetails.EmailAddress", email);
            return _employees.Find(filter).FirstOrDefault();
        }

        public Employee? GetEmployeeByResetToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var filter = Builders<Employee>.Filter.Eq(emp => emp.PasswordResetToken, token);
            return _employees.Find(filter).FirstOrDefault();
        }

        public void SavePasswordResetToken(string employeeId, string token, DateTime tokenExpiry)
        {
            if (string.IsNullOrEmpty(employeeId))
                throw new ArgumentException("Employee ID is required", nameof(employeeId));

            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token is required", nameof(token));

            var update = Builders<Employee>.Update
                .Set(emp => emp.PasswordResetToken, token)
                .Set(emp => emp.PasswordResetTokenExpiry, tokenExpiry);

            var result = _employees.UpdateOne(
                emp => emp.Id == employeeId,
                update);

            if (result.MatchedCount == 0)
                throw new KeyNotFoundException($"Employee {employeeId} not found");
        }

        public void UpdatePasswordAndClearToken(string employeeId, string hashedPassword)
        {
            if (string.IsNullOrEmpty(employeeId))
                throw new ArgumentException("Employee ID is required", nameof(employeeId));

            if (string.IsNullOrEmpty(hashedPassword))
                throw new ArgumentException("Hashed password is required", nameof(hashedPassword));

            var update = Builders<Employee>.Update
                .Set(emp => emp.Password, hashedPassword)
                .Set(emp => emp.PasswordResetToken, (string?)null)
                .Set(emp => emp.PasswordResetTokenExpiry, (DateTime?)null);

            var result = _employees.UpdateOne(
                emp => emp.Id == employeeId,
                update);

            if (result.MatchedCount == 0)
                throw new KeyNotFoundException($"Employee {employeeId} not found");
        }
    }
}