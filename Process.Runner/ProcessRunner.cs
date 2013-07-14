using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ThreadState = System.Threading.ThreadState;

namespace Proc.Runner
{
    public class ProcessRunner : IProcessRunner
    {
        private readonly object _sync = new object();

        private const int bufSize = 4 * 1024;
        private void readToEnd(Stream s, Stream ms)
        {
            lock (_sync)
            {
                s.CopyTo(ms);
            }
        }

        public string WorkingDirectory { get; set; }

        protected long executionTimeout = 3 * 1000; // 3 sec
        public long ExeutionTimeout
        {
            get { return executionTimeout; }
            set { executionTimeout = value; }
        }

        public ProcessRunner()
        {
            WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
        }

        public ProcessRunner(string workingDirectory)
        {
            WorkingDirectory = workingDirectory;
        }

        public RunResult Run(string executableFileName, params string[] args)
        {
            return Run(executableFileName, null, args);
        }

        public RunResult Run(string executableFileName, Stream input, params string[] args)
        {
            using (Process p = new Process
            {
                StartInfo = new ProcessStartInfo(executableFileName, string.Join(" ", args))
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    WorkingDirectory = WorkingDirectory,

                },
                EnableRaisingEvents = true,
            })
            {
                var finished = false;
                p.Exited += (s, e) =>
                    {
                        lock (_sync)
                        {
                            finished = true;
                        }
                    };

                var ms = new MemoryStream();
                var err = new MemoryStream();

                p.Start();

                var w = new Stopwatch();
                w.Start();

                if (input != null)
                    input.CopyTo(p.StandardInput.BaseStream);

                p.StandardInput.BaseStream.WriteByte(26);  // 26 = ^Z = end of file
                p.StandardInput.BaseStream.WriteByte(13);  // 13 = ^J(\n) return 
                p.StandardInput.Flush();

                Thread rdr = new Thread(() =>
                {
                    try
                    {
                        while (true)
                        {
                            readToEnd(p.StandardOutput.BaseStream, ms);
                            readToEnd(p.StandardError.BaseStream, err);

                            if (finished)
                                break;
                        }
                    }
                    catch //dummy catch
                    {
                    }
                });
                rdr.Start();

                while (rdr.ThreadState == ThreadState.Running)
                {
                    Thread.Sleep(5);

                    if (w.ElapsedMilliseconds > ExeutionTimeout)
                    {
                        rdr.Abort();
                        break;
                    }
                }

                w.Stop();

                if (!finished)
                {
                    var wr = new StreamWriter(err);
                    wr.WriteLine("\nError : Execution timeout exceeded!");
                    wr.Flush();
                }

                ms.Seek(0, SeekOrigin.Begin);
                err.Seek(0, SeekOrigin.Begin);

                return new RunResult(p.ExitCode, ms, err, w.ElapsedMilliseconds);
            }
        }

    }
}
