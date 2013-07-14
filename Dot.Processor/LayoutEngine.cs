namespace Dot.Processor
{
    public enum LayoutEngine
    {
        dot,
        fdp,
        sfdp,
        circo,
        neato,
        twopi,
        osage
    }

    public static class LayoutEngineExt
    {
        public static LayoutEngine Parse(this string val)
        {
            return string.IsNullOrWhiteSpace(val) ? LayoutEngine.dot : (LayoutEngine)System.Enum.Parse(typeof(LayoutEngine), val);
        }
        
        public static LayoutEngine TryParse(this string val)
        {
            try
            {
                return Parse(val);
            }
            catch
            {
                return LayoutEngine.dot;
            }
        }
    }
}
