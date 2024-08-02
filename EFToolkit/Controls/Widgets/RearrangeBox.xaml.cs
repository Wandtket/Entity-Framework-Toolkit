using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EFToolkit.Controls.Widgets
{

    public sealed partial class RearrangeBox : UserControl
    {

        public RearrangeBox()
        {
            this.InitializeComponent();
        }


        public string Text { get { return RearrangeSearch.Text; } }

        public int FoundCount { set { RearrangeSearchFoundBox.Text = "Found: " + value.ToString(); } }


        public delegate void TextChangedHandler(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args);
        public event TextChangedHandler TextChanged;
        private void RearrangeSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            TextChanged?.Invoke(sender, args);
        }


        public delegate void FindUpHandler(object sender, RoutedEventArgs e);
        public event FindUpHandler FindUp_Clicked;
        private void FindUpButton_Click(object sender, RoutedEventArgs e)
        {
            FindUp_Clicked?.Invoke(sender, e);
        }

        public delegate void FindDownHandler(object sender, RoutedEventArgs e);
        public event FindDownHandler FindDown_Clicked;
        private void FindDownButton_Click(object sender, RoutedEventArgs e)
        {
            FindDown_Clicked?.Invoke(sender, e);
        }


    }


}
