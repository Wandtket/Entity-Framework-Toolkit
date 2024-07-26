using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFToolkit.Pages;
using EFToolkit.Models;

namespace EFToolkit
{

    public partial class Toolkit
    {
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

    }
}
