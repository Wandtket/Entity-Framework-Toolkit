using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFToolkit.Models
{
    public partial class NamespaceItem : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<string> usings = new();

        [ObservableProperty]
        private string nameSpace = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ClassItem> classItems = new();
    }

}
