using EFToolkit.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EFToolkit.Models;
using EFToolkit.Enums;

namespace EFToolkit
{

    public partial class Toolkit
    {

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

            Input = Input.Replace("    }\r\n}", "    }\r" + Event);

            return Input;
        }



        public static async Task<string> BaseModel(string Input, ModelOptions Options)
        {
            ClassItem Model = new ClassItem();

            string? Line = null;
            StringReader stringReader = new StringReader(Input);
            int numLines = Input.Split('\n').Length;

            for (int i = 0; i < numLines; i++)
            {
                Line = await stringReader.ReadLineAsync();

                if (Options == ModelOptions.Standard)
                {
                    //Fix Class
                    if (Line.Contains(" class ") && Line.Contains("INotifyPropertyChanged"))
                    {
                        Input = Input.Replace(Line, Line.Replace("INotifyPropertyChanged", ""));
                    }
                    else if (Line.Contains(" class ") && Line.Contains(" : ObservableObject"))
                    {
                        Input = Input.Replace(Line, Line.Replace(" : ObservableObject", ""));
                    }
                }
                else if (Options == ModelOptions.INotifyPropertyChanged)
                {
                    //Fix Class
                    if (Line.Contains(" class ") && !Line.Contains(":"))
                    {
                        Input = Input.Replace(Line, Line.TrimEnd() + " : INotifyPropertyChanged");
                    }
                    else if (Line.Contains(" class ") && Line.Contains(":") && !Line.Contains("ObservableObject"))
                    {
                        Input = Input.Replace(Line, Line.TrimEnd() + ", INotifyPropertyChanged");
                    }
                    else if (Line.Contains(" partial class ") && Line.Contains(" : ObservableObject"))
                    {
                        string NewLine = Line;
                        NewLine = NewLine.Replace(" : ObservableObject", " : INotifyPropertyChanged");
                        NewLine = NewLine.Replace("partial ", "");
                        Input = Input.Replace(Line, NewLine);
                    }



                }
                else if (Options == ModelOptions.MVVM)
                {

                }
            }


            return Input;
        }


        public static async Task<NamespaceItem> ModelBuilder(string Input)
        {
            string? Line = null;
            StringReader stringReader = new StringReader(Input);
            int numLines = Input.Split('\n').Length;

            NamespaceItem NameSpace = new();

            string? CurrentSummary = "";

            ClassItem? CurrentClassItem = null;

            List<string>? CurrentAttributes = new();
            PropertyItem? CurrentPropertyItem = null;

            for (int i = 0; i < numLines; i++)
            {
                Line = await stringReader.ReadLineAsync();

                if (string.IsNullOrEmpty(Line)) { continue; }

                if (Line.StartsWith("using")) { NameSpace.Usings.Add(Line); }
                if (Line.StartsWith("namespace")) { NameSpace.NameSpace = Line.Replace("namespace ", "").Trim(); }

                //Summary
                if (Line.Contains("/// ") && !Line.Contains("<"))
                {
                    CurrentSummary = Line.Replace("/// ", "").Trim();
                }

                //Class
                if (Line.Contains("class "))
                {
                    string TempLine = Line;

                    if (CurrentClassItem != null)
                    {
                        NameSpace.ClassItems.Add(CurrentClassItem);
                        CurrentClassItem = null;
                    }

                    string Access = "";
                    if (TempLine.Contains("public")) { Access = "public "; }
                    else if (TempLine.Contains("private")) { Access = "private "; }

                    if (TempLine.Contains("protected")) { Access = Access + "protected "; }
                    if (TempLine.Contains("override")) { Access = Access + "override "; }
                    if (TempLine.Contains("partial")) { Access = Access + "partial "; }

                    string ClassName = TempLine.Remove(0, TempLine.IndexOf("class ") + 6);
                    if (ClassName.Contains(" ")) { ClassName = ClassName.Substring(0, ClassName.IndexOf(" ")); }
                    else { ClassName = ClassName.Substring(0, ClassName.Length); }

                    List<string> Interfaces = new();
                    if (TempLine.Contains(" : ") && !TempLine.Contains(","))
                    {
                        string Interface = TempLine.Remove(0, TempLine.IndexOf(" : ") + 3);
                        Interface = Interface.Substring(0, Interface.Length);
                        Interfaces.Add(Interface);
                    }
                    else if (TempLine.Contains(" : ") && TempLine.Contains(","))
                    {
                        string Interface = TempLine.Remove(0, TempLine.IndexOf(" : ") + 3);
                        string[] EachInterface = Interface.Split(", ");
                        Interfaces = new List<string>(EachInterface);
                    }

                    ClassItem NewClassItem = new()
                    {
                        Access = Access,
                        Summary = CurrentSummary,
                        Name = ClassName,
                        Interfaces = Interfaces,
                    };
                    CurrentClassItem = NewClassItem;
                    CurrentSummary = "";
                }


                //Attributes
                if (Line.Contains("[") && Line.Contains("]"))
                {
                    CurrentAttributes.Add(Line.Trim());
                }


                //Property
                if (!Line.Contains("class ") && !Line.Contains("event") && !Line.Contains("void")
                    && Line.Contains("public ") || Line.Contains("private ") || Line.Contains("protected "))
                {
                    PropertyItem subItem = new PropertyItem();

                    if (Line.Contains("public ")) { subItem.Access = "public"; }
                    else if (Line.Contains("private ")) { subItem.Access = "private"; }
                    else if (Line.Contains("protected ")) { subItem.Access = "protected"; }

                    if (Line.Contains("static ")) { subItem.IsStatic = true; Line = Line.Replace("static ", ""); }
                    if (Line.Contains("override ")) { subItem.IsOverride = true; Line = Line.Replace("override ", ""); }

                    string Type = Line.Remove(0, Line.IndexOf(subItem.Access + " ") + subItem.Access.Length + 1);
                    if (Type.Contains(" ")) { Type = Type.Substring(0, Type.IndexOf(" ")); }
                    else if (Type.Contains("(")) { Type = Type.Substring(0, Type.IndexOf("(")); }

                    string Name = Line.Remove(0, Line.IndexOf(Type) + Type.Length).Trim();
                    if (Name.Contains(" ")) { Name = Name.Substring(0, Name.IndexOf(" ")); }
                    if (Name.Contains(";")) { Name = Name.Substring(0, Name.IndexOf(";")); }
                    else { Name = Name.Substring(0, Name.Length); }

                    //GetSet
                    if (Line.Contains("{ get") && Line.Contains("}"))
                    {
                        string GetSet = Line.Remove(0, Line.IndexOf("{"));
                        subItem.GetSet = GetSet.Substring(0, GetSet.IndexOf("}") + 1);
                    }
                    else if (Line.Contains(" => "))
                    {
                        string GetSet = Line.Remove(0, Line.IndexOf(" => "));
                        subItem.GetSet = GetSet.Substring(0, GetSet.Length);
                    }

                    //Values
                    if (Line.Contains(" = "))
                    {
                        string Value = Line.Remove(0, Line.IndexOf(" = ") + 3);
                        subItem.Value = Value.Substring(0, Value.Length);
                        subItem.Value = subItem.Value.Replace(";", "");
                    }

                    subItem.Summary = CurrentSummary;
                    subItem.Attributes = new List<string>(CurrentAttributes);
                    subItem.Type = Type;
                    subItem.Name = Name;

                    if (!Name.Contains("(") && !Type.Contains("("))
                    {
                        CurrentPropertyItem = subItem;
                        CurrentClassItem.PropertyItems.Add(subItem);
                        CurrentSummary = "";
                        CurrentAttributes.Clear();

                        continue;
                    }
                }

            }

            if (CurrentClassItem != null)
            {
                NameSpace.ClassItems.Add(CurrentClassItem);
                CurrentClassItem = null;
            }

            Debug.WriteLine(JsonSerializer.Serialize<NamespaceItem>(NameSpace));
            return NameSpace;
        }

        public static async Task<string> ConvertModel(NamespaceItem Model, ModelOptions Options)
        {
            string Usings = string.Join("\n", Model.Usings) + "\n\n";

            string Namespace = "";
            if (!string.IsNullOrEmpty(Model.NameSpace)) { Namespace = $"namespace {Model.NameSpace}\n" + "{\n"; }

            string ClassName = "";
            string Properties = "";

            string Body = "";

            foreach (var Class in Model.ClassItems)
            {
                string Interfaces = "";
                if (Class.Interfaces.Count > 0)
                {
                    var INotify = Class.Interfaces.Where(x => x.Contains("INotifyPropertyChanged")).FirstOrDefault();
                    var Observable = Class.Interfaces.Where(x => x.Contains("ObservableObject")).FirstOrDefault();

                    if (INotify != null) { Class.Interfaces.Remove(INotify); }
                    if (Observable != null) { Class.Interfaces.Remove(Observable); }

                    if (Options == ModelOptions.INotifyPropertyChanged) { Class.Interfaces.Add("INotifyPropertyChanged"); }
                    else if (Options == ModelOptions.MVVM) { Class.Interfaces.Insert(0, "ObservableObject"); }

                    Interfaces = " : " + string.Join(", ", Class.Interfaces);
                }

                ClassName = "\t" + Class.Access + "class " + Class.Name + Interfaces + "\n\t{\n";

                foreach (var Property in Class.PropertyItems)
                {
                    string Attributes = string.Join("\n\t\t", Property.Attributes);

                    if (Options == ModelOptions.Standard)
                    {
                        Properties = Properties + "\t\t" + Attributes + "\n\t\t" +
                            Property.Access + " " + Property.Type + " " + Property.Name + " " + Property.GetSet + Property.Value + "\n\n";
                    }
                    else if (Options == ModelOptions.INotifyPropertyChanged)
                    {
                        string Value = "";
                        if (!string.IsNullOrEmpty(Property.Value)) { Value = $" = {Property.Value}"; }

                        string INotifyPropertyString = "\n\t\t{\n" +
                                "\t\t\tget { return " + Property.Name.ToLower() + "; } \n" +
                                "\t\t\tset { \n " +
                                "\t\t\t\tif (" + Property.Name.ToLower() + " != value) \n" +
                                "\t\t\t\t{ \n" +
                                "\t\t\t\t\t" + Property.Name.ToLower() + " = value; \n" +
                                "\t\t\t\t\tNotifyPropertyChanged(\"" + Property.Name.Trim() + "\"); \n" +
                                "\t\t\t\t} \n" +
                                "\t\t\t} \n" +
                                "\t\t}\n" +
                                "\t\tprivate " + Property.Type + " " + Property.Name.ToLower() + Value + "; \n \n";

                        Properties = Properties + "\t\t" + Attributes + "\n\t\t" +
                            Property.Access + " " + Property.Type + " " + Property.Name + INotifyPropertyString;

                    }
                    else if (Options == ModelOptions.MVVM)
                    {
                        Properties = Properties + Attributes + "\n" + "private " + Property.Type + " " + Property.Name.FirstCharToLowerCase() + "; ";
                    }
                }
            }


            if (Options == ModelOptions.INotifyPropertyChanged)
            {
                string Event = "\n\t\t" + "public event PropertyChangedEventHandler PropertyChanged;" + "\n" +
                    "\t\t" + "public void NotifyPropertyChanged(string propertyName)" + "\n" +
                    "\t\t" + "{" + "\n" +
                    "\t\t" + "\t" + "PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));" + "\n" +
                    "\t\t" + "}\n";

                Body = Usings + Namespace + ClassName + Properties + Event + "\t}\n}";
            }
            else
            {
                Body = Usings + Namespace + ClassName + Properties + "\t}\n}";
            }


            return Body;
        }


    }

}
