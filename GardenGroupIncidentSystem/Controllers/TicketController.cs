using DocumentFormat.OpenXml.Wordprocessing;
using GardenGroupIncidentSystem.Models;
using GardenGroupIncidentSystem.Services;
using GardenGroupIncidentSystem.Services.Filtering;
using GardenGroupIncidentSystem.Services.Sorting;
using Microsoft.AspNetCore.Mvc;

namespace NoSQL_Project.Controllers
{
    public class TicketController : Controller
    {

        private readonly ITicketService _ticketService;
        private readonly ITicketSorter _ticketSorter;
        private readonly IKeywordFilterService _keywordFilterService;

        // Constructor with dependency injection for ticket service
        public TicketController(ITicketService ticketService, ITicketSorter ticketSorter, IKeywordFilterService keywordFilterService)
        {
            _ticketService = ticketService;
            _ticketSorter = ticketSorter;
            _keywordFilterService = keywordFilterService;
        }

        // Displays a list of all tickets
        public IActionResult Index(
    string q = "", string status = "all", string priority = "all", string type = "all",
    string sort = "priority", string dir = "desc")
        {
            try
            {
                // Load tickets
                var tickets = _ticketService.GetAllTickets();

                // Apply keyword filter using KeywordFilterService
                tickets = _keywordFilterService.Filter(tickets, q);

                // Dropdown filters
                if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
                {
                    if (Enum.TryParse<Status>(status, true, out var statusEnum))
                        tickets = tickets.Where(t => t.TicketStatus == statusEnum).ToList();
                }

                if (!string.Equals(priority, "all", StringComparison.OrdinalIgnoreCase))
                    tickets = tickets.Where(t => t.Priority.Equals(priority, StringComparison.OrdinalIgnoreCase)).ToList();

                if (!string.Equals(type, "all", StringComparison.OrdinalIgnoreCase))
                    tickets = tickets.Where(t => t.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();

                // Sort
                if (string.Equals(sort, "priority", StringComparison.OrdinalIgnoreCase))
                {
                    bool asc = string.Equals(dir, "asc", StringComparison.OrdinalIgnoreCase);
                    tickets = _ticketSorter.SortByPriority(tickets, asc).ToList();
                }

                // ViewBag
                ViewBag.TotalTickets = tickets.Count;
                ViewBag.OpenCount = tickets.Count(t => t.TicketStatus == Status.Open);
                ViewBag.ResolvedCount = tickets.Count(t => t.TicketStatus == Status.Resolved);
                ViewBag.ClosedCount = tickets.Count(t => t.TicketStatus == Status.Closed);

                ViewBag.Query = q;
                ViewBag.StatusFilter = status;
                ViewBag.PriorityFilter = priority;
                ViewBag.TypeFilter = type;
                ViewBag.Sort = sort;
                ViewBag.Dir = dir;

                return View(tickets);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading tickets: {ex.Message}";
                return View(new List<Ticket>());
            }
        }


        // Displays details for a specific ticket by ID
        [HttpGet]
        public IActionResult Details(string id)
        {
            var ticket = _ticketService.GetTicketById(id);
            if (ticket == null) return NotFound();
            return View(ticket);
        }
        // Displays the create ticket form
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Creates a new ticket
        [HttpPost]
        public IActionResult Create(Ticket ticket)
        {
            try
            {
                var loggedInUser = HttpContext.Session.GetObject<Employee>("LoggedInUser");
                if (loggedInUser == null)
                {
                    TempData["Error"] = "Session expired. Please log in again.";
                    return RedirectToAction("Index", "Login");
                }

                // Basic validation for required fields
                if (string.IsNullOrWhiteSpace(ticket.Subject) ||
                    string.IsNullOrWhiteSpace(ticket.Type) ||
                    string.IsNullOrWhiteSpace(ticket.Priority) ||
                    string.IsNullOrWhiteSpace(ticket.Description))
                {
                    TempData["Error"] = "All fields are required!";
                    return RedirectToAction("Create");
                }

                // Assign ticket defaults
                ticket.EmployeeID = loggedInUser.Id;
                ticket.TicketStatus = Status.Open;
                ticket.DateTimeReport = DateTime.Now;
                ticket.Deadline = DateTime.Now.AddDays(3);

                // Save ticket
                var createdTicket = _ticketService.CreateTicket(ticket);
                TempData["Success"] = $"Ticket {createdTicket.Id} created successfully.";
                if(loggedInUser.EmployeeRole == Role.regular)
                    return RedirectToAction("Index", "Dashboard");
                else
                {
                    return RedirectToAction("Index", "Ticket");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to create ticket: {ex.Message}";
                return RedirectToAction("Create");
            }
        }


        [HttpGet]
        public IActionResult Edit(string id)
        {
            var ticket = _ticketService.GetTicketById(id);
            if (ticket == null)
            {
                TempData["Error"] = $"Ticket {id} not found.";
                return RedirectToAction("Index");
            }
            return View(ticket);
        }

        [HttpPost]
        public IActionResult Edit(string id, Ticket ticket, string HandoverTo, string HandoverReason)
        {
            try
            {
                _ticketService.UpdateTicketWithWorkflow(id, ticket, HandoverTo, HandoverReason);
                TempData["Success"] = $"Ticket {id} updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }


        // Deletes a ticket by ID
        [HttpPost]
        public IActionResult Delete(string id)
        {
            try
            {
                _ticketService.DeleteTicket(id);
                TempData["Success"] = $"Ticket {id} deleted successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to delete ticket {id}: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}