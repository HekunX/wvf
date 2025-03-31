using Microsoft.JSInterop;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfNet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MyRuntime _myRuntime;

        public MainWindow()
        {
            InitializeComponent();
            _myRuntime = new MyRuntime(this.webview,this.Dispatcher);

            Task.Run(() => 
            {
                Task.Delay(10000).Wait();
                var b = new byte[] { 1, 2, 3, 4 };
                Dispatcher.InvokeAsync(() =>
                {
                    _myRuntime.InvokeVoidAsync("test", b);
                });
            });
        }
    }
}