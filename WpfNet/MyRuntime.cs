using HSoft.JSRuntimeBase;
using Microsoft.Extensions.FileProviders;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using WebView2 = Microsoft.Web.WebView2.Wpf.WebView2;

namespace WpfNet
{
    public class MyRuntime : WebViewJSRuntime
    {
        private readonly Dispatcher _dispatcher;
        private readonly WebView2 _webView2;
        private readonly StaticContentProvider _staticContentProvider;

        public MyRuntime(WebView2 webView2,Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _webView2 = webView2;
            _webView2.WebMessageReceived += _webView2_WebMessageReceived;
            _staticContentProvider = new StaticContentProvider(new EmbeddedFileProvider(Assembly.GetEntryAssembly()),new Uri("http://0.0.0.0"), "wwwroot");
            _webView2.CoreWebView2InitializationCompleted += _webView2_CoreWebView2InitializationCompleted;
        }
        public static string RemovePossibleQueryString(string? url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }
            var indexOfQueryString = url.IndexOf("?", StringComparison.Ordinal);
            return (indexOfQueryString == -1)
                ? url
                : url.Substring(0, indexOfQueryString);
        }

        private protected static string GetHeaderString(IDictionary<string, string> headers) =>
string.Join(Environment.NewLine, headers.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        private void CoreWebView2_WebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            var uri = RemovePossibleQueryString(e.Request.Uri);

            if (_staticContentProvider.TryGetResponseContent(uri, true, out var statusCode, out var statusMessage, out var stream, out var headers))
            {
                e.Response = _webView2.CoreWebView2.Environment.CreateWebResourceResponse(new AutoCloseOnReadCompleteStream(stream), statusCode, statusMessage, GetHeaderString(headers));
            }
        }

        private async void _webView2_CoreWebView2InitializationCompleted(object? sender,CoreWebView2InitializationCompletedEventArgs e)
        {
            var coreWebView2 = _webView2.CoreWebView2;
            coreWebView2.AddWebResourceRequestedFilter("http://0.0.0.0/*", CoreWebView2WebResourceContext.All);
            coreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;

            coreWebView2.Settings.IsStatusBarEnabled = false;
            coreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            coreWebView2.Settings.IsSwipeNavigationEnabled = false;
            if (StaticContentProvider.TryGetFrameworkFile("assets/webview.umd.js", out var stream, out var contentType))
            {
                using StreamReader sr = new StreamReader(stream);
                var script = sr.ReadToEnd();
                await coreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(script);
            }
            else
            {
                throw new Exception("资源框架文件未找到！");
            }
        }

        private void _webView2_WebMessageReceived(object? sender,CoreWebView2WebMessageReceivedEventArgs e)
        {
            this.OnWebMessageReceived(e.WebMessageAsJson);
        }

        protected override void PostWebMessageAsString(string message)
        {
            _webView2.Dispatcher.InvokeAsync(() => { _webView2.CoreWebView2.PostWebMessageAsString(message); });
        }
    }
}
