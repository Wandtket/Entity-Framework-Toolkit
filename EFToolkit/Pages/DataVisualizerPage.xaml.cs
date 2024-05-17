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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EFToolkit.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DataVisualizerPage : Page
    {

        ObservableCollection<VisualizerItem> VisualizerItems = new();


        public DataVisualizerPage()
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

        private void VisualizerGrid_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Delete)
            {
                if (VisualizerGrid.SelectedItems.Count == 1)
                {
                    VisualizerItem item = (VisualizerItem)VisualizerGrid.SelectedItem;
                    VisualizerItems.Remove(item);
                }
                else if (VisualizerGrid.SelectedItems.Count > 1)
                {
                    List<VisualizerItem> ItemsToDelete = new List<VisualizerItem>();
                    for (int i = 0; i < VisualizerGrid.SelectedItems.Count; i++)
                    {
                        VisualizerItem item = (VisualizerItem)VisualizerGrid.SelectedItems[i];
                        ItemsToDelete.Add(item);                
                    }

                    foreach (var item in ItemsToDelete)
                    {
                        VisualizerItems.Remove(item);
                    }

                }

                VisualizerItemCount.Text = VisualizerItems.Count().ToString();
                ConvertTable();
            }
        }

        private void IncludeAll_Click(object sender, RoutedEventArgs e)
        {
            AppBarToggleButton toggle = (AppBarToggleButton)sender;
            if (toggle.IsChecked == true)
            {
                foreach (VisualizerItem item in VisualizerItems) { item.Include = true; }
            }
            else
            {
                foreach (VisualizerItem item in VisualizerItems) { item.Include = false; }
            }

            ConvertTable();
        }

        private async void PasteTable_Click(object sender, RoutedEventArgs e)
        {
            if (VisualizerItems.Count > 0) { VisualizerItems.Clear(); }

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

                if (rowsInClipboard.Length == 1) { await MessageBox.Show("Could not find SQL Column Name, did you copy the result table with headers?", "ERROR"); return; }

                // loop through the lines, split them into cells and place the values in the corresponding cell.
                int iRow = 0;             
                while (iRow < rowsInClipboard.Length)
                {
                    //split row into cell values
                    string[] valuesInRow = rowsInClipboard[iRow].Split(columnSplitter);

                    //cycle through cell values
                    int iCol = 0;

                    if(iRow == 0)
                    {
                        for (int j = 0; j < valuesInRow.Length; j++)
                        {
                            VisualizerItems.Add(new VisualizerItem()
                            {
                                ColumnName = valuesInRow[j],
                                ObjectName = Toolkit.ConvertSQLColumnName(valuesInRow[j]),
                                Include = IncludeAll.IsChecked,
                            });
                        }
                    }
                    else if (iRow == 1)
                    {
                        for (int j = 0; j < valuesInRow.Length; j++)
                        {
                            VisualizerItems[j].Value = valuesInRow[j];
                        }
                    }

                    while (iCol < valuesInRow.Length)  {  iCol += 1;  }
                    iRow += 1;
                }
            }

            VisualizerGrid.ItemsSource = VisualizerItems;
            VisualizerItemCount.Text = VisualizerItems.Count().ToString();

            ConvertTable();
        }

        private void ClearTable_Click(object sender, RoutedEventArgs e)
        {
            VisualizerItems.Clear();
            VisualizerItemCount.Text = VisualizerItems.Count().ToString();
        }

        private void CopyOutput_Click(object sender, RoutedEventArgs e)
        {
            DataPackage package = new();
            package.SetText(Output.GetText());
            Clipboard.SetContent(package);
        }



        private async void ConvertTable()
        {
            OutputProgress.Visibility = Visibility.Visible;
            await Output.SetText("");

            await Output.SetText(Toolkit.ConvertTableToSelectStatement(VisualizerItems, TableName.Text));

            OutputProgress.Visibility = Visibility.Collapsed;
        }

        private async void ClearOutput_Click(object sender, RoutedEventArgs e)
        {
            await Output.SetText("");
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SearchTable_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && sender.Text.Length > 0)
            {
                VisualizerGrid.SelectedItems.Clear();

                int FoundCount = 0;
                VisualizerItem? FoundItem = null;
                for (int i = 0; i < VisualizerItems.Count; i++)
                {
                    VisualizerItem Item = (VisualizerItem)VisualizerItems[i];

                    if (Item.ColumnName.ToLower().Contains(sender.Text.ToLower()))
                    {
                        VisualizerGrid.SelectedItems.Add(Item);
                        FoundCount = FoundCount + 1;
                        FoundItem = Item;
                    }

                    if (Item.ObjectName.ToLower().Contains(sender.Text.ToLower()))
                    {
                        VisualizerGrid.SelectedItems.Add(Item);
                        FoundCount = FoundCount + 1;
                        FoundItem = Item;
                    }

                    if (Item.Value.ToLower().Contains(sender.Text.ToLower()))
                    {
                        VisualizerGrid.SelectedItems.Add(Item);
                        FoundCount = FoundCount + 1;
                        FoundItem = Item;
                    }
                }

                if (FoundCount == 1)
                {
                    VisualizerGrid.ScrollIntoView(FoundItem, VisualizerGrid.Columns[0]);
                }
            }
            else
            {
                VisualizerGrid.SelectedItems.Clear();
            }
        }

        private void TableName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConvertTable();
        }

        private void Include_Click(object sender, RoutedEventArgs e)
        {
            ConvertTable();
        }


    }
}
