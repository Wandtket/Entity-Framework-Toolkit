using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;


namespace EFToolkit.Models
{

    public partial class TableItem : ObservableObject
    {

        [ObservableProperty]
        private string primaryTable = "";

        [Ignore]
        public string PrimaryTableShort 
        { 
            get 
            { 
                if (primaryTable?.Length >= 20)
                {
                    return primaryTable?.Substring(0, 20) + "...";
                }
                else { return primaryTable; }
            } 
        }

        [ObservableProperty]
        private ObservableCollection<DesignItem> primaryDesignItems = new();

        [ObservableProperty]
        private string secondaryTable = "";

        [Ignore]
        public string SecondaryTableShort
        {
            get
            {
                if (secondaryTable?.Length >= 20)
                {
                    return secondaryTable?.Substring(0, 20) + "...";
                }
                else { return secondaryTable; }
            }
        }

        [ObservableProperty]
        private ObservableCollection<DesignItem> secondaryDesignItems = new();
   

        [ObservableProperty]
        [Ignore]
        private bool existsInPrimary = true;

        [ObservableProperty]
        [Ignore]
        private bool existsInSecondary = false;

        [ObservableProperty]
        private bool matches = true;

    }
}
