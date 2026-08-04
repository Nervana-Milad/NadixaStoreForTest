using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Entities;
using Nadixa.Web.Models;
using System.Diagnostics;
using Nadixa.Application.DTOS;


namespace Nadixa.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHomeService _homeService;
        public HomeController(
            ILogger<HomeController> logger,
            UserManager<AppUser> userManager,
            IHomeService homeService)
        {
            _logger = logger;
            _userManager = userManager;
            _homeService = homeService;
        }

        public async Task<IActionResult> Index(int? categoryId)
        {
            var user = await _userManager.GetUserAsync(User);
            var data = await _homeService.GetIndexDataAsync(categoryId, user?.Id);

            ViewBag.Categories = data.Categories;
            ViewBag.BestSellers = data.BestSellers;
            ViewBag.ActivePromotions = data.ActivePromotions;   // ?? ÌÏíÏ


            return View(data.Products);
        }

        [HttpGet]
        public async Task<IActionResult> GlobalSearch(string term)
        {
            var result = await _homeService.GlobalSearchAsync(term);
            return Json(result);
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}