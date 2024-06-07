using EFToolkit.Extensions;
using EFToolkit.Pages;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.WindowManagement;
using AppWindow = Microsoft.UI.Windowing.AppWindow;
using AppWindowTitleBar = Microsoft.UI.Windowing.AppWindowTitleBar;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EFToolkit
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        public SafeFileHandle Icon;


        public MainWindow()
        {
            this.InitializeComponent();
            Initialize();

            this.Resize(616, 1067);
        }

        private async void Initialize()
        {
            LoadIcon();

            this.EnableMICABackdrop();
            this.ExtendContentIntoTitleBar();

            while (App.Current.ActiveWindow == null) { await Task.Delay(50); }
            while (App.Current.ActiveWindow.Content.XamlRoot == null) { await Task.Delay(50); }

            Main.SetDragRegionForCustomTitleBar(this.GetAppWindow());
        }

        public void LoadIcon()
        {
            var hwndd = new Windows.Win32.Foundation.HWND(WinRT.Interop.WindowNative.GetWindowHandle(this));
            Icon = PInvoke.LoadImage(null, @"logo.ico", GDI_IMAGE_TYPE.IMAGE_ICON, 16, 16, IMAGE_FLAGS.LR_LOADFROMFILE);
            PInvoke.SendMessage(hwndd, 0x0080, new WPARAM(0), new LPARAM(Icon.DangerousGetHandle()));
        }


        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState == WindowActivationState.CodeActivated)
            {
                App.Current.ActiveWindow = this;
            }
            else if (args.WindowActivationState == WindowActivationState.Deactivated)
            {

            }
        }

        private async void Window_SizeChanged(object sender, Microsoft.UI.Xaml.WindowSizeChangedEventArgs args)
        {
            await Task.Delay(10);
            Main.SetDragRegionForCustomTitleBar(this.GetAppWindow());
        }

    }
}
