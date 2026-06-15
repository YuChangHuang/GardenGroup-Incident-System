/*using MongoDB.Driver;
using GardenGroupIncidentSystem.Models;

namespace GardenGroupIncidentSystem.Services
{
    public class MongoDBService
    {
        private readonly IMongoDatabase _database;

        public MongoDBService(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<Employee> GetEmployeeCollection()
        {
            return _database.GetCollection<Employee>("Employee");
        }
    }
}*/