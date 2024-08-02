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

        public object ItemsSource { get { return AcronymTextBox.ItemsSource; } set { AcronymTextBox.ItemsSource = value; AcronymFlyoutBox.ItemsSource = value; } }

        public object SuggestedItemsSource { get { return AcronymTextBox.SuggestedItemsSource; } set { AcronymTextBox.SuggestedItemsSource = value; AcronymFlyoutBox.SuggestedItemsSource = value; } }


        private void AcronymTextBox_TokenItemAdded(CommunityToolkit.WinUI.Controls.TokenizingTextBox sender, object args)
        {
            Toolkit.SaveData();
            TokenItemAdded?.Invoke(sender, args);
        }

        private void AcronymTextBox_TokenItemRemoved(CommunityToolkit.WinUI.Controls.TokenizingTextBox sender, object args)
        {
            Toolkit.SaveData();
            TokenItemRemoved?.Invoke(sender, args);
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

        private void UserControl_SizeChanged(object sender, Microsoft.UI.Xaml.SizeChangedEventArgs e)
        {
            if (e.NewSize.Width <= 300)
            {
                CollapseButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                AcronymTextBox.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                CollapseButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                AcronymTextBox.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            }
        }


        public delegate void TokenItemAddedHandler(CommunityToolkit.WinUI.Controls.TokenizingTextBox sender, object args);
        public event TokenItemAddedHandler TokenItemAdded;

        public delegate void TokenItemRemovedHandler(CommunityToolkit.WinUI.Controls.TokenizingTextBox sender, object args);
        public event TokenItemRemovedHandler TokenItemRemoved;

    }
}
