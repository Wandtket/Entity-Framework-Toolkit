using EFToolkit.Controls.Dialogs;
using EFToolkit.Extensions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EFToolkit.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DatabasePage : Page
    {

        public DatabasePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            DatabaseItemList.ItemsSource = Toolkit.DatabaseItems;
            DatabaseTotal.Text = DatabaseItemList.Items.Count.ToString();

            SchemaItemList.ItemsSource = Toolkit.SchemaItems;
            SchemasTotal.Text = SchemaItemList.Items.Count.ToString();
        }

        public void ToggleTeachTips(bool Toggle)
        {
            DatabaseTeachTip.IsOpen = Toggle;
            SchemaTeachTip.IsOpen = Toggle;
        }

        private void AddDatabase_Click(object sender, RoutedEventArgs e)
        {
            var Database = new DatabaseItem() { };

            Toolkit.DatabaseItems.Add(Database);
            DatabaseItemList.SelectedItem = Database;

            DatabaseTotal.Text = DatabaseItemList.Items.Count.ToString();
        }

        private async void SaveDatabase_Click(object sender, RoutedEventArgs e)
        {
            Toolkit.SaveDatabaseItems();
            await MessageBox.Show("Databases should save automatically but this button feels good to press sometimes...", "Databases Saved!");
        }


        private void AddSchema_Click(object sender, RoutedEventArgs e)
        {
            var Item = new SchemaItem() { };

            Toolkit.SchemaItems.Add(Item);
            SchemaItemList.SelectedItem = Item;

            SchemasTotal.Text = SchemaItemList.Items.Count.ToString();
        }

        private async void SaveSchema_Click(object sender, RoutedEventArgs e)
        {
            Toolkit.SaveSchemaItems();
            await MessageBox.Show("Libraries should save automatically but this button feels good to press sometimes...", "Libraries Saved!");
        }


        private async void RemoveDatabase_Click(object sender, RoutedEventArgs e)
        {
            var s = (FrameworkElement)sender;
            var d = s.DataContext;

            DatabaseItem Item = (DatabaseItem)DatabaseItemList.SelectedItem;
            Toolkit.DatabaseItems.Remove(Item);
            Toolkit.SaveDatabaseItems();

            DatabaseTotal.Text = DatabaseItemList.Items.Count.ToString();
        }

        private async void RemoveSchema_Click(object sender, RoutedEventArgs e)
        {
            var s = (FrameworkElement)sender;
            var d = s.DataContext;

            SchemaItem Item = (SchemaItem)SchemaItemList.SelectedItem;
            Toolkit.SchemaItems.Remove(Item);
            Toolkit.SaveSchemaItems();

            SchemasTotal.Text = SchemaItemList.Items.Count.ToString();
        }


        private async void ClearSchemas_Click(object sender, RoutedEventArgs e)
        {
            var Result = await ConfirmBox.Show("This action cannot be undone", "Clear Schemas?");
            if (Result == ContentDialogResult.Primary)
            {
                Toolkit.SchemaItems.Clear();
                Toolkit.SelectedSchemaItems.Clear();
                Toolkit.SaveSchemaItems();

                SchemasTotal.Text = SchemaItemList.Items.Count.ToString();
            }
        }


        private async void Database_LostFocus(object sender, RoutedEventArgs e)
        {
            Toolkit.SaveDatabaseItems();
        }

        private async void Schema_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            box.Text = box.Text.Trim().Replace(" ", "");

            SchemaItem SelectedItem = (SchemaItem)SchemaItemList.SelectedItem;

            var Duplicate = Toolkit.SchemaItems.Where(x => x.Schema == box.Text).Count();
            if (Duplicate > 1)
            {
                box.ShowError("Duplicate Found!", true, Duration: 5);

                var DuplicateList = Toolkit.SchemaItems.Where(x => x.Schema == box.Text);
                foreach (var item in DuplicateList)
                {
                    SchemaItemList.SelectedItems.Add(item);
                }
            }

            Toolkit.SaveSchemaItems();
        }

    }
}
