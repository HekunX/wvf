using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSoft.WebView.NetFramework.WPF
{
    public class WebView:Microsoft.Web.WebView2.Wpf.WebView2
    {
        public WebViewJSRuntime JSRuntime { get; private set; }
        
        public async Task AttatchObjectToWindowAsync<T>(string objectName,T o) where T:class
        {
            if (!JSRuntime.InitalizedTaskSource.Task.IsCompleted)
            {
                await JSRuntime.InitalizedTaskSource.Task;
            }
            await JSRuntime.InvokeVoidAsync("_registerInstance", objectName, DotNetObjectReference.Create(o));

        }
        public WebView()
        {
            JSRuntime = new WebViewJSRuntime(this, Dispatcher);
        }
    }
}
