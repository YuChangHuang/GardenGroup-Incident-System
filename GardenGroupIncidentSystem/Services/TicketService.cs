using GardenGroupIncidentSystem.Models;
using GardenGroupIncidentSystem.Services.Repositories;
using System;
using System.Collections.Generic;

namespace GardenGroupIncidentSystem.Services
{
    /// <summary>
    /// Service layer for Ticket operations.
    /// Encapsulates business logic and validation before database access.
    /// </summary>
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        // CRUD

        public List<Ticket> GetAllTickets()
        {
            return _ticketRepository.GetAllTickets();
        }

        public Ticket GetTicketById(string ticketId)
        {
            if (string.IsNullOrEmpty(ticketId))
                throw new ArgumentException("Ticket ID cannot be null or empty.");

            return _ticketRepository.GetTicketById(ticketId);
        }

        public Ticket CreateTicket(Ticket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException(nameof(ticket));

            return _ticketRepository.CreateTicket(ticket);
        }

        public void UpdateTicket(string ticketId, Ticket updatedTicket)
        {
            if (string.IsNullOrEmpty(ticketId))
                throw new ArgumentException("Ticket ID cannot be null or empty.");

            if (updatedTicket == null)
                throw new ArgumentNullException(nameof(updatedTicket));

            _ticketRepository.UpdateTicket(ticketId, updatedTicket);
        }

        public void DeleteTicket(string ticketId)
        {
            if (string.IsNullOrEmpty(ticketId))
                throw new ArgumentException("Ticket ID cannot be null or empty.");

            _ticketRepository.DeleteTicket(ticketId);
        }

        // FILTERS

        public List<Ticket> GetFilteredTickets(string status, string priority, string type)
        {
            // Pass filters to repository
            return _ticketRepository.GetFilteredTickets(status, priority, type);
        }

        // ANALYTICS

        public Dictionary<Status, int> GetTicketCountByStatus()
        {
            return _ticketRepository.GetTicketCountByStatus();
        }

        public Dictionary<string, int> GetTicketCountByPriority()
        {
            return _ticketRepository.GetTicketCountByPriority();
        }
        public void UpdateTicketWithWorkflow(string ticketId, Ticket updatedTicket, string handoverTo, string handoverReason)
        {
            if (string.IsNullOrEmpty(ticketId))
                throw new ArgumentException("Ticket ID cannot be null or empty.");

            var existing = _ticketRepository.GetTicketById(ticketId);
            if (existing == null)
                throw new InvalidOperationException($"Ticket {ticketId} not found.");

            //  Update status
            existing.TicketStatus = updatedTicket.TicketStatus;

            //  Mark finished if Resolved or Closed
            if (existing.TicketStatus == Status.Resolved || existing.TicketStatus == Status.Closed)
            {
                var lastWork = existing.WorkedBy.LastOrDefault();
                if (lastWork != null)
                {
                    lastWork.Timestamps.FinishedAt = DateTime.Now;
                    lastWork.Active = false;
                }
            }

            //  Handle handover
            if (!string.IsNullOrEmpty(handoverTo))
            {
                var current = existing.WorkedBy.LastOrDefault();

                // Mark current worker as inactive
                if (current != null)
                {
                    current.Timestamps.HandedAt = DateTime.Now;
                    current.Active = false;
                }

                string byEmployee = current?.Employee?.Id ?? "System";

                var newWorkedBy = new WorkedBy
                {
                    Employee = new Employee { Id = handoverTo },
                    Handover = new Handover
                    {
                        ToEmployee = handoverTo,
                        By = byEmployee,
                        Why = string.IsNullOrEmpty(handoverReason) ? "Manual handover" : handoverReason
                    },
                    Timestamps = new Timestamps
                    {
                        AssignedAt = DateTime.Now,
                        HandedAt = null,
                        FinishedAt = null
                    },
                    Active = true
                };

                existing.WorkedBy.Add(newWorkedBy);
            }

            //Save back to DB
            _ticketRepository.UpdateTicket(ticketId, existing);
        }
    }
}