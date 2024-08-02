using EFToolkit.Controls.Dialogs;
using EFToolkit.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;

namespace EFToolkit
{

    public partial class Toolkit
    {
        public static ObservableCollection<AcronymLibrary> AcronymLibraries = new();
        public static ObservableCollection<AcronymLibrary> SelectedAcronymLibraries = new();

        public static ObservableCollection<DatabaseItem> DatabaseItems = new();

        public static ObservableCollection<SchemaItem> SchemaItems = new();
        public static ObservableCollection<SchemaItem> SelectedSchemaItems = new();

        public static string DataFileName = "Entity Framework Toolkit Data.eftk";
        private static string SelectedSchemaItemFileName = "SelectedSchemas.efsl";
        private static string SelectedAcronymLibraryFileName = "SelectedAcronyms.efal";


        public static async Task LoadData()
        {
            StorageFolder Folder = ApplicationData.Current.LocalFolder;

            //Load Main Shareable Data
            if (File.Exists(Folder.Path + "\\" + DataFileName))
            {
                StorageFile file = await Folder.GetFileAsync(DataFileName);
                var Data = JsonSerializer.Deserialize<DataItem>(File.ReadAllText(file.Path));
                if (Data != null)
                {
                    #region Acronym Libraries

                    foreach (var Library in Data.AcronymLibraries)
                    {
                        Library.LibraryItems = new ObservableCollection<AcronymItem>(Library.LibraryItems.OrderBy(x => x.Acronym).ToList());
                    }

                    //Add an All Item.
                    if (Data.AcronymLibraries.Where(x => x.Title == "All").FirstOrDefault() == null)
                    {
                        Data.AcronymLibraries.Add(new AcronymLibrary() { Title = "All" });
                    }

                    AcronymLibraries = new ObservableCollection<AcronymLibrary>(Data.AcronymLibraries.OrderBy(x => x.Title));

                    #endregion

                    #region Database Items 

                    DatabaseItems = new ObservableCollection<DatabaseItem>(Data.DatabaseItems.OrderBy(x => x.InitialCatalog));

                    #endregion

                    #region Schema Items 

                    SchemaItems = new ObservableCollection<SchemaItem>(Data.SchemaItems.OrderBy(x => x.Schema));

                    #endregion
                }
            }

            //Load Previous User Selection
            //Selected Libraries
            if (File.Exists(Folder.Path + "\\" + SelectedAcronymLibraryFileName))
            {
                StorageFile file = await Folder.GetFileAsync(SelectedAcronymLibraryFileName);
                var Libraries = JsonSerializer.Deserialize<ObservableCollection<AcronymLibrary>>(File.ReadAllText(file.Path));
                if (Libraries != null)
                {
                    foreach (var Library in Libraries)
                    {
                        Library.LibraryItems = new ObservableCollection<AcronymItem>(Library.LibraryItems.OrderBy(x => x.Acronym).ToList());
                    }

                    SelectedAcronymLibraries = new ObservableCollection<AcronymLibrary>(Libraries.OrderBy(x => x.Title));
                }
            }

            //Selected Schemas
            if (File.Exists(Folder.Path + "\\" + SelectedSchemaItemFileName))
            {
                StorageFile file = await Folder.GetFileAsync(SelectedSchemaItemFileName);
                var Libraries = JsonSerializer.Deserialize<ObservableCollection<SchemaItem>>(File.ReadAllText(file.Path));
                if (Libraries != null)
                {
                    SelectedSchemaItems = new ObservableCollection<SchemaItem>(Libraries.OrderBy(x => x.Schema));
                }
            }

        }

        public static async Task SaveData()
        {
            StorageFolder Folder = ApplicationData.Current.LocalFolder;

            DataItem Data = new DataItem()
            {
                AcronymLibraries = AcronymLibraries,
                DatabaseItems = DatabaseItems,
                SchemaItems = SchemaItems,
            };

            //All Libaries
            StorageFile file = await Folder.CreateFileAsync(DataFileName, CreationCollisionOption.OpenIfExists);
            var Json = JsonSerializer.Serialize(Data);
            await File.WriteAllTextAsync(file.Path, Json);

            //Selected Libaries
            StorageFile SelectedAcronymsfile = await Folder.CreateFileAsync(SelectedAcronymLibraryFileName, CreationCollisionOption.OpenIfExists);
            var SelectedAcronymsJson = JsonSerializer.Serialize(SelectedAcronymLibraries);
            await File.WriteAllTextAsync(SelectedAcronymsfile.Path, SelectedAcronymsJson);

            StorageFile SelectedSchemasfile = await Folder.CreateFileAsync(SelectedSchemaItemFileName, CreationCollisionOption.OpenIfExists);
            var SelectedSchemasJson = JsonSerializer.Serialize(SelectedSchemaItems);
            await File.WriteAllTextAsync(SelectedSchemasfile.Path, SelectedSchemasJson);
        }


        public static async void ImportData()
        {
            // Create a file picker
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Current.ActiveWindow);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your file picker
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Add(".eftk");

            // Open the picker for the user to pick a file
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                var Data = JsonSerializer.Deserialize<DataItem>(File.ReadAllText(file.Path));

                var result = await ConfirmBox.Show("This action will import the data from the selected file, nothing will be removed or modified.", "Import Data?");
                if (result == ContentDialogResult.Primary)
                {
                    foreach (var Library in Data.AcronymLibraries) { AcronymLibraries.Add(Library); }

                    foreach (var Database in Data.DatabaseItems) { DatabaseItems.Add(Database); }

                    foreach (var Schema in Data.SchemaItems) { SchemaItems.Add(Schema); }
                }
            }
        }

        public static async void ExportData()
        {
            // Create a file picker
            FileSavePicker savePicker = new Windows.Storage.Pickers.FileSavePicker();

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Current.ActiveWindow);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

            // Set options for your file picker
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("EFToolkit Data File", new List<string>() { ".eftk" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "Entity Framework Toolkit Data";

            // Open the picker for the user to pick a file
            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);

                DataItem Data = new DataItem()
                {
                    AcronymLibraries = AcronymLibraries,
                    DatabaseItems = DatabaseItems,
                    SchemaItems = SchemaItems,
                };

                var Json = JsonSerializer.Serialize(Data);

                // write to file
                await File.WriteAllTextAsync(file.Path, Json);

                // Another way to write a string to the file is to use this instead:
                // await FileIO.WriteTextAsync(file, "Example file contents.");

                // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status == FileUpdateStatus.Complete)
                {

                }
            }
            else
            {

            }

        }
    }
}
