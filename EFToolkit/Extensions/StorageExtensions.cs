using EFToolkit.Controls.Dialogs;
using Microsoft.UI.Xaml;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using WinRT;


namespace EFToolkit.Extensions
{

    internal class Files
    {



        /// <summary>
        /// Provide any image file or image URL and this will open it in the default photo viewer. 
        /// </summary>
        /// <param name="ImageFilePath"></param>
        public static async Task OpenImage(string ImageFilePath)
        {
            try
            {
                StorageFile Image = await StorageFile.GetFileFromPathAsync(ImageFilePath);

                StorageFolder TempFolder = await Folders.GetTempFolder();
                StorageFile NewFile = await Image.CopyAsync(TempFolder, Image.Name, NameCollisionOption.GenerateUniqueName);

                await Launcher.LaunchFileAsync(NewFile);              
            }
            catch (Exception e)
            {
                await MessageBox.Show("An error has occurred attempting to open the image");
            }
        }



        /// <summary>
        /// Store files temporarily (ex: Webview2 2MB NavigateToString Bug)
        /// <see href="https://github.com/MicrosoftEdge/WebView2Feedback/issues/1355"/>
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="FileExtension"></param>
        /// <param name="Contents"></param>
        public static async Task<StorageFile> SaveTempFile(string FileName, string FileExtension, string Contents)
        {
            StorageFolder TempFolder = await Folders.GetTempFolder();

            StorageFile TempFile = await TempFolder.CreateFileAsync(FileName + FileExtension, CreationCollisionOption.GenerateUniqueName);
            await FileIO.WriteTextAsync(TempFile, Contents, Windows.Storage.Streams.UnicodeEncoding.Utf8);

            return TempFile;
        }

        /// <summary>
        /// Delete the temporary file once it has been used.
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="FileExtension"></param>
        public static async Task DeleteTempFile(string FileName, string FileExtension)
        {
            StorageFolder LocalFolder = ApplicationData.Current.LocalFolder;
            await LocalFolder.CreateFolderAsync("Temp", CreationCollisionOption.OpenIfExists);

            StorageFolder TempFolder = await LocalFolder.GetFolderAsync("Temp");

            StorageFile TabFile = await TempFolder.GetFileAsync(FileName + FileExtension);
            File.Delete(TabFile.Path);
        }



        private static PrivateFontCollection _privateFontCollection = new PrivateFontCollection();

        public static FontFamily GetFontFamilyByName(string name)
        {
            return _privateFontCollection.Families.FirstOrDefault(x => x.Name == name);
        }

        public static void AddFont(string fullFileName)
        {
            AddFont(File.ReadAllBytes(fullFileName));
        }

        public static void AddFont(byte[] fontBytes)
        {
            var handle = GCHandle.Alloc(fontBytes, GCHandleType.Pinned);
            IntPtr pointer = handle.AddrOfPinnedObject();
            try
            {
                _privateFontCollection.AddMemoryFont(pointer, fontBytes.Length);
            }
            finally
            {
                handle.Free();
            }
        }

    }

    internal class Folders
    {


        /// <summary>
        /// A temporary folder is sometimes required for opening files without modifying the original.
        /// </summary>
        /// <returns></returns>
        public static async Task<StorageFolder> GetTempFolder()
        {
            StorageFolder LocalFolder = ApplicationData.Current.LocalFolder;
            await LocalFolder.CreateFolderAsync("Temp", CreationCollisionOption.OpenIfExists);

            StorageFolder TempFolder = await LocalFolder.GetFolderAsync("Temp");
            return TempFolder;
        }

        /// <summary>
        /// Clearing the temporary folder on startup will prevent the app file size from getting too large.
        /// </summary>
        public static async Task ClearTempFolder()
        {
            StorageFolder folder = await Folders.GetTempFolder();
            try
            {
                //Attempt to delete the folder
                await folder.DeleteAsync();

            }
            catch { }

        }

    }


    /// <summary>
    /// Required to set owner window for File Picker.
    /// </summary>
    public static class WindowsStoragePickerExtensions
    {
        [ComImport]
        [Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IInitializeWithWindow
        {
            void Initialize(IntPtr hwnd);
        }
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
        internal interface IWindowNative
        {
            IntPtr WindowHandle { get; }
        }

        /// <summary>
        /// Sets the owner window for this <see cref="FileOpenPicker"/>. This is required when running in WinUI for Desktop.
        /// </summary>
        public static void SetOwnerWindow(this FileOpenPicker picker, Window window)
        {
            SetOwnerWindow(picker.As<IInitializeWithWindow>(), window);
        }

        /// <summary>
        /// Sets the owner window for this <see cref="FileSavePicker"/>. This is required when running in WinUI for Desktop.
        /// </summary>
        public static void SetOwnerWindow(this FileSavePicker picker, Window window)
        {
            SetOwnerWindow(picker.As<IInitializeWithWindow>(), window);
        }

        /// <summary>
        /// Sets the owner window for this <see cref="FolderPicker"/>. This is required when running in WinUI for Desktop.
        /// </summary>
        public static void SetOwnerWindow(this FolderPicker picker, Window window)
        {
            SetOwnerWindow(picker.As<IInitializeWithWindow>(), window);
        }

        private static void SetOwnerWindow(IInitializeWithWindow picker, Window window)
        {
            // See https://github.com/microsoft/microsoft-ui-xaml/issues/4100#issuecomment-774346918
            picker.Initialize(window.As<IWindowNative>().WindowHandle);
        }
    }

}
