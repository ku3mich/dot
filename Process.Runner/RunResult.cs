using System.IO;

namespace Proc.Runner
{
    public class RunResult
    {
        public int ExitCode { get; protected set; }
        public MemoryStream StdOut { get; protected set; }
        public MemoryStream StdError { get; protected set; }
        
        /// <summary>
        /// Time ran in ms
        /// </summary>
        public long TimeRan { get; protected set; }

        public RunResult(int code, MemoryStream o, MemoryStream e, long timeRan)
        {
            ExitCode = code;
            StdOut = o;
            StdError = e;
            TimeRan = timeRan;
        }
    }
}
