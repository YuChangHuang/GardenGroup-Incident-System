using GardenGroupIncidentSystem.Models;
using System.Collections.Generic;

namespace GardenGroupIncidentSystem.Services.Repositories
{
    public interface ITicketRepository
    {
        // CREATE
        Ticket CreateTicket(Ticket ticket);
        void CreateTickets(List<Ticket> tickets);

        // READ
        List<Ticket> GetAllTickets();
        Ticket GetTicketById(string ticketId);
        List<Ticket> GetTicketsByStatus(Status status);
        List<Ticket> GetTicketsByEmployee(string employeeId);
        public List<Ticket> GetFilteredTickets(string status, string priority, string type);

        // UPDATE
        void UpdateTicket(string ticketId, Ticket updatedTicket);

        // DELETE
        void DeleteTicket(string ticketId);

        // AGGREGATION
        Dictionary<Status, int> GetTicketCountByStatus();
        Dictionary<string, int> GetTicketCountByPriority();
    }
}