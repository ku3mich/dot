using System.IO;

namespace Dot.Processor
{
    public interface IDotProcessor
    {
        Stream Generate(string dotSource, LayoutEngine engine, OutputFormat fmt);
    }
}
