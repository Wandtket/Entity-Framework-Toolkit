using CommunityToolkit.WinUI.Controls;
using EFToolkit.Controls.Dialogs;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts.DataProvider;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EFToolkit.Controls.Widgets
{
    public sealed partial class SchemaBox : UserControl
    {
        public SchemaBox()
        {
            this.InitializeComponent();
        }


        /// <summary>
        /// Returns the user typed text from the box
        /// </summary>
        public string Text { get { return SchemaTextBox.Text; } }

        /// <summary>
        /// Returns the selected items in order + the user typed text as string.
        /// </summary>
        public string Value 
        { 
            get 
            {
                string SchemaPath = "";
                foreach (SchemaLibrary library in Toolkit.SelectedSchemaLibraries)
                {
                    SchemaPath = SchemaPath + library.Schema;
                }
                return SchemaPath + SchemaTextBox.Text;
            } 
        }


        public object ItemsSource { get { return SchemaTextBox.ItemsSource; } set { SchemaTextBox.ItemsSource = value; } }

        public object SuggestedItemsSource { get { return SchemaTextBox.SuggestedItemsSource; } set { SchemaTextBox.SuggestedItemsSource = value; } }




        private async void SchemaTextBox_TokenItemAdded(CommunityToolkit.WinUI.Controls.TokenizingTextBox sender, object args)
        {
            Toolkit.SaveSchemaLibaries();
        }

        private void SchemaTextBox_TokenItemRemoved(CommunityToolkit.WinUI.Controls.TokenizingTextBox sender, object args)
        {
            Toolkit.SaveSchemaLibaries();
        }


        private async void SchemaLibraryTokenTextBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {           
                ObservableCollection<SchemaLibrary> filteredList = new();
                foreach (var filter in Toolkit.SchemaLibraries)
                {
                    if (filter.Schema.Contains(SchemaTextBox.Text.Trim(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        filteredList.Add(filter);
                    }
                }

                SchemaTextBox.SuggestedItemsSource = filteredList.Distinct();
            }
        }


         
}
}
