using System;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline.html;

namespace PdfRenderer.Processor
{
    public class CustomImageHTMLTagProcessor : AbstractTagProcessor
    {
        public override IList<IElement> End(IWorkerContext ctx, Tag tag, IList<IElement> currentContent)
        {
            IDictionary<string, string> attrs = tag.Attributes;
            iTextSharp.text.Image image;
            var src = attrs["src"];
            if (src.StartsWith("data:image/"))
            {
                var base64Data = src.Substring(src.IndexOf(",") + 1);
                var imagedata = Convert.FromBase64String(base64Data);
                image = iTextSharp.text.Image.GetInstance(imagedata);
            }
            else
            {
                image = iTextSharp.text.Image.GetInstance(src);
            }

            if (image == null)
            {
                throw new RuntimeWorkerException("No resource with the name: src");
            }
            HtmlPipelineContext htmlPipelineContext = this.GetHtmlPipelineContext(ctx);
            return new List<IElement>(1)
            {
                this.GetCssAppliers().Apply(
                    new Chunk((iTextSharp.text.Image)this.GetCssAppliers().Apply(image, tag, htmlPipelineContext), 0f, 0f, true),
                    tag,
                    htmlPipelineContext)
            };
        }
    }
}
