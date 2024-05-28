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
using EFToolkit.Controls.Widgets;
using System.Diagnostics;

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

        public void ToggleTeachTips(bool Toggle)
        {
            TableNameTeachTip.IsOpen = Toggle;
            SearchTeachTip.IsOpen = Toggle;
            ClassNameTeachTip.IsOpen = Toggle;
            OutputTypeTeachTip.IsOpen = Toggle;
            SQLTableTeachTip.IsOpen = Toggle;
            PastingTeachTip.IsOpen = Toggle;
            OutputTeachTip.IsOpen = Toggle;
            AcronymsTeachTip.IsOpen = Toggle;
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
                while (iRow < rowsInClipboard.Length)
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

                        //Visual Studio SQL Server Object Explorer has an empty column to account for
                        if (valuesInRow[0] == string.Empty && valuesInRow.Count() > 3)
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

                        if (!string.IsNullOrEmpty(valuesInRow[DataTypeColumnIndex]))
                        {
                            DesignItems.Add(new DesignItem()
                            {
                                ColumnName = valuesInRow[NameColumnIndex],
                                ObjectName = Toolkit.ConvertSQLColumnName(valuesInRow[NameColumnIndex]),
                                DataType = valuesInRow[DataTypeColumnIndex],
                                AllowNulls = AllowNulls,
                                DefaultValue = DefaultColumn,
                            });
                        }
                    }
                    //Pasting from Select Statement Describer
                    else
                    {
                        int NameColumnIndex = 2;
                        int DataTypeColumnIndex = 5;
                        int AllowNullsColumnIndex = 3;
                        int PrimaryKeyColumnIndex = 27;

                        bool AllowNulls = false;
                        try
                        {
                            if (valuesInRow[AllowNullsColumnIndex].ToLower() == "1") { AllowNulls = true; }
                            else if (valuesInRow[AllowNullsColumnIndex].ToLower() == "0") { AllowNulls = false; }
                        }
                        catch { await MessageBox.Show("There was an error copying the SQL Table, please copy the table and try again.", "Error"); return; }

                        bool PrimaryKey = false;
                        try
                        {
                            if (valuesInRow[PrimaryKeyColumnIndex].ToLower() == "1") { PrimaryKey = true; }
                            else if (valuesInRow[PrimaryKeyColumnIndex].ToLower() == "0") { PrimaryKey = false; }
                        } catch { }

                        //Skip row if copied with headers.
                        if (valuesInRow[DataTypeColumnIndex].ToLower() != "system_type_name"
                            && !string.IsNullOrEmpty(valuesInRow[DataTypeColumnIndex]))
                        {
                            DesignItems.Add(new DesignItem()
                            {
                                IsPrimaryKey = PrimaryKey,
                                ColumnName = valuesInRow[NameColumnIndex],
                                ObjectName = Toolkit.ConvertSQLColumnName(valuesInRow[NameColumnIndex]),
                                DataType = valuesInRow[DataTypeColumnIndex],
                                AllowNulls = AllowNulls,
                            });
                        }
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
                await Output.SetText(Toolkit.ConvertTableToConfiguration(DesignItems, TableName.Text, TableName.Value, ClassName.Text));
            }
            if (DTOToggleButton.IsChecked == true) 
            {
                await Output.SetText(Toolkit.ConvertTableToDto(DesignItems, TableName.Value, ClassName.Text, Settings.Current.DTO_Options));
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

        private async void SearchTable_TextChanged(SearchBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput 
                && sender.SearchStrings.Count != 0 || !string.IsNullOrEmpty(sender.Text))
            {
                FilterDesignGrid(sender);
            }
            else if (string.IsNullOrEmpty(sender.Text) && SearchBox.SearchStrings.Count == 0)
            {
                DesignerGrid.ItemsSource = DesignItems;
                sender.Focus(FocusState.Keyboard);
            }
        }

        private void SearchBox_TokenItemRemoved(SearchBox sender, object args)
        {
            FilterDesignGrid(sender);
        }


        private void FilterDesignGrid(SearchBox sender)
        {
            ObservableCollection<DesignItem> filteredList = new();

            if (sender.SearchStrings.Count >= 1)
            {
                foreach (var Search in sender.SearchStrings)
                {
                    var ColumnNames = DesignItems.Where(a => a.ColumnName.Contains(Search, StringComparison.CurrentCultureIgnoreCase)
                    && a.ColumnName.Contains(sender.Text, StringComparison.CurrentCultureIgnoreCase));

                    var ObjectNames = DesignItems.Where(a => a.ObjectName.Contains(Search, StringComparison.CurrentCultureIgnoreCase)
                    && a.ObjectName.Contains(sender.Text, StringComparison.CurrentCultureIgnoreCase));

                    var Combined = ColumnNames.Concat(ObjectNames);
                    filteredList = new ObservableCollection<DesignItem>(filteredList.Concat(Combined).Distinct());
                }
            }
            else
            {
                var ColumnNames = DesignItems.Where(a => a.ColumnName.Contains(sender.Text, StringComparison.CurrentCultureIgnoreCase));
                var ObjectNames = DesignItems.Where(a => a.ObjectName.Contains(sender.Text, StringComparison.CurrentCultureIgnoreCase));

                var Combined = ColumnNames.Concat(ObjectNames);
                filteredList = new ObservableCollection<DesignItem>(filteredList.Concat(Combined));
            }

            DesignerGrid.ItemsSource = filteredList;
        }



        private async void DesignerGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var s = (FrameworkElement)e.OriginalSource;
            var d = s.DataContext;

            DesignItem Item = (DesignItem)d;
            DesignerGrid.SelectedItem = Item;           

            MenuFlyout menuFlyout = new MenuFlyout();
            MenuFlyoutSeparator separator = new();

            MenuFlyoutItem CopyColumnItem = new MenuFlyoutItem { Text = "Copy SQL Column Name", Icon = new SymbolIcon(Symbol.Copy), Tag = Item };
            MenuFlyoutItem CopyObjectItem = new MenuFlyoutItem { Text = "Copy Object Name", Icon = new SymbolIcon(Symbol.Copy), Tag = Item };

            FontIcon KeyIcon = new FontIcon() { Glyph = "\uE192", FontFamily = new FontFamily("Segoe MDL2 Assets") };
            MenuFlyoutItem KeyItem = new MenuFlyoutItem { Text = "Set as Primary Key", Icon = KeyIcon, Tag = Item };

            KeyItem.Click += KeyItem_Click;
            CopyColumnItem.Click += CopyColumnItem_Click;
            CopyObjectItem.Click += CopyObjectItem_Click;

            menuFlyout.Items.Add(KeyItem);
            menuFlyout.Items.Add(separator);
            menuFlyout.Items.Add(CopyColumnItem);
            menuFlyout.Items.Add(CopyObjectItem);

            FrameworkElement? senderElement = sender as FrameworkElement;
            menuFlyout.Placement = FlyoutPlacementMode.Bottom;

            var tappedItem = (UIElement)e.OriginalSource;
            menuFlyout.ShowAt(tappedItem, e.GetPosition(tappedItem));
        }

        private void CopyObjectItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem flyoutItem = (MenuFlyoutItem)sender;
            DesignItem Item = (DesignItem)flyoutItem.Tag;

            DataPackage package = new();
            package.SetText(Item.ObjectName);
            Clipboard.SetContent(package);
        }

        private void CopyColumnItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem flyoutItem = (MenuFlyoutItem)sender;
            DesignItem Item = (DesignItem)flyoutItem.Tag;

            DataPackage package = new();
            package.SetText(Item.ColumnName);
            Clipboard.SetContent(package);
        }

        private void KeyItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem flyoutItem = (MenuFlyoutItem)sender;
            DesignItem Item = (DesignItem)flyoutItem.Tag;

            var OldKey = DesignItems.Where(x => x.IsPrimaryKey == true).FirstOrDefault();
            if (OldKey != null) { OldKey.IsPrimaryKey = false; }

            Item.IsPrimaryKey = true;
            ConvertTable();
        }

        private void DesignerGrid_CellEditEnded(object sender, CommunityToolkit.WinUI.UI.Controls.DataGridCellEditEndedEventArgs e)
        {
            ConvertTable();
        }

    }

}
