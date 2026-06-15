namespace GardenGroupIncidentSystem.Services.Sorting
{
    using System.Collections.Generic;
    using GardenGroupIncidentSystem.Models;

    public interface ITicketSorter
    {
        IEnumerable<Ticket> SortByPriority(IEnumerable<Ticket> tickets, bool ascending = true);
    }
}
