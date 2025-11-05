using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSystem.Data;
using TicketSystem.Models;

namespace TicketSystem.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin"))
            {
                var all = await _db.Tickets
                    .Include(t => t.CreatedBy)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
                return View(all);
            }
            else
            {
                var mine = await _db.Tickets
                    .Where(t => t.CreatedById == user!.Id)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
                return View(mine);
            }
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(string title, string firstMessage)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(firstMessage))
            {
                ModelState.AddModelError("", "Título e mensagem são obrigatórios.");
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            var t = new Ticket
            {
                Title = title.Trim(),
                CreatedById = user!.Id,
                Status = TicketStatus.Open
            };
            _db.Tickets.Add(t);
            await _db.SaveChangesAsync();

            var m = new TicketMessage
            {
                TicketId = t.Id,
                SenderId = user!.Id,
                Content = firstMessage.Trim()
            };
            _db.TicketMessages.Add(m);
            await _db.SaveChangesAsync();

            return RedirectToAction("Details", new { id = t.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _db.Tickets
                .Include(t => t.Messages.OrderBy(m => m.SentAt))
                .Include(t => t.CreatedBy)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();

            // Access control
            var user = await _userManager.GetUserAsync(User);
            var isStaff = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
            if (!isStaff && ticket.CreatedById != user!.Id) return Forbid();

            ViewBag.IsStaff = isStaff;
            return View(ticket);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int id, string content)
        {
            var ticket = await _db.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var isStaff = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
            if (!isStaff && ticket.CreatedById != user!.Id) return Forbid();

            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Details", new { id });

            var msg = new TicketMessage
            {
                TicketId = id,
                SenderId = user!.Id,
                Content = content.Trim()
            };
            _db.TicketMessages.Add(msg);
            await _db.SaveChangesAsync();

            return RedirectToAction("Details", new { id });
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, TicketStatus status)
        {
            var ticket = await _db.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();
            ticket.Status = status;
            await _db.SaveChangesAsync();
            return RedirectToAction("Details", new { id });
        }
    }
}
