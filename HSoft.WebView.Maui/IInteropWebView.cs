using HSoft.JSRuntimeBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSoft.WebView.Maui
{
    public interface IInteropWebView : IView
    {
        string? HostPage { get; set; }
        string? StartPath { get; set; }
        bool UseHttps { get; set; } 
        WebViewJSRuntime JSRuntime { get; }
        event Action<WebViewJSRuntime> WebViewJSRuntimeInitialized;
    }
}
