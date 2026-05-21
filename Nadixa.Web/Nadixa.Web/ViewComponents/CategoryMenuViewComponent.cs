using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nadixa.Infrastructure.Data;

namespace Nadixa.Web.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly NadixaDbContext _context;

        public CategoryMenuViewComponent(NadixaDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _context.ProductCategories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }
    }
}
