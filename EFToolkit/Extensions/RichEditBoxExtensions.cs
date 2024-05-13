using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Printing3D;

namespace EFToolkit.Extensions
{
    public static class RichEditBoxExtensions
    {
        public static void SetText(this RichEditBox text, string Value)
        {
            List<ColorRange> colorRanges = new List<ColorRange>();
            


            text.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, Value);
        }

        public static string GetText(this RichEditBox text)
        {
            text.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out string value);
            return value;
        }

        public static void ColorCode(this RichEditBox editor)
        {                    
            var selectedText = editor.TextDocument.Selection.se("string", editor.GetText().Length, Microsoft.UI.Text.FindOptions.None);
            if (selectedText != null)
            {
                var charFormatting = selectedText;
                charFormatting.ForegroundColor = Colors.Green;
                selectedText.CharacterFormat = charFormatting;
            }


        }


        

        private class ColorRange
        {
            public int BeginIndex { get; set; }

            public int EndIndex { get; set; }

            public Colors Color { get; set; }

        }



    }
}
