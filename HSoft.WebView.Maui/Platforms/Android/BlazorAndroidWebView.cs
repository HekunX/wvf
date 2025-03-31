using Android.Content;
using Android.Views;
using AWebView = Android.Webkit.WebView;

namespace HSoft.WebView.Maui.Platforms.Android
{
    internal class BlazorAndroidWebView : AWebView
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BlazorAndroidWebView"/>
        /// </summary>
        /// <param name="context">The <see cref="Context"/>.</param>
        public BlazorAndroidWebView(Context context) : base(context)
        {
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent? e)
        {
            if (keyCode == Keycode.Back && CanGoBack() && e?.RepeatCount == 0)
            {
                GoBack();
                return true;
            }
            return false;
        }
    }
}
