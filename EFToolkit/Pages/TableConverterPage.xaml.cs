using CommunityToolkit.WinUI;
using EFToolkit.Controls.Dialogs;
using EFToolkit.Controls.Widgets;
using EFToolkit.Extensions;
using EFToolkit.Models;
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
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;



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
        List<string> TableList = new();

        public TableConverterPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            TableName.SuggestedItemsSource = Toolkit.SchemaItems;
            TableName.ItemsSource = Toolkit.SelectedSchemaItems;

            AcronymLibrarySelector.SuggestedItemsSource = Toolkit.AcronymLibraries;
            AcronymLibrarySelector.ItemsSource = Toolkit.SelectedAcronymLibraries;

            OriginalDataGridStyle = DesignerGrid.RowStyle;

            InitializeDatabaseItems();
        }

        public Style OriginalDataGridStyle;


        private async void DesignerGrid_KeyUp(object sender, KeyRoutedEventArgs e)
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
            else if (e.Key == Windows.System.VirtualKey.Enter)
            {
                DesignerGrid.SelectedIndex = DesignerGrid.SelectedIndex - 1;
                if (DesignerGrid.RowStyle == (Style)App.Current.Resources["BasicDataGridRowStyle"])
                {
                    DesignerGrid.RowStyle = OriginalDataGridStyle;
                }
                else
                {
                    DesignerGrid.RowStyle = (Style)App.Current.Resources["BasicDataGridRowStyle"];
                }
            }
            else if (e.Key == Windows.System.VirtualKey.Up || e.Key == Windows.System.VirtualKey.Down)
            {

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

        private async void ClearTable_Click(object sender, RoutedEventArgs e)
        {
            var result = await ConfirmBox.Show("This action cannot be undone", "Clear Table?");
            if (result == ContentDialogResult.Primary)
            {
                DesignItems.Clear();
                DesignItemCount.Text = DesignItems.Count().ToString();
            }
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
                await Output.SetText(Toolkit.ConvertTableToDto(DesignItems, TableName.Value, ClassName.Text, Settings.Current.DTOModelOptions));
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

        private async void RearrangeButton_Click(object sender, RoutedEventArgs e)
        {
            await RearrangeDialog.ShowAsync();
            ConvertTable();
        }

        private void DesignerGrid_LayoutUpdated(object sender, object e)
        {
            if (DesignerGrid.ItemsSource != null)
            {
                var items = (ObservableCollection<DesignItem>)DesignerGrid.ItemsSource;
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].Index = (i + 1);
                    items[i].RearrangeText = items[i].Index + ": " + 
                        items[i].ObjectName + 
                        " - " + 
                        items[i].DataType;
                }
            }
        }


        private void InitializeDatabaseItems()
        {
            if (Toolkit.DatabaseItems.Count > 0)
            {
                TableMenu.Items.Clear();
                SelectButton.Visibility = Visibility.Visible;

                foreach (DatabaseItem item in Toolkit.DatabaseItems)
                {
                    var MenuItem = new MenuFlyoutItem()
                    {
                        Text = item.Title,
                        Tag = item,
                    };
                    MenuItem.Click += MenuItem_Click;
                    TableMenu.Items.Add(MenuItem);
                }
            }
        }

        private async void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menu = (MenuFlyoutItem)sender;
            DatabaseItem DatabaseItem = (DatabaseItem)menu.Tag;

            var Database = Toolkit.DatabaseItems.Where(x => x.Title == DatabaseItem.Title).FirstOrDefault();

            bool ManualCredentialInput = false;
            bool UsernameProvided = false;
            if (!string.IsNullOrEmpty(Database.UserId)) { UsernameProvided = true; }

            if (string.IsNullOrEmpty(Database.Password)) 
            {
                var Credentials = await CredentialBox.Show($"{Database.Title} Credentials", Database.UserId);
                if (Credentials != null) 
                {
                    Database.UserId = Credentials.Username;
                    Database.Password = Credentials.Password;
                    ManualCredentialInput = true;                  
                }
                else { return; }
            }

            using (SqlConnection connection = new SqlConnection(Database.GetConnectionString()))
            {
                //Clear the username and password if user manually input them so it doesn't save later;
                if (ManualCredentialInput == true) { Database.Password = ""; }
                if (UsernameProvided == false) { Database.UserId = ""; }

                //Open the connection
                try { await connection.OpenAsync(); } 
                catch { await MessageBox.Show("Connection to the database could not be established " +
                    "please check your connection string and try again.", "Connection Error"); return; }


                //Get list of tables and show to user for them to select
                DataTable schema = connection.GetSchema("Tables");
                TableList = new List<string>();
                foreach (DataRow row in schema.Rows)
                {
                    TableList.Add(row[2].ToString());
                }
                TableList.Sort();

                TableListView.ItemsSource = TableList;
                await TableSelectDialog.ShowAsync();
                if (TableListView.SelectedItem == null) { return; }
                string tableName = TableListView.SelectedItem.ToString();

                TableFilter.Text = "";

                //Get Table Schema 
                String[] columnRestrictions = new String[4];
                columnRestrictions[2] = tableName;
                DataTable allColumnsSchemaTable = connection.GetSchema("Columns", columnRestrictions);
                string Schema = allColumnsSchemaTable.Rows[0]["TABLE_SCHEMA"].ToString() + ".";

                var SchemaItem = Toolkit.SchemaItems.Where(x => x.Schema == Schema).FirstOrDefault();
                if (SchemaItem == null)
                {
                    SchemaItem = new SchemaItem() { Schema = Schema };
                    Toolkit.SchemaItems.Add(SchemaItem);
                }
                Toolkit.SelectedSchemaItems.Clear();
                Toolkit.SelectedSchemaItems.Add(SchemaItem);
                Toolkit.SaveData();

                //Get column information data 
                string script = $@"EXEC sp_describe_first_result_set @tsql = N' 
                                    select Top 1 * From {Schema}{tableName}
                                    ' 
                                    , @params = NULL, @browse_information_mode = 0;";
                SqlCommand cmd = new SqlCommand(script, connection);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable Table = new DataTable();
                try { da.Fill(Table); } 
                catch (Exception Ex) { await MessageBox.Show(Ex.Message.ToString(), 
                    "An error occurred populating the table."); return; }

                //Clear any previous design items
                DesignItems.Clear();
                await Output.SetText("");

                //Populate new Design Items
                foreach (DataRow row in Table.Rows)
                {
                    var columnName = row.Field<string>("name");
                    var dataType = row.Field<string>("system_type_name");

                    var allowNulls = row.Field<bool>("is_nullable");
                    var isIdentity = row.Field<bool>("is_identity_column");

                    DesignItems.Add(new DesignItem()
                    {
                        ColumnName = columnName,
                        ObjectName = Toolkit.ConvertSQLColumnName(columnName),                      
                        DataType = dataType,
                        AllowNulls = allowNulls,   
                        IsPrimaryKey = isIdentity,
                    });
                }



                TableName.Text = tableName;
                ClassName.Text = Toolkit.ConvertSQLColumnName(TableName.Text);

                DesignerGrid.ItemsSource = DesignItems;
                DesignItemCount.Text = DesignItems.Count().ToString();

                ConvertTable();

                connection.Close();
                connection.Dispose();
            }
            
        }


        private void TableFilterSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (TableFilter.Text.Length > 1)
                {
                    List<string> filteredList = new List<string>();
                    foreach (string filter in TableList)
                    {
                        if (filter.Contains(TableFilter.Text.Trim(), StringComparison.CurrentCultureIgnoreCase))
                        {
                            filteredList.Add(filter);
                        }
                    }

                    TableListView.ItemsSource = filteredList.Distinct();
                }
                else if (SearchBox.Text == "")
                {
                    TableListView.ItemsSource = TableList;
                }
            }
        }

        private void TableListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TableListView.SelectedItems.Count > 0)
            {
                TableSelectDialog.IsPrimaryButtonEnabled = true;
            }
            else { TableSelectDialog.IsPrimaryButtonEnabled = false; }
        }

        private void TableSelectDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            TableListView.SelectedItem = null;
        }

        List<DesignItem?> FoundItems = new();
        int SearchFoundIndex = 0;

        private void RearrangeSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && sender.Text.Length > 1)
            {
                RearrangeListView.DeselectAll();

                FoundItems = new();
                for (int i = 0; i < RearrangeListView.Items.Count; i++)
                {
                    DesignItem item = (DesignItem)RearrangeListView.Items[i];
                    if (!string.IsNullOrEmpty(item.ObjectName) || !string.IsNullOrEmpty(item.ColumnName))
                    {
                        if (item.ObjectName.ToLower().Contains(sender.Text.ToLower()) ||
                            item.ColumnName.ToLower().Contains(sender.Text.ToLower()))
                        {
                            RearrangeListView.SelectRange(new ItemIndexRange(i, 1));
                            FoundItems.Add(item);
                        }
                    }
                }

                if (FoundItems.Count >= 1)
                {
                    RearrangeListView.ScrollIntoView(FoundItems[0]);
                }

                RearrangeSearch.FoundCount = FoundItems.Count;
            }
            else
            {
                RearrangeListView.DeselectAll();
                RearrangeListView.SelectedIndex = -1;
                RearrangeSearch.FoundCount = 0;
            }
        }

        private void FindUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchFoundIndex <= FoundItems.Count)
            {
                if (SearchFoundIndex != 0) { SearchFoundIndex--; }
                RearrangeListView.ScrollIntoView(FoundItems[SearchFoundIndex]);
            }
        }

        private void FindDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchFoundIndex <= FoundItems.Count)
            {
                if (SearchFoundIndex != FoundItems.Count - 1) { SearchFoundIndex++; }
                RearrangeListView.ScrollIntoView(FoundItems[SearchFoundIndex]);
            }
        }

    }

}
