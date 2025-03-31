using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSoft.WebView.Maui
{
    public static class Extensions
    {
        public static MauiAppBuilder RegisterMauiWebView(this MauiAppBuilder appHostBuilder)
        {
            appHostBuilder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<IInteropWebView,InteropWebViewHandler>();
            });
            return appHostBuilder;
        }
    }
}
