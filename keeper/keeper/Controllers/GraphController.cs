using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Dot.Processor;
using Proc.Runner;

namespace keeper.Controllers
{
    public class GraphController : Controller
    {
        private readonly IDotProcessor dot;

        public GraphController(IDotProcessor d)
        {
            dot = d;
        }

        // TODO: sanitize filename
        public ActionResult Render(string file, string l)
        {
            var src = readSrc(file);

            using (var ms = new MemoryStream())
            {
                var svgtext = GetGraphSvgStrem(src, l).AsString();

                StreamWriter w = new StreamWriter(ms);
                w.WriteLine(svgtext);
                w.WriteLine("<p><a href=\"{0}\">Download {1} in SVG format</a></p>", Url.Action("SvgDownload", new { file, l }), file);
                w.WriteLine("<pre class=\"dotsrc\">{0}</pre>", HttpUtility.HtmlEncode(src));
                w.Flush();

                return View(w.BaseStream.AsString() as object);
            }
        }

        public ContentResult Svg(string file, string l)
        {
            return new ContentResult
                {
                    ContentType = "image/svg+xml",
                    ContentEncoding = Encoding.ASCII,
                    Content = GetGraphSvgStrem(readSrc(file), l)
                        .AsString()
                };
        }

        public ActionResult SvgDownload(string file, string l)
        {
            var f = File(GetGraphSvgStrem(readSrc(file), l), "image/svg+xml");
            f.FileDownloadName = string.Format("{0}.svg", Path.GetFileNameWithoutExtension(file));
            return f;
        }

        private string readSrc(string fileName)
        {
            return System.IO.File.ReadAllText(Server.MapPath("~/App_Data/dots/" + fileName));
        }

        private Stream GetGraphSvgStrem(string dotSrc, string l)
        {
            LayoutEngine eng = l.Parse();
            return dot.Generate(dotSrc, eng, OutputFormat.svg);
        }
    }
}
