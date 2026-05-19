using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;
using Nadixa.Web.Models.ViewModels;
using System.Threading.Tasks;

namespace Nadixa.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly NadixaDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public ProfileController(NadixaDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) 
            {
                return RedirectToAction("Login", "Auth");
            }

            var orders = await _context.Orders.Where(o => o.UserId == user.Id).OrderByDescending(o => o.CreatedAt).ToListAsync();

           
            var orderViewModel = orders.Select(o => new OrderViewModel
            {
                Id = o.Id,
                CreatedAt = o.CreatedAt,
                Status = o.Status.ToString(),
                GrandTotal = o.TotalPrice
            }).ToList();

            var profileViewModel = new ProfileViewModel
            {
                FullName = user.FirstName + " " + user.LastName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                Orders = orderViewModel
            };

            return View(profileViewModel);
        }
    }
}
