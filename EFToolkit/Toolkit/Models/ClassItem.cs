using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ObservableCollection<string> interfaces = new();

        [ObservableProperty]
        private ObservableCollection<PropertyItem> propertyItems = new();
    }
}
