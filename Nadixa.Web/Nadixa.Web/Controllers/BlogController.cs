using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nadixa.Infrastructure.Data;
using Nadixa.Web.Models.ViewModels;

namespace Nadixa.Web.Controllers
{
    public class BlogController : Controller
    {
        private readonly NadixaDbContext _context;

        public BlogController(NadixaDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int? categoryId)
        {

            var query = _context.Blogs.Include(b => b.BlogCategory).AsQueryable();

            if(categoryId.HasValue)
            {
                query = query.Where(b => b.BlogCategoryId == categoryId);
            }


            var blogs = await query
                .OrderByDescending(b => b.CreateAt)
                .Select(b => new BlogViewModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    ImageUrl = b.ImageUrl,
                    Author = "Admin",
                    Category = b.BlogCategory.Name,
                    ShortDescription = b.Content.Length > 100 ? b.Content.Substring(0, 100) : b.Content,
                    Date = b.CreateAt
                })
                .ToListAsync();
            ViewBag.Categories = await _context.BlogCategories.ToListAsync();
            return View(blogs);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id);
            if (blog == null)
            {
                return NotFound();
            }
            return View(blog);
        }
    }
}
