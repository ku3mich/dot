using System;
using System.IO;
using System.Text;
using Proc.Runner;
using StringUtils;

namespace Dot.Processor
{
    public class DotProcessor : IDotProcessor
    {
        private const string dotExe = "dot.exe";
        private readonly IProcessRunner processRunner;

        public DotProcessor(IProcessRunner processRunner)
        {
            this.processRunner = processRunner;
        }

        public Stream Generate(string dotSource, LayoutEngine engine, OutputFormat fmt)
        {
            var res = processRunner.Run(dotExe, new MemoryStream(Encoding.ASCII.GetBytes(dotSource)), engine.AsCmdLineParam(), fmt.AsCmdLineParam());
            if (res.ExitCode!=0)
                throw new Exception(
                    string.Format("Process [{4}] finished with non-zero code:[{0}] \nwhile generating data format: [{1}] using layout engine: [{2}]\nsrc={3}",
                        res.ExitCode, fmt, engine, dotSource.Shorter(), dotExe));
            
            return res.StdOut;
        }
    }
}
