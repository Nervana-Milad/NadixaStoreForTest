using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nadixa.Infrastructure.Data;
using Nadixa.Web.Models;
using Nadixa.Web.Models.ViewModels;
using System;
using System.Diagnostics;

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

        public IActionResult Index()
        {
            var products = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Colors)
            .Where(p => !p.IsDeleted) // from BaseEntity
            .Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                Price = p.Price,
                OldPrice = p.OldPrice,
                IsFeatured = p.IsFeatured,


                // get first image safely
                // safe main image
                MainImageUrl = p.Images
            .Where(i => i.IsMain)
            .Select(i => i.ImageUrl)
            .FirstOrDefault() ?? "~/images/bags/no-image.png",
                ImageUrls = p.Images.Select(i => i.ImageUrl).ToList(),
                Colors = p.Colors.Select(c => new ColorViewModel
                {
                    Name = c.Name,
                    HexCode = c.HexCode
                }).ToList()
            })
            .ToList();


            return View(products); // always returns a model
        }
        
        public async Task<IActionResult> ProductDetail(int id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var product = _context.Products.Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Colors)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    CategoryName = p.Category.Name,
                    Description = p.Description,
                    Price = p.Price,
                    OldPrice = p.OldPrice,
                    MainImageUrl = p.MainImageUrl,
                    ImageUrls = p.Images.Select(i => i.ImageUrl).ToList()
                })
                .FirstOrDefault();

            if(product == null)
            {
                return NotFound();
            }
            return View(product);
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
