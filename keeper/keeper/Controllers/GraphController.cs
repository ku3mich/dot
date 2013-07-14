using System;
using System.IO;
using System.Text;
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

        public ActionResult Render(string file, string l)
        {
            var src = readSrc(file);

            StreamWriter w = new StreamWriter(GetGraphSvgStrem(src, l));
            w.WriteLine("<pre>");
            w.WriteLine(src);
            w.WriteLine("</pre>");
            w.Flush();

            return View(w.BaseStream.AsString() as object);
        }

        public ActionResult Svg(string file, string l)
        {
            return new ContentResult
                {
                    ContentType = "image/svg+xml",
                    ContentEncoding = Encoding.ASCII,
                    Content = GetGraphSvgStrem(readSrc(file), l).AsString()
                };
        }

        private string readSrc(string fileName)
        {
            return System.IO.File.ReadAllText(Server.MapPath("~/App_Data/dots/" + fileName));
        }

        private Stream GetGraphSvgStrem(string file, string l)
        {
            LayoutEngine eng = l.Parse();
            return dot.Generate(file, eng, OutputFormat.svg);
        }
    }
}
