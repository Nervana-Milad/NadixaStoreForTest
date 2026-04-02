using Microsoft.AspNetCore.Mvc.Rendering;
using System.Runtime.CompilerServices;

namespace Nadixa.Web.Helpers
{
    public static class NavHelper
    {
        public static string IsActive(this IHtmlHelper html, string controller = null, string action = null)
        {
            var routeData = html.ViewContext.RouteData;

            var routeAction = routeData.Values["action"]?.ToString();
            var routeController = routeData.Values["controller"]?.ToString();

            var controllerMatch = controller == null || controller == routeController;
            var actionMatch = action == null || action == routeAction;

            return controllerMatch && actionMatch ? "active-menu" : "";
        }
    }
}
