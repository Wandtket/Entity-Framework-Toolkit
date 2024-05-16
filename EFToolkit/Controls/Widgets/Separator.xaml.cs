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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EFToolkit.Controls.Widgets
{
    public sealed partial class Separator : UserControl
    {
        public Separator()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
        "Orientation",
        typeof(Orientation),
        typeof(Separator),
        new PropertyMetadata(Orientation.Horizontal, OnOrientationChange));

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        private static void OnOrientationChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Separator box = (Separator)d;

            if ((Orientation)e.NewValue == Orientation.Horizontal)
            {
                box.HorizontalSeparator.Visibility = Visibility.Visible;
                box.VerticalSeparator.Visibility = Visibility.Collapsed;
            }
            else if ((Orientation)e.NewValue == Orientation.Vertical)
            {
                box.HorizontalSeparator.Visibility = Visibility.Collapsed;
                box.VerticalSeparator.Visibility = Visibility.Visible;
            }
        }
    }
}
