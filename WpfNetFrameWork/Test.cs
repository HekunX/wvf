
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace WpfNetFrameWork
{
    public static class Test
    {
        [JSInvokable]
        public static Task<float> Hello(float value)
        {
            return Task.FromResult(23.2f);
        }
    }
}
 