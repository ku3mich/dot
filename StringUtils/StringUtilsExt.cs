using System.Linq;
using System.Text.RegularExpressions;

namespace StringUtils
{
    public static class StringUtilsExt
    {
        public static string Shorter(this string text, int head = 5, int tail = 5)
        {
            var lines = Regex.Replace(text, @"\r", "").Split('\n').ToArray();

            if (lines.Length < (head + tail + 1))
                return text;

            return string.Join("\n", 
                lines
                    .Take(head)
                    .Union(new [] { "... skipped ..." })
                    .Union(
                        lines.Skip(lines.Length - tail - 1)
                        .Take(tail)
                        ));
        }

    }
}
