using iTextSharp.text;
using PdfRenderer.Helper;
using PdfRenderer.Renderer;
using System;
using System.Web;
using System.Web.Mvc;

namespace PdfRenderer.MvcActionResult
{
    public class PdfActionResult : ActionResult
    {
        protected string ViewName { get; set; }
        protected object Model { get; set; }
        protected string Filename { get; set; }
        protected Rectangle PageConfig { get; set; }
        protected bool IsAttachment { get; set; }

        /// <summary>
        /// Can be called like this in any controller action "return new PdfActionResult(viewPath, model, fileName,PageSize.A4.Rotate(),isAttachement: true);"
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <param name="filename"></param>
        /// <param name="pageConfig"></param>
        /// <param name="isAttachment"></param>
        public PdfActionResult(string viewName, object model, string filename, Rectangle pageConfig = null, bool isAttachment = false)
        {
            this.ViewName = viewName;
            this.Model = model;
            this.Filename = filename;
            this.PageConfig = pageConfig;
            this.IsAttachment = isAttachment;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            // get rendered HTML from view
            string html = ViewRenderer.RenderViewToString(context, ViewName, Model);
            var cssFiles = CssHelper.GetAllCssPaths(html);
            //html = ClearHtmlStrategy.Clear(html);
            // generate the PDF content from HTML
            var pageConfig = PageConfig != null ? PageConfig : PageSize.A4;
            byte[] content = HtmlToPdfRenderer.Render(html, cssFiles, pageConfig);
            var contentDisposition = IsAttachment ? "attachement" : "inline";
            HttpResponseBase response = context.HttpContext.Response;

            response.Clear();
            response.ClearContent();
            response.ClearHeaders();
            response.ContentType = "application/pdf";
            response.AppendHeader("Content-Disposition", $"{contentDisposition};filename={Filename}.pdf");
            response.AddHeader("Content-Length", content.Length.ToString());
            response.BinaryWrite(content);
            response.OutputStream.Flush();
            response.OutputStream.Close();
            response.End();
        }
    }
}