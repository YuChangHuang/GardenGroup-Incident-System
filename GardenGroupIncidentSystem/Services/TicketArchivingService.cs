using GardenGroupIncidentSystem.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GardenGroupIncidentSystem.Services
{
    /// <summary>
    /// INDIVIDUAL FUNCTIONALITY: Ticket Archiving Service
    /// Developer: [YOUR NAME] ([YOUR STUDENT ID])
    /// 
    /// Purpose: Archive old tickets to a secondary (archive) database collection
    /// Requirement: Developed as a SEPARATE CLASS
    /// 
    /// Features:
    /// - Archive tickets older than 2 years with simple button click
    /// - Move tickets to ArchivedTickets collection
    /// - Restore archived tickets if needed
    /// - Search archived tickets
    /// - View archive statistics
    /// </summary>
    public class TicketArchivingService
    {
        private readonly IMongoCollection<Ticket> _tickets;
        private readonly IMongoCollection<Ticket> _archivedTickets;

        /// <summary>
        /// Constructor - Initializes MongoDB collections
        /// </summary>
        /// <param name="database">MongoDB database instance</param>
        public TicketArchivingService(IMongoDatabase database)
        {
            // Active tickets collection
            _tickets = database.GetCollection<Ticket>("Tickets");

            // Archive collection (secondary database)
            _archivedTickets = database.GetCollection<Ticket>("ArchivedTickets");
        }

        /// <summary>
        /// MAIN ARCHIVING METHOD
        /// Archive tickets older than 2 years (default)
        /// This is triggered by the button click
        /// </summary>
        /// <returns>Number of tickets archived</returns>
        public int ArchiveOldTickets()
        {
            // Calculate date 2 years ago
            DateTime twoYearsAgo = DateTime.Now.AddYears(-2);

            // Call the main archiving method with 2-year cutoff
            return ArchiveTicketsByDate(twoYearsAgo);
        }

        /// <summary>
        /// Archive tickets before a specific date
        /// Only archives closed or resolved tickets for safety
        /// </summary>
        /// <param name="cutoffDate">Archive tickets before this date</param>
        /// <returns>Number of tickets archived</returns>
        public int ArchiveTicketsByDate(DateTime cutoffDate)
        {
            try
            {
                // Build MongoDB filter to find archivable tickets
                var filterBuilder = Builders<Ticket>.Filter;
                var filter = filterBuilder.And(
                    // Ticket must be older than cutoff date
                    filterBuilder.Lt(t => t.DateTimeReport, cutoffDate),

                    // Ticket must be closed or resolved
                    filterBuilder.Or(
                        filterBuilder.Eq(t => t.TicketStatus, Status.Closed),
                        filterBuilder.Eq(t => t.TicketStatus, Status.Resolved)
                    )
                );

                // Find all tickets matching the criteria
                var ticketsToArchive = _tickets.Find(filter).ToList();

                // If no tickets found, return 0
                if (!ticketsToArchive.Any())
                    return 0;

                int archivedCount = 0;

                // Process each ticket
                foreach (var ticket in ticketsToArchive)
                {
                    // Insert into archive collection
                    _archivedTickets.InsertOne(ticket);

                    // Remove from active tickets collection
                    var deleteFilter = Builders<Ticket>.Filter.Eq(t => t.Id, ticket.Id);
                    var deleteResult = _tickets.DeleteOne(deleteFilter);

                    // Count successful archives
                    if (deleteResult.DeletedCount > 0)
                        archivedCount++;
                }

                return archivedCount;
            }
            catch (Exception ex)
            {
                // Log error (in production, use proper logging)
                Console.WriteLine($"Error during archiving: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Get all archived tickets
        /// </summary>
        /// <returns>List of archived tickets, sorted by date (newest first)</returns>
        public List<Ticket> GetArchivedTickets()
        {
            return _archivedTickets
                .Find(_ => true)
                .SortByDescending(t => t.DateTimeReport)
                .ToList();
        }

        /// <summary>
        /// Search archived tickets by keyword
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>List of matching archived tickets</returns>
        public List<Ticket> SearchArchivedTickets(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Ticket>();

            // Build regex filter for case-insensitive search
            var regex = new MongoDB.Bson.BsonRegularExpression(searchTerm, "i");
            var filterBuilder = Builders<Ticket>.Filter;

            var filter = filterBuilder.Or(
                filterBuilder.Regex(t => t.Subject, regex),
                filterBuilder.Regex(t => t.Description, regex),
                filterBuilder.Regex(t => t.EmployeeID, regex),
                filterBuilder.Regex(t => t.Id, regex)
            );

            return _archivedTickets
                .Find(filter)
                .SortByDescending(t => t.DateTimeReport)
                .ToList();
        }

        /// <summary>
        /// Restore an archived ticket back to active tickets
        /// </summary>
        /// <param name="ticketId">ID of the archived ticket</param>
        /// <returns>True if restore successful, false otherwise</returns>
        public bool RestoreArchivedTicket(string ticketId)
        {
            if (string.IsNullOrWhiteSpace(ticketId))
                return false;

            try
            {
                // Find the archived ticket
                var ticket = _archivedTickets
                    .Find(t => t.Id == ticketId)
                    .FirstOrDefault();

                if (ticket == null)
                    return false;

                // Insert back into active tickets
                _tickets.InsertOne(ticket);

                // Remove from archive
                var deleteFilter = Builders<Ticket>.Filter.Eq(t => t.Id, ticketId);
                var deleteResult = _archivedTickets.DeleteOne(deleteFilter);

                return deleteResult.DeletedCount > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get count of tickets eligible for archiving
        /// </summary>
        /// <param name="cutoffDate">Date before which tickets can be archived</param>
        /// <returns>Count of archivable tickets</returns>
        public long GetArchivableTicketsCount(DateTime cutoffDate)
        {
            var filterBuilder = Builders<Ticket>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Lt(t => t.DateTimeReport, cutoffDate),
                filterBuilder.Or(
                    filterBuilder.Eq(t => t.TicketStatus, Status.Closed),
                    filterBuilder.Eq(t => t.TicketStatus, Status.Resolved)
                )
            );

            return _tickets.CountDocuments(filter);
        }

        /// <summary>
        /// Get archiving statistics for dashboard
        /// </summary>
        /// <returns>Dictionary containing archive statistics</returns>
        public Dictionary<string, object> GetArchivingStatistics()
        {
            // Count active tickets
            var activeTickets = _tickets.CountDocuments(_ => true);

            // Count archived tickets
            var archivedTickets = _archivedTickets.CountDocuments(_ => true);

            // Find oldest active ticket
            var oldestActiveTicket = _tickets
                .Find(_ => true)
                .SortBy(t => t.DateTimeReport)
                .FirstOrDefault();

            // Find newest archived ticket
            var newestArchivedTicket = _archivedTickets
                .Find(_ => true)
                .SortByDescending(t => t.DateTimeReport)
                .FirstOrDefault();

            // Calculate archive percentage
            double archivePercentage = 0;
            if (activeTickets + archivedTickets > 0)
            {
                archivePercentage = Math.Round(
                    (archivedTickets / (double)(activeTickets + archivedTickets)) * 100,
                    2
                );
            }

            return new Dictionary<string, object>
            {
                ["ActiveTicketsCount"] = activeTickets,
                ["ArchivedTicketsCount"] = archivedTickets,
                ["TotalTickets"] = activeTickets + archivedTickets,
                ["ArchivePercentage"] = archivePercentage,
                ["OldestActiveTicketDate"] = oldestActiveTicket?.DateTimeReport.ToString("yyyy-MM-dd") ?? "N/A",
                ["NewestArchivedTicketDate"] = newestArchivedTicket?.DateTimeReport.ToString("yyyy-MM-dd") ?? "N/A"
            };
        }

        /// <summary>
        /// Preview tickets that would be archived without actually archiving them
        /// </summary>
        /// <param name="cutoffDate">Date to check against</param>
        /// <returns>List of tickets that would be archived (limited to 100)</returns>
        public List<Ticket> PreviewArchivableTickets(DateTime cutoffDate)
        {
            var filterBuilder = Builders<Ticket>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Lt(t => t.DateTimeReport, cutoffDate),
                filterBuilder.Or(
                    filterBuilder.Eq(t => t.TicketStatus, Status.Closed),
                    filterBuilder.Eq(t => t.TicketStatus, Status.Resolved)
                )
            );

            return _tickets
                .Find(filter)
                .SortBy(t => t.DateTimeReport)
                .Limit(100)
                .ToList();
        }

        /// <summary>
        /// Validate archiving operation before execution
        /// </summary>
        /// <param name="cutoffDate">Date to validate</param>
        /// <returns>Validation result with message and affected count</returns>
        public (bool IsValid, string Message, int AffectedCount) ValidateArchiving(DateTime cutoffDate)
        {
            // Check if date is in the future
            if (cutoffDate > DateTime.Now)
                return (false, "Cutoff date cannot be in the future", 0);

            // Get count of tickets to be archived
            var count = (int)GetArchivableTicketsCount(cutoffDate);

            // Allow archiving even if 0 tickets (for testing)
            return (true, $"Ready to archive {count} tickets", count);
        }

        /// <summary>
        /// Archive tickets by age in years
        /// </summary>
        /// <param name="years">Number of years (e.g., 2 for tickets older than 2 years)</param>
        /// <returns>Number of tickets archived</returns>
        public int ArchiveTicketsByAge(int years)
        {
            if (years <= 0)
                return 0;

            DateTime cutoffDate = DateTime.Now.AddYears(-years);
            return ArchiveTicketsByDate(cutoffDate);
        }

        /// <summary>
        /// Get archive summary by year
        /// Useful for statistics and reporting
        /// </summary>
        /// <returns>Dictionary of year to ticket count</returns>
        public Dictionary<int, long> GetArchiveSummaryByYear()
        {
            var archivedTickets = _archivedTickets.Find(_ => true).ToList();

            return archivedTickets
                .GroupBy(t => t.DateTimeReport.Year)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => (long)g.Count());
        }
    }
}