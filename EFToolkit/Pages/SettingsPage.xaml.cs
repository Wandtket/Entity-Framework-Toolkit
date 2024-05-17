using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;
using EFToolkit.Controls.Dialogs;
using System.Text.Json;
using System.Reflection;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EFToolkit.Pages
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }


        private async void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {

        }

        private async void AppFolder_Click(object sender, RoutedEventArgs e)
        {
            string? Directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (Directory != null)
            {
                await Launcher.LaunchFolderPathAsync(Directory);
            }
        }

        private async void LocalFolder_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchFolderAsync(ApplicationData.Current.LocalFolder);
        }


    }

    public static class Settings
    {

        private static ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        public static string ProjectDirectory 
        { 
            
            get { return localSettings.Values[projectdirectory] as string ?? ""; } 
            set { localSettings.Values[projectdirectory] = value; } 
        }
        private static string projectdirectory = "project_directory";


        public static bool ModelSummary
        {
            get { return localSettings.Values[modelsummary] as bool? ?? true; }
            set { localSettings.Values[modelsummary] = value; }
        }
        private static string modelsummary = "model_summary";

        public static string ModelPrefix
        {

            get { return localSettings.Values[modelprefix] as string ?? ""; }
            set { localSettings.Values[modelprefix] = value; }
        }
        private static string modelprefix = "model_prefix";

        public static string ModelSuffix
        {

            get { return localSettings.Values[modelsuffix] as string ?? ""; }
            set { localSettings.Values[modelsuffix] = value; }
        }
        private static string modelsuffix = "model_suffix";



        public static bool DtoSummary
        {
            get { return localSettings.Values[dtosummary] as bool? ?? true; }
            set { localSettings.Values[dtosummary] = value; }
        }
        private static string dtosummary = "dto_summary";

        public static string DTOPrefix
        {

            get { return localSettings.Values[dtoprefix] as string ?? ""; }
            set { localSettings.Values[dtoprefix] = value; }
        }
        private static string dtoprefix = "dto_prefix";

        public static string DTOSuffix
        {

            get { return localSettings.Values[dtosuffix] as string ?? ""; }
            set { localSettings.Values[dtosuffix] = value; }
        }
        private static string dtosuffix = "dto_suffix";


        public static string ConfigurationName
        {

            get { return localSettings.Values[configurationname] as string ?? "builder"; }
            set { localSettings.Values[configurationname] = value; }
        }
        private static string configurationname = "configuration_name";



        /// <summary>
        /// Determines if the RichEditBoxes should color code.
        /// </summary>
        public static bool CodeColoring
        {
            get { return localSettings.Values[codecoloring] as bool? ?? true; }
            set { localSettings.Values[codecoloring] = value; }
        }
        private static string codecoloring = "code_coloring";



        public static string DatabaseSchema
        {
            get { return localSettings.Values[databaseschema] as string ?? "dbo."; }
            set { localSettings.Values[databaseschema] = value; }
        }
        private static string databaseschema = "database_schema";



        /// <summary>
        /// Determines how DTO's should be built.
        /// </summary>
        public static DTO_Options DTO_Options
        {
            get { try { return JsonSerializer.Deserialize<DTO_Options>(localSettings.Values[dto_options] as string); } catch { return DTO_Options.Standard; } }
            set { localSettings.Values[dto_options] = JsonSerializer.Serialize(value); }
        }
        private static string dto_options = "dto_options";
        public static IList<DTO_Options> DTO_OptionList = Enum.GetValues(typeof(DTO_Options)).Cast<DTO_Options>().ToList();


        /// <summary>
        /// How object names should formatted throughout the application
        /// </summary>
        public static CodeFormatOptions CodeFormatOptions
        {
            get { try { return JsonSerializer.Deserialize<CodeFormatOptions>(localSettings.Values[codeformatoptions] as string); } catch { return CodeFormatOptions.CamelCase; } }
            set { localSettings.Values[codeformatoptions] = JsonSerializer.Serialize(value); }
        }
        private static string codeformatoptions = "code_format_options";
        public static IList<CodeFormatOptions> CodeFormatOptionsList = Enum.GetValues(typeof(CodeFormatOptions)).Cast<CodeFormatOptions>().ToList();




        public static bool AcronymCharacterReplacer
        {
            get { return localSettings.Values[acronymcharacterreplacer] as bool? ?? true; }
            set { localSettings.Values[acronymcharacterreplacer] = value; }
        }
        private static string acronymcharacterreplacer = "acronym_character_replacer";



    }


}
