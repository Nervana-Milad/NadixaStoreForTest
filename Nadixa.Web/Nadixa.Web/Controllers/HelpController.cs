using Microsoft.AspNetCore.Mvc;

namespace Nadixa.Web.Controllers
{
    public class HelpController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
