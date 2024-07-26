using EFToolkit.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFToolkit.Models;

namespace EFToolkit
{

    public partial class Toolkit
    {

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
                                if (SQLColumnName.Length! > NextIndex + 1)
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
    }
}
