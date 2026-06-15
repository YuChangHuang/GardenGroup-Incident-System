using DocumentFormat.OpenXml.Wordprocessing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;
using System;
using System.Collections.Generic;

namespace GardenGroupIncidentSystem.Models
{
    public enum Role
    {
        supervisor,
        servicedesk_1,
        servicedesk_2,
        servicedesk_3,
        service_desk,
        regular
    }

    [BsonIgnoreExtraElements]
    public class Employee
    {

        [BsonElement("_id")]
        public string Id { get; set; }

        [BsonElement("Password")]
        public string Password { get; set; }

        [BsonElement("Role")]
        [BsonRepresentation(BsonType.String)]
        public Role EmployeeRole { get; set; }

        [BsonElement("Name")]
        public Names Name { get; set; }

        [BsonElement("ContactDetails")]
        public ContactDetail ContactDetails { get; set; }

        [BsonElement("PasswordResetToken")]
        public string? PasswordResetToken { get; set; }

        [BsonElement("PasswordResetTokenExpiry")]
        public DateTime? PasswordResetTokenExpiry { get; set; }

        [BsonIgnoreExtraElements]
        public class ContactDetail
        {
            [BsonElement("EmailAddress")]
            public string EmailAddress { get; set; }

            [BsonElement("Location")]
            public string Location { get; set; }

            [BsonElement("PhoneNumber")]
            public string PhoneNumber { get; set; }

            public ContactDetail() { }

            public ContactDetail(string emailaddress, string location, string phoneNumber)
            {
                EmailAddress = emailaddress;
                Location = location;
                PhoneNumber = phoneNumber;
            }
        }

        [BsonIgnoreExtraElements]
        public class Names
        {
            [BsonElement("FirstName")]
            public string FirstName { get; set; }

            [BsonElement("LastName")]
            public string LastName { get; set; }

            public Names() { }

            public Names(string firstName, string lastName)
            {
                FirstName = firstName;
                LastName = lastName;
            }
        }

        public Employee() { }
        public Employee(string id, Names name)
        {
            Id = id;
            Name = name;
        }

        public Employee(string id, string password, Role employeeRole, string emailAddress, string location, string phoneNumber, string firstName, string lastName)
        {
            Id = id;
            Password = password;
            EmployeeRole = employeeRole;
            ContactDetails = new ContactDetail(emailAddress, location, phoneNumber);
            Name = new Names(firstName, lastName);
        }

        public class EmployeeStatistics
        {
            public int TotalEmployees { get; set; }
            public Dictionary<string, int> ByRole { get; set; }
            public Dictionary<string, int> ByLocation { get; set; }
        }
    }
}