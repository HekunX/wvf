using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfNet
{
    public static class Call
    {
        [JSInvokable]
        public static Task<Stream> Test(byte[] bytes)
        {
            Stream stream = new MemoryStream(new byte[] { 1, 2, 3 });
            return Task.FromResult(stream);
        }
    }
}
