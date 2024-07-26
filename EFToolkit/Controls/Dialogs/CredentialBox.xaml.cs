using EFToolkit.Extensions;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using EFToolkit.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EFToolkit.Controls.Dialogs
{

    /// <summary>
    /// CommentBox is a larger InputBox and won't allow the enter key to submit the dialog.
    /// </summary>
    internal class CredentialBox
    {

        public static async Task<CredentialItem?> Show(string Title = "", string Username = "", string primaryText = "Submit", string secondaryText = "Cancel")
        {
            try
            {
                CredentialBoxContent Dialog = new CredentialBoxContent();
                Dialog.XamlRoot = App.Current.ActiveWindow.Content.XamlRoot;
                Dialog.Title = Title;
                Dialog.PrimaryButtonText = primaryText;
                Dialog.SecondaryButtonText = secondaryText;
                Dialog.DefaultButton = ContentDialogButton.Secondary;

                Dialog.Username.Text = Username;               

                //Show
                ContentDialogResult result = await Dialog.ShowAsync();
                if (result == ContentDialogResult.Primary && !string.IsNullOrEmpty(Dialog.Username.Text) && !string.IsNullOrEmpty(Dialog.Password.Password))
                {
                    return new CredentialItem() { Username = Dialog.Username.Text, Password = Dialog.Password.Password };
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }


    public sealed partial class CredentialBoxContent : ContentDialog
    {
        public CredentialBoxContent()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Username.Text)) { Password.Focus(Microsoft.UI.Xaml.FocusState.Keyboard); }
        }
    }
}
