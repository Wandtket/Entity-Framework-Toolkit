using Microsoft.UI.Xaml;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using WinRT;


namespace EFToolkit.Extensions
{

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
