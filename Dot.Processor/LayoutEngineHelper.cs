namespace Dot.Processor
{
    public static class LayoutEngineHelper
    {
        public static string AsCmdLineParam(this LayoutEngine engine)
        {
            return string.Format("-K{0}", engine.ToString().ToLower());
        }
    }
}
