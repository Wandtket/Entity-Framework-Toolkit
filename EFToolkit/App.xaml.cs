using EFToolkit.Extensions;
using EFToolkit.Pages;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using EFToolkit.Enums;
using EFToolkit.Controls.Dialogs;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EFToolkit
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.UnhandledException += App_UnhandledException;
        }


        public new static App Current => (App)Application.Current;


        public MainWindow ActiveWindow { get; set; }


        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var mainInstance = Microsoft.Windows.AppLifecycle.AppInstance.FindOrRegisterForKey("main");

            if (!mainInstance.IsCurrent)
            {
                // Redirect the activation (and args) to the "main" instance, and exit.
                var activatedEventArgs =
                    Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();
                await mainInstance.RedirectActivationToAsync(activatedEventArgs);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                return;
            }

            var activatedEventArgs2 = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();
            if (activatedEventArgs2.Kind == Microsoft.Windows.AppLifecycle.ExtendedActivationKind.File)
            {
                StorageFile File = activatedEventArgs2.Data as StorageFile;
                Debug.WriteLine(File.FileType);
                return;
            }

            await Toolkit.LoadData();

            App.Current.ActiveWindow = new MainWindow();

            Settings.Current.ResetTeachTips();

            App.Current.ActiveWindow.Activate();
            App.Current.ActiveWindow.Maximize();
        }

        private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            await ErrorBox.Show(e.Exception);
        }
    }
}
