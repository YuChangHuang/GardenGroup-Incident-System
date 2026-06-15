namespace GardenGroupIncidentSystem.Services.Sorting
{
    using System.Collections.Generic;
    using System.Linq;
    using GardenGroupIncidentSystem.Models;

    public sealed class TicketSorter : ITicketSorter
    {
        // High > Medium > Low
        private static readonly Dictionary<string, int> PriorityRank = new()
        {
            ["High"] = 3,
            ["Medium"] = 2,
            ["Low"] = 1
        };

        public IEnumerable<Ticket> SortByPriority(IEnumerable<Ticket> tickets, bool ascending = true)
        {
            if (tickets == null) return Enumerable.Empty<Ticket>();

            var ordered = tickets.OrderBy(t =>
            {
                if (t?.Priority == null) return 0; // null/unknown last
                return PriorityRank.TryGetValue(t.Priority, out var rank) ? rank : 0;
            });

            return ascending ? ordered : ordered.Reverse();
        }
    }
}