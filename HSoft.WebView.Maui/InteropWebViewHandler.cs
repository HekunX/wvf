using Android.Content.Res;
using HSoft.JSRuntimeBase;
using Microsoft.Maui.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AWebView = Android.Webkit.WebView;
namespace HSoft.WebView.Maui
{
    public partial class InteropWebViewHandler
    {
        private string? HostPage { get; set; }
        private string? StartPath { get; set; }
        public WebViewJSRuntime WebViewJSRuntime { get; private set; }
        public static PropertyMapper<IInteropWebView, InteropWebViewHandler> PropertyMapper = new(ViewHandler.ViewMapper)
        {
            [nameof(IInteropWebView.HostPage)] = MapHostPage,
            [nameof(IInteropWebView.StartPath)] = MapStartPath
        };
        public InteropWebViewHandler() : base(PropertyMapper)
        {
            
        }
        public InteropWebViewHandler(PropertyMapper mapper) : base(mapper ?? PropertyMapper)
        {
        }



        public static void MapHostPage(InteropWebViewHandler handler,IInteropWebView webView)
        {
            handler.HostPage = webView.HostPage;
            handler.StartWebViewCoreIfPossible();

        }
        public static void MapStartPath(InteropWebViewHandler handler, IInteropWebView webView)
        {
            handler.StartPath = webView.StartPath;
        }
    }
}
