using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Controll = Microsoft.Web.WebView2.Wpf.WebView2;
using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Windows.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;



namespace HSoft.WebView.NetFramework.WPF
{
    public class WebViewJSRuntime : JSRuntime
    {
        private readonly Controll _webView2;
        private readonly Dispatcher _dispatcher;
        private StaticContentProvider _staticContentProvider;
        internal TaskCompletionSource<object> InitalizedTaskSource = new TaskCompletionSource<object>();
        public WebViewJSRuntime(Controll webView2,Dispatcher dispatcher)
        {
            _webView2 = webView2;
            _dispatcher = dispatcher;
            _staticContentProvider = new StaticContentProvider(new Uri("http://0.0.0.0"), "wwwroot");
            
            _webView2.CoreWebView2InitializationCompleted += OnCoreWebView2InitializationCompleted;
            _webView2.WebMessageReceived += OnWebMessageReceived;
            _ = TryInitializeWebView2();
        }

        private static string? GetWebView2UserDataFolder()
        {
            if (Assembly.GetEntryAssembly() is { } mainAssembly)
            {
                // In case the application is running from a non-writable location (e.g., program files if you're not running
                // elevated), use our own convention of %LocalAppData%\YourApplicationName.WebView2.
                // We may be able to remove this if https://github.com/MicrosoftEdge/WebView2Feedback/issues/297 is fixed.
                var applicationName = mainAssembly.GetName().Name;
                var result = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    $"{applicationName}.WebView2");

                return result;
            }

            return null;
        }
        private async Task<bool> TryInitializeWebView2()
        {
            var env = await CoreWebView2Environment.CreateAsync(userDataFolder: GetWebView2UserDataFolder());
            await _webView2.EnsureCoreWebView2Async(env);
            return true;
        }
        private void OnWebMessageReceived(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            var json = JsonSerializer.Deserialize<string>(e.WebMessageAsJson);
            var objects = JsonSerializer.Deserialize<JsonElement[]>(json, JsonSerializerOptions);
            var messageType = objects[0].GetString();
            switch (messageType)
            {
                case "beginInvokeDotNetFromJS":
                    {
                        string callId = objects[1].GetString();
                        string assemblyName = objects[2].GetString();
                        string methodIdentifier = objects[3].GetString();
                        long dotNetObjectId = objects[4].GetInt64();
                        string argsJson = objects[5].GetString();
                        DotNetInvocationInfo dotNetInvocationInfo = new DotNetInvocationInfo(assemblyName, methodIdentifier, dotNetObjectId, callId);
                        DotNetDispatcher.BeginInvokeDotNet(this, dotNetInvocationInfo, argsJson);
                        break;
                    }
                case "endInvokeJSFromDotNet":
                    {
                        string callId = objects[1].GetString();
                        bool succeeded = objects[2].GetBoolean();
                        string resultOrError = objects[3].GetString();
                        DotNetDispatcher.EndInvokeJS(this, resultOrError);
                        break;
                    }
                case "initialized":
                    {
                        if(!InitalizedTaskSource.Task.IsCompleted) InitalizedTaskSource.SetResult(null);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

        }

        private async void OnCoreWebView2InitializationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            var coreWebView2 = _webView2.CoreWebView2;
            coreWebView2.AddWebResourceRequestedFilter("http://0.0.0.0/*",CoreWebView2WebResourceContext.All);
            coreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;

            coreWebView2.Settings.IsStatusBarEnabled = false;
            coreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            coreWebView2.Settings.IsSwipeNavigationEnabled = false;
            if(StaticContentProvider.TryGetFrameworkFile("assets/webview.umd.js", out var stream, out var contentType))
            {
                using StreamReader sr = new StreamReader(stream);
                var script = sr.ReadToEnd();
                var beforeScript = @"
window.chrome.webview.addEventListener('message', e => {

    window.external.onReceiveMessage(e.data);
});

window.external.sendMessage = (message) => {
    window.chrome.webview.postMessage(message);
};

";
                await coreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(beforeScript + script);
            }
            else
            {
                throw new Exception("资源框架文件未找到！");
            }
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
        private void CoreWebView2_WebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            var uri = RemovePossibleQueryString(e.Request.Uri);

            if(_staticContentProvider.TryGetResponseContent(uri, true, out var statusCode, out var statusMessage, out var stream, out var headers))
            {
               e.Response =   _webView2.CoreWebView2.Environment.CreateWebResourceResponse(new AutoCloseOnReadCompleteStream(stream), statusCode, statusMessage, GetHeaderString(headers));
            }
        }
        private protected static string GetHeaderString(IDictionary<string, string> headers) =>
    string.Join(Environment.NewLine, headers.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        protected override void BeginInvokeJS(long taskId, string identifier, string? argsJson, JSCallResultType resultType, long targetInstanceId)
        {
            var json = JsonSerializer.Serialize(new object[] { "BeginInvokeJS", taskId, identifier, argsJson, resultType, targetInstanceId }, JsonSerializerOptions);
            _dispatcher.InvokeAsync(() =>
            {
                _webView2.CoreWebView2.PostWebMessageAsString(json);
            });
        }

        protected override void EndInvokeDotNet(DotNetInvocationInfo invocationInfo, in DotNetInvocationResult invocationResult)
        {
            var resultJsonOrErrorMessage = invocationResult.Success
    ? invocationResult.ResultJson
    : invocationResult.Exception.ToString();
            var json = JsonSerializer.Serialize(new object[] { "EndInvokeDotNet", invocationInfo.CallId, invocationResult.Success, resultJsonOrErrorMessage }, JsonSerializerOptions);
            _dispatcher.InvokeAsync(() =>
            {
                _webView2.CoreWebView2.PostWebMessageAsString(json);
            });
        }
    }
}
