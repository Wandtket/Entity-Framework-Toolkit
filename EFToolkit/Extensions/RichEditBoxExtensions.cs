using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EFToolkit.Extensions
{
    public static class RichEditBoxExtensions
    {
        public static void SetText(this RichEditBox text, string Value)
        {
            text.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, Value);
        }

        public static string GetText(this RichEditBox text)
        {
            text.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out string value);
            return value;
        }
    }
}
