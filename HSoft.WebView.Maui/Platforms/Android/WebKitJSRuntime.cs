using Android.Net;
using HSoft.JSRuntimeBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AWebView = Android.Webkit.WebView;
using AUri = Android.Net.Uri;
using Android.Webkit;
using HSoft.WebView.Maui.Platforms.Android;
using static Android.Webkit.WebMessagePort;
namespace HSoft.WebView.Maui
{



    public class WebKitJSRuntime : WebViewJSRuntime
    {
        private readonly AWebView _webView;
        private InteropWebViewHandler _handler;
        private WebMessagePort _webMessagePort;
        private class JavaScriptValueCallback : Java.Lang.Object, IValueCallback
        {
            private readonly Action<Java.Lang.Object?> _callback;

            public JavaScriptValueCallback(Action<Java.Lang.Object?> callback)
            {
                ArgumentNullException.ThrowIfNull(callback);
                _callback = callback;
            }

            public void OnReceiveValue(Java.Lang.Object? value)
            {
                _callback(value);
            }
        }
        private class WebMessageCallback : WebMessagePort.WebMessageCallback
        {
            private readonly Action<string?> _onMessageReceived;

            public WebMessageCallback(Action<string?> onMessageReceived)
            {
                _onMessageReceived = onMessageReceived ?? throw new ArgumentNullException(nameof(onMessageReceived));
            }

            public override void OnMessage(WebMessagePort? port, WebMessage? message)
            {
                if (message is null)
                {
                    throw new ArgumentNullException(nameof(message));
                }

                _onMessageReceived(message.Data);
            }
        }

        public WebKitJSRuntime(InteropWebViewHandler handler)
        {
            _webView = handler.PlatformView;
            _handler = handler;
        }
        public void RunBlazorStartupScripts(AWebView view)
        {
            view.EvaluateJavascript(@"
            (function(){
            if(window.external.started == true) return true;
            window.external.started = true;
            var port;

            window.addEventListener('message', function (event) {
                if (event.data == 'capturePort') {
                    if (event.ports[0] != null) {
                        port = event.ports[0];
                        port.start();
                        port.addEventListener('message',event => {
                            window.external.onReceiveMessage(event.data);
                        })
                        window.external.sendMessage = (message) => {
                            port.postMessage(message);
                        }
                    }
                }
            }, false);
            return false;

})();

", new JavaScriptValueCallback((blazorStarted) => {
                if(blazorStarted.ToString() == "true")
                {
                    return;
                }
                SetUpMessageChannel();
                if (StaticContentProvider.TryGetFrameworkFile("assets/webview.umd.js", out var stream, out var contentType))
                {
                    StreamReader streamReader = new StreamReader(stream);
                    var script = streamReader.ReadToEnd();
                    view.EvaluateJavascript(script, new JavaScriptValueCallback(res =>
                    {

                    }));
                }
            }));
        }
        internal void SetUpMessageChannel()
        {
            // These ports will be closed automatically when the webview gets disposed.

            //创建一个通道，返回两个端口，第一个端口用来发送和接受来自JS的数据，第二个端口传给前端
            //前端用这个端口接受数据或者发送数据
            var nativeToJSPorts = _webView.CreateWebMessageChannel();

            var nativeToJs = new WebMessageCallback(message =>
            {
                OnWebMessageReceived(message!);
                
            });
            
            _webMessagePort = nativeToJSPorts[0];
            var destPort = new[] { nativeToJSPorts[1] };
            _webMessagePort.SetWebMessageCallback(nativeToJs);
           
            //这个用来向前端传递端口
            _webView.PostWebMessage(new WebMessage("capturePort", destPort),AUri.Parse(_handler.StartUri));
        }


        protected override void PostWebMessageAsString(string message)
        {
            _webMessagePort.PostMessage(new WebMessage(message));
        }
    }
}
