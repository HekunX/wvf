using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfNetFrameWork
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        // 声明Win32 API函数
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        public MainWindow()
        {
            //var path = @"I:\HSoft\WVF\WpfNetFrameWork\bin\Debug\runtimes\win-x86\native\WebView2Loader.dll";
            //var ptr = LoadLibrary(path);
            //if (ptr == IntPtr.Zero)
            //{
            //    int errorCode = Marshal.GetLastWin32Error();
            //    throw new Exception($"Failed to load unmanaged DLL. Error code: {errorCode}");
            //}
            InitializeComponent();
        }
    }
}
