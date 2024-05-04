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
            if (ModelToggleButton.IsChecked == true) { ConvertToModel(); }
            if (ConfigurationToggleButton.IsChecked == true) { ConvertToConfiguration(); }
            if (DTOToggleButton.IsChecked == true) { ConvertToDto(); }
        }

        private void ConvertToModel()
        {
            Output.SetText("");

            string Objects = "";
            foreach (DesignItem Item in DesignItems)
            {
                if (Item.ColumnName != "")
                {
                    string AllowNull = "";
                    if (Item.AllowNulls == true) { AllowNull = "?"; }

                    string ColumnName = Item.ColumnName.Trim();
                    string ColumnType = Item.DataType.Trim();

                    Objects = Objects +
                        "\t" + @"/// <summary>" + "\n" +
                        "\t" + @"/// " + ColumnName + " - " + ColumnType + "\n" +
                        "\t" + @"/// </summary>" + "\n" +
                        "\t" + "public " + ConvertSQLType(ColumnType) + AllowNull + " " + ColumnName + " { get; set; }" + "\n" + "\n";
                }
            }

            string Body = @"/// <summary>" + "\n" +
                                @"/// dbo." + TableName.Text + "\n" +
                                @"/// </summary>" + "\n" +
                                "public class " + TableName.Text + "\n" + "{" + "\n" +
                                Objects +
                                "\n" + "}";

            Output.SetText(Body);

            CopyOutput_Click(null, null);
        }

        private void ConvertToConfiguration()
        {
            Output.SetText("");

            string Body = "";
            foreach (DesignItem Item in DesignItems)
            {
                if (Item.ColumnName.Trim() != "")
                {
                    string ColumnName = Item.ColumnName.Trim();

                    Body = Body +
                        $"\t builder.Property(s => s.{ColumnName}) \n" +
                        $"\t \t .HasColumnName(\"{ColumnName}\"); \n \n";
                }
            }

            Output.SetText(Body);
            CopyOutput_Click(null, null);
        }


        private void ConvertToDto()
        {
            Output.SetText("");

            string Objects = "";
            foreach (DesignItem Item in DesignItems)
            {
                if (Item.ColumnName.Trim() != "")
                {
                    string AllowNull = "";
                    if (Item.AllowNulls == true) { AllowNull = "?"; }

                    string ColumnName = Item.ColumnName.Trim();
                    string ColumnType = ConvertSQLType(Item.DataType.Trim(), true);

                    string Trim = "";
                    if (ColumnType == "string") { Trim = ".Trim()"; }

                    if (ColumnName.ToLower() == "override") { ColumnName = "_Override"; }

                    Objects = Objects +
                    "\t" + $"[JsonPropertyName(\"{ColumnName}\")]" + "\n" +
                    "\t" + "public " + ColumnType + AllowNull + " " + ColumnName + "\n" +
                    "\t" + "{" + "\n" +
                    "\t" + "\t" + "get { return " + ColumnName.ToLower() + "; }" + "\n" +
                    "\t" + "\t" + "set { " + "\n" +
                    "\t" + "\t" + "\t" + "if (" + ColumnName.ToLower() + " != value" + Trim + ")" + "\n" +
                    "\t" + "\t" + "\t" + "{" + "\n" +
                    "\t" + "\t" + "\t" + "\t" + ColumnName.ToLower() + " = value" + Trim + ";" + "\n" +
                    "\t" + "\t" + "\t" + "\t" + $"NotifyPropertyChanged(\"{ColumnName}\");" + "\n" +
                    "\t" + "\t" + "\t" + "}" + "\n" +
                    "\t" + "\t" + "}" + "\n" +
                    "\t" + "}" + "\n" +
                    "private " + ColumnType + AllowNull + " " + ColumnName.ToLower() + "; " + "\n" + "\n" + "\n";
                }
            }

            string Body = @"/// <summary>" + "\n" +
                                @"/// dbo." + TableName.Text + "\n" +
                                @"/// </summary>" + "\n" +
                                "public class " + TableName.Text + "Dto" + " : INotifyPropertyChanged" + "\n" + "{" + "\n" +
                                Objects +

                                "\t" + "public event PropertyChangedEventHandler PropertyChanged;" + "\n" +
                                "\t" + "public void NotifyPropertyChanged(string propertyName)" + "\n" +
                                "\t" + "{" + "\n" +
                                "\t" + "\t" + "PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));" + "\n" +
                                "\t" + "}" +

                                "\n" + "}";

            Output.SetText(Body);
            CopyOutput_Click(null, null);
        }

        private string ConvertSQLType(string sqlType, bool Dto = false)
        {
            string CType = "";

            if (sqlType.StartsWith("varbinary")) { CType = "byte"; }
            else if (sqlType.StartsWith("binary")) { CType = "byte"; }
            else if (sqlType.StartsWith("varchar")) { CType = "string"; }
            else if (sqlType.StartsWith("char")) { CType = "string"; }
            else if (sqlType.StartsWith("nvarchar")) { CType = "string"; }
            else if (sqlType.StartsWith("nchar")) { CType = "string"; }
            else if (sqlType.StartsWith("text")) { CType = "string"; }
            else if (sqlType.StartsWith("ntext")) { CType = "string"; }
            else if (sqlType.StartsWith("bit")) { CType = "bool"; }
            else if (sqlType.StartsWith("tinyint")) { CType = "byte"; }
            else if (sqlType.StartsWith("smallint")) { CType = "Int16"; }
            else if (sqlType.StartsWith("int")) { CType = "int"; }
            else if (sqlType.StartsWith("bigint")) { CType = "Int64"; }
            else if (sqlType.StartsWith("smallmoney")) { CType = "decimal"; }
            else if (sqlType.StartsWith("money")) { CType = "decimal"; }
            else if (sqlType.StartsWith("numeric")) { CType = "decimal"; }
            else if (sqlType.StartsWith("decimal")) { CType = "decimal"; }
            else if (sqlType.StartsWith("datetime")) { CType = "DateTime"; }
            else if (sqlType.StartsWith("float")) { CType = "double"; }
            else if (sqlType.StartsWith("real")) { CType = "Single"; }
            else if (sqlType.StartsWith("smalldatetime")) { CType = "DateTime"; }
            else if (sqlType.StartsWith("sql_variant")) { CType = "Object"; }

            if (Dto == true) { if (sqlType.StartsWith("smallint")) { CType = "int"; } }
            if (Dto == true) { if (sqlType.StartsWith("bigint")) { CType = "int"; } }

            return CType;
        }

        private void ClearOutput_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }


    public partial class DesignItem : ObservableObject
    {

        [ObservableProperty]
        private string columnName = string.Empty;

        [ObservableProperty]
        private string dataType = string.Empty;

        [ObservableProperty]
        private bool allowNulls = false;
    }
}
