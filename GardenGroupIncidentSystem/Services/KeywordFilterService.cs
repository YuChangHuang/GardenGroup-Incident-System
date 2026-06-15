using GardenGroupIncidentSystem.Models;
using GardenGroupIncidentSystem.Services.Repositories;
using System.Collections.Generic;

namespace GardenGroupIncidentSystem.Services.Filtering
{
    /// <summary>
    /// Applies keyword filtering logic via its repository.
    /// </summary>
    public class KeywordFilterService : IKeywordFilterService
    {
        private readonly IKeywordFilterRepository _repository;

        public KeywordFilterService(IKeywordFilterRepository repository)
        {
            _repository = repository;
        }

        public List<Ticket> Filter(List<Ticket> tickets, string keyword)
        {
            return _repository.FilterByKeyword(tickets, keyword);
        }
    }
}
