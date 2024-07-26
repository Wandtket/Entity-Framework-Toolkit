using ColorCode.Styling;
using EFToolkit.Extensions;
using HtmlAgilityPack;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.ApplicationModel.DataTransfer;
using EFToolkit.Models;
using EFToolkit.Enums;


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

            Output.DefaultBackgroundColor = (App.Current.Resources["HTMLBlockBackground"] as SolidColorBrush).Color;
            Output.EnsureCoreWebView2Async();
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

                    string html = "";
                    string NewBackgroundColor = "";
                    if (App.Current.RequestedTheme == ApplicationTheme.Dark)
                    {
                        var formatter = new HtmlFormatter(StyleDictionary.DefaultDark);
                        html = formatter.GetHtmlString(Body, ColorCode.Languages.CSharp);
                        NewBackgroundColor = "#303440";
                    }
                    else if (App.Current.RequestedTheme == ApplicationTheme.Light)
                    {
                        var formatter = new HtmlFormatter(StyleDictionary.DefaultLight);
                        html = formatter.GetHtmlString(Body, ColorCode.Languages.CSharp);

                    }


                    HtmlDocument Document = new HtmlDocument();
                    Document.LoadHtml(html);

                    string CurrentStyle = Document.DocumentNode.SelectSingleNode("//div").GetAttributeValue("style", "");
                    string BackgroundColor = CurrentStyle.Substring(CurrentStyle.IndexOf("background-color:"), 7);
                    CurrentStyle = CurrentStyle.Replace(BackgroundColor, App.Current.Resources["HTMLBlockBackground"].ToString());

                    Document.DocumentNode.SelectSingleNode("//div").SetAttributeValue("style", CurrentStyle + "tab-size:5;");
                    
                    Output.NavigateToString(Document.DocumentNode.OuterHtml);

                    await Input.SetText(Input.GetText());
                }
            }
        }

        private void Input_TextChanged(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
