using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml.pipeline.html;
using PdfRenderer.Processor;

namespace PdfRenderer.Renderer
{
    public class HtmlToPdfRenderer
    {
        public static byte[] Render(string html, List<string> cssFiles = null, Rectangle pageSize = null)
        {
            if (pageSize == null)
            {
                pageSize = PageSize.A4.Rotate();
            }
            using (var stream = new MemoryStream())
            {
                // create an iTextSharp Document which is an abstraction of a PDF but **NOT** a PDF  
                using (var document = new Document(pageSize))
                {
                    // create a writer that's bound to our PDF abstraction and our stream  
                    using (var writer = PdfWriter.GetInstance(document, stream))
                    {
                        // open the document for writing  
                        document.Open();
                        HtmlPipelineContext htmlContext = new HtmlPipelineContext(null);
                        ITagProcessorFactory factory = Tags.GetHtmlTagProcessorFactory();
                        factory.AddProcessor(new CustomImageHTMLTagProcessor(), new string[] { "img" });
                        htmlContext.SetTagFactory(factory);

                        var isAnyCssFiles = cssFiles != null && cssFiles.Count > 0;
                        //create a cssresolver to apply css
                        ICSSResolver cssResolver = XMLWorkerHelper.GetInstance().GetDefaultCssResolver(!isAnyCssFiles);
                        if (isAnyCssFiles)
                        {
                            foreach (var cssfile in cssFiles)
                            {
                                if (cssfile.StartsWith("http"))
                                    cssResolver.AddCssFile(cssfile, true);
                                else
                                    cssResolver.AddCssFile(System.Web.HttpContext.Current.Server.MapPath(cssfile), true);
                            }
                        }
                        //create and attach pipeline 
                        IPipeline pipeline = new CssResolverPipeline(cssResolver, new HtmlPipeline(htmlContext, new PdfWriterPipeline(document, writer)));

                        XMLWorker worker = new XMLWorker(pipeline, true);
                        XMLParser xmlParser = new XMLParser(true, worker);
                        using (var srHtml = new StringReader(html))
                        {
                            xmlParser.Parse(srHtml);
                        }

                        // close document  
                        document.Close();
                    }
                }

                // get bytes from stream  
                byte[] bytes = stream.ToArray();
                bytes = AddPageNumbers(bytes, pageSize);
                // success  
                return bytes;
            }
        }

        private static byte[] AddPageNumbers(byte[] pdf, Rectangle docRectangle)
        {
            using (var ms = new MemoryStream())
            {
                // we create a reader for a certain document
                using (PdfReader reader = new PdfReader(pdf))
                {
                    // we retrieve the total number of pages
                    int n = reader.NumberOfPages;
                    // step 1: creation of a document-object
                    using (Document document = new Document(docRectangle))
                    // step 2: we create a writer that listens to the document
                    using (var writer = PdfWriter.GetInstance(document, ms))
                    {
                        // step 3: we open the document
                        document.Open();
                        // step 4: we add content
                        PdfContentByte cb = writer.DirectContent;

                        int p = 0;
                        int rotation;
                        for (int page = 1; page <= reader.NumberOfPages; page++)
                        {
                            document.NewPage();
                            p++;

                            PdfImportedPage importedPage = writer.GetImportedPage(reader, page);
                            rotation = reader.GetPageRotation(page);
                            if (rotation.Equals(90) || rotation.Equals(270))
                                cb.AddTemplate(importedPage, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(page).Height);
                            else
                                cb.AddTemplate(importedPage, 1f, 0, 0, 1f, 0, 0);

                            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                            cb.BeginText();
                            cb.SetFontAndSize(bf, 10);
                            cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, $"Page {+p} sur {n}", (reader.GetPageSizeWithRotation(page).Width / 2) - 30, 15, 0);
                            cb.EndText();
                        }
                        document.Close();
                    }
                }
                return ms.ToArray();
            }
        }
    }
}
