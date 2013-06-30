using System;
using System.IO;

namespace caha
{
    class Program
    {
        static void Main(string[] args)
        {
            string inp;

            if (args.Length != 1)
            {
                Console.WriteLine("usage: caha.exe filename");
                return;
            }

            using (var f = new FileStream(args[0], FileMode.Open))
            using (var r = new StreamReader(f))
            {
                inp = r.ReadToEnd();
            }

            var g = new Ansi2Html.Ansi2HtmlConverter();
            var outp = g.Convert(inp);
            Console.WriteLine(outp);
        }
    }
}
