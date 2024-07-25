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
using EFToolkit.Controls.Dialogs;
using EFToolkit.Extensions;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EFToolkit.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModelEditorPage : Page
    {
        public ModelEditorPage()
        {
            this.InitializeComponent();
        }

        public void ToggleTeachTips(bool Toggle)
        {
            //TeachTip.IsOpen = Toggle;
        }


        private async void CopyOutput_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private async void RearrangeButton_Click(object sender, RoutedEventArgs e)
        {

        }


        private async void Input_Paste(object sender, TextControlPasteEventArgs e)
        {
            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                string text = await dataPackageView.GetTextAsync();

                if (!string.IsNullOrEmpty(text))
                {
                    ModelOptions Options = ModelOptions.Standard;
                    if (StandardModelOption.IsChecked == true) { Options = ModelOptions.Standard; }
                    else if (INotifyModelOption.IsChecked == true) { Options = ModelOptions.INotifyPropertyChanged; }
                    else if (MVVModelOption.IsChecked == true) { Options = ModelOptions.MVVM; }

                    NamespaceItem Item = await Toolkit.ModelBuilder(text);
                    string Body = await Toolkit.ConvertModel(Item, Options);

                    await Output.SetText(Body);
                    await Input.SetText(Input.GetText());
                }
            }
        }

        private void Input_TextChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
