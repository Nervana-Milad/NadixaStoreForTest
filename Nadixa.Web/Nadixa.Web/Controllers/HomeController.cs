using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nadixa.Core.Entities;
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
        private readonly UserManager<AppUser> _userManager;

        public HomeController(ILogger<HomeController> logger, NadixaDbContext context, UserManager<AppUser> userManager)

        {
            _logger = logger;
            _context = context;
            _userManager = userManager;

        }
        public async Task<IActionResult> Index(int? categoryId)
        {
            var user = await _userManager.GetUserAsync(User);

            var productQuery = _context.Products.Include(p => p.ProductCategory).AsQueryable();


            if (categoryId.HasValue)
            {
                productQuery = productQuery.Where(p => p.ProductCategoryId == categoryId);
            }

            var products = await productQuery.ToListAsync();


            ViewBag.Categories = await _context.ProductCategories.ToListAsync();

            Dictionary<int, int> cartItems = new();

            if (user != null)
            {
                cartItems = await _context.Carts
                    .Where(c => c.UserId == user.Id)
                    .SelectMany(c => c.Items)
                    .ToDictionaryAsync(
                        i => i.ProductId,
                        i => i.Quantity);
            }

            ViewBag.CartItems = cartItems;

            var bestSellers = _context.OrderItems
                 .Where(oi => oi.Order.Status != OrderStatus.Cancelled)
                 .GroupBy(oi => oi.ProductId)
                 .OrderByDescending(g => g.Sum(oi => oi.Quantity))
                 .Take(8)
                 .Select(g => g.First().Product)
                 .ToList();

            ViewBag.Categories = _context.ProductCategories.ToList();
            ViewBag.BestSellers = bestSellers; // ? ?????? ??? View


            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> GlobalSearch(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new { products = new List<object>(), categories = new List<object>(), blogs = new List<object>() });

            var products = await _context.Products
                .Where(p => p.Name.Contains(term) || p.Description.Contains(term))
                .Take(5)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    price = p.Price,
                    imageUrl = p.MainImageUrlPath,
                    url = "/Product/Detail/" + p.Id
                })
                .ToListAsync();

            var categories = await _context.ProductCategories
                .Where(c => c.Name.Contains(term))
                .Take(3)
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    url = "/Product/Index?categoryId=" + c.Id
                })
                .ToListAsync();

            var blogs = await _context.Blogs
                .Where(b => b.Title.Contains(term) || b.Content.Contains(term))
                .Take(3)
                .Select(b => new
                {
                    id = b.Id,
                    name = b.Title,
                    url = "/Blog/Detail/" + b.Id
                })
                .ToListAsync();

            return Json(new { products, categories, blogs });
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
