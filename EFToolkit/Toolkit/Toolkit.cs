using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using EFToolkit.Controls.Dialogs;
using EFToolkit.Extensions;
using EFToolkit.Pages;
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
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using System.Text.RegularExpressions;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;

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
                HasNoKey =  $"\t \t {Settings.Current.ConfigurationName}.HasNoKey(); \n \n";
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

        #endregion


        #region Select Describer


        public static string ConvertSelectToDescriber(string SelectStatement)
        {           
            string Prefix = "EXEC sp_describe_first_result_set @tsql = N' \n";

            string Body = SelectStatement.Replace("'", "") + "' \n";

            string Suffix = ", @params = NULL, @browse_information_mode = 0; \n \n" +
                "/*Once executed, copy the results table without headers into the\r\nTable Converter tool in Entity Framework Toolkit..*/";

            string DescribeCommand = Prefix + Body + Suffix;
            return DescribeCommand;
        }

        #endregion


        #region Data Visualizer

        public static string ConvertTableToSelectStatement(IList<VisualizerItem> VisualizerItems, string TableName)
        {
            string Objects = "";
            int NewLineIncrement = 0;

            List<VisualizerItem> SelectedItems = VisualizerItems.Where(x => x.Include == true).ToList();
            for (int i = 0; i < SelectedItems.Count; i++)
            {
                if (SelectedItems[i].ColumnName != "")
                {
                    string ColumnName = SelectedItems[i].ColumnName.Trim();
                    NewLineIncrement++;

                    if (!string.IsNullOrEmpty(TableName))
                    {
                        string Delimiter = ", ";

                        //Create a new line after every x number of columns
                        if (NewLineIncrement == Settings.Current.DataVisualizerNewLineIncrement || NewLineIncrement.ToString().Length > 1 && NewLineIncrement.ToString().EndsWith("0")) { Delimiter = ", \n \t"; NewLineIncrement = 0; }
                        if (i == SelectedItems.IndexOf(SelectedItems.Last())) { Delimiter = ""; }

                        Objects = Objects + TableName + "." + ColumnName + Delimiter;
                    }
                    else
                    {
                        string Delimiter = $"";
                        string Comma = ",";
                        if (NewLineIncrement == Settings.Current.DataVisualizerNewLineIncrement || NewLineIncrement.ToString().Length > 1 && NewLineIncrement.ToString().EndsWith("0")) { Delimiter = $"\n \t"; NewLineIncrement = 0; }                    
                        if (i == SelectedItems.IndexOf(SelectedItems.First())) { Comma = ""; }

                        Objects = Objects + $"{Comma}[" + ColumnName + "] " + Delimiter;
                    }
                }
            }

            string Body = @"select all " + Objects + "\n" + 
                          "--Paste joins, unions, clauses, etc here...--";

            return Body;
        }


        #endregion


        #region ModelEditor 


        public static string ConvertModel(string Input, ModelOptions Options)
        {

            if (Options == ModelOptions.Standard)
            {
                return "";
            }
            else if (Options == ModelOptions.MVVM)
            {
                return "";
            }
            else if (Options == ModelOptions.INotifyPropertyChanged)
            {              
                if (ModelInputType(Input) == ModelOptions.Standard)
                {
                    return ConvertFromStandardModel(Input);
                }
                else if (ModelInputType(Input) == ModelOptions.INotifyPropertyChanged)
                {
                    return "";
                }
                else if (ModelInputType(Input) == ModelOptions.MVVM)
                {
                    return "";
                }
                else { return ""; }
            }
            else { return ""; }
        }

        private static ModelOptions ModelInputType(string Input)
        {
            if (Input.Contains(" { get; set; }")) { return ModelOptions.Standard; }
            else if (Input.Contains("NotifyPropertyChanged")) { return ModelOptions.INotifyPropertyChanged; }
            else if (Input.Contains("[ObservableProperty]")) { return ModelOptions.MVVM; }
            else { return ModelOptions.Standard; }
        }

        private static string ConvertFromStandardModel(string Input)
        {
            int InputCount = Regex.Matches(Input, " { get; set; }").Count();

            for (int i = 0; i < InputCount; i++)
            {
                try
                {
                    int StartIndex = Input.IndexOf(" { get; set; }");

                    string Subject = Input.Substring(StartIndex - 50, 50);

                    string Type = Input.Substring(StartIndex - 50, 50).TrimStart().TrimEnd();
                    Type = Type.Replace(Type.Substring(0, Type.IndexOf(" ")), "");
                    Type = Type.Replace("</summary>", "");
                    Type = Type.Replace(">", "");
                    Type = Type.Replace("<", "");
                    Type = Type.Replace("/", "");
                    Type = Type.Replace("private", "");
                    Type = Type.Replace("public", "");
                    Type = Type.Replace("readonly", "");
                    Type = Type.TrimEnd().TrimStart();

                    Subject = Subject.Replace(Subject.Substring(0, Subject.LastIndexOf(" ")), "").Trim();

                    string INotifyPropertyString = "\n \t{ \n" +
                        "\t \t get { return " + Subject.ToLower() + "; } \n" +
                        "\t \t set { \n " +
                        "\t \t \t if (" + Subject.ToLower() + " != value) \n" +
                        "\t \t \t { \n" +
                        "\t \t \t \t" + Subject.ToLower() + " = value; \n" +
                        "\t \t \t \t NotifyPropertyChanged(\"" + Subject.Trim() + "\"); \n" +
                        "\t \t \t } \n" +
                        "\t \t } \n" +
                        "\t} \n" +
                        "\tprivate " + Type.ToLower() + "; \n \n";

                    Input = Input.Replace(Subject + " { get; set; }", Subject + INotifyPropertyString);

                    Input = Input.Replace("public " + Type, "\tpublic " + Type);

                    //Input = Input.Replace("/// ", "\t///");

                    
                    //MessageBox.Show(Type);
                }
                catch { }
            }

            string Event = "\t" + "public event PropertyChangedEventHandler PropertyChanged;" + "\n" +
                                "\t" + "public void NotifyPropertyChanged(string propertyName)" + "\n" +
                                "\t" + "{" + "\n" +
                                "\t" + "\t" + "PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));" + "\n" +
                                "\t" + "}" +

                                "\n" + "}";

            Input = Input.Replace("    }\r\n}", "    }\r" + Event );

            return Input;
        }





        #endregion


        #region Acronym Libraries


        public static string ConvertSQLColumnName(string SQLColumnName)
        {
            ObservableCollection<AcronymLibrary> List = new();
 
            if (SelectedAcronymLibraries.Where(x => x.Title == "All").FirstOrDefault() != null) { List = AcronymLibraries; }
            else { List = SelectedAcronymLibraries; }

            foreach (AcronymLibrary library in List)
            {
                foreach (var Item in library.LibraryItems)
                {
                    if (!string.IsNullOrEmpty(Item.Acronym))
                    {
                        if (Item.Options == "Contains")
                        {
                            if (SQLColumnName.Contains(Item.Acronym))
                            {
                                //Check the next characters casing in case the acronym is only two or so digits long.
                                var NextIndex = SQLColumnName.IndexOf(Item.Acronym) + Item.Acronym.Length;
                                if (SQLColumnName.Length !> NextIndex + 1)
                                {
                                    var CheckCasing = SQLColumnName.Substring(NextIndex, 1);
                                    if (Char.IsUpper(Convert.ToChar(CheckCasing)) || CheckCasing == "_")
                                    {
                                        SQLColumnName = SQLColumnName.ReplaceFirstOccurrence(Item.Acronym, Item.Translation);
                                    }
                                }      
                                else if (SQLColumnName.EndsWith(Item.Acronym))
                                {
                                    SQLColumnName = SQLColumnName.ReplaceLastOccurrence(Item.Acronym, Item.Translation);
                                }
                            }
                        }
                        else if (Item.Options == "Equals")
                        {
                            if (SQLColumnName == Item.Acronym)
                            {
                                SQLColumnName = SQLColumnName = Item.Translation;
                            }
                        }
                        else if (Item.Options == "Starts With")
                        {
                            if (SQLColumnName.StartsWith(Item.Acronym))
                            {
                                SQLColumnName = SQLColumnName.ReplaceFirstOccurrence(Item.Acronym, Item.Translation);
                            }
                        }
                        else if (Item.Options == "Ends With")
                        {
                            if (SQLColumnName.EndsWith(Item.Acronym))
                            {
                                SQLColumnName = SQLColumnName.ReplaceLastOccurrence(Item.Acronym, Item.Translation);
                            }
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
        public static string ConvertModel(ModelOptions Options = ModelOptions.INotifyPropertyChanged)
        {



            return "";
        }

        #endregion


        #region Data


        public static ObservableCollection<AcronymLibrary> AcronymLibraries = new();
        public static ObservableCollection<AcronymLibrary> SelectedAcronymLibraries = new();

        public static ObservableCollection<DatabaseItem> DatabaseItems = new();

        public static ObservableCollection<SchemaItem> SchemaItems = new();
        public static ObservableCollection<SchemaItem> SelectedSchemaItems = new();

        public static string DataFileName = "Entity Framework Toolkit Data.eftk";
        private static string SelectedSchemaItemFileName = "SelectedSchemas.efsl";
        private static string SelectedAcronymLibraryFileName = "SelectedAcronyms.efal";


        public static async Task LoadData()
        {
            StorageFolder Folder = ApplicationData.Current.LocalFolder;

            //Load Main Shareable Data
            if (File.Exists(Folder.Path + "\\" + DataFileName))
            {
                StorageFile file = await Folder.GetFileAsync(DataFileName);
                var Data = JsonSerializer.Deserialize<DataItem>(File.ReadAllText(file.Path));
                if (Data != null)
                {
                    #region Acronym Libraries

                    foreach (var Library in Data.AcronymLibraries)
                    {
                        Library.LibraryItems = new ObservableCollection<AcronymItem>(Library.LibraryItems.OrderBy(x => x.Acronym).ToList());
                    }

                    //Add an All Item.
                    if (Data.AcronymLibraries.Where(x => x.Title == "All").FirstOrDefault() == null)
                    {
                        Data.AcronymLibraries.Add(new AcronymLibrary() { Title = "All" });
                    }

                    AcronymLibraries = new ObservableCollection<AcronymLibrary>(Data.AcronymLibraries.OrderBy(x => x.Title));

                    #endregion

                    #region Database Items 

                    DatabaseItems = new ObservableCollection<DatabaseItem>(Data.DatabaseItems.OrderBy(x => x.InitialCatalog));

                    #endregion

                    #region Schema Items 

                    SchemaItems = new ObservableCollection<SchemaItem>(Data.SchemaItems.OrderBy(x => x.Schema));

                    #endregion
                }
            }

            //Load Previous User Selection
            //Selected Libraries
            if (File.Exists(Folder.Path + "\\" + SelectedAcronymLibraryFileName))
            {
                StorageFile file = await Folder.GetFileAsync(SelectedAcronymLibraryFileName);
                var Libraries = JsonSerializer.Deserialize<ObservableCollection<AcronymLibrary>>(File.ReadAllText(file.Path));
                if (Libraries != null)
                {
                    foreach (var Library in Libraries)
                    {
                        Library.LibraryItems = new ObservableCollection<AcronymItem>(Library.LibraryItems.OrderBy(x => x.Acronym).ToList());
                    }

                    SelectedAcronymLibraries = new ObservableCollection<AcronymLibrary>(Libraries.OrderBy(x => x.Title));
                }
            }

            //Selected Schemas
            if (File.Exists(Folder.Path + "\\" + SelectedSchemaItemFileName))
            {
                StorageFile file = await Folder.GetFileAsync(SelectedSchemaItemFileName);
                var Libraries = JsonSerializer.Deserialize<ObservableCollection<SchemaItem>>(File.ReadAllText(file.Path));
                if (Libraries != null)
                {
                    SelectedSchemaItems = new ObservableCollection<SchemaItem>(Libraries.OrderBy(x => x.Schema));
                }
            }

        }

        public static async void SaveData()
        {
            StorageFolder Folder = ApplicationData.Current.LocalFolder;

            DataItem Data = new DataItem()
            {
                AcronymLibraries = AcronymLibraries,
                DatabaseItems = DatabaseItems,
                SchemaItems = SchemaItems,
            };

            //All Libaries
            StorageFile file = await Folder.CreateFileAsync(DataFileName, CreationCollisionOption.OpenIfExists);
            var Json = JsonSerializer.Serialize(Data);
            await File.WriteAllTextAsync(file.Path, Json);

            //Selected Libaries
            StorageFile SelectedAcronymsfile = await Folder.CreateFileAsync(SelectedAcronymLibraryFileName, CreationCollisionOption.OpenIfExists);
            var SelectedAcronymsJson = JsonSerializer.Serialize(SelectedAcronymLibraries);
            await File.WriteAllTextAsync(SelectedAcronymsfile.Path, SelectedAcronymsJson);

            StorageFile SelectedSchemasfile = await Folder.CreateFileAsync(SelectedSchemaItemFileName, CreationCollisionOption.OpenIfExists);
            var SelectedSchemasJson = JsonSerializer.Serialize(SelectedSchemaItems);
            await File.WriteAllTextAsync(SelectedSchemasfile.Path, SelectedSchemasJson);
        }


        public static async void ImportData()
        {
            // Create a file picker
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Current.ActiveWindow);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your file picker
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Add(".eftk");

            // Open the picker for the user to pick a file
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                var Data = JsonSerializer.Deserialize<DataItem>(File.ReadAllText(file.Path));

                var result = await ConfirmBox.Show("This action will import the data from the selected file, nothing will be removed or modified.", "Import Data?"); 
                if (result == ContentDialogResult.Primary)
                {
                    foreach (var Library in Data.AcronymLibraries) { AcronymLibraries.Add(Library); }

                    foreach (var Database in Data.DatabaseItems) { DatabaseItems.Add(Database); }

                    foreach (var Schema in Data.SchemaItems) { SchemaItems.Add(Schema); }
                }
            }
        }

        public static async void ExportData()
        {
            // Create a file picker
            FileSavePicker savePicker = new Windows.Storage.Pickers.FileSavePicker();

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Current.ActiveWindow);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

            // Set options for your file picker
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("EFToolkit Data File", new List<string>() { ".eftk" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "Entity Framework Toolkit Data";

            // Open the picker for the user to pick a file
            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);

                DataItem Data = new DataItem()
                {
                    AcronymLibraries = AcronymLibraries,
                    DatabaseItems = DatabaseItems,
                    SchemaItems = SchemaItems,
                };

                var Json = JsonSerializer.Serialize(Data);

                // write to file
                await File.WriteAllTextAsync(file.Path, Json);

                // Another way to write a string to the file is to use this instead:
                // await FileIO.WriteTextAsync(file, "Example file contents.");

                // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status == FileUpdateStatus.Complete)
                {

                }
            }
            else
            {

            }

        }

        #endregion

    }



    /// <summary>
    /// Stores the entirety of Shareable data in a single file.
    /// </summary>
    public partial class DataItem : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<AcronymLibrary> acronymLibraries = new();

        [ObservableProperty]
        private ObservableCollection<DatabaseItem> databaseItems = new();

        [ObservableProperty]
        private ObservableCollection<SchemaItem> schemaItems = new();
    }


    public partial class DesignItem : ObservableObject
    {
        [ObservableProperty]
        private int index = 0;

        [ObservableProperty]
        private string rearrangeText = string.Empty;

        [ObservableProperty]
        private bool isPrimaryKey = false;

        [ObservableProperty]
        private string columnName = string.Empty;

        [ObservableProperty]
        private string objectName = string.Empty;

        [ObservableProperty]
        private string dataType = string.Empty;

        [ObservableProperty]
        private bool allowNulls = false;

        [ObservableProperty]
        private string defaultValue = string.Empty;

    }


    public partial class VisualizerItem : ObservableObject
    {
        [ObservableProperty]
        private bool? include = true;

        [ObservableProperty]
        private string columnName = string.Empty;

        [ObservableProperty]
        private string objectName = string.Empty;

        [ObservableProperty]
        private string value = string.Empty;
    }


    public partial class ModelItem : ObservableObject
    {
        [ObservableProperty]
        private List<string> usings = new();

        [ObservableProperty]
        private string nameSpace = string.Empty;

        [ObservableProperty]
        private string summary = string.Empty;

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private List<ModelSubItem> items = new();
    }

    public partial class ModelSubItem : ObservableObject
    {
        [ObservableProperty]
        private string summary = string.Empty;

        [ObservableProperty]
        private List<string> attributes = new();

        [ObservableProperty]
        private string access = string.Empty;

        [ObservableProperty]
        private string declaration = string.Empty;

        [ObservableProperty]
        private string name = string.Empty;
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
        private string options = "Contains";

        [ObservableProperty]
        private string acronym;

        [ObservableProperty]
        private string translation;

    }


    public partial class SchemaItem : ObservableObject
    {
        [ObservableProperty]
        private string schema;
    }


    public partial class DatabaseItem : ObservableObject
    {
        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string dataSource;

        [ObservableProperty]
        private string initialCatalog;

        [ObservableProperty]
        private string userId;

        [ObservableProperty]
        private string password;

        public string GetConnectionString()
        {
            return $"Data Source ={DataSource};Initial Catalog={initialCatalog};User Id={userId};Password={password}";
        }
    }


    public partial class CredentialItem : ObservableObject
    {
        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string password;
    }


    public enum ModelOptions
    {
        Standard,
        INotifyPropertyChanged,
        MVVM,
    }

    public enum CodeFormatOptions
    {
        PascalCase,
        snake_case,
    }



}
