using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFToolkit.Models
{

    /// <summary>
    /// Stores the entirety of Shareable data in a single file.
    /// </summary>
    public partial class DataItem : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<AcronymLibrary> acronymLibraries = new();

        [ObservableProperty]
        private ObservableCollection<DatabaseItem> databaseItems = new();

        [ObservableProperty]
        private ObservableCollection<SchemaItem> schemaItems = new();
    }



}
