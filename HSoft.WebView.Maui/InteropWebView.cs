using HSoft.JSRuntimeBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSoft.WebView.Maui
{
    public class InteropWebView : View, IInteropWebView
    {
        public string? HostPage { get; set; }
        public string? StartPath { get; set; }
        public bool UseHttps { get; set; }
        public WebViewJSRuntime JSRuntime
        {
            get
            {
                return ((InteropWebViewHandler)Handler).WebViewJSRuntime;
            }
        }


        public event Action<WebViewJSRuntime> WebViewJSRuntimeInitialized;

        internal void RaiseInitializedEvent(WebViewJSRuntime js)
        {
            WebViewJSRuntimeInitialized?.Invoke(js);
        }
    }
}
