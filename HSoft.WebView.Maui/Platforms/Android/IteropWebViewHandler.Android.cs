using HSoft.WebView.Maui.Platforms.Android;
using Microsoft.Maui.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Android.Views.ViewGroup;
using AWebView = Android.Webkit.WebView;
namespace HSoft.WebView.Maui
{
    public partial class InteropWebViewHandler : ViewHandler<IInteropWebView, AWebView>
    {
        public string StartUri { get;private set; }
        WebKitWebViewClient _webKitWebViewClient;
      
        protected override AWebView CreatePlatformView()
        {
            var webview = new BlazorAndroidWebView(Context)
            {
                LayoutParameters = new Android.Widget.AbsoluteLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent, 0, 0)
            };
         
            AWebView.SetWebContentsDebuggingEnabled(enabled: true);
            webview.Settings.SetSupportMultipleWindows(true);
            webview.Settings.JavaScriptEnabled = true;
            webview.Settings.DomStorageEnabled = true;
                   _webKitWebViewClient = new( this);
            webview.SetWebViewClient(_webKitWebViewClient);
            return webview;
        }

        public void RunBlazorStartupScripts(AWebView view)
        {
            (WebViewJSRuntime as WebKitJSRuntime).RunBlazorStartupScripts(view);
        }
        private void StartWebViewCoreIfPossible()
        {
            WebViewJSRuntime = new WebKitJSRuntime(this);
            ((InteropWebView)VirtualView).RaiseInitializedEvent(WebViewJSRuntime);
            Uri uri = new Uri(WebKitWebViewClient.AppOriginUri, VirtualView.StartPath ?? "/"!);
            StartUri = uri.AbsoluteUri;
            PlatformView.LoadUrl(uri.AbsoluteUri);
        }

    }
}
