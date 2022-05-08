using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PootchdayBot.Tools
{
    public static class ValidChecker
    {
        public static ulong IsZero(ulong value)
        {
            if (value == null)
                throw new ArgumentNullException("Value can't be Null");
            if (value == 0)
                throw new Exception("Value can't be Zero");
            else
                return value;
        }

        public static string IsEmpty(string value)
        {
            if (value == null)
                throw new ArgumentNullException("Value can't be Null");
            if (value == "")
                throw new Exception("Value can't be Empty");
            else
                return value;
        }

        public static DateTime IsNull(DateTime value)
        {
            if (value == null)
                throw new ArgumentNullException("Value can't be Null");
            else
                return value;
        }
    }
}
