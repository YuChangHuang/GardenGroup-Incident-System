using GardenGroupIncidentSystem.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GardenGroupIncidentSystem.Services.Repositories
{
    /// <summary>
    /// Repository implementation for Ticket data access.
    /// Handles all CRUD and aggregation operations for tickets.
    /// Design Choice: Repository Pattern
    /// - Separates database logic from business logic
    /// - Improves testability and maintainability
    /// </summary>
    public class TicketRepository : ITicketRepository
    {
        private readonly IMongoCollection<Ticket> _tickets;

        public TicketRepository(IMongoDatabase db)
        {
            _tickets = db.GetCollection<Ticket>("Tickets");
        }

        // CREATE

        public Ticket CreateTicket(Ticket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException(nameof(ticket));

            if (string.IsNullOrEmpty(ticket.Id))
            {
                ticket.Id = GenerateNextTicketId();
            }

            _tickets.InsertOne(ticket);
            return ticket;
        }

        public void CreateTickets(List<Ticket> tickets)
        {
            if (tickets == null || tickets.Count == 0)
                throw new ArgumentException("Ticket list cannot be empty.");

            foreach (var t in tickets)
            {
                if (string.IsNullOrEmpty(t.Id))
                    t.Id = GenerateNextTicketId();
            }

            _tickets.InsertMany(tickets);
        }

        // Read all the Tickets

        public List<Ticket> GetAllTickets()
        {
            return _tickets.Find(_ => true).ToList();
        }

        public Ticket GetTicketById(string ticketId)
        {
            if (string.IsNullOrEmpty(ticketId))
                return null;

            return _tickets.Find(t => t.Id == ticketId).FirstOrDefault();
        }
        // Filter tickets based on status, priority, and type
        public List<Ticket> GetFilteredTickets(string status, string priority, string type)
        {
            var filterBuilder = Builders<Ticket>.Filter;
            var filters = new List<FilterDefinition<Ticket>>();

            // Status filter
            if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<Status>(status, true, out var statusEnum))
                    filters.Add(filterBuilder.Eq(t => t.TicketStatus, statusEnum));
            }

            // Priority filter
            if (!string.Equals(priority, "all", StringComparison.OrdinalIgnoreCase))
                filters.Add(filterBuilder.Eq(t => t.Priority, priority));

            // Type filter
            if (!string.Equals(type, "all", StringComparison.OrdinalIgnoreCase))
                filters.Add(filterBuilder.Eq(t => t.Type, type));

            var finalFilter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            return _tickets.Find(finalFilter).ToList();
        }


        public List<Ticket> GetTicketsByStatus(Status status)
        {
            return _tickets.Find(t => t.TicketStatus == status).ToList();
        }

        public List<Ticket> GetTicketsByEmployee(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
                return new List<Ticket>();

            return _tickets.Find(t => t.EmployeeID == employeeId).ToList();
        }

        public void UpdateTicket(string ticketId, Ticket updatedTicket)
        {
            if (string.IsNullOrEmpty(ticketId) || updatedTicket == null)
                throw new Exception("Null ticket id or Ticket");
            if (GetTicketById(ticketId) == null)
                throw new Exception("Ticket not found");
            _tickets.ReplaceOne(t => t.Id == ticketId, updatedTicket);
        }
        // Delete only if the ticket status is Closed or Resolved
        public void DeleteTicket(string ticketId)
        {
            Ticket ticket = GetTicketById(ticketId);
            if (string.IsNullOrEmpty(ticketId))
                throw new Exception("ticket id is null");
            if (ticket.TicketStatus != Status.Closed)
                throw new Exception("Only tickets with status 'Closed' or 'Resolved' can be deleted.");
            else if (ticket.TicketStatus != Status.Resolved)
                throw new Exception("Only tickets with status 'Closed' or 'Resolved' can be deleted.");
            _tickets.DeleteOne(t => t.Id == ticketId);
        }

        public Dictionary<Status, int> GetTicketCountByStatus()
        {
            var pipeline = _tickets.Aggregate()
                .Group(
                    t => t.TicketStatus,
                    g => new { Status = g.Key, Count = g.Count() }
                )
                .ToList();

            return pipeline.ToDictionary(x => x.Status, x => x.Count);
        }
        // Aggregation to get ticket counts by priority
        public Dictionary<string, int> GetTicketCountByPriority()
        {
            var pipeline = _tickets.Aggregate()
                .Group(
                    t => t.Priority,
                    g => new { Priority = g.Key, Count = g.Count() }
                )
                .ToList();

            return pipeline.ToDictionary(x => x.Priority, x => x.Count);
        }

        private string GenerateNextTicketId()
        {
            var lastTicket = _tickets.Find(_ => true)
                .SortByDescending(t => t.Id)
                .FirstOrDefault();

            int max = 0;
            if (lastTicket != null && lastTicket.Id.StartsWith("T") && int.TryParse(lastTicket.Id.Substring(1), out int num))
                max = num;

            return $"T{(max + 1):D4}";
        }

    }
}