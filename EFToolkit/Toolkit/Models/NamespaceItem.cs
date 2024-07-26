using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFToolkit.Models
{
    public partial class NamespaceItem : ObservableObject
    {
        [ObservableProperty]
        private List<string> usings = new();

        [ObservableProperty]
        private string nameSpace = string.Empty;

        [ObservableProperty]
        private List<ClassItem> classItems = new();
    }

}
