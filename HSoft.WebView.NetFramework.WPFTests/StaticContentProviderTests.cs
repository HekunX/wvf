using Microsoft.VisualStudio.TestTools.UnitTesting;
using HSoft.WebView.NetFramework.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSoft.WebView.NetFramework.WPF.Tests
{
    [TestClass()]
    public class StaticContentProviderTests
    {
        [TestMethod()]
        public void TryGetFrameworkFileTest()
        {
            StaticContentProvider.TryGetFrameworkFile("assets/webview.umd.js",out var content,out var contentType);
        }
    }
}