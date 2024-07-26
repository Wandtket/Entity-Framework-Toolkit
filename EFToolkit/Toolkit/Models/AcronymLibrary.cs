using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFToolkit.Models
{
    public partial class AcronymLibrary : ObservableObject
    {
        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private ObservableCollection<AcronymItem> libraryItems = new();
    }

}
