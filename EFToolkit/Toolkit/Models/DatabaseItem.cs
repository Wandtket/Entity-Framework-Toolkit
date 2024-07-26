using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFToolkit.Models
{
    public partial class DatabaseItem : ObservableObject
    {
        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string dataSource;

        [ObservableProperty]
        private string initialCatalog;

        [ObservableProperty]
        private string userId;

        [ObservableProperty]
        private string password;

        public string GetConnectionString()
        {
            return $"Data Source ={DataSource};Initial Catalog={initialCatalog};User Id={userId};Password={password}";
        }
    }

}
