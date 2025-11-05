using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TicketSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(60)]
        public string? FirstName { get; set; }

        [MaxLength(80)]
        public string? LastName { get; set; }
    }
}
