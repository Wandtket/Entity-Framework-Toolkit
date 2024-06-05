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
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

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


    public class Settings : INotifyPropertyChanged
    {
        private static Settings _instance = new Settings();
        public static Settings Current { get { return _instance; } }


        private ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        public string ProjectDirectory 
        { 
            
            get { return localSettings.Values[projectdirectory] as string ?? ""; } 
            set { localSettings.Values[projectdirectory] = value; } 
        }
        private string projectdirectory = "project_directory";


        public bool TeachTipsOpen
        {
            get { return localSettings.Values[teachtipsopen] as bool? ?? false; }
            set { localSettings.Values[teachtipsopen] = value; OnPropertyChanged("TeachTipsOpen"); }
        }
        private string teachtipsopen = "teach_tips_open";
        public void ResetTeachTips()
        {
            localSettings.Values[teachtipsopen] = false;
        }



        public bool ModelSummary
        {
            get { return localSettings.Values[modelsummary] as bool? ?? true; }
            set { localSettings.Values[modelsummary] = value; }
        }
        private string modelsummary = "model_summary";

        public bool ColumnAttribute
        {
            get { return localSettings.Values[columnattribute] as bool? ?? true; }
            set { localSettings.Values[columnattribute] = value; }
        }
        private string columnattribute = "column_attribute";


        public bool ModelNameSuggestion
        {
            get { return localSettings.Values[modelnamesuggestion] as bool? ?? true; }
            set { localSettings.Values[modelnamesuggestion] = value; }
        }
        private string modelnamesuggestion = "model_name_suggestion";


        public string ModelPrefix
        {

            get { return localSettings.Values[modelprefix] as string ?? ""; }
            set { localSettings.Values[modelprefix] = value; }
        }
        private string modelprefix = "model_prefix";

        public string ModelSuffix
        {

            get { return localSettings.Values[modelsuffix] as string ?? ""; }
            set { localSettings.Values[modelsuffix] = value; }
        }
        private string modelsuffix = "model_suffix";



        public bool DtoSummary
        {
            get { return localSettings.Values[dtosummary] as bool? ?? true; }
            set { localSettings.Values[dtosummary] = value; }
        }
        private string dtosummary = "dto_summary";

        public string DTOPrefix
        {

            get { return localSettings.Values[dtoprefix] as string ?? ""; }
            set { localSettings.Values[dtoprefix] = value; }
        }
        private string dtoprefix = "dto_prefix";

        public string DTOSuffix
        {

            get { return localSettings.Values[dtosuffix] as string ?? "Dto"; }
            set { localSettings.Values[dtosuffix] = value; }
        }
        private string dtosuffix = "dto_suffix";



        public string ConfigurationName
        {

            get { return localSettings.Values[configurationname] as string ?? "builder"; }
            set { localSettings.Values[configurationname] = value; }
        }
        private string configurationname = "configuration_name";

        public string PrimaryKeyStandard
        {

            get { return localSettings.Values[primarykeystandard] as string ?? ""; }
            set { localSettings.Values[primarykeystandard] = value; }
        }
        private string primarykeystandard = "primary_key_standard";


        /// <summary>
        /// Determines if the RichEditBoxes should color code.
        /// </summary>
        public bool CodeColoring
        {
            get { return localSettings.Values[codecoloring] as bool? ?? true; }
            set { localSettings.Values[codecoloring] = value; }
        }
        private string codecoloring = "code_coloring";


        public bool Promotion
        {
            get { return localSettings.Values[promotion] as bool? ?? true; }
            set { localSettings.Values[promotion] = value; OnPropertyChanged("Promotion"); }
        }
        private string promotion = "promotion";



        public bool DataVisualizerIncludeAll
        {
            get { return localSettings.Values[datavisualizerincludeall] as bool? ?? false; }
            set { localSettings.Values[datavisualizerincludeall] = value; }
        }
        private string datavisualizerincludeall = "data_visualizer_include_all";


        /// <summary>
        /// Determines how DTO's should be built.
        /// </summary>
        public ModelOptions DTOModelOptions
        {
            get { try { return JsonSerializer.Deserialize<ModelOptions>(localSettings.Values[dtomodeloptions] as string); } catch { return ModelOptions.Standard; } }
            set { localSettings.Values[dtomodeloptions] = JsonSerializer.Serialize(value); }
        }
        private string dtomodeloptions = "dtomodeloptions";
        public IList<ModelOptions> DTO_OptionList = Enum.GetValues(typeof(ModelOptions)).Cast<ModelOptions>().ToList();


        /// <summary>
        /// How object names should formatted throughout the application
        /// </summary>
        public CodeFormatOptions CodeFormatOptions
        {
            get { try { return JsonSerializer.Deserialize<CodeFormatOptions>(localSettings.Values[codeformatoptions] as string); } catch { return CodeFormatOptions.PascalCase; } }
            set { localSettings.Values[codeformatoptions] = JsonSerializer.Serialize(value); }
        }
        private string codeformatoptions = "code_format_options";
        public IList<CodeFormatOptions> CodeFormatOptionsList = Enum.GetValues(typeof(CodeFormatOptions)).Cast<CodeFormatOptions>().ToList();


        public bool AcronymCharacterReplacer
        {
            get { return localSettings.Values[acronymcharacterreplacer] as bool? ?? true; }
            set { localSettings.Values[acronymcharacterreplacer] = value; }
        }
        private string acronymcharacterreplacer = "acronym_character_replacer";


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string Name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(Name));
        }

    }


}
