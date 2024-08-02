using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFToolkit.Models
{
    public partial class CredentialItem : ObservableObject
    {
        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string password;
    }

}
