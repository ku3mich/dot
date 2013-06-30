using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ansi2Html
{
    public interface IAnsi2HtmlConverter
    {
        string Convert(string ansiSource);
    }
}
