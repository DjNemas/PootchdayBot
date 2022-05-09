using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PootchdayBot.Tools
{
    public class FormatString
    {
        public static string HandleDiscordSpecialChar(string str)
        {
            str = str.Replace("_", "\\_");
            str = str.Replace("*", "\\*");
            str = str.Replace("~", "\\~");
            str = str.Replace("|", "\\|");
            str = str.Replace("`", "\\`");
            return str;
        }
    }
}
