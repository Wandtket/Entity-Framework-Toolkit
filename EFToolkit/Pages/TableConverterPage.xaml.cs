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
using static System.Net.Mime.MediaTypeNames;
using CommunityToolkit.WinUI.Controls;
using System.Threading.Tasks;

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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            TableName.SuggestedItemsSource = Toolkit.SchemaLibraries;
            TableName.ItemsSource = Toolkit.SelectedSchemaLibraries;

            AcronymLibrarySelector.SuggestedItemsSource = Toolkit.AcronymLibraries;
            AcronymLibrarySelector.ItemsSource = Toolkit.SelectedAcronymLibraries;
        }

        private void DesignerGrid_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Delete)
            {
                if (DesignerGrid.SelectedItems.Count == 1)
                {
                    DesignItem item = (DesignItem)DesignerGrid.SelectedItem;
                    DesignItems.Remove(item);
                }
                else if (DesignerGrid.SelectedItems.Count > 1)
                {
                    List<DesignItem> ItemsToDelete = new List<DesignItem>();
                    for (int i = 0; i < DesignerGrid.SelectedItems.Count; i++)
                    {
                        DesignItem item = (DesignItem)DesignerGrid.SelectedItems[i];
                        ItemsToDelete.Add(item);
                    }

                    foreach (var item in ItemsToDelete)
                    {
                        DesignItems.Remove(item);
                    }
                }

                DesignItemCount.Text = DesignItems.Count().ToString();
                ConvertTable();
            }
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


                    //Pasting from Table Designer SQL Management Studio / Visual Studio SQL Object Explorer
                    if (valuesInRow.Count() < 6)
                    {
                        //Set Column Indexes
                        int NameColumnIndex = 0;
                        int DataTypeColumnIndex = 1;
                        int AllowNullsColumnIndex = 2;

                        int DefaultColumnIndex = 4;
                        string DefaultColumn = "";

                        //Visual Studio SQL Serber Objet Explorer has an empty column to account for
                        if (valuesInRow[0] == string.Empty)
                        {
                            NameColumnIndex = 1;
                            DataTypeColumnIndex = 2;
                            AllowNullsColumnIndex = 3;
                            DefaultColumn = valuesInRow[DefaultColumnIndex];
                        }
                        

                        //Convert string to bool
                        bool AllowNulls = false;
                        try
                        {
                            if (valuesInRow[AllowNullsColumnIndex].ToLower() == "checked") { AllowNulls = true; }
                            else if (valuesInRow[AllowNullsColumnIndex].ToLower() == "unchecked") { AllowNulls = false; }
                            else if (valuesInRow[AllowNullsColumnIndex].ToLower() == "true") { AllowNulls = true; }
                            else if (valuesInRow[AllowNullsColumnIndex].ToLower() == "false") { AllowNulls = false; }
                        }
                        catch { await MessageBox.Show("There was an error copying the SQL Table, please copy the table and try again.", "Error"); return; }

                        DesignItems.Add(new DesignItem()
                        {
                            ColumnName = valuesInRow[NameColumnIndex],
                            ObjectName = Toolkit.ConvertSQLColumnName(valuesInRow[NameColumnIndex]),
                            DataType = valuesInRow[DataTypeColumnIndex],
                            AllowNulls = AllowNulls,
                            DefaultValue = DefaultColumn,
                        });
                    }
                    //Pasting from Select Statement Describer
                    else
                    {
                        int NameColumnIndex = 2;
                        int DataTypeColumnIndex = 5;
                        int AllowNullsColumnIndex = 3;


                        bool AllowNulls = false;
                        try
                        {
                            if (valuesInRow[AllowNullsColumnIndex].ToLower() == "1") { AllowNulls = true; }
                            else if (valuesInRow[AllowNullsColumnIndex].ToLower() == "0") { AllowNulls = false; }
                        }
                        catch { await MessageBox.Show("There was an error copying the SQL Table, please copy the table and try again.", "Error"); return; }

                        DesignItems.Add(new DesignItem()
                        {
                            ColumnName = valuesInRow[NameColumnIndex],
                            ObjectName = Toolkit.ConvertSQLColumnName(valuesInRow[NameColumnIndex]),
                            DataType = valuesInRow[DataTypeColumnIndex],
                            AllowNulls = AllowNulls,
                        });
                    }


                    while (iCol < valuesInRow.Length) { iCol += 1; }
                    iRow += 1;
                }
            }

            DesignerGrid.ItemsSource = DesignItems;
            DesignItemCount.Text = DesignItems.Count().ToString();

            ConvertTable();
        }

        private void ClearTable_Click(object sender, RoutedEventArgs e)
        {
            DesignItems.Clear();
            DesignItemCount.Text = DesignItems.Count().ToString();
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

            ConvertTable();
        }

        private void ConfigurationToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ModelToggleButton.IsChecked = false;
            DTOToggleButton.IsChecked = false;
            ConfigurationToggleButton.IsChecked = true;

            ConvertTable();
        }

        private void DTOToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ModelToggleButton.IsChecked = false;
            ConfigurationToggleButton.IsChecked = false;
            DTOToggleButton.IsChecked = true;

            ConvertTable();
        }


        private async void ConvertTable()
        {
            OutputProgress.Visibility = Visibility.Visible;
            await Output.SetText("");

            if (ModelToggleButton.IsChecked == true) 
            {
                await Output.SetText(Toolkit.ConvertTableToModel(DesignItems, TableName.Value, ClassName.Text));
            }
            if (ConfigurationToggleButton.IsChecked == true) 
            {
                await Output.SetText(Toolkit.ConvertTableToConfiguration(DesignItems, TableName.Text));
            }
            if (DTOToggleButton.IsChecked == true) 
            {
                await Output.SetText(Toolkit.ConvertTableToDto(DesignItems, TableName.Value, ClassName.Text, Settings.DTO_Options));
            }

            OutputProgress.Visibility = Visibility.Collapsed;
        }

        private async void ClearOutput_Click(object sender, RoutedEventArgs e)
        {
            await Output.SetText("");
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AllowNulls_Click(object sender, RoutedEventArgs e)
        {
            ConvertTable();
        }


        private void TableName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConvertTable();
        }

        private async void TableName_LostFocus(object sender, RoutedEventArgs e)
        {
            //Delay to see if user is selecting a token item.
            await Task.Delay(100);

            if (string.IsNullOrEmpty(ClassName.Text))
            {            
                ClassName.Text = Toolkit.ConvertSQLColumnName(TableName.Text);
            }
        }

        private void ClassName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConvertTable();
        }

        private void SearchTable_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && sender.Text.Length > 0)
            {
                DesignerGrid.SelectedItems.Clear();

                int FoundCount = 0;
                DesignItem? FoundItem = null;
                for (int i = 0; i < DesignItems.Count; i++)
                {
                    DesignItem Item = (DesignItem)DesignItems[i];

                    if (Item.ColumnName.ToLower().Contains(sender.Text.ToLower()))
                    {
                        DesignerGrid.SelectedItems.Add(Item);
                        FoundCount = FoundCount + 1;
                        FoundItem = Item;
                    }

                    if (Item.ObjectName.ToLower().Contains(sender.Text.ToLower()))
                    {
                        DesignerGrid.SelectedItems.Add(Item);
                        FoundCount = FoundCount + 1;
                        FoundItem = Item;
                    }

                    if (Item.DataType.ToLower().Contains(sender.Text.ToLower()))
                    {
                        DesignerGrid.SelectedItems.Add(Item);
                        FoundCount = FoundCount + 1;
                        FoundItem = Item;
                    }
                }

                if (FoundCount == 1)
                {
                    DesignerGrid.ScrollIntoView(FoundItem, DesignerGrid.Columns[0]);
                }
            }
            else
            {
                DesignerGrid.SelectedItems.Clear();
            }
        }

    }

}
