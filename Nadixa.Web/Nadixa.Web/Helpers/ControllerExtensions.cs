using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nadixa.Web.Helpers
{
    public static class ControllerExtensions
    {
        public static async Task<string> RenderPartialViewToStringAsync<TModel>(
            this Controller controller, string viewName, TModel model)
        {
            controller.ViewData.Model = model;

            using var writer = new StringWriter();

            var viewEngine = controller.HttpContext.RequestServices
                .GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

            var viewResult = viewEngine!.FindView(controller.ControllerContext, viewName, false);

            if (viewResult.View == null)
                throw new InvalidOperationException($"View '{viewName}' not found.");

            var viewContext = new ViewContext(
                controller.ControllerContext,
                viewResult.View,
                controller.ViewData,
                controller.TempData,
                writer,
                new HtmlHelperOptions());

            await viewResult.View.RenderAsync(viewContext);

            return writer.GetStringBuilder().ToString();
        }
    }
}