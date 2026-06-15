using GardenGroupIncidentSystem.Models;
using System.Collections.Generic;

namespace GardenGroupIncidentSystem.Services.Repositories
{
    /// <summary>
    /// Defines logic for filtering a list of tickets by keyword.
    /// </summary>
    public interface IKeywordFilterRepository
    {
        List<Ticket> FilterByKeyword(List<Ticket> tickets, string keyword);
    }
}
