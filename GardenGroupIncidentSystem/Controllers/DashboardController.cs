using Microsoft.AspNetCore.Mvc;
using GardenGroupIncidentSystem.Models;
using GardenGroupIncidentSystem.Services;

namespace GardenGroupIncidentSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ITicketService _ticketService;

        public DashboardController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        private Employee? GetLoggedInUser()
        {
            return HttpContext.Session.GetObject<Employee>("LoggedInUser");
        }

        private bool IsRegularUser(Employee user) => user.EmployeeRole == Role.regular;

        [HttpGet]
        public IActionResult Index(string statusFilter = "All")
        {
            var user = GetLoggedInUser();
            if (user == null || !IsRegularUser(user))
                return RedirectToAction("Index", "Login");

            // Get tickets for the logged-in user
            var tickets = _ticketService.GetAllTickets()
                .Where(t => t.EmployeeID == user.Id)
                .ToList();

            // Counts before filtering
            int open = tickets.Count(t => t.TicketStatus == Status.Open);
            int resolved = tickets.Count(t => t.TicketStatus == Status.Resolved);
            int closed = tickets.Count(t => t.TicketStatus == Status.Closed);
            int total = tickets.Count;

            // Apply status filter if provided
            List<Ticket> filteredTickets = new List<Ticket>(tickets);
            if(!string.IsNullOrEmpty(statusFilter) && statusFilter.ToLower() != "all")
            {
                if (Enum.TryParse<Status>(statusFilter, true, out var filterStatus))
                {
                    filteredTickets = tickets
                        .Where(t => t.TicketStatus == filterStatus)
                        .ToList();
                }
            }

            ViewBag.Total = total;
            ViewBag.OpenCount = open;
            ViewBag.ResolvedCount = resolved;
            ViewBag.ClosedCount = closed;

            ViewBag.PercentOpen = total > 0 ? Math.Round((double)open / total * 100, 2) : 0;
            ViewBag.PercentResolved = total > 0 ? Math.Round((double)resolved / total * 100, 2) : 0;
            ViewBag.PercentClosed = total > 0 ? Math.Round((double)closed / total * 100, 2) : 0;

            ViewBag.FilteredTickets = filteredTickets;
            ViewBag.Tickets = filteredTickets;

            return View();
        }
    }
}
