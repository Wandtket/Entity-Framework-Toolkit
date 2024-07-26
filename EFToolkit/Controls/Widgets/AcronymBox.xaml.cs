using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using EFToolkit.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EFToolkit.Controls.Widgets
{
    public sealed partial class AcronymBox : UserControl
    {
        public AcronymBox()
        {
            this.InitializeComponent();
        }



        /// <summary>
        /// Returns the user typed text from the box
        /// </summary>
        public string Text { get { return AcronymTextBox.Text; } }

        public object ItemsSource { get { return AcronymTextBox.ItemsSource; } set { AcronymTextBox.ItemsSource = value; } }

        public object SuggestedItemsSource { get { return AcronymTextBox.SuggestedItemsSource; } set { AcronymTextBox.SuggestedItemsSource = value; } }


        private void AcronymTextBox_TokenItemAdded(CommunityToolkit.WinUI.Controls.TokenizingTextBox sender, object args)
        {
            Toolkit.SaveData();
        }

        private void AcronymTextBox_TokenItemRemoved(CommunityToolkit.WinUI.Controls.TokenizingTextBox sender, object args)
        {
            Toolkit.SaveData();
        }

        public delegate void TextChangedHandler(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args);
        public event TextChangedHandler TextChanged;
        private void AcronymTextBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ObservableCollection<AcronymLibrary> filteredList = new();
                foreach (var filter in Toolkit.AcronymLibraries)
                {
                    if (filter.Title.Contains(AcronymTextBox.Text.Trim(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        filteredList.Add(filter);
                    }
                }

                AcronymTextBox.SuggestedItemsSource = filteredList.Distinct();
            }

            TextChanged?.Invoke(sender, args);
        }
    }
}
