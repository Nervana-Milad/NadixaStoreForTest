using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nadixa.Infrastructure.Data;

namespace Nadixa.Web.ViewComponents
{
    public class FooterCategoriesViewComponent : ViewComponent
    {
        private readonly NadixaDbContext _context;

        public FooterCategoriesViewComponent(NadixaDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _context.ProductCategories.ToListAsync();
            return View(categories);
        }
    }
}
