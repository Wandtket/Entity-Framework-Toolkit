using EFToolkit.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFToolkit.Models;
using EFToolkit.Enums;

namespace EFToolkit
{

    public partial class Toolkit
    {

        /// <summary>
        /// Converts a SQL Designer Table to C# Entity Framework Model
        /// </summary>
        /// <param name="DesignItems"></param>
        /// <returns></returns>
        public static string ConvertTableToModel(IList<DesignItem> DesignItems, string TableName, string ClassName)
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

                    string Summary = "";
                    if (Settings.Current.ModelSummary == true)
                    {
                        string Key = "";
                        if (Item.IsPrimaryKey == true) { Key = "🗝 "; }

                        Summary =
                        "\t" + @"/// <summary>" + "\n" +
                        "\t" + @"/// " + Key + ColumnName + " - " + ColumnType + "\n" +
                        "\t" + @"/// </summary>" + "\n";
                    }

                    string ColumnAttribute = "";
                    if (Settings.Current.ColumnAttribute == true)
                    {
                        ColumnAttribute = "\t" + $"[Column(\"{ColumnName}\")]" + "\n";
                    }

                    Objects = Objects +
                        Summary + ColumnAttribute +
                        "\t" + "public " + ConvertSQLType(ColumnType) + AllowNull + " " + ObjectName + " { get; set; }" + "\n" + "\n";
                }
            }

            string Header = "";
            if (Settings.Current.ModelSummary == true)
            {
                Header = @"/// <summary>" + "\n" +
                                @"/// " + TableName + "\n" +
                                @"/// </summary>" + "\n";
            }

            string Body = Header +
                         "public class " + Settings.Current.ModelPrefix + ClassName + Settings.Current.ModelSuffix + "\n" + "{" + "\n" +
                          Objects +
                         "" + "}";

            return Body;
        }

        /// <summary>
        /// Converts a SQL Designer Table to C# Entity Framework Configuration
        /// </summary>
        /// <param name="DesignItems"></param>
        /// <returns></returns>
        public static string ConvertTableToConfiguration(ObservableCollection<DesignItem> DesignItems, string TableName, string FullTableName, string ClassName)
        {

            string ModelName = Settings.Current.ModelPrefix + ClassName + Settings.Current.ModelSuffix;

            string HasNoKey = "";
            if (!DesignItems.Where(x => x.IsPrimaryKey == true).Any())
            {
                HasNoKey = $"\t \t {Settings.Current.ConfigurationName}.HasNoKey(); \n \n";
            }

            string Body = $"internal class {ModelName}Configuration : IEntityTypeConfiguration<{ModelName}> \n" +
                                "{ \n" +
                                $"\t public void Configure(EntityTypeBuilder<{ModelName}> {Settings.Current.ConfigurationName}) \n" +
                                "\t { \n" +
                                "\t \t" + $"{Settings.Current.ConfigurationName}.ToTable(\"{TableName}\");" + "\n \n" +
                                HasNoKey;


            foreach (DesignItem Item in DesignItems)
            {
                if (Item.ColumnName.Trim() != "")
                {
                    string ColumnName = Item.ColumnName.Trim();
                    string ObjectName = Item.ObjectName.Trim();

                    string Key = "";
                    if (Item.IsPrimaryKey)
                    {
                        if (!string.IsNullOrEmpty(Settings.Current.PrimaryKeyStandard))
                        {
                            ObjectName = Settings.Current.PrimaryKeyStandard;
                        }

                        Key = $"\t \t {Settings.Current.ConfigurationName}.HasKey(s => s.{ObjectName}); \n";
                    }

                    Body = Body + Key +
                        $"\t \t {Settings.Current.ConfigurationName}.Property(s => s.{ObjectName}) \n" +
                        $"\t \t \t .HasColumnName(\"{ColumnName}\"); \n \n";
                }
            }

            string Header = "";
            if (Settings.Current.ModelSummary == true)
            {
                Header = $"/// <summary> \n" +
                                $"/// {FullTableName} \n" +
                                "/// </summary> \n";
            }

            string Footer = "\t } \n }";

            Body = Header + Body + Footer;

            return Body;
        }

        /// <summary>
        /// Converts a SQL Designer Table to C# Entity Framework Configuration
        /// </summary>
        /// <param name="DesignItems"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static string ConvertTableToDto(ObservableCollection<DesignItem> DesignItems, string TableName, string ClassName, ModelOptions Options = ModelOptions.MVVM)
        {
            string Objects = "";

            if (Options == ModelOptions.Standard)
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

                        if (ObjectName.ToLower() == "override") { ObjectName = "_Override"; }

                        string Summary = "";
                        if (Settings.Current.DtoSummary == true)
                        {
                            string Key = "";
                            if (Item.IsPrimaryKey == true) { Key = "🗝 "; }

                            Summary =
                            "\t" + @"/// <summary>" + "\n" +
                            "\t" + @"/// " + Key + ColumnName + " - " + Item.DataType.Trim() + "\n" +
                            "\t" + @"/// </summary>" + "\n";
                        }

                        Objects = Objects + Summary +
                        "\t" + $"[JsonPropertyName(\"{ObjectName}\")]" + "\n" +
                        "\t" + "public " + ColumnType + AllowNull + " " + ObjectName + " { get; set; } \n \n \n";
                    }
                }

                string Header = "";
                if (Settings.Current.DtoSummary == true)
                {
                    Header = @"/// <summary>" + "\n" +
                                    @"/// " + TableName + "\n" +
                                    @"/// </summary>" + "\n";
                }

                string Body = Header +
                            "public class " + Settings.Current.DTOPrefix + ClassName + Settings.Current.DTOSuffix + "\n" + "{" + "\n \n" +
                            Objects +
                            "}";

                return Body;
            }
            else if (Options == ModelOptions.INotifyPropertyChanged)
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

                        string Trim = "";
                        if (Item.DataType.ToLower().StartsWith("char")) { Trim = ".Trim()"; }

                        if (ObjectName.ToLower() == "override") { ObjectName = "_Override"; }

                        string Summary = "";
                        if (Settings.Current.DtoSummary == true)
                        {
                            string Key = "";
                            if (Item.IsPrimaryKey == true) { Key = "🗝 "; }

                            Summary =
                            "\t" + @"/// <summary>" + "\n" +
                            "\t" + @"/// " + Key + ColumnName + " - " + Item.DataType.Trim() + "\n" +
                            "\t" + @"/// </summary>" + "\n";
                        }

                        Objects = Objects + Summary +
                        "\t" + $"[JsonPropertyName(\"{ObjectName}\")]" + "\n" +
                        "\t" + "public " + ColumnType + AllowNull + " " + ObjectName + "\n" +
                        "\t" + "{" + "\n" +
                        "\t" + "\t" + "get { return " + ObjectName.ToLower() + "; }" + "\n" +
                        "\t" + "\t" + "set { " + "\n" +
                        "\t" + "\t" + "\t" + "if (" + ObjectName.ToLower() + " != value" + Trim + ")" + "\n" +
                        "\t" + "\t" + "\t" + "{" + "\n" +
                        "\t" + "\t" + "\t" + "\t" + ObjectName.ToLower() + " = value" + Trim + ";" + "\n" +
                        "\t" + "\t" + "\t" + "\t" + $"NotifyPropertyChanged(\"{ObjectName}\");" + "\n" +
                        "\t" + "\t" + "\t" + "}" + "\n" +
                        "\t" + "\t" + "}" + "\n" +
                        "\t" + "}" + "\n" +
                        "\t private " + ColumnType + AllowNull + " " + ObjectName.ToLower() + "; " + "\n" + "\n" + "\n";
                    }
                }

                string Header = "";
                if (Settings.Current.DtoSummary == true)
                {
                    Header = @"/// <summary>" + "\n" +
                                    @"/// " + TableName + "\n" +
                                    @"/// </summary>" + "\n";
                }

                string Body = Header +
                            "public class " + Settings.Current.DTOPrefix + ClassName + Settings.Current.DTOSuffix + " : INotifyPropertyChanged" + "\n" + "{" + "\n" +
                            Objects +

                            "\t" + "public event PropertyChangedEventHandler PropertyChanged;" + "\n" +
                            "\t" + "public void NotifyPropertyChanged(string propertyName)" + "\n" +
                            "\t" + "{" + "\n" +
                            "\t" + "\t" + "PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));" + "\n" +
                            "\t" + "}" +

                            "\n" + "}";

                return Body;
            }
            else if (Options == ModelOptions.MVVM)
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

                        ObjectName = char.ToLowerInvariant(ObjectName[0]) + ObjectName.Substring(1);
                        if (ObjectName.ToLower() == "override") { ObjectName = "_Override"; }

                        string Summary = "";
                        if (Settings.Current.DtoSummary == true)
                        {
                            string Key = "";
                            if (Item.IsPrimaryKey == true) { Key = "🗝 "; }

                            Summary =
                            "\t" + @"/// <summary>" + "\n" +
                            "\t" + @"/// " + Key + ColumnName + " - " + Item.DataType.Trim() + "\n" +
                            "\t" + @"/// </summary>" + "\n";
                        }

                        Objects = Objects + Summary +
                        "\t" + $"[JsonPropertyName(\"{ObjectName}\")]" + "\n" +
                        "\t" + $"[ObservableProperty]" + "\n" +
                        "\t" + "private " + ColumnType + AllowNull + " " + ObjectName + "; \n \n \n";
                    }
                }

                string Body = @"/// <summary>" + "\n" +
                                    @"/// " + TableName + "\n" +
                                    @"/// </summary>" + "\n" +
                                    "public partial class " + Settings.Current.DTOPrefix + ClassName + Settings.Current.DTOSuffix + " : ObservableObject" + "\n" + "{" + "\n" +
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
            else if (sqlType.StartsWith("date")) { CType = "DateTime"; }
            else if (sqlType.StartsWith("float")) { CType = "double"; }
            else if (sqlType.StartsWith("real")) { CType = "Single"; }
            else if (sqlType.StartsWith("smalldatetime")) { CType = "DateTime"; }
            else if (sqlType.StartsWith("sql_variant")) { CType = "object"; }
            else if (sqlType.StartsWith("uniqueidentifier")) { CType = "Guid"; }

            if (Dto == true) { if (sqlType.StartsWith("smallint")) { CType = "int"; } }
            if (Dto == true) { if (sqlType.StartsWith("bigint")) { CType = "int"; } }

            return CType;
        }

    }
}
