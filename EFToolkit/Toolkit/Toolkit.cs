using CommunityToolkit.Mvvm.ComponentModel;
using EFToolkit.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFToolkit
{


    public static class Toolkit
    {

        /// <summary>
        /// Converts a SQL Designer Table to C# Entity Framework Model
        /// </summary>
        /// <param name="DesignItems"></param>
        /// <returns></returns>
        public static string ConvertToModel(IList<DesignItem> DesignItems, string TableName)
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

                    Objects = Objects +
                        "\t" + @"/// <summary>" + "\n" +
                        "\t" + @"/// " + ColumnName + " - " + ColumnType + "\n" +
                        "\t" + @"/// </summary>" + "\n" +
                        "\t" + "public " + ConvertSQLType(ColumnType) + AllowNull + " " + ColumnName + " { get; set; }" + "\n" + "\n";
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
        public static string ConvertToConfiguration(ObservableCollection<DesignItem> DesignItems)
        {
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

            return Body;
        }

        /// <summary>
        /// Converts a SQL Designer Table to C# Entity Framework Configuration
        /// </summary>
        /// <param name="DesignItems"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static string ConvertToDto(ObservableCollection<DesignItem> DesignItems, string TableName, DTO_Options Options = DTO_Options.INotifyPropertyChanged)
        {
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
            else if (sqlType.StartsWith("sql_variant")) { CType = "Object"; }

            if (Dto == true) { if (sqlType.StartsWith("smallint")) { CType = "int"; } }
            if (Dto == true) { if (sqlType.StartsWith("bigint")) { CType = "int"; } }

            return CType;
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

    public enum DTO_Options
    {
        INotifyPropertyChanged,
        MVVM,
    }


}
