using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Nadixa.Web.Controllers
{
    public class Language : Controller
    {
        public IActionResult Index(string culture)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                );

            string returnURL = Request.Headers.Referer.ToString();
            return Redirect(returnURL);
        }
    }
}
