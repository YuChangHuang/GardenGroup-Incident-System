using GardenGroupIncidentSystem.Models;
using System.Collections.Generic;

namespace GardenGroupIncidentSystem.Services.Filtering
{
    /// <summary>
    /// Service interface for keyword-based filtering of tickets.
    /// </summary>
    public interface IKeywordFilterService
    {
        List<Ticket> Filter(List<Ticket> tickets, string keyword);
    }
}
