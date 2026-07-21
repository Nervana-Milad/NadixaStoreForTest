//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using Nadixa.Core.DTOS;
//using Nadixa.Core.Entities;
//using Nadixa.Core.Interfaces;
//using Nadixa.Infrastructure.Data;
//using Nadixa.Web.Models;
//using Nadixa.Web.Models.ViewModels;
//using System;
//using System.Diagnostics;
//using System.Reflection.Metadata.Ecma335;

//namespace Nadixa.Web.Controllers
//{
//    public class HomeController : Controller
//    {

//        private readonly ILogger<HomeController> _logger;
//        private readonly NadixaDbContext _context;
//        private readonly UserManager<AppUser> _userManager;
//        private readonly IPromotionService _promotionService;

//        public HomeController(ILogger<HomeController> logger, NadixaDbContext context, UserManager<AppUser> userManager, IPromotionService promotionService)

//        {
//            _logger = logger;
//            _context = context;
//            _userManager = userManager;
//            _promotionService = promotionService;


//        }
//        public async Task<IActionResult> Index(int? categoryId)
//        {
//            var user = await _userManager.GetUserAsync(User);
//            var productQuery = _context.Products.Include(p => p.ProductCategory).AsQueryable();

//            if (categoryId.HasValue)
//            {
//                productQuery = productQuery.Where(p => p.ProductCategoryId == categoryId);
//            }

//            var products = await productQuery.ToListAsync();
//            ViewBag.Categories = await _context.ProductCategories.ToListAsync();

//            Dictionary<int, int> cartItems = new();
//            if (user != null)
//            {
//                cartItems = await _context.Carts
//                    .Where(c => c.UserId == user.Id)
//                    .SelectMany(c => c.Items)
//                    .ToDictionaryAsync(i => i.ProductId, i => i.Quantity);
//            }
//            ViewBag.CartItems = cartItems;

//            var notifyRequestedProductIds = new HashSet<int>();
//            if (user != null)
//            {
//                notifyRequestedProductIds = (await _context.StockNotificationRequests
//                    .Where(r => r.UserId == user.Id && !r.IsNotified)
//                    .Select(r => r.ProductId)
//                    .ToListAsync())
//                    .ToHashSet();
//            }
//            ViewBag.NotifyRequestedProductIds = notifyRequestedProductIds;

//            var bestSellers = await _context.OrderItems
//                 .Where(oi => oi.Order.Status != OrderStatus.Cancelled)
//                 .Include(oi => oi.Product)
//                    .ThenInclude(p => p.ProductCategory)
//                 .GroupBy(oi => oi.ProductId)
//                 .OrderByDescending(g => g.Sum(oi => oi.Quantity))
//                 .Take(8)
//                 .Select(g => g.First().Product)
//                 .ToListAsync();

//            ViewBag.BestSellers = bestSellers;

//            var activePromotions = await _promotionService.GetActivePromotionsAsync();
//            ViewBag.ActivePromotions = activePromotions;

//            var productsForPromoCheck = products
//                .Concat(bestSellers)
//                .GroupBy(p => p.Id)
//                .Select(g => g.First());

//            var productPromotions = new Dictionary<int, ProductPromoInfo>();

//            foreach (var product in productsForPromoCheck)
//            {
//                var promo = activePromotions
//                    .Where(p =>
//                        !p.IsFirstPurchaseOnly &&
//                        (p.Scope == PromotionScope.AllProducts ||
//                         (p.Scope == PromotionScope.Category && p.ProductCategoryId == product.ProductCategoryId) ||
//                         (p.Scope == PromotionScope.SubCategory && p.ProductSubCategoryId == product.ProductSubCategoryId) ||
//                         (p.Scope == PromotionScope.SpecificProduct && p.ProductId == product.Id)))
//                    .OrderByDescending(p => p.Priority)
//                    .FirstOrDefault();

//                if (promo == null) continue;

//                productPromotions[product.Id] = new ProductPromoInfo
//                {
//                    BadgeText = promo.BadgeText,
//                    BadgeColorHex = promo.BadgeColorHex,
//                    DiscountedPrice = promo.Type == PromotionType.BuyXGetYFree
//                        ? null
//                        : _promotionService.CalculateDiscountedPrice(product.Price, promo)
//                };
//            }

//            ViewBag.ProductPromotions = productPromotions;

//            return View(products);

//        }


//        [HttpGet]
//        public async Task<IActionResult> GlobalSearch(string term)
//        {
//            if (string.IsNullOrWhiteSpace(term))
//                return Json(new { products = new List<object>(), categories = new List<object>(), blogs = new List<object>() });

//            var products = await _context.Products
//                .Where(p => p.Name.Contains(term) || p.Description.Contains(term))
//                .Take(5)
//                .Select(p => new
//                {
//                    id = p.Id,
//                    name = p.Name,
//                    price = p.Price,
//                    imageUrl = p.MainImageUrlPath,
//                    url = "/Product/Detail/" + p.Id
//                })
//                .ToListAsync();

//            var categories = await _context.ProductCategories
//                .Where(c => c.Name.Contains(term))
//                .Take(3)
//                .Select(c => new
//                {
//                    id = c.Id,
//                    name = c.Name,
//                    url = "/Product/Index?categoryId=" + c.Id
//                })
//                .ToListAsync();

//            var blogs = await _context.Blogs
//                .Where(b => b.Title.Contains(term) || b.Content.Contains(term))
//                .Take(3)
//                .Select(b => new
//                {
//                    id = b.Id,
//                    name = b.Title,
//                    url = "/Blog/Detail/" + b.Id
//                })
//                .ToListAsync();

//            return Json(new { products, categories, blogs });
//        }

//        public IActionResult About()
//        {
//            return View();
//        }

//        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//        public IActionResult Error()
//        {
//            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
//        }
//    }
//}


using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using Nadixa.Web.Models;
using System.Diagnostics;

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
            ViewBag.CartItems = data.CartItems;
            ViewBag.NotifyRequestedProductIds = data.NotifyRequestedProductIds;
            ViewBag.BestSellers = data.BestSellers;
            ViewBag.ProductPromotions = data.ProductPromotions;

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