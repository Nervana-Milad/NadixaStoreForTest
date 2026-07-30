using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.Interfaces
{

    /// <summary>
    /// Renders a Razor view to an HTML string — used for generating email templates.
    /// </summary>
    public interface IRazorViewRenderer
        {
            Task<string> RenderAsync<TModel>(string viewName, TModel model);
        }
    

}
