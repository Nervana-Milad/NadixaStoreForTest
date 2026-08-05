using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;
using Nadixa.Web.Models.ViewModels;
using System.Threading.Tasks;

namespace Nadixa.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;
        private readonly UserManager<AppUser> _userManager;
        public ProfileController(IProfileService profileService, UserManager<AppUser> userManager)
        {
            _profileService = profileService;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var profile = await _profileService.GetProfileAsync(
                user.Id,
                $"{user.FirstName} {user.LastName}",
                user.Email ?? string.Empty,
                user.PhoneNumber);

            return View(profile);
        }
    }
}
