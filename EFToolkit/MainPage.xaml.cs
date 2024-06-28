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
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EFToolkit
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            Loaded += Page_Loaded;
            SizeChanged += MainPage_SizeChanged;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App.Current.ActiveWindow.ExtendsContentIntoTitleBar = true;
            App.Current.ActiveWindow.SetTitleBar(CustomDragRegion);
            CustomDragRegion.MinWidth = 188;
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            App.Current.ActiveWindow.SetTitleBar(CustomDragRegion);
        }

        private void MainView_PaneOpened(NavigationView sender, object args)
        {
            App.Current.ActiveWindow.SetTitleBar(CustomDragRegion);
        }

        private void MainView_PaneClosed(NavigationView sender, object args)
        {
            App.Current.ActiveWindow.SetTitleBar(CustomDragRegion);
        }

        private async void MainView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem == TableConverterViewItem)
            {
                PageFrame.Navigate(typeof(TableConverterPage));
            }
            else if (args.SelectedItem == SelectDescriberViewItem)
            {
                PageFrame.Navigate(typeof(SelectDescriberPage));
            }
            else if (args.SelectedItem == DataVisualizerViewItem)
            {
                PageFrame.Navigate(typeof(DataVisualizerPage));
            }
            else if (args.SelectedItem == ModelEditorViewItem)
            {
                PageFrame.Navigate(typeof(ModelEditorPage));
            }
            else if (args.SelectedItem == AcronymViewItem)
            {
                PageFrame.Navigate(typeof(AcronymPage));
            }
            else if (args.SelectedItem == DatabaseViewItem)
            {
                PageFrame.Navigate(typeof(DatabasePage));
            }
            else if (args.IsSettingsSelected == true)
            {
                PageFrame.Navigate(typeof(SettingsPage));
            }
        }


        private void Help_Click(object sender, RoutedEventArgs e)
        {
            Settings.Current.TeachTipsOpen = !Settings.Current.TeachTipsOpen;
            if (MainView.SelectedItem == TableConverterViewItem)
            {
                TableConverterPage Page = PageFrame.Content as TableConverterPage;
                Page?.ToggleTeachTips(Settings.Current.TeachTipsOpen);
            }
            else if (MainView.SelectedItem == SelectDescriberViewItem)
            {
                SelectDescriberPage Page = PageFrame.Content as SelectDescriberPage;
                Page?.ToggleTeachTips(Settings.Current.TeachTipsOpen);
            }
            else if (MainView.SelectedItem == DataVisualizerViewItem)
            {
                DataVisualizerPage Page = PageFrame.Content as DataVisualizerPage;
                Page?.ToggleTeachTips(Settings.Current.TeachTipsOpen);
            }
            else if (MainView.SelectedItem == ModelEditorViewItem)
            {
                ModelEditorPage Page = PageFrame.Content as ModelEditorPage;
                Page?.ToggleTeachTips(Settings.Current.TeachTipsOpen);
            }
            else if (MainView.SelectedItem == AcronymViewItem)
            {
                AcronymPage Page = PageFrame.Content as AcronymPage;
                Page?.ToggleTeachTips(Settings.Current.TeachTipsOpen);
            }
            else if (MainView.SelectedItem == DatabaseViewItem)
            {
                DatabasePage Page = PageFrame.Content as DatabasePage;
                Page?.ToggleTeachTips(Settings.Current.TeachTipsOpen);
            }
        }


        private async void Coffee_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://www.buymeacoffee.com/wandtket"));

        }

        private async void Suggestion_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/Wandtket/Entity-Framework-Toolkit/issues/new"));
        }


    }
}
