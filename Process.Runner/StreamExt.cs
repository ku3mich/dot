using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Proc.Runner
{
    public static class StreamExt
    {
        public static string AsString(this Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            var rdr = new StreamReader(stream);
            return rdr.ReadToEnd();
        }
    }
}
