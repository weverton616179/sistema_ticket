using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSystem.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string CreatedById { get; set; } = string.Empty;
        public ApplicationUser? CreatedBy { get; set; }

        public string? AssignedToId { get; set; }
        public ApplicationUser? AssignedTo { get; set; }

        public TicketStatus Status { get; set; } = TicketStatus.Open;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TicketMessage> Messages { get; set; } = new List<TicketMessage>();
    }
}
