using System.IO;

namespace Proc.Runner
{
    public interface IProcessRunner
    {
        string WorkingDirectory { get; set; }
        long ExeutionTimeout { get; set; }

        RunResult Run(string executableFileName, params string[] args);
        RunResult Run(string executableFileName, Stream input, params string[] args);
    }
}
