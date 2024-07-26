using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFToolkit.Models
{
    public partial class ClassItem : ObservableObject
    {
        [ObservableProperty]
        private string summary = string.Empty;

        [ObservableProperty]
        private string access = string.Empty;

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private List<string> interfaces = new();

        [ObservableProperty]
        private List<PropertyItem> propertyItems = new();
    }
}
