using GardenGroupIncidentSystem.Models;
using System.Collections.Generic;
using System.Linq;

namespace GardenGroupIncidentSystem.Services.Repositories
{
    /// <summary>
    /// Provides in-memory filtering of tickets using a keyword.
    /// </summary>
    public class KeywordFilterRepository : IKeywordFilterRepository
    {
        public List<Ticket> FilterByKeyword(List<Ticket> tickets, string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return tickets;

            string lowerKeyword = keyword.ToLower();

            return tickets.Where(ticket =>
                (!string.IsNullOrWhiteSpace(ticket.Subject) && ticket.Subject.ToLower().Contains(lowerKeyword)) ||
                (!string.IsNullOrWhiteSpace(ticket.Description) && ticket.Description.ToLower().Contains(lowerKeyword)) ||
                (!string.IsNullOrWhiteSpace(ticket.Type) && ticket.Type.ToLower().Contains(lowerKeyword)) ||
                (!string.IsNullOrWhiteSpace(ticket.Priority) && ticket.Priority.ToLower().Contains(lowerKeyword))
            ).ToList();
        }
    }
}
