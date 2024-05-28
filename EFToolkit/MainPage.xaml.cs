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
        }

        public void SetDragRegionForCustomTitleBar(AppWindow appWindow)
        {
            // Check to see if customization is supported.
            // The method returns true on Windows 10 since Windows App SDK 1.2, and on all versions of
            // Windows App SDK on Windows 11.
            if (AppWindowTitleBar.IsCustomizationSupported()
                && appWindow.TitleBar.ExtendsContentIntoTitleBar)
            {
                double rightPaddingWidth = 0, leftPaddingWidth = 0;

                double scaleAdjustment = DisplayExtensions.GetScaleAdjustment();
                if (scaleAdjustment != 0)
                {
                    rightPaddingWidth = appWindow.TitleBar.RightInset / scaleAdjustment;
                    if (rightPaddingWidth < 0)
                    {
                        rightPaddingWidth = 0;
                    }

                    leftPaddingWidth = appWindow.TitleBar.LeftInset / scaleAdjustment;
                    if (leftPaddingWidth < 0)
                    {
                        leftPaddingWidth = 0;
                    }
                }

                RightPaddingColumn.Width = new GridLength(rightPaddingWidth);
                LeftPaddingColumn.Width = new GridLength(leftPaddingWidth);

                List<Windows.Graphics.RectInt32> dragRectsList = new();

                Windows.Graphics.RectInt32 dragRectL;
                dragRectL.X = (int)((LeftPaddingColumn.ActualWidth) * scaleAdjustment);
                dragRectL.Y = 0;
                dragRectL.Height = (int)(AppTitleBar.ActualHeight * scaleAdjustment);
                dragRectL.Width = (int)((IconColumn.ActualWidth
                                        + TitleColumn.ActualWidth
                                        + LeftDragColumn.ActualWidth) * scaleAdjustment);
                //dragRectsList.Add(dragRectL);

                Windows.Graphics.RectInt32 dragRectR;
                dragRectR.X = (int)((LeftPaddingColumn.ActualWidth
                                    + IconColumn.ActualWidth
                                    //+ TitleTextBlock.ActualWidth
                                    + LeftDragColumn.ActualWidth
                                    + TabColumn.ActualWidth) * scaleAdjustment);
                dragRectR.Y = 0;
                dragRectR.Height = (int)(AppTitleBar.ActualHeight * scaleAdjustment);
                dragRectR.Width = (int)(RightDragColumn.ActualWidth * scaleAdjustment);
                dragRectsList.Add(dragRectR);

                Windows.Graphics.RectInt32[] dragRects = dragRectsList.ToArray();

                appWindow.TitleBar.SetDragRectangles(dragRects);
            }
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
            else if (args.SelectedItem == ModelFixerViewItem)
            {
                PageFrame.Navigate(typeof(ModelFixerPage));
            }
            else if (args.SelectedItem == AcronymLibraryItem)
            {
                PageFrame.Navigate(typeof(AcronymLibraryPage));
            }
            else if (args.SelectedItem == SchemaLibraryItem)
            {
                PageFrame.Navigate(typeof(SchemaLibraryPage));
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

        }
        
        private async void Suggestion_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/Wandtket/Entity-Framework-Toolkit/issues/new"));
        }

    }
}
