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
using System.Collections.ObjectModel;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EFToolkit.Controls.Widgets
{
    public sealed partial class SearchBox : UserControl
    {

        public SearchBox()
        {
            this.InitializeComponent();

            SearchTextBox.ItemsSource = SearchStrings;
            SearchStrings.CollectionChanged += SearchStrings_CollectionChanged;
        }


        public ObservableCollection<string> SearchStrings = new();
        private void SearchStrings_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            string PlaceholderText = "Search";
            if (SearchStrings.Count > 0)
            {
                PlaceholderText = "Searching: ";
                foreach (var SearchString in SearchStrings)
                {
                    PlaceholderText = PlaceholderText + SearchString + ", ";
                }
            }
            SearchTextBox.PlaceholderText = PlaceholderText;
        }


        /// <summary>
        /// Returns the user typed text from the box
        /// </summary>
        public string Text { get { return SearchTextBox.Text; } }


        public object ItemsSource { get { return SearchTextBox.ItemsSource; } set { SearchTextBox.ItemsSource = value; } }

        public object SuggestedItemsSource { get { return SearchTextBox.SuggestedItemsSource; } set { SearchTextBox.SuggestedItemsSource = value; } }


        private async void SearchTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && !string.IsNullOrEmpty(SearchTextBox.Text))
            {
                SearchStrings.Add(SearchTextBox.Text.Trim());
            }
        }



        public delegate void TextChangedHandler(SearchBox sender, AutoSuggestBoxTextChangedEventArgs args);
        public event TextChangedHandler TextChanged;
        private void SearchTextBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            TextChanged?.Invoke(this, args);
        }


        public delegate void TokenItemRemovedHandler(SearchBox sender, object args);
        public event TokenItemRemovedHandler TokenItemRemoved;
        private void SearchTextBox_TokenItemRemoved(CommunityToolkit.WinUI.Controls.TokenizingTextBox sender, object args)
        {
            TokenItemRemoved?.Invoke(this, args);
        }

    }
}
