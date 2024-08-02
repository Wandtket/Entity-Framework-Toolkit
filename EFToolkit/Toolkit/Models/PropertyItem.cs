using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFToolkit.Models
{

    public partial class PropertyItem : ObservableObject
    {
        [ObservableProperty]
        private string summary = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> attributes = new();

        [ObservableProperty]
        private string access = string.Empty;

        [ObservableProperty]
        private bool isStatic = false;

        [ObservableProperty]
        private bool isOverride = false;

        [ObservableProperty]
        private string type = string.Empty;

        [ObservableProperty]
        private string originalName = string.Empty;

        [ObservableProperty]
        private string newName = string.Empty;

        [ObservableProperty]
        private string getSet = string.Empty;

        [ObservableProperty]
        private string value = string.Empty;
    }

}
