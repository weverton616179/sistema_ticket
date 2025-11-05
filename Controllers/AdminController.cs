using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TicketSystem.Models;

namespace TicketSystem.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index() => RedirectToAction("Users");

        public async Task<IActionResult> Users()
        {
            var users = _userManager.Users.ToList();
            var vm = new List<(ApplicationUser User, IList<string> Roles)>();
            foreach (var u in users)
            {
                vm.Add((u, await _userManager.GetRolesAsync(u)));
            }
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> SetRole(string userId, string role)
        {
            var u = await _userManager.FindByIdAsync(userId);
            if (u == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(u);
            await _userManager.RemoveFromRolesAsync(u, roles);
            if (!await _roleManager.RoleExistsAsync(role)) return BadRequest("Role inválida.");
            await _userManager.AddToRoleAsync(u, role);
            return RedirectToAction("Users");
        }
    }
}
