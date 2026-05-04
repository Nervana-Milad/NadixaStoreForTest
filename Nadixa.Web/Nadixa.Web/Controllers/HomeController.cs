using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nadixa.Infrastructure.Data;
using Nadixa.Web.Models;
using Nadixa.Web.Models.ViewModels;
using System;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

namespace Nadixa.Web.Controllers
{
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private readonly NadixaDbContext _context;

        public HomeController(ILogger<HomeController> logger, NadixaDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public IActionResult Index(int? categoryId)
        {
            var productQuery = _context.Products.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue)
            {
                productQuery = productQuery.Where(p => p.CategoryId == categoryId);
            }

            var products = productQuery.ToList();

            ViewBag.Categories = _context.ProductCategories.ToList();
            return View(products);
        }


        public IActionResult About()
        {
            return View();
        }
        public IActionResult Privacy()
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
