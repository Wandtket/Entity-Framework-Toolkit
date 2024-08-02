using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFToolkit.Models
{
    public partial class AcronymItem : ObservableObject
    {

        [ObservableProperty]
        private string options = "Contains";

        [ObservableProperty]
        private string acronym;

        [ObservableProperty]
        private string translation;

    }

}
