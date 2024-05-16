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

        private async void Input_TextChanged(object sender, RoutedEventArgs e)
        {
            if (Input.GetText() == "") { Output.SetText(""); return; }

            Output.SetText(Toolkit.ConvertSelectToDescriber(Input.GetText()));
        }

    }
}