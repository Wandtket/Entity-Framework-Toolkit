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
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using EFToolkit.Controls.Dialogs;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using EFToolkit.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EFToolkit.Pages
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TableConverterPage : Page
    {

        ObservableCollection<DesignItem> DesignItems = new();

        public TableConverterPage()
        {
            this.InitializeComponent();
        }


        private void DesignerGrid_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Delete)
            {
                DesignItem item = (DesignItem)DesignerGrid.SelectedItem;
                DesignItems.Remove(item);

                Convert();
            }
        }

        private async void PasteAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            PasteTable_Click(null, null);
        }

        private async void PasteTable_Click(object sender, RoutedEventArgs e)
        {
            if (DesignItems.Count > 0) { DesignItems.Clear(); }

            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                string text = await dataPackageView.GetTextAsync();

                //Seperate rows by Carriage Return
                char[] rowSplitter = { (char)10, (char)13 };

                //Seperate Columns by Tab
                char[] columnSplitter = { (char)9 };

                //get the text from clipboard
                string dataInClipboard = await dataPackageView.GetTextAsync();

                //split it into lines
                string[] rowsInClipboard = dataInClipboard.Split(rowSplitter, StringSplitOptions.RemoveEmptyEntries);

                // loop through the lines, split them into cells and place the values in the corresponding cell.
                int iRow = 0;
                while (iRow < rowsInClipboard.Length - 1)
                {
                    //split row into cell values
                    string[] valuesInRow = rowsInClipboard[iRow].Split(columnSplitter);

                    //cycle through cell values
                    int iCol = 0;

                    //Set Column Indexes
                    int NameColumnIndex = 0;
                    int DataTypeColumnIndex = 1;
                    int AllowNullsColumnIndex = 2;
                    int DefaultColumnIndex = 3;

                    //Visual Studio SQL Serber Objet Explorer has an empty column to account for
                    if (valuesInRow[0] == string.Empty) 
                    {
                        NameColumnIndex = 1;
                        DataTypeColumnIndex = 2;
                        AllowNullsColumnIndex = 3;
                        DefaultColumnIndex = 4;
                    }

                    //Convert string to bool
                    bool AllowNulls = false;
                    if (valuesInRow[AllowNullsColumnIndex].ToLower() == "checked") { AllowNulls = true; }
                    else if (valuesInRow[AllowNullsColumnIndex].ToLower() == "unchecked") { AllowNulls = false; }
                    else if (valuesInRow[AllowNullsColumnIndex].ToLower() == "true") { AllowNulls = true; }
                    else if (valuesInRow[AllowNullsColumnIndex].ToLower() == "false") { AllowNulls = false; }
                    

                    DesignItems.Add(new DesignItem() 
                    { 
                        ColumnName = valuesInRow[NameColumnIndex],
                        DataType = valuesInRow[DataTypeColumnIndex],
                        AllowNulls = AllowNulls,
                    });

                    while (iCol < valuesInRow.Length) { iCol += 1; }
                    iRow += 1;
                }
            }

            DesignerGrid.ItemsSource = DesignItems;
        }

        private void ClearTable_Click(object sender, RoutedEventArgs e)
        {
            DesignItems.Clear();
        }


        private void CopyOutput_Click(object sender, RoutedEventArgs e)
        {
            DataPackage package = new();
            package.SetText(Output.GetText());
            Clipboard.SetContent(package);
        }


        private void ModelToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationToggleButton.IsChecked = false;
            DTOToggleButton.IsChecked = false;
            ModelToggleButton.IsChecked = true;

            Convert();
        }

        private void ConfigurationToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ModelToggleButton.IsChecked = false;
            DTOToggleButton.IsChecked = false;
            ConfigurationToggleButton.IsChecked = true;

            Convert();
        }

        private void DTOToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ModelToggleButton.IsChecked = false;
            ConfigurationToggleButton.IsChecked = false;
            DTOToggleButton.IsChecked = true;

            Convert();
        }


        private void Convert()
        {
            Output.SetText("");

            if (ModelToggleButton.IsChecked == true) 
            {
                Output.SetText(Toolkit.ConvertToModel(DesignItems, TableName.Text));
            }
            if (ConfigurationToggleButton.IsChecked == true) 
            {
                Output.SetText(Toolkit.ConvertToConfiguration(DesignItems));
            }
            if (DTOToggleButton.IsChecked == true) 
            {
                Output.SetText(Toolkit.ConvertToDto(DesignItems, TableName.Text));
            }
        }

        private void ClearOutput_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }

}
