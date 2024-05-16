using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
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
using System.Text.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace EFToolkit
{


    public static class Toolkit
    {

        #region Table Converter

        /// <summary>
        /// Converts a SQL Designer Table to C# Entity Framework Model
        /// </summary>
        /// <param name="DesignItems"></param>
        /// <returns></returns>
        public static string ConvertTableToModel(IList<DesignItem> DesignItems, string TableName)
        {
            string Objects = "";
            foreach (DesignItem Item in DesignItems)
            {
                if (Item.ColumnName != "")
                {
                    string AllowNull = "";
                    if (Item.AllowNulls == true) { AllowNull = "?"; }

                    string ColumnName = Item.ColumnName.Trim();
                    string ColumnType = Item.DataType.Trim();
                    string ObjectName = Item.ObjectName.Trim();

                    Objects = Objects +
                        "\t" + @"/// <summary>" + "\n" +
                        "\t" + @"/// " + ColumnName + " - " + ColumnType + "\n" +
                        "\t" + @"/// </summary>" + "\n" +
                        "\t" + "public " + ConvertSQLType(ColumnType) + AllowNull + " " + ObjectName + " { get; set; }" + "\n" + "\n";
                }
            }

            string Body = @"/// <summary>" + "\n" +
                                @"/// dbo." + TableName + "\n" +
                                @"/// </summary>" + "\n" +
                                "public class " + TableName + "\n" + "{" + "\n" +
                                Objects +
                                "\n" + "}";

            return Body;
        }

        /// <summary>
        /// Converts a SQL Designer Table to C# Entity Framework Configuration
        /// </summary>
        /// <param name="DesignItems"></param>
        /// <returns></returns>
        public static string ConvertTableToConfiguration(ObservableCollection<DesignItem> DesignItems)
        {
            string Body = "";
            foreach (DesignItem Item in DesignItems)
            {
                if (Item.ColumnName.Trim() != "")
                {
                    string ColumnName = Item.ColumnName.Trim();
                    string ObjectName = Item.ObjectName.Trim();

                    Body = Body +
                        $"\t builder.Property(s => s.{ObjectName}) \n" +
                        $"\t \t .HasColumnName(\"{ColumnName}\"); \n \n";
                }
            }

            return Body;
        }

        /// <summary>
        /// Converts a SQL Designer Table to C# Entity Framework Configuration
        /// </summary>
        /// <param name="DesignItems"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static string ConvertTableToDto(ObservableCollection<DesignItem> DesignItems, string TableName, DTO_Options Options = DTO_Options.MVVM)
        {
            string Objects = "";

            if (Options == DTO_Options.Standard)
            {
                foreach (DesignItem Item in DesignItems)
                {
                    if (Item.ColumnName.Trim() != "")
                    {
                        string AllowNull = "";
                        if (Item.AllowNulls == true) { AllowNull = "?"; }

                        string ColumnName = Item.ColumnName.Trim();
                        string ColumnType = ConvertSQLType(Item.DataType.Trim(), true);
                        string ObjectName = Item.ObjectName.Trim();

                        if (ColumnName.ToLower() == "override") { ColumnName = "_Override"; }

                        Objects = Objects +
                        "\t" + $"[JsonPropertyName(\"{ObjectName}\")]" + "\n" +
                        "\t" + "public " + ColumnType + AllowNull + " " + ObjectName + " { get; set; } \n \n \n";
                    }
                }

                string Body = @"/// <summary>" + "\n" +
                                    @"/// dbo." + TableName + "\n" +
                                    @"/// </summary>" + "\n" +
                                    "public class " + TableName + "Dto" + "\n" + "{" + "\n \n" +
                                    Objects +
                                    "}";

                return Body;
            }
            else if (Options == DTO_Options.INotifyPropertyChanged)
            {
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
                        "\t private " + ColumnType + AllowNull + " " + ColumnName.ToLower() + "; " + "\n" + "\n" + "\n";
                    }
                }

                string Body = @"/// <summary>" + "\n" +
                                    @"/// dbo." + TableName + "\n" +
                                    @"/// </summary>" + "\n" +
                                    "public class " + TableName + "Dto" + " : INotifyPropertyChanged" + "\n" + "{" + "\n" +
                                    Objects +

                                    "\t" + "public event PropertyChangedEventHandler PropertyChanged;" + "\n" +
                                    "\t" + "public void NotifyPropertyChanged(string propertyName)" + "\n" +
                                    "\t" + "{" + "\n" +
                                    "\t" + "\t" + "PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));" + "\n" +
                                    "\t" + "}" +

                                    "\n" + "}";

                return Body;
            }
            else if (Options == DTO_Options.MVVM)
            {

                foreach (DesignItem Item in DesignItems)
                {
                    if (Item.ColumnName.Trim() != "")
                    {
                        string AllowNull = "";
                        if (Item.AllowNulls == true) { AllowNull = "?"; }

                        string ColumnName = Item.ColumnName.Trim();
                        ColumnName = char.ToLowerInvariant(ColumnName[0]) + ColumnName.Substring(1);
                        if (ColumnName.ToLower() == "override") { ColumnName = "_Override"; }

                        string ColumnType = ConvertSQLType(Item.DataType.Trim(), true);

                        Objects = Objects +
                        "\t" + $"[JsonPropertyName(\"{ColumnName}\")]" + "\n" +
                        "\t" + $"[ObservableProperty]" + "\n" +
                        "\t" + "private " + ColumnType + AllowNull + " " + ColumnName + "; \n \n \n";
                    }
                }

                string Body = @"/// <summary>" + "\n" +
                                    @"/// dbo." + TableName + "\n" +
                                    @"/// </summary>" + "\n" +
                                    "public partial class " + TableName + "Dto" + " : ObservableObject" + "\n" + "{" + "\n" +
                                    Objects +
                                    "}";

                return Body;
            }

            return "";
        }


        /// <summary>
        /// Converts SQL Types to C# Equivalent
        /// </summary>
        /// <param name="sqlType"></param>
        /// <param name="Dto"></param>
        /// <returns></returns>
        private static string ConvertSQLType(string sqlType, bool Dto = false)
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
            else if (sqlType.StartsWith("sql_variant")) { CType = "object"; }

            if (Dto == true) { if (sqlType.StartsWith("smallint")) { CType = "int"; } }
            if (Dto == true) { if (sqlType.StartsWith("bigint")) { CType = "int"; } }

            return CType;
        }

        #endregion

        #region Select Describer


        public static string ConvertSelectToDescriber(string SelectStatement)
        {           
            string Prefix = "EXEC sp_describe_first_result_set @tsql = N' \n";

            string Body = SelectStatement.Replace("'", "") + "' \n";

            string Suffix = ", @params = NULL, @browse_information_mode = 0;";

            string DescribeCommand = Prefix + Body + Suffix;
            return DescribeCommand;
        }

        #endregion


        #region AcronymTranslator

        public static ObservableCollection<AcronymLibrary> AcronymLibraries = new();
        public static string AcronymLibraryFileName = "AcronymLibraries.efal";

        public static async void LoadLibaries()
        {
            StorageFolder Folder = ApplicationData.Current.LocalFolder;

            if (File.Exists(Folder.Path + "\\" + AcronymLibraryFileName))
            {
                StorageFile file = await Folder.GetFileAsync("AcronymLibraries.efal");
                var Libraries = JsonSerializer.Deserialize<ObservableCollection<AcronymLibrary>>(File.ReadAllText(file.Path));
                if (Libraries != null)
                {
                    AcronymLibraries = Libraries;
                }
            }
        }

        public static async void SaveLibraries()
        {
            StorageFolder Folder = ApplicationData.Current.LocalFolder;

            StorageFile file = await Folder.CreateFileAsync("AcronymLibraries.efal", CreationCollisionOption.OpenIfExists);
            var Json = JsonSerializer.Serialize(AcronymLibraries);
            File.WriteAllText(file.Path, Json);
        }

        public static string ConvertSQLColumnName(string SQLColumnName)
        {

            foreach (AcronymLibrary library in AcronymLibraries)
            {
                foreach (var Item in library.LibraryItems)
                {
                    if (Item.Acronym != null)
                    {
                        if (SQLColumnName.Contains(Item.Acronym))
                        {
                            SQLColumnName = SQLColumnName.Replace(Item.Acronym, Item.Translation);
                        }
                    }
                }
            }

            return SQLColumnName;
        }



        #endregion

        #region ModelFixer

        /// <summary>
        /// Adds MVVM or INotifyPropertyChanged to an existing model or switches them.
        /// </summary>
        /// <param name="Options"></param>
        /// <returns></returns>
        public static string ConvertModel(DTO_Options Options = DTO_Options.INotifyPropertyChanged)
        {



            return "";
        }

        #endregion
    }


    public partial class DesignItem : ObservableObject
    {

        [ObservableProperty]
        private string columnName = string.Empty;

        [ObservableProperty]
        private string dataType = string.Empty;

        [ObservableProperty]
        private bool allowNulls = false;

        [ObservableProperty]
        private string defaultValue = string.Empty;

        [ObservableProperty]
        private string objectName = string.Empty;

    }

    public partial class AcronymLibrary : ObservableObject
    {

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private ObservableCollection<AcronymItem> libraryItems = new();

    }

    public partial class AcronymItem : ObservableObject
    {

        [ObservableProperty]
        private string acronym;

        [ObservableProperty]
        private string translation;

    }




    public enum DTO_Options
    {
        Standard,
        INotifyPropertyChanged,
        MVVM,
    }

    public enum CodeFormatOptions
    {
        CamelCase,
        Snake_Case,
    }



}
