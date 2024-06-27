using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace EFToolkit.Extensions
{

    public static class DataTransfer
    {

        public static void Show(string uri)
        {
            IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(App.Current.ActiveWindow);
            IDataTransferManagerInterop interop = Windows.ApplicationModel.DataTransfer.DataTransferManager.As<IDataTransferManagerInterop>();

            var guid = Guid.Parse("a5caee9b-8708-49d1-8d36-67d25a8da00c");
            var iop = DataTransferManager.As<IDataTransferManagerInterop>();
            var transferManager = DataTransferManager.FromAbi(iop.GetForWindow(windowHandle, guid));

            transferManager.DataRequested += (DataTransferManager dtm, DataRequestedEventArgs args) =>
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetWebLink(new Uri(uri));
                dataPackage.Properties.Title = "Share uri \"" + uri + "\"";
                args.Request.Data = dataPackage;
            };

            interop.ShowShareUIForWindow(windowHandle);
        }


        [System.Runtime.InteropServices.ComImport, System.Runtime.InteropServices.Guid("3A3DCD6C-3EAB-43DC-BCDE-45671CE800C8")]
        [System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
        interface IDataTransferManagerInterop
        {
            IntPtr GetForWindow([System.Runtime.InteropServices.In] IntPtr appWindow, [System.Runtime.InteropServices.In] ref Guid riid);
            void ShowShareUIForWindow(IntPtr appWindow);
        }

    }
}
