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
using System.Collections.ObjectModel;
using EFToolkit.Controls.Dialogs;
using System.Collections;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Data;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.VisualBasic;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Pickers;
using Windows.Storage;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EFToolkit.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModelEditorPage : Page
    {

        NamespaceItem CurrentNamespaceItem;

        List<ModelAttributes> SelectedAttributes = new(); 

        public ModelEditorPage()
        {
            this.InitializeComponent();          
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            OutputWebView.DefaultBackgroundColor = (App.Current.Resources["HTMLBlockBackground"] as SolidColorBrush).Color;
            OutputWebView.EnsureCoreWebView2Async();

            if (Settings.Current.ModelEditorSelectedType == "StandardModelOption") { StandardModelOption.IsChecked = true; }
            else if (Settings.Current.ModelEditorSelectedType == "INotifyModelOption") { INotifyModelOption.IsChecked = true; }
            else if (Settings.Current.ModelEditorSelectedType == "MVVModelOption") { MVVModelOption.IsChecked = true; }

            if (JsonPropertyNameToggle.IsChecked) { SelectedAttributes.Add(ModelAttributes.JsonPropertyName); }
            else { SelectedAttributes.Remove(ModelAttributes.JsonPropertyName); }

            if (ColumnToggle.IsChecked) { SelectedAttributes.Add(ModelAttributes.Column); }
            else { SelectedAttributes.Remove(ModelAttributes.Column); }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            AcronymLibrarySelector.SuggestedItemsSource = Toolkit.AcronymLibraries;
            AcronymLibrarySelector.ItemsSource = Toolkit.SelectedAcronymLibraries;
        }


        public void ToggleTeachTips(bool Toggle)
        {
            //TeachTip.IsOpen = Toggle;
        }


        private async void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                FileTypeFilter = { ".cs", ".txt", }
            };

            picker.SetOwnerWindow(App.Current.ActiveWindow);

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                string Text = System.IO.File.ReadAllText(file.Path);
                await Input.SetText(Text);
                Convert(Text);
            }
        }

        private async void Input_Paste(object sender, TextControlPasteEventArgs e)
        {
            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                string text = await dataPackageView.GetTextAsync();
                Convert(text);
            }
        }

        private async void PasteModel_Click(object sender, RoutedEventArgs e)
        {
            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                string text = await dataPackageView.GetTextAsync();
                await Input.SetText(text);
                Convert(text);
            }
        }

        private async void ClearInput_Click(object sender, RoutedEventArgs e)
        {
            await Input.SetText("");
            OutputWebView.NavigateToString("");
        }



        private async void Convert(string text, bool FromInput = true)
        {
            ModelOptions Options = ModelOptions.Standard;
            if (StandardModelOption.IsChecked == true) { Options = ModelOptions.Standard; }
            else if (INotifyModelOption.IsChecked == true) { Options = ModelOptions.INotifyPropertyChanged; }
            else if (MVVModelOption.IsChecked == true) { Options = ModelOptions.MVVM; }

            if (FromInput == true)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    CurrentNamespaceItem = await Toolkit.ModelBuilder(text);
                    RearrangeAppButton.IsEnabled = true;
                }
            } 

            string Body = await Toolkit.ConvertModel(CurrentNamespaceItem, Options, SelectedAttributes);

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

            OutputWebView.NavigateToString(Document.DocumentNode.OuterHtml);

            await Output.SetText(Body);
            await Input.SetText(Input.GetText());          
        }

        private void ModelOption_Click(object sender, RoutedEventArgs e)
        {
            RadioMenuFlyoutItem SelectedItem = (RadioMenuFlyoutItem)sender;
            Settings.Current.ModelEditorSelectedType = SelectedItem.Name;

            Convert(Input.GetText());
        }


        private void AcronymLibrarySelector_TokenItemAdded(CommunityToolkit.WinUI.Controls.TokenizingTextBox sender, object args)
        {
            Convert(Input.GetText());
        }

        private void AcronymLibrarySelector_TokenItemRemoved(CommunityToolkit.WinUI.Controls.TokenizingTextBox sender, object args)
        {
            Convert(Input.GetText());
        }




        private async void RearrangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentNamespaceItem.ClassItems.Count > 0)
            {
                ClassItemsCombobox.ItemsSource = CurrentNamespaceItem.ClassItems;

                if (ClassItemsCombobox.Items.Count == 1) { ClassItemsCombobox.SelectedIndex = 0; }

                try { await RearrangeDialog.ShowAsync(); } catch { }
            }
            else
            {
                await MessageBox.Show("No items to rearrange", "Notice");
            }
        }

        List<PropertyItem?> FoundItems = new();
        int SearchFoundIndex = 0;

        private void RearrangeSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && sender.Text.Length > 1)
            {
                RearrangeListView.DeselectAll();

                FoundItems = new();
                for (int i = 0; i < RearrangeListView.Items.Count; i++)
                {
                    PropertyItem item = (PropertyItem)RearrangeListView.Items[i];
                    if (!string.IsNullOrEmpty(item.OriginalName) || !string.IsNullOrEmpty(item.NewName))
                    {
                        if (item.OriginalName.ToLower().Contains(sender.Text.ToLower()) ||
                            item.NewName.ToLower().Contains(sender.Text.ToLower()))
                        {
                            RearrangeListView.SelectRange(new ItemIndexRange(i, 1));
                            FoundItems.Add(item);
                        }
                    }
                }

                if (FoundItems.Count >= 1)
                {
                    RearrangeListView.ScrollIntoView(FoundItems[0]);
                }

                RearrangeSearch.FoundCount = FoundItems.Count;
            }
            else
            {
                RearrangeListView.DeselectAll();
                RearrangeListView.SelectedIndex = -1;
                RearrangeSearch.FoundCount = 0;
            }
        }

        private void RearrangeSearch_FindUp_Clicked(object sender, RoutedEventArgs e)
        {
            if (SearchFoundIndex <= FoundItems.Count)
            {
                if (SearchFoundIndex != 0) { SearchFoundIndex--; }
                RearrangeListView.ScrollIntoView(FoundItems[SearchFoundIndex]);
            }
        }

        private void RearrangeSearch_FindDown_Clicked(object sender, RoutedEventArgs e)
        {
            if (SearchFoundIndex <= FoundItems.Count)
            {
                if (SearchFoundIndex != FoundItems.Count - 1) { SearchFoundIndex++; }
                RearrangeListView.ScrollIntoView(FoundItems[SearchFoundIndex]);
            }
        }


        private void ClassItemsCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            ClassItem? SelectedItem = box?.SelectedItem as ClassItem;
            RearrangeListView.ItemsSource = SelectedItem?.PropertyItems;
        }

        private void RearrangeDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Convert("", FromInput: false);
            ResetRearrangeDialog();
        }

        private void RearrangeDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Convert(Input.GetText());
            ResetRearrangeDialog();
        }

        private void RearrangeDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ResetRearrangeDialog();
        }

        private void ResetRearrangeDialog()
        {
            ClassItemsCombobox.ItemsSource = null;
            ClassItemsCombobox.Items.Clear();
        }


        private async void CopyOutput_Click(object sender, RoutedEventArgs e)
        {
            DataPackage package = new();
            package.SetText(Output.GetText());
            Clipboard.SetContent(package);
        }


        private void ToggleJsonPropertyName_Click(object sender, RoutedEventArgs e)
        {
            ToggleMenuFlyoutItem item = (ToggleMenuFlyoutItem)sender;

            if (item.IsChecked) { SelectedAttributes.Add(ModelAttributes.JsonPropertyName); }
            else { SelectedAttributes.Remove(ModelAttributes.JsonPropertyName); }

            Convert(Input.GetText());
        }

        private void ToggleColumn_Click(object sender, RoutedEventArgs e)
        {
            ToggleMenuFlyoutItem item = (ToggleMenuFlyoutItem)sender;

            if (item.IsChecked) { SelectedAttributes.Add(ModelAttributes.Column); }
            else { SelectedAttributes.Remove(ModelAttributes.Column); }

            Convert(Input.GetText());
        }


    }
}
