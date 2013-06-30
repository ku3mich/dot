using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public ActionResult Index(string id, string l)
        {
            string src = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/dots/" + id));

            var eng = l == null ? LayoutEngine.dot : (LayoutEngine)Enum.Parse(typeof(LayoutEngine), l);

            var d = dot.Generate(src, eng, OutputFormat.svg);
            StreamWriter w = new StreamWriter(d);
            w.WriteLine("<pre>");
            w.WriteLine(src);
            w.WriteLine("</pre>");
            w.Flush();
            return View(d.AsString() as object);
        }

    }
}
