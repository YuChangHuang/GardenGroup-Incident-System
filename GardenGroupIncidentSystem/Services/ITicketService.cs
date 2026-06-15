using GardenGroupIncidentSystem.Models;
using System.Collections.Generic;

namespace GardenGroupIncidentSystem.Services
{
    public interface ITicketService
    {
        // CRUD
        List<Ticket> GetAllTickets();
        Ticket GetTicketById(string ticketId);
        Ticket CreateTicket(Ticket ticket);
        void UpdateTicket(string ticketId, Ticket updatedTicket);
        void DeleteTicket(string ticketId);

        // Filters
        List<Ticket> GetFilteredTickets(string status, string priority, string type);

        // Analytics / counts
        Dictionary<Status, int> GetTicketCountByStatus();
        Dictionary<string, int> GetTicketCountByPriority();

        void UpdateTicketWithWorkflow(string ticketId, Ticket updatedTicket, string handoverTo, string handoverReason);
    }
}