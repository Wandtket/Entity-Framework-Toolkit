using EFToolkit.Pages;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;

namespace EFToolkit.Extensions
{
    public static class RichEditBoxExtensions
    {
        public static async Task SetText(this RichEditBox text, string Value)
        {
            text.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, Value);

            if (Settings.Current.CodeColoring == true)
            {
                await text.ColorCode();
            }
            else
            {
                var range = text.Document.GetRange(0, text.GetText().Length);
                range.CharacterFormat.ForegroundColor = (App.Current.Resources["Foreground"] as SolidColorBrush).Color;
                text.Document.ApplyDisplayUpdates();
            }
        }

        public static string GetText(this RichEditBox text)
        {
            text.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out string value);
            return value;
        }

        public static async Task ColorCode(this RichEditBox text)
        {
            text.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;

            text.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out string value);

            var Range = text.Document.GetRange(0, value.Length);
            Range.CharacterFormat.ForegroundColor = Colors.White;

            foreach (var Format in Codelist)
            {
                if (Format.BeginText == null && Format.EndText == null)
                {
                    if (value.Contains(Format.Text)) 
                    {
                        foreach (var index in value.AllIndicesOf(Format.Text))
                        {
                            int BeginIndex = index;
                            int EndIndex = index + Format.Text.Length;

                            var range = text.Document.GetRange(BeginIndex, EndIndex);
                            if (range != null)
                            {
                                range.CharacterFormat.ForegroundColor = Format.Color.IsThemeAware();
                            }
                        };
                    }                   
                }
                else
                {
                    if (value.Contains(Format.BeginText) && value.Contains(Format.EndText))
                    {
                        var BeginIndexes = value.AllIndicesOf(Format.BeginText).ToArray();
                        var EndIndexes = value.AllIndicesOf(Format.EndText).ToArray();

                        for (int i = 0; i < BeginIndexes.Count(); i++)
                        {
                            var range = text.Document.GetRange(BeginIndexes[i], EndIndexes[i] + Format.EndText.Length);
                            if (range != null)
                            {
                                range.CharacterFormat.ForegroundColor = Format.Color.IsThemeAware();
                            }
                        }
                    }
                }
            }
            
            text.Document.ApplyDisplayUpdates();
            text.Document.ClearUndoRedoHistory();

            text.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }


        private static Color CodeBlue = Color.FromArgb(1, 84, 156, 214);
        private static Color CodeGreen = Color.FromArgb(1, 78, 201, 145);
        private static Color CodeLightGreen = Color.FromArgb(1, 78, 201, 176);
        private static Color CodeYellow = Color.FromArgb(1, 220, 220, 162);
        private static Color CodeLightYellow = Color.FromArgb(1, 134, 198, 145);
          

        private static readonly List<CodeFormatting> Codelist = new List<CodeFormatting>()
        {
            new CodeFormatting() { Text = "string ", Color = CodeBlue },
            new CodeFormatting() { Text = "string? ", Color = CodeBlue },

            new CodeFormatting() { Text = "int ", Color = CodeBlue },
            new CodeFormatting() { Text = "int? ", Color = CodeBlue },

            new CodeFormatting() { Text = "bool ", Color = CodeBlue },
            new CodeFormatting() { Text = "bool? ", Color = CodeBlue },

            new CodeFormatting() { Text = "decimal ", Color = CodeBlue },
            new CodeFormatting() { Text = "decimal? ", Color = CodeBlue },

            new CodeFormatting() { Text = "byte ", Color = CodeBlue },
            new CodeFormatting() { Text = "byte? ", Color = CodeBlue },

            new CodeFormatting() { Text = "object ", Color = CodeBlue },
            new CodeFormatting() { Text = "object? ", Color = CodeBlue },

            new CodeFormatting() { Text = "double ", Color = CodeBlue },
            new CodeFormatting() { Text = "double? ", Color = CodeBlue },

            new CodeFormatting() { Text = "DateTime ", Color = CodeLightYellow },
            new CodeFormatting() { Text = "DateTime? ", Color = CodeLightYellow },

            new CodeFormatting() { Text = "Guid ", Color = CodeLightYellow },
            new CodeFormatting() { Text = "Guid? ", Color = CodeLightYellow },


            new CodeFormatting() { Text = "Single ", Color = CodeLightYellow },
            new CodeFormatting() { Text = "Single? ", Color = CodeLightYellow },

            new CodeFormatting() { Text = "Int16 ", Color = CodeLightYellow },
            new CodeFormatting() { Text = "Int16? ", Color = CodeLightYellow },

            new CodeFormatting() { Text = "Int64 ", Color = CodeLightYellow },
            new CodeFormatting() { Text = "Int64? ", Color = CodeLightYellow },

            new CodeFormatting() { Text = "internal ", Color = CodeBlue },
            new CodeFormatting() { Text = "partial ", Color = CodeBlue },

            new CodeFormatting() { Text = "public ", Color = CodeBlue },
            new CodeFormatting() { Text = "private ", Color = CodeBlue },
            new CodeFormatting() { Text = "static ", Color = CodeBlue },
            new CodeFormatting() { Text = "readonly ", Color = CodeBlue },

            new CodeFormatting() { Text = "class ", Color = CodeBlue },
            new CodeFormatting() { Text = "new ", Color = CodeBlue },

            new CodeFormatting() { Text = "void ", Color = CodeBlue },


            new CodeFormatting() { Text = "return ", Color = CodeBlue },


            new CodeFormatting() { Text = "get; ", Color = CodeBlue },
            new CodeFormatting() { Text = "set; ", Color = CodeBlue },

            new CodeFormatting() { Text = "get ", Color = CodeBlue },
            new CodeFormatting() { Text = "set ", Color = CodeBlue },

            //new CodeFormatting() { Text = "{", Color = CodeBlue },        
            //new CodeFormatting() { Text = "}", Color = CodeBlue },

            new CodeFormatting() { Text = "if ", Color = CodeBlue },


            new CodeFormatting() { Text = "ObservableObject", Color = CodeLightGreen },

            new CodeFormatting() { Text = "[ObservableProperty]", Color = CodeLightGreen },

            new CodeFormatting() { Text = "NotifyPropertyChanged", Color = CodeYellow },

            new CodeFormatting() { BeginText = "[", EndText = "]", Color = Colors.White },

            new CodeFormatting() { BeginText = "(", EndText = ")", Color = Colors.White },

            new CodeFormatting() { BeginText = "/// <summary>", EndText = "/// </summary>", Color = CodeGreen },

            new CodeFormatting() { BeginText = "[JsonPropertyName(\"", EndText = "\")]", Color = CodeLightGreen },

            new CodeFormatting() { BeginText = "[Column(\"", EndText = "\")]", Color = CodeLightGreen },

        };
               
        public class CodeFormatting
        {

            public string Text { get; set; }

            public Color Color { get; set; }

            public string? BeginText { get; set; } = null;

            public string? EndText { get; set; } = null;

        }


    }
}
