using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSystem.Data;
using TicketSystem.Models;

namespace TicketSystem.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // require login for all endpoints
    public class TicketsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: /api/tickets
        [HttpGet]
        public async Task<IActionResult> GetTickets()
        {
            var user = await _userManager.GetUserAsync(User);
            var isStaff = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

            var q = _db.Tickets.AsQueryable();
            if (!isStaff)
                q = q.Where(t => t.CreatedById == user!.Id);

            var list = await q
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new { t.Id, t.Title, t.Status, t.CreatedAt, t.CreatedById })
                .ToListAsync();

            return Ok(list);
        }

        // GET: /api/tickets/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetTicket(int id)
        {
            var ticket = await _db.Tickets
                .Include(t => t.Messages.OrderBy(m => m.SentAt))
                .FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var isStaff = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
            if (!isStaff && ticket.CreatedById != user!.Id) return Forbid();

            return Ok(new
            {
                ticket.Id,
                ticket.Title,
                ticket.Status,
                ticket.CreatedAt,
                messages = ticket.Messages.Select(m => new { m.Id, m.Content, m.SenderId, m.SentAt })
            });
        }

        public record CreateTicketDto(string Title, string FirstMessage);

        // POST: /api/tickets
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTicketDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.FirstMessage))
                return BadRequest("Título e mensagem são obrigatórios.");

            var user = await _userManager.GetUserAsync(User);

            var t = new Ticket
            {
                Title = dto.Title.Trim(),
                CreatedById = user!.Id,
                Status = TicketStatus.Open
            };
            _db.Tickets.Add(t);
            await _db.SaveChangesAsync();

            _db.TicketMessages.Add(new TicketMessage
            {
                TicketId = t.Id,
                SenderId = user!.Id,
                Content = dto.FirstMessage.Trim()
            });
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTicket), new { id = t.Id }, new { t.Id, t.Title, t.Status, t.CreatedAt });
        }

        public record UpdateTicketDto(string? Title, TicketStatus? Status);

        // PUT: /api/tickets/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTicketDto dto)
        {
            var ticket = await _db.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            var isStaff = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
            if (!isStaff) return Forbid();

            if (!string.IsNullOrWhiteSpace(dto.Title)) ticket.Title = dto.Title!.Trim();
            if (dto.Status.HasValue) ticket.Status = dto.Status.Value;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: /api/tickets/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = await _db.Tickets.Include(t => t.Messages).FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();

            var isStaff = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
            if (!isStaff) return Forbid();

            _db.Tickets.Remove(ticket);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
