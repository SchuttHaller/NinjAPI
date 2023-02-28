using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Helpers
{
    internal static class StringExtensions
    {
        internal static bool ContainsNoCase(this ICollection<string> strings, string item)
        {
            return strings.Contains(item, StringComparer.OrdinalIgnoreCase);
        }
    }
}
