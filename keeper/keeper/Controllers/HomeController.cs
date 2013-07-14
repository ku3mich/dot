using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Ansi2Html;
using Dot.Processor;
using Proc.Runner;

namespace keeper.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProcessRunner runner;
        private readonly IAnsi2HtmlConverter cvt;
        private readonly IDotProcessor dot;

        public HomeController(IProcessRunner rnr, IAnsi2HtmlConverter c, IDotProcessor dotprc)
        {
            runner = rnr;
            cvt = c;
            dot = dotprc;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Dir()
        {
            runner.WorkingDirectory = Server.MapPath("~/");
            var str = runner.Run("cmd", "/c", "ls -h --color").StdOut.AsString();
            var html = cvt.Convert(str);

            return View("Raw", html as object);
        }

        public ActionResult GitLog()
        {
            runner.WorkingDirectory = @"D:\test\dot\viz.js\";
            var str = runner
                .Run("git", "--no-pager log --color=always -p --ignore-all-space --graph --word-diff")
                .StdOut
                .AsString();

            var html = cvt.Convert(str);

            return View("Raw", html as object);
        }

        public ActionResult HistoryGraph()
        {
            runner.WorkingDirectory = Server.MapPath("~/") + "../../viz.js";

            StringBuilder html = new StringBuilder();
            // 0                1             2     3
            // hash/parents(space separated)/subj/label
            var res = runner.Run("git", "--no-pager log --pretty=format:\"%H%x09%P%x09%s%x09%d\" --color=never");

            if (res.ExitCode == 0)
            {
                var lines = res.StdOut.AsString().Split('\n').ToArray();
                StringBuilder dotsrc = new StringBuilder();
                dotsrc.AppendLine("digraph gr{ node[shape=box];");
                foreach (var line in lines)
                {
                    var fields = line.Split('\t');
                    dotsrc.AppendFormat("\"{0}\"[label=\"{1}\\n{2}\"];\n", fields[0], fields[2], fields[3]);
                    var prnts = fields[1].Split(' ').ToArray();
                    foreach (var prn in prnts.Where(s => !string.IsNullOrWhiteSpace(s)))
                    {
                        dotsrc.AppendFormat("\"{0}\"->\"{1}\";\n", prn, fields[0]);
                    }
                }
                dotsrc.AppendLine("}");
                html.AppendLine(
                        dot
                            .Generate(dotsrc.ToString(), LayoutEngine.dot, OutputFormat.svg)
                            .AsString()
                    );
                html.AppendLine("<pre>");
                html.AppendLine(dotsrc.ToString());
                html.AppendLine("</pre>");
                html.AppendLine("<pre>");
                html.AppendLine(res.StdOut.AsString());
                html.AppendLine("</pre>");
            }
            else
            {
                html.AppendFormat("<pre>Error({0}):{1}</pre>", res.ExitCode, res.StdError.AsString());
            }

            return View("Raw", html.ToString() as object);
        }

        public ActionResult VisualizeGraph()
        {
            var f = Directory
                .GetFiles(Server.MapPath("~/App_Data/dots"))
                .Select(Path.GetFileName)
                .ToArray();

            return View(f);
        }
    }
}
