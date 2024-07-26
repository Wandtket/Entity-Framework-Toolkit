using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFToolkit.Models
{

    public partial class DesignItem : ObservableObject
    {
        [ObservableProperty]
        private int index = 0;

        [ObservableProperty]
        private string rearrangeText = string.Empty;

        [ObservableProperty]
        private bool isPrimaryKey = false;

        [ObservableProperty]
        private string columnName = string.Empty;

        [ObservableProperty]
        private string objectName = string.Empty;

        [ObservableProperty]
        private string dataType = string.Empty;

        [ObservableProperty]
        private bool allowNulls = false;

        [ObservableProperty]
        private string defaultValue = string.Empty;

    }

}
