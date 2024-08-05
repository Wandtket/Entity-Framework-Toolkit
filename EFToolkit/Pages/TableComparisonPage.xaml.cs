using CommunityToolkit.WinUI.UI.Controls;
using EFToolkit.Controls.Dialogs;
using EFToolkit.Extensions;
using EFToolkit.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.UI;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EFToolkit.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TableComparisonPage : Page
    {
        DatabaseItem? SelectedPrimaryDatabase = null;
        DatabaseItem? SelectedSecondaryDatabase = null;

        ObservableCollection<TableItem> TableList = new();


        public TableComparisonPage()
        {
            this.InitializeComponent();
        }

        public void ToggleTeachTips(bool Toggle)
        {
            SelectPrimaryTeachTip.IsOpen = Toggle;
            SelectSecondaryTeachTip.IsOpen = Toggle;
            QueryTablesTeachTip.IsOpen = Toggle;
            TableListViewTeachTip.IsOpen = Toggle;
            PrimaryDesignerGridTeachTip.IsOpen = Toggle;
            SecondaryDesignerGridTeachTip.IsOpen = Toggle;
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            InitializeDatabaseItems();
        }

        private void InitializeDatabaseItems()
        {
            if (Toolkit.DatabaseItems.Count > 0)
            {
                PrimaryTableMenu.Items.Clear();
                SecondaryTableMenu.Items.Clear();

                SelectPrimaryButton.IsEnabled = true;

                foreach (DatabaseItem item in Toolkit.DatabaseItems)
                {
                    var PrimaryMenuItem = new RadioMenuFlyoutItem()
                    {
                        Text = item.Title,
                        Tag = item,
                    };
                    var SecondaryMenuItem = new RadioMenuFlyoutItem()
                    {
                        Text = item.Title,
                        Tag = item,
                    };                   

                    PrimaryMenuItem.Click += PrimaryMenuItem_Click;
                    SecondaryMenuItem.Click += SecondaryMenuItem_Click;

                    PrimaryTableMenu.Items.Add(PrimaryMenuItem);
                    SecondaryTableMenu.Items.Add(SecondaryMenuItem);
                }
            }
        }


        private async void PrimaryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (PrimaryTableMenu.Items.Count == 0)
            {
                await MessageBox.Show("You must add at least two databases in the database library to use this feature.", "NOTICE");
                return;
            }

            Reset();

            RadioMenuFlyoutItem menu = (RadioMenuFlyoutItem)sender;
            DatabaseItem DatabaseItem = (DatabaseItem)menu.Tag;

            SelectedPrimaryDatabase = DatabaseItem;
            SelectSecondaryButton.IsEnabled = true;

            foreach (var SecondItem in SecondaryTableMenu.Items) { SecondItem.IsEnabled = true; }

            var secondmenu = SecondaryTableMenu.Items.Where(x => ((RadioMenuFlyoutItem)x).Text == menu.Text).FirstOrDefault();
            if (secondmenu != null) { secondmenu.IsEnabled = false; }

            PrimaryDatabaseTitle.Text = DatabaseItem.Title;
        }

        private void SecondaryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RadioMenuFlyoutItem menu = (RadioMenuFlyoutItem)sender;
            DatabaseItem DatabaseItem = (DatabaseItem)menu.Tag;

            Reset();

            SelectedSecondaryDatabase = DatabaseItem;

            QueryTablesButton.IsEnabled = true;

            SecondaryDatabaseTitle.Text = DatabaseItem.Title;
        }


        private void QueryTablesButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPrimaryDatabase == null) { return; }
            if (SelectedSecondaryDatabase == null) { return; }

            FetchTables();
        }


        private async void FetchTables()
        {
            Reset();

            //Ask for credentials for primary database
            var PrimaryDatabase = Toolkit.DatabaseItems.Where(x => x.Title == SelectedPrimaryDatabase.Title).FirstOrDefault();

            bool PrimaryManualCredentialInput = false;
            bool PrimaryUsernameProvided = false;
            if (!string.IsNullOrEmpty(PrimaryDatabase.UserId)) { PrimaryUsernameProvided = true; }

            if (string.IsNullOrEmpty(PrimaryDatabase.Password))
            {
                var Credentials = await CredentialBox.Show($"{PrimaryDatabase.Title} Credentials", PrimaryDatabase.UserId);
                if (Credentials != null)
                {
                    PrimaryDatabase.UserId = Credentials.Username;
                    PrimaryDatabase.Password = Credentials.Password;
                    PrimaryManualCredentialInput = true;
                }
                else { return; }
            }

            //Ask for credentials for secondary database
            var SecondaryDatabase = Toolkit.DatabaseItems.Where(x => x.Title == SelectedSecondaryDatabase.Title).FirstOrDefault();

            bool SecondaryManualCredentialInput = false;
            bool SecondaryUsernameProvided = false;
            if (!string.IsNullOrEmpty(SecondaryDatabase.UserId)) { SecondaryUsernameProvided = true; }

            if (string.IsNullOrEmpty(SecondaryDatabase.Password))
            {
                var Credentials = await CredentialBox.Show($"{SecondaryDatabase.Title} Credentials", SecondaryDatabase.UserId);
                if (Credentials != null)
                {
                    SecondaryDatabase.UserId = Credentials.Username;
                    SecondaryDatabase.Password = Credentials.Password;
                    SecondaryManualCredentialInput = true;
                }
                else { return; }
            }


            //Fetch table data for primary connection
            using SqlConnection PrimaryConnection = new SqlConnection(SelectedPrimaryDatabase.GetConnectionString());
            {
                //Clear the username and password if user manually input them so it doesn't save later;
                if (PrimaryManualCredentialInput == true) { PrimaryDatabase.Password = ""; }
                if (PrimaryUsernameProvided == false) { PrimaryDatabase.UserId = ""; }

                //Open the connection
                try { await PrimaryConnection.OpenAsync(); }
                catch
                {
                    await MessageBox.Show("Connection to the database could not be established " +
                    "please check your connection string and try again.", "Connection Error"); return;
                }

                //Get list of tables and show to user for them to select
                DataTable schema = PrimaryConnection.GetSchema("Tables");

                string Script = "";
                List<string> TableNames = new();

                //SQL should execute in alphabetical order
                foreach (DataRow row in schema.Rows) { TableNames.Add(row[2].ToString()); }
                TableNames = new List<string>(TableNames.Order());

                //Generating stored procedures in alphabetical order
                foreach (string TableName in TableNames)
                {
                    Script = Script + $@"EXEC sp_describe_first_result_set @tsql = N' 
                                        select Top 1 * From [{TableName}]
                                        ' 
                                        , @params = NULL, @browse_information_mode = 0;" + "\n\n";
                }


                TableNames = new List<string>(TableNames.Order());

                SqlCommand cmd = new SqlCommand(Script, PrimaryConnection);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                using (var reader = cmd.ExecuteReader(CommandBehavior.KeyInfo))
                {
                    int Index = 0;
                    while (!reader.IsClosed)
                    {                                       
                        try { ds.Tables.Add(TableNames[Index]).Load(reader); Index++; } catch { Index++; continue; }                        
                    }
                }

                foreach (DataTable Table in ds.Tables)
                {
                    TableItem NewItem = new TableItem() { PrimaryTable = Table.TableName };

                    foreach (DataRow designrow in Table.Rows)
                    {
                        var columnName = designrow.Field<string>("name");
                        var dataType = designrow.Field<string>("system_type_name");

                        var allowNulls = designrow.Field<bool>("is_nullable");
                        var isIdentity = designrow.Field<bool>("is_identity_column");

                        NewItem.PrimaryDesignItems.Add(new DesignItem()
                        {
                            ColumnName = columnName,
                            DataType = dataType,
                            AllowNulls = allowNulls,
                            IsPrimaryKey = isIdentity,
                        });
                    }

                    TableList.Add(NewItem);
                }

            }

            //Fetch table data for secondary connection
            using SqlConnection SecondaryConnection = new SqlConnection(SelectedSecondaryDatabase.GetConnectionString());
            {
                //Clear the username and password if user manually input them so it doesn't save later;
                if (SecondaryManualCredentialInput == true) { SecondaryDatabase.Password = ""; }
                if (SecondaryUsernameProvided == false) { SecondaryDatabase.UserId = ""; }

                //Open the connection
                try { await SecondaryConnection.OpenAsync(); }
                catch
                {
                    await MessageBox.Show("Connection to the database could not be established " +
                    "please check your connection string and try again.", "Connection Error"); return;
                }

                //Get list of tables and show to user for them to select
                DataTable schema = SecondaryConnection.GetSchema("Tables");

                string Script = "";
                List<string> TableNames = new();

                //SQL should execute in alphabetical order
                foreach (DataRow row in schema.Rows) { TableNames.Add(row[2].ToString()); }
                TableNames = new List<string>(TableNames.Order());

                //Generating stored procedures in alphabetical order
                foreach (string TableName in TableNames)
                {
                    Script = Script + $@"EXEC sp_describe_first_result_set @tsql = N' 
                                        select Top 1 * From [{TableName}]
                                        ' 
                                        , @params = NULL, @browse_information_mode = 0;" + "\n\n";
                }

                SqlCommand cmd = new SqlCommand(Script, SecondaryConnection);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                using (var reader = cmd.ExecuteReader(CommandBehavior.KeyInfo))
                {
                    int Index = 0;
                    while (!reader.IsClosed)
                    {
                        try { ds.Tables.Add(TableNames[Index]).Load(reader); Index++; } catch { Index++; continue; }
                    }
                }

                foreach (DataTable Table in ds.Tables)
                {
                    bool Exists = true;

                    var Item = TableList.Where(x => x.PrimaryTable == Table.TableName).FirstOrDefault();
                    if (Item == null)
                    {
                        Item = new TableItem()
                        {
                            SecondaryTable = Table.TableName,
                            ExistsInPrimary = false,
                            Matches = false,
                        };
                        Exists = false;
                    }
                    else
                    {
                        Item.SecondaryTable = Table.TableName;
                        Item.ExistsInPrimary = true;
                        Item.ExistsInSecondary = true;
                    }


                    foreach (DataRow designrow in Table.Rows)
                    {
                        var columnName = designrow.Field<string>("name");
                        var dataType = designrow.Field<string>("system_type_name");

                        var allowNulls = designrow.Field<bool>("is_nullable");
                        var isIdentity = designrow.Field<bool>("is_identity_column");

                        Item.SecondaryDesignItems.Add(new DesignItem()
                        {
                            ColumnName = columnName,
                            DataType = dataType,
                            AllowNulls = allowNulls,
                            IsPrimaryKey = isIdentity,
                        });
                    }

                    if (Exists == false)
                    {
                        TableList.Add(Item);
                    }
                }

            }


            //Compare Primary and Secondary
            foreach (TableItem Item in TableList)
            {
                if (Item.PrimaryTable != Item.SecondaryTable) { Item.Matches = false; }
                if (string.IsNullOrEmpty(Item.PrimaryTable)) { Item.ExistsInPrimary = false; }
                if (string.IsNullOrEmpty(Item.SecondaryTable)) { Item.ExistsInSecondary = false; }

                foreach (DesignItem design in Item.PrimaryDesignItems)
                {
                    var Exists = Item.SecondaryDesignItems.Where(x => x.ColumnName == design.ColumnName).FirstOrDefault();
                    if (Exists == null) { Item.Matches = false; }
                }

                foreach (DesignItem design in Item.SecondaryDesignItems)
                {
                    var Exists = Item.PrimaryDesignItems.Where(x => x.ColumnName == design.ColumnName).FirstOrDefault();
                    if (Exists == null) { Item.Matches = false; }
                }
            }


            TableList = new ObservableCollection<TableItem>(TableList.OrderBy(x => x.PrimaryTable));
            ApplyFilter();
        }

        private void Reset()
        {
            TableList.Clear();
            TableListView.ItemsSource = null;
            TableFilter.Text = "";
            PrimaryDesignerGrid.ItemsSource = null;
            SecondaryDesignerGrid.ItemsSource = null;
        }


        private void TableListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TableItem? SelectedItem = (TableItem)TableListView.SelectedItem;

            if (SelectedItem != null)
            {
                PrimaryDesignerGrid.ItemsSource = SelectedItem.PrimaryDesignItems;
                SecondaryDesignerGrid.ItemsSource = SelectedItem.SecondaryDesignItems;

                PrimaryDatabaseSelectedTableTitle.Text = SelectedItem.PrimaryTable;
                SecondaryDatabaseSelectedTableTitle.Text = SelectedItem.SecondaryTable;

                PrimaryDesignItemCount.Text = SelectedItem.PrimaryDesignItems.Count().ToString();
                SecondaryDesignItemCount.Text = SelectedItem.SecondaryDesignItems.Count().ToString();
            }
        }

        private void TableFilter_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput )
            {
                ApplyFilter();
            }
        }

        private void ApplyFilter()
        {
            ObservableCollection<TableItem> filteredList =
                    new(TableList.Where(x => x.PrimaryTable.Contains(TableFilter.Text.Trim(), StringComparison.CurrentCultureIgnoreCase) ||
                                        x.SecondaryTable.Contains(TableFilter.Text.Trim(), StringComparison.CurrentCultureIgnoreCase)));

            if (Missing.IsOn == true)
            {
                filteredList = new(filteredList.Where(x => string.IsNullOrEmpty(x.PrimaryTable) ||
                                string.IsNullOrEmpty(x.SecondaryTable)));
            }

            if (Matches.IsOn == true)
            {
                filteredList = new(filteredList.Where(x => x.Matches == false));
            }

            TableListView.ItemsSource = filteredList.Distinct();

            TotalTablesPrimary.Text = filteredList.Distinct().Where(x => !string.IsNullOrEmpty(x.PrimaryTable)).Count().ToString();
            TotalTablesSecondary.Text = filteredList.Distinct().Where(x => !string.IsNullOrEmpty(x.SecondaryTable)).Count().ToString();                      
            TotalTables.Text = filteredList.Distinct().Count().ToString();
        }

        private void Matches_Toggled(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void Missing_Toggled(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Reset();

            PrimaryTableMenu.Items.Clear();
            SecondaryTableMenu.Items.Clear();

            SelectSecondaryButton.IsEnabled = false;
            QueryTablesButton.IsEnabled = false;

            PrimaryDatabaseTitle.Text = "No Primary Database Selected";
            SecondaryDatabaseTitle.Text = "No Secondary Database Selected";

            PrimaryDatabaseSelectedTableTitle.Text = "";
            SecondaryDatabaseSelectedTableTitle.Text = "";

            TotalTablesPrimary.Text = "0";
            TotalTablesSecondary.Text = "0";
            TotalTables.Text = "0";

            PrimaryDesignItemCount.Text = "0";
            SecondaryDesignItemCount.Text = "0";
        }

        private async void OpenInExcel_Click(object sender, RoutedEventArgs e)
        {
            if (TableListView.ItemsSource != null)
            {
                IEnumerable<TableItem> DataSource = (IEnumerable<TableItem>)TableListView.ItemsSource;
                await DataSource.OpenInExcel(SelectedPrimaryDatabase.Title + " - " + SelectedSecondaryDatabase.Title + " Comparison");
            }
        }

        private async void ExportFile_Click(object sender, RoutedEventArgs e)
        {
            if (TableListView.ItemsSource != null)
            {
                IEnumerable<TableItem> DataSource = (IEnumerable<TableItem>)TableListView.ItemsSource;
                await DataSource.ExportToExcel(SelectedPrimaryDatabase.Title + " - " + SelectedSecondaryDatabase.Title + " Comparison");
            }
        }

        private async void PrimaryDesignerGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var Item = e.Row.DataContext as DesignItem;

            var Table = TableListView.SelectedItem as TableItem;

            var Comparison = Table.SecondaryDesignItems
                .Where(x => x.ColumnName == Item.ColumnName &&
                        x.DataType == Item.DataType &&
                        x.DefaultValue == Item.DefaultValue &&
                        x.IsPrimaryKey == Item.IsPrimaryKey &&
                        x.AllowNulls == Item.AllowNulls)
                .FirstOrDefault();

            if (Comparison == null)
            {
                e.Row.Background = new SolidColorBrush(Colors.Green.IsThemeAware());
            }
        }

        private void SecondaryDesignerGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var Item = e.Row.DataContext as DesignItem;

            var Table = TableListView.SelectedItem as TableItem;

            var Comparison = Table.PrimaryDesignItems
                .Where(x => x.ColumnName == Item.ColumnName &&
                        x.DataType == Item.DataType &&
                        x.DefaultValue == Item.DefaultValue &&
                        x.IsPrimaryKey == Item.IsPrimaryKey &&
                        x.AllowNulls == Item.AllowNulls)
                .FirstOrDefault();

            if (Comparison == null)
            {
                e.Row.Background = new SolidColorBrush(Colors.Green.IsThemeAware());
            }
        }

        private void CopyPrimary_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CopySecondary_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
