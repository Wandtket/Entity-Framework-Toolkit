using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.UI.Animations;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WinRT.Interop;
using WinRT;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Foundation;


namespace EFToolkit.Extensions
{


    public static class WindowExtensions
    {       
        static WindowsSystemDispatcherQueueHelper m_wsdqHelper; // See separate sample below for implementation
        static Microsoft.UI.Composition.SystemBackdrops.MicaController? m_micaController;
        static Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration? m_configurationSource;

        /// <summary>
        /// Used to get the native Window Handler.
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public static AppWindow GetAppWindow(this Window window)
        {
            var hwndd = new Windows.Win32.Foundation.HWND(WinRT.Interop.WindowNative.GetWindowHandle(window));
            WindowId wndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwndd);
            return AppWindow.GetFromWindowId(wndId);
        }

        /// <summary>
        /// Maximizes the Window
        /// </summary>
        /// <param name="window"></param>
        public static void Maximize(this Window window)
        {
            var hwndd = new Windows.Win32.Foundation.HWND(WinRT.Interop.WindowNative.GetWindowHandle(window));
            PInvoke.ShowWindow(hwndd, SHOW_WINDOW_CMD.SW_MAXIMIZE);
        }

        /// <summary>
        /// Maximizes the Window
        /// </summary>
        /// <param name="window"></param>
        public static void Resize(this Window window, int Height, int Width)
        {
            window.AppWindow.Resize(new Windows.Graphics.SizeInt32(Width, Height));
        }


        /// <summary>
        /// Replaces default TitleBar in Window
        /// </summary>
        /// <param name="window"></param>
        public static void ExtendContentIntoTitleBar(this Window window)
        {
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var m_AppWindow = GetAppWindow(window);

                var titleBar = m_AppWindow.TitleBar;
                titleBar.ExtendsContentIntoTitleBar = true;

                var Foreground = Application.Current.Resources["Foreground"] as SolidColorBrush;
                if (Foreground != null) { titleBar.ButtonForegroundColor = Foreground.Color; };

                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

                var brush = Application.Current.Resources["BoxColor"] as SolidColorBrush;
                if (brush != null) { titleBar.ButtonHoverBackgroundColor = brush.Color; }

                titleBar.PreferredHeightOption = TitleBarHeightOption.Standard;
            }
        }

        /// <summary>
        /// Enables MICA Backdrop on MainWindow
        /// </summary>
        /// <param name="window"></param>
        public static void EnableMICABackdrop(this Window window)
        {
            if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
            {
                m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
                m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

                m_configurationSource = new Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration();

                m_configurationSource.IsInputActive = true;
                SetConfigurationSourceTheme(window);

                m_micaController = new Microsoft.UI.Composition.SystemBackdrops.MicaController();

                // Enable the system backdrop.
                // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
                m_micaController.AddSystemBackdropTarget(window.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                m_micaController.SetSystemBackdropConfiguration(m_configurationSource);

                window.Closed += delegate (object sender, WindowEventArgs args)
                {
                    if (m_micaController is not null)
                    {
                        m_micaController.Dispose();
                        m_micaController = null;
                    }

                    m_configurationSource = null;
                };
            }
        }


        private static void SetConfigurationSourceTheme(Window window)
        {
            if (m_configurationSource == null)
            {
                return;
            }

            switch (((FrameworkElement)window.Content).ActualTheme)
            {
                case ElementTheme.Dark:
                    m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Dark;
                    break;
                case ElementTheme.Light:
                    m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Light;
                    break;
                case ElementTheme.Default:
                    m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Default;
                    break;
            }
        }



        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
        {
            if (IsWindows10OrGreater(17763))
            {
                var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                if (IsWindows10OrGreater(18985))
                {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                }

                int useImmersiveDarkMode = enabled ? 1 : 0;
                return DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
            }

            return false;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        private static bool IsWindows10OrGreater(int build = -1)
        {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
        }


    }



    class WindowsSystemDispatcherQueueHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        struct DispatcherQueueOptions
        {
            internal int dwSize;
            internal int threadType;
            internal int apartmentType;
        }

        [DllImport("CoreMessaging.dll")]
        private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object dispatcherQueueController);

        object m_dispatcherQueueController = null;
        public void EnsureWindowsSystemDispatcherQueueController()
        {
            if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
            {
                // one already exists, so we'll just use it.
                return;
            }

            if (m_dispatcherQueueController == null)
            {
                DispatcherQueueOptions options;
                options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
                options.threadType = 2;    // DQTYPE_THREAD_CURRENT
                options.apartmentType = 2; // DQTAT_COM_STA

                CreateDispatcherQueueController(options, ref m_dispatcherQueueController);
            }
        }
    }

}
