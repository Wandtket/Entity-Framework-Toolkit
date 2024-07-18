using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using EFToolkit.Extensions;
using EFToolkit.Controls.Dialogs;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EFToolkit.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SelectDescriberPage : Page
    {
        public SelectDescriberPage()
        {
            this.InitializeComponent();
        }

        public void ToggleTeachTips(bool Toggle)
        {
            SelectStatementInputTeachTip.IsOpen = Toggle;
            SelectStatementOutputTeachTip.IsOpen = Toggle;
        }

        private async void Input_TextChanged(object sender, RoutedEventArgs e)
        {
            if (Input.GetText() == "") { Output.SetText(""); return; }

            await Output.SetText(Toolkit.ConvertSelectToDescriber(Input.GetText()));
        }

        private async void Input_Paste(object sender, TextControlPasteEventArgs e)
        {
            await Task.Delay(50);
            await Input.SetText(Input.GetText());
        }
    }
}
