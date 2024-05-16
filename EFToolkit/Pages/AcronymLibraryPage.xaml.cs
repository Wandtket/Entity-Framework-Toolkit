using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using EFToolkit.Controls.Dialogs;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EFToolkit.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AcronymLibraryPage : Page
    {


        public AcronymLibraryPage()
        {
            this.InitializeComponent();

            AcronymLibraryList.ItemsSource = Toolkit.AcronymLibraries;
        }



        private void AddLibrary_Click(object sender, RoutedEventArgs e)
        {
            var Library = new AcronymLibrary() { };
            Library.LibraryItems.Add(new AcronymItem());

            Toolkit.AcronymLibraries.Add(Library);
            AcronymLibraryList.SelectedItem = Library;
        }

        private void AddTranslation_Click(object sender, RoutedEventArgs e)
        {
            AcronymLibrary SelectedItem = (AcronymLibrary)AcronymLibraryList.SelectedItem;
            SelectedItem.LibraryItems.Add(new AcronymItem());
            TranslationsTotal.Text = SelectedItem.LibraryItems.Count.ToString();
        }

        private async void PasteTranslationItem_Click(object sender, RoutedEventArgs e)
        {
            AcronymLibrary SelectedItem = (AcronymLibrary)AcronymLibraryList.SelectedItem;

            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                string text = await dataPackageView.GetTextAsync();

                //Seperate rows by Carriage Return
                char[] rowSplitter = { (char)10, (char)13 };

                //Seperate Columns by Tab
                char[] columnSplitter = { (char)9 };

                //get the text from clipboard
                string dataInClipboard = await dataPackageView.GetTextAsync();

                //split it into lines
                string[] rowsInClipboard = dataInClipboard.Split(rowSplitter, StringSplitOptions.RemoveEmptyEntries);

                // loop through the lines, split them into cells and place the values in the corresponding cell.
                int iRow = 0;
                while (iRow < rowsInClipboard.Length)
                {
                    //split row into cell values
                    string[] valuesInRow = rowsInClipboard[iRow].Split(columnSplitter);

                    //cycle through cell values
                    int iCol = 0;
                
                    if (Settings.CodeFormatOptions == CodeFormatOptions.CamelCase)
                    {
                        SelectedItem.LibraryItems.Add(new AcronymItem()
                        {
                            Acronym = valuesInRow[0].Trim().Replace(" ", ""),
                            Translation = valuesInRow[1].Trim().Replace(" ", ""),
                        });
                    }
                    else if (Settings.CodeFormatOptions == CodeFormatOptions.Snake_Case)
                    {
                        SelectedItem.LibraryItems.Add(new AcronymItem()
                        {
                            Acronym = valuesInRow[0].Trim().Replace(" ", ""),
                            Translation = valuesInRow[1].Trim().Replace(" ", "_"),
                        });
                    }

                    while (iCol < valuesInRow.Length) { iCol += 1; }
                    iRow += 1;
                }
            }

            TranslationsTotal.Text = SelectedItem.LibraryItems.Count.ToString();
        }

        private async void ClearTranslations_Click(object sender, RoutedEventArgs e)
        {
            AcronymLibrary SelectedItem = (AcronymLibrary)AcronymLibraryList.SelectedItem;
            var Result = await ConfirmBox.Show("This action cannot be undone", "Clear Library Items?");
            if (Result == ContentDialogResult.Primary)
            {
                SelectedItem.LibraryItems.Clear();
                TranslationsTotal.Text = SelectedItem.LibraryItems.Count.ToString();
                Toolkit.SaveLibraries();
            }
        }


        private void SaveLibrary_Click(object sender, RoutedEventArgs e)
        {
            Toolkit.SaveLibraries();
        }

        private async void RemoveLibrary_Click(object sender, RoutedEventArgs e)
        {
            var Result = await ConfirmBox.Show("This action will clear all acronyms in this library and cannot be undone.", "Delete Library?");
            if (Result == ContentDialogResult.Primary)
            {
                var s = (FrameworkElement)sender;
                var d = s.DataContext;

                AcronymLibrary Library = (AcronymLibrary)AcronymLibraryList.SelectedItem;
                Toolkit.AcronymLibraries.Remove(Library);
            }
        }



        private void SearchTranslation_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && sender.Text.Length > 1)
            {
                AcronymLibrary SelectedItem = (AcronymLibrary)AcronymLibraryList.SelectedItem;

                AcronymItemList.DeselectAll();

                int FoundCount = 0;
                AcronymItem? FoundItem = null;
                for (int i = 0; i < AcronymItemList.Items.Count; i++)
                {
                    AcronymItem Item = (AcronymItem)AcronymItemList.Items[i];
                    if (!string.IsNullOrEmpty(Item.Acronym))
                    {
                        if (Item.Acronym.Contains(sender.Text, StringComparison.CurrentCultureIgnoreCase))
                        {
                            AcronymItemList.SelectRange(new ItemIndexRange(i, 1));
                            FoundCount = FoundCount + 1;
                            FoundItem = Item;
                        }
                        else if (Item.Translation.Contains(sender.Text, StringComparison.CurrentCultureIgnoreCase))
                        {
                            AcronymItemList.SelectRange(new ItemIndexRange(i, 1));
                            FoundCount = FoundCount + 1;
                            FoundItem = Item;
                        }
                    }
                }

                if (FoundCount == 1)
                {
                    AcronymItemList.ScrollIntoView(FoundItem);
                }
            }
            else
            {
                AcronymItemList.DeselectAll();
            }
        }

        private void AcronymLibraryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AcronymLibrary SelectedItem = (AcronymLibrary)AcronymLibraryList.SelectedItem;
            if (SelectedItem != null)
            {
                AcronymItemList.ItemsSource = SelectedItem.LibraryItems;
                TranslationsTotal.Text = SelectedItem.LibraryItems.Count.ToString();
            }
        }

        private void Acronym_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            var s = (FrameworkElement)sender;
            var d = s.DataContext;
            AcronymItem Item = (AcronymItem)d;
            AcronymLibrary SelectedItem = (AcronymLibrary)AcronymLibraryList.SelectedItem;

            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var NewItem = new AcronymItem();
                SelectedItem.LibraryItems.Insert(SelectedItem.LibraryItems.IndexOf(Item) + 1, NewItem);

                AcronymItemList.SelectedItem = NewItem;
                TranslationsTotal.Text = SelectedItem.LibraryItems.Count.ToString();
            }
        }


        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var s = (FrameworkElement)sender;
            var d = s.DataContext;

            AcronymItem Item = (AcronymItem)d;
            AcronymLibrary SelectedItem = (AcronymLibrary)AcronymLibraryList.SelectedItem;

            SelectedItem.LibraryItems.Remove(Item);
            TranslationsTotal.Text = SelectedItem.LibraryItems.Count.ToString();
        }

        private void Acronym_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            box.Text = box.Text.Trim().Replace(" ", "");
            Toolkit.SaveLibraries();
        }


        private void Translation_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            if (Settings.CodeFormatOptions == CodeFormatOptions.CamelCase)
            {
                box.Text = box.Text.Trim().Replace(" ", "");
            }
            else if (Settings.CodeFormatOptions == CodeFormatOptions.Snake_Case)
            {
                box.Text = box.Text.Trim().Replace(" ", "_");
            }
            Toolkit.SaveLibraries();
        }

    }



}
