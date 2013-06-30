namespace Dot.Processor
{
    public static class OutputFormatHelper
    {
        public static string AsCmdLineParam(this OutputFormat engine)
        {
            return string.Format("-T{0}", engine.ToString().ToLower());
        }
    }
}
