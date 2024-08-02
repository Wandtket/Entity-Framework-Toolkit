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
using System.Xml.Linq;
using System.Net.NetworkInformation;
using System.Collections.ObjectModel;
using EFToolkit.Controls.Dialogs;

namespace EFToolkit
{


    public partial class Toolkit
    {

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

                    ObservableCollection<string> Interfaces = new();
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
                        Interfaces = new ObservableCollection<string>(EachInterface);
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

                    //Skip if item is duplicate
                    if (CurrentClassItem.PropertyItems.Where(x => x.OriginalName.ToLower() == Name.ToLower()).FirstOrDefault() != null)
                    { continue; }

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
                    subItem.Attributes = new ObservableCollection<string>(CurrentAttributes);
                    subItem.Type = Type;
                    subItem.OriginalName = Name;
                    subItem.NewName = Toolkit.ConvertSQLColumnName(Name);

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

            //Remove Duplicates
            Debug.WriteLine(JsonSerializer.Serialize<NamespaceItem>(NameSpace));
            return NameSpace;
        }

        public static async Task<string> ConvertModel(NamespaceItem Model, ModelOptions Options, List<ModelAttributes> SelectedAttributes)
        {
            if (Options == ModelOptions.Standard)
            {
                var System = Model.Usings.Where(x => x.Contains("System.ComponentModel")).FirstOrDefault();
                if (System != null) { Model.Usings.Remove(System); }

                var Mvvm = Model.Usings.Where(x => x.Contains("Mvvm.ComponentModel")).FirstOrDefault();
                if (Mvvm != null) { Model.Usings.Remove(Mvvm); }
            }
            if (Options == ModelOptions.INotifyPropertyChanged)
            {
                var Using = Model.Usings.Where(x => x.Contains("System.ComponentModel")).FirstOrDefault();
                if (Using == null) { Model.Usings.Add("using System.ComponentModel;"); }

                var Mvvm = Model.Usings.Where(x => x.Contains("Mvvm.ComponentModel")).FirstOrDefault();
                if (Mvvm != null) { Model.Usings.Remove(Mvvm); }
            }
            else if (Options == ModelOptions.MVVM)
            {
                var Using = Model.Usings.Where(x => x.Contains("Mvvm.ComponentModel")).FirstOrDefault();
                if (Using == null) { Model.Usings.Add("using CommunityToolkit.Mvvm.ComponentModel;"); }

                var System = Model.Usings.Where(x => x.Contains("System.ComponentModel")).FirstOrDefault();
                if (System != null) { Model.Usings.Remove(System); }
            }

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
                }

                if (Options == ModelOptions.INotifyPropertyChanged) { Class.Interfaces.Add("INotifyPropertyChanged"); }
                else if (Options == ModelOptions.MVVM) 
                {                    
                    Class.Interfaces.Clear(); 
                    Class.Interfaces.Insert(0, "ObservableObject");
                    if (!Class.Access.Contains("partial"))
                    {
                        Class.Access = Class.Access + "partial ";
                    }
                }

                if (Class.Interfaces.Count > 0)
                {
                    Interfaces = " : " + string.Join(", ", Class.Interfaces);
                }

                ClassName = "\t" + Class.Access + "class " + Class.Name + Interfaces + "\n\t{\n";

                foreach (var Property in Class.PropertyItems)
                {
                    string Summary = "";
                    if (!string.IsNullOrEmpty(Property.Summary))
                    {
                        Summary = "\t\t/// <summary>\n" +
                            $"\t\t/// {Property.Summary}\n" +
                            "\t\t/// </summary>\n";
                    }

                    Property.NewName = Property.NewName.FirstCharToUpperCase();
                    Property.Attributes = AddOrRemoveAttributes(Property, SelectedAttributes);

                    if (Options == ModelOptions.Standard)
                    {
                        var ObservableProperty = Property.Attributes.Where(x => x.Contains("[ObservableProperty]")).FirstOrDefault();
                        if (ObservableProperty != null) { Property.Attributes.Remove(ObservableProperty); }

                        string Attributes = string.Join("\n\t\t", Property.Attributes);

                        if (string.IsNullOrEmpty(Property.GetSet)) { Property.GetSet = "{ get; set; }"; }

                        Properties = Properties + Summary + "\t\t" + Attributes + "\n\t\t" +
                            Property.Access + " " + Property.Type + " " + Property.NewName + " " + Property.GetSet + Property.Value + "\n\n";
                    }
                    else if (Options == ModelOptions.INotifyPropertyChanged)
                    {
                        var ObservableProperty = Property.Attributes.Where(x => x.Contains("[ObservableProperty]")).FirstOrDefault();
                        if (ObservableProperty != null) { Property.Attributes.Remove(ObservableProperty); }

                        string Attributes = string.Join("\n\t\t", Property.Attributes);

                        string Value = "";
                        if (!string.IsNullOrEmpty(Property.Value)) { Value = $" = {Property.Value}"; }

                        string INotifyPropertyString = "\n\t\t{\n" +
                                "\t\t\tget { return " + Property.NewName.ToLower() + "; } \n" +
                                "\t\t\tset { \n " +
                                "\t\t\t\tif (" + Property.NewName.ToLower() + " != value) \n" +
                                "\t\t\t\t{ \n" +
                                "\t\t\t\t\t" + Property.NewName.ToLower() + " = value; \n" +
                                "\t\t\t\t\tNotifyPropertyChanged(\"" + Property.NewName.Trim() + "\"); \n" +
                                "\t\t\t\t} \n" +
                                "\t\t\t} \n" +
                                "\t\t}\n" +
                                "\t\tprivate " + Property.Type + " " + Property.NewName.ToLower() + Value + "; \n \n";

                        Properties = Properties + Summary + "\t\t" + Attributes + "\n\t\t" +
                            Property.Access + " " + Property.Type + " " + Property.NewName + INotifyPropertyString;

                    }
                    else if (Options == ModelOptions.MVVM)
                    {
                        string Attributes = string.Join("\n\t\t", Property.Attributes);

                        string Value = "";
                        if (!string.IsNullOrEmpty(Property.Value)) { Value = $" = {Property.Value}"; }

                        if (!Attributes.Contains("ObservableProperty")) { Attributes = Attributes + "\n\t\t" + "[ObservableProperty]"; }

                        Properties = Properties + Summary + "\t\t" + Attributes + "\n\t\t" + 
                            "private " + Property.Type + " " + Property.NewName.FirstCharToLowerCase() + Value + "; \n \n";
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

        public static ObservableCollection<string> AddOrRemoveAttributes(PropertyItem Item, List<ModelAttributes> SelectedAttributes)
        {
            //Column
            if (SelectedAttributes.Contains(ModelAttributes.Column))
            {
                var Value = Item.Attributes.Where(x => x.StartsWith("[Column")).FirstOrDefault();
                if (Value == null) { Item.Attributes.Add("[Column(\"" + Item.OriginalName + "\")]"); }
            }
            else
            {
                var Value = Item.Attributes.Where(x => x.StartsWith("[Column")).FirstOrDefault();
                if (Value != null) { Item.Attributes.Remove(Value); }
            }

            //JsonPropertyValue
            if (SelectedAttributes.Contains(ModelAttributes.JsonPropertyName))
            {
                var Value = Item.Attributes.Where(x => x.StartsWith("[JsonPropertyName")).FirstOrDefault();
                if (Value == null)
                {
                    Item.Attributes.Add("[JsonPropertyName(\"" + Item.NewName + "\")]");
                }
                else
                {
                    Item.Attributes.Remove(Value);
                    Item.Attributes.Insert(0, "[JsonPropertyName(\"" + Item.NewName + "\")]");
                }
            }
            else
            {
                var Value = Item.Attributes.Where(x => x.StartsWith("[JsonPropertyName")).FirstOrDefault();
                if (Value != null) { Item.Attributes.Remove(Value); }
            }

            return Item.Attributes;
        }


    }

}
