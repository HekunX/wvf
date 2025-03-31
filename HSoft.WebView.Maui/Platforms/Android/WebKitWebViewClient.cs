using Android.Content;
using Android.Webkit;
using HSoft.JSRuntimeBase;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Android.Webkit.WebMessagePort;
using AWebView = Android.Webkit.WebView;
namespace HSoft.WebView.Maui.Platforms.Android
{

    public class WebKitWebViewClient:WebViewClient
    {
        public static  string AppOrigin = $"https://0.0.0.0/";

        public static  Uri AppOriginUri = new(AppOrigin);
        private readonly InteropWebViewHandler _interopWebViewHandler;
        private readonly StaticContentProvider _staticContentProvider;

        public WebKitWebViewClient(InteropWebViewHandler interopWebViewHandler)
        {
            AppOrigin = interopWebViewHandler.VirtualView.UseHttps ? "https://0.0.0.0/" : "http://0.0.0.0/";
            AppOriginUri = new Uri(AppOrigin);
            var contentRootDir = Path.GetDirectoryName(interopWebViewHandler.VirtualView.HostPage!) ?? string.Empty;
            var hostPageRelativePath = Path.GetRelativePath(contentRootDir, interopWebViewHandler.VirtualView.HostPage!);
            _interopWebViewHandler = interopWebViewHandler;
            _staticContentProvider = new StaticContentProvider(new AndroidMauiAssetFileProvider(interopWebViewHandler.Context.Assets, contentRootDir), WebKitWebViewClient.AppOriginUri, hostPageRelativePath);
        }


        
        public override void OnPageFinished(AWebView? view, string? url)
        {
            base.OnPageFinished(view, url);
            _interopWebViewHandler.RunBlazorStartupScripts(view);
        }
        public static string RemovePossibleQueryString(string? url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }
            var indexOfQueryString = url.IndexOf('?', StringComparison.Ordinal);
            return (indexOfQueryString == -1)
                ? url
                : url.Substring(0, indexOfQueryString);
        }
        public override WebResourceResponse? ShouldInterceptRequest(global::Android.Webkit.WebView? view, IWebResourceRequest? request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestUri = request?.Url?.ToString();
            var allowFallbackOnHostPage = AppOriginUri.IsBaseOfPage(requestUri);
            requestUri = RemovePossibleQueryString(requestUri);
            if (requestUri != null && AppOriginUri.IsBaseOf(new Uri(requestUri)) &&
                _staticContentProvider.TryGetResponseContent(requestUri, allowFallbackOnHostPage, out var statusCode, out var statusMessage, out var content, out var headers))
            {
                var contentType = headers["Content-Type"];
                return new WebResourceResponse(contentType, "UTF-8", statusCode, statusMessage, headers, content);
            }
            else
            {
  
            }
            return base.ShouldInterceptRequest(view, request);
        }

    }
}
