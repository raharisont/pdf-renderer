using System.IO;
using System.Web.Mvc;

namespace PdfRenderer.Renderer
{
    public class ViewRenderer
    {
        /// <summary>
        /// Get string html render from razor view engine
        /// </summary>
        /// <param name="context">from any controller use : this.ControllerContext</param>
        /// <param name="viewPath">view path</param>
        /// <param name="model">object model</param>
        /// <param name="partial">true or false</param>
        /// <returns></returns>
        public static string RenderViewToString(ControllerContext context,
            string viewPath,
            object model = null,
            bool partial = false)
        {
            // first find the ViewEngine for this view
            ViewEngineResult viewEngineResult = null;
            if (partial)
                viewEngineResult = ViewEngines.Engines.FindPartialView(context, viewPath);
            else
                viewEngineResult = ViewEngines.Engines.FindView(context, viewPath, null);

            if (viewEngineResult == null)
                throw new FileNotFoundException("View cannot be found.");

            // get the view and attach the model to view data
            var view = viewEngineResult.View;
            context.Controller.ViewData.Model = model;

            string result = null;

            using (var sw = new StringWriter())
            {
                var ctx = new ViewContext(context, view,
                    context.Controller.ViewData,
                    context.Controller.TempData,
                    sw);
                view.Render(ctx, sw);
                result = sw.ToString();
            }

            return result;
        }
    }
}
