using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSoft.JSRuntimeBase
{
    public static class UriExtensions
    {
        public static bool IsBaseOfPage(this Uri baseUri, string? uriString)
        {
            if (Path.HasExtension(uriString))
            {
                // If the path ends in a file extension, it's not referring to a page.
                return false;
            }

            var uri = new Uri(uriString!);
            return baseUri.IsBaseOf(uri);
        }
    }
}
