using EFToolkit.Controls.Dialogs;
using EFToolkit.Extensions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EFToolkit.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SchemaLibraryPage : Page
    {
        public SchemaLibraryPage()
        {
            this.InitializeComponent();

            SchemaLibraryList.ItemsSource = Toolkit.SchemaLibraries;
        }

        private void AddSchema_Click(object sender, RoutedEventArgs e)
        {
            var Library = new SchemaLibrary() { };

            Toolkit.SchemaLibraries.Add(Library);
            SchemaLibraryList.SelectedItem = Library;
        }

        private async void SaveSchema_Click(object sender, RoutedEventArgs e)
        {
            Toolkit.SaveSchemaLibaries();
            await MessageBox.Show("Libraries should save automatically but this button feels good to press sometimes...", "Libraries Saved!");
        }

        private void SchemaLibraryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SchemaLibrary SelectedItem = (SchemaLibrary)SchemaLibraryList.SelectedItem;
            if (SelectedItem != null)
            {
                SchemasTotal.Text = SchemaLibraryList.Items.Count.ToString();
            }
        }

        private async void RemoveLibrary_Click(object sender, RoutedEventArgs e)
        {
            var s = (FrameworkElement)sender;
            var d = s.DataContext;

            SchemaLibrary Library = (SchemaLibrary)SchemaLibraryList.SelectedItem;
            Toolkit.SchemaLibraries.Remove(Library);
            Toolkit.SaveSchemaLibaries();
            
        }

        private async void ClearSchemas_Click(object sender, RoutedEventArgs e)
        {
            var Result = await ConfirmBox.Show("This action cannot be undone", "Clear Schemas?");
            if (Result == ContentDialogResult.Primary)
            {
                Toolkit.SchemaLibraries.Clear();
                Toolkit.SelectedSchemaLibraries.Clear();
                Toolkit.SaveSchemaLibaries();
            }
        }

        private async void Schema_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            box.Text = box.Text.Trim().Replace(" ", "");

            SchemaLibrary SelectedItem = (SchemaLibrary)SchemaLibraryList.SelectedItem;

            var Duplicate = Toolkit.SchemaLibraries.Where(x => x.Schema == box.Text).Count();
            if (Duplicate > 1)
            {
                box.ShowError("Duplicate Found!", true, Duration: 5);

                var DuplicateList = Toolkit.SchemaLibraries.Where(x => x.Schema == box.Text);
                foreach (var item in DuplicateList)
                {
                    SchemaLibraryList.SelectedItems.Add(item);
                }
            }

            Toolkit.SaveSchemaLibaries();
        }

    }
}
