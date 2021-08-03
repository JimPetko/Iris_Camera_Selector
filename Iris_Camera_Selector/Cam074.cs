using System;
using System.Runtime.InteropServices;

namespace Iris_Camera_Selector
{
    public class Cam074
    {
        // <constants>

        public const byte CAM_074_DEFAULT_MODE = 0;
        public const byte CAM_074_LUM_MODE = 1;

        // <errors>

        public const int CAM_074_STATUS_SUCCESS = 0;

        public const int CAM_074_STATUS_UNKNOWN_ERROR = 1;
        public const int CAM_074_STATUS_BAD_ARGUMENT = 2;
        public const int CAM_074_STATUS_INVALID_CMD_ID = 3;
        public const int CAM_074_STATUS_NOT_IMPLEMENTED = 4;

        public const int CAM_074_STATUS_INVALID_DATA_BLOCK = 5;
        public const int CAM_074_STATUS_INVALID_CHUNK_ID = 6;
        public const int CAM_074_STATUS_DATA_BLOCK_TOO_LARGE = 7;
        public const int CAM_074_STATUS_MISSSING_DATA_CHUNKS = 8;
        public const int CAM_074_STATUS_SENSOR_DISABLED = 9;

        public const int CAM_074_STATUS_USB_RD_ERR = 10;
        public const int CAM_074_STATUS_USB_WR_ERR = 11;
        public const int CAM_074_STATUS_FLASH_RD_ERR = 12;
        public const int CAM_074_STATUS_FLASH_WR_ERR = 13;
        public const int CAM_074_STATUS_OUT_OF_RANGE = 14;

        public const int CAM_074_STATUS_HW_ERR = 15;
        public const int CAM_074_STATUS_DEVICE_IN_SLEEP_ERR = 16;
        public const int CAM_074_STATUS_DEVICE_IN_AUTOFOCUS = 17;

        // <command ids>

        public const int CAM_074_GET_ALL_TEMPERATURES = 119;
        public const int CAM_074_GET_MODE = 131;
        public const int CAM_074_SET_MODE = 132;

        // < >

        private const byte reportId = 4;
        private IntPtr hDevHandle;
        private const int RD_WR_TIMEOUT = 1000;

        public int hidReport(byte commandId, ref byte[] data, byte length)
        {
            int stat = 0;

            IntPtr HIDReportEvent = Kernel32.CreateEvent(IntPtr.Zero, false, true, "Cam063Event");
            if (HIDReportEvent == IntPtr.Zero)
            {
                return 0x0100;
            }

            byte[] buffer = new byte[64];
            buffer[0] = reportId;
            buffer[1] = commandId;

            Array.Copy(data, 0, buffer, 2, length);

            uint bytesTransfered = 0;

            Kernel32.OVERLAPPED HIDOverlapped = new Kernel32.OVERLAPPED();
            HIDOverlapped.Offset = 0;
            HIDOverlapped.OffsetHigh = 0;
            HIDOverlapped.EventHandle = HIDReportEvent;
            HIDOverlapped.Internal = UIntPtr.Zero;
            HIDOverlapped.InternalHigh = UIntPtr.Zero;

            int result = Kernel32.WriteFile(hDevHandle, ref buffer[0],
                                     64, out bytesTransfered, ref HIDOverlapped);
            if (result == 0)
            {
                int hr = Marshal.GetLastWin32Error();
                if (hr != Kernel32.ERROR_IO_PENDING)
                {
                    Kernel32.CancelIo(hDevHandle);
                    Kernel32.CloseHandle(HIDReportEvent);
                    return 0x0200;
                }
            }

            result = Kernel32.WaitForSingleObject(HIDReportEvent, RD_WR_TIMEOUT);
            if (result == Kernel32.WAIT_TIMEOUT)
            {
                return 0x0300;
            }
            else if (result == Kernel32.WAIT_ABANDONED)
            {
                return 0x0400;
            }

            result = Kernel32.ReadFile(hDevHandle, ref buffer[0], 64, out bytesTransfered, ref HIDOverlapped);

            if (result == 0)
            {
                int hr = Marshal.GetLastWin32Error();
                if (hr != Kernel32.ERROR_IO_PENDING)
                {
                    Kernel32.CancelIo(hDevHandle);
                    Kernel32.CloseHandle(HIDReportEvent);
                    return 0x0600;
                }
            }

            result = Kernel32.WaitForSingleObject(HIDReportEvent, RD_WR_TIMEOUT);
            if (result == Kernel32.WAIT_TIMEOUT)
            {
                return 0x0300;
            }
            else if (result == Kernel32.WAIT_ABANDONED)
            {
                return 0x0400;
            }

            if (buffer[0] != reportId)
            {
                stat = 0x0700;      // error report id
            }

            if (buffer[1] != commandId)
            {
                stat = 0x0800;      // error cmd id
            }

            if (buffer[2] != 0)
            {
                stat = buffer[2];   // error reported from camera
            }

            if (length > 1)
            {
                Array.Copy(buffer, 3, data, 0, length - 2);
            }

            Kernel32.CloseHandle(HIDReportEvent);

            return stat;
        }

        public int open(short VendorId, short ProductId)
        {
            Guid hidGuid = Guid.Empty;
            IntPtr hDevInfo;
            bool success;
            SetupDI.SP_DEVICE_INTERFACE_DATA interfaceData;
            SetupDI.SP_DEVICE_INTERFACE_DETAIL_DATA interfaceDetail;
            SetupDI.SP_DEVINFO_DATA devInfoData;
            uint size;
            uint index = 0;
            bool goodDevice = false;

            Win32Hid.HidD_GetHidGuid(ref hidGuid);

            hDevInfo = SetupDI.SetupDiGetClassDevs(ref hidGuid, IntPtr.Zero, IntPtr.Zero,
                        SetupDI.DiGetClassFlags.Present | SetupDI.DiGetClassFlags.DeviceInterface);

            index = 0;
            while (true)
            {
                interfaceData = new SetupDI.SP_DEVICE_INTERFACE_DATA();
                interfaceData.cbSize = (uint)Marshal.SizeOf(interfaceData);

                success = SetupDI.SetupDiEnumDeviceInterfaces(hDevInfo, IntPtr.Zero, ref hidGuid, index, ref interfaceData);
                index++;

                if (success == false) { break; }

                devInfoData = new SetupDI.SP_DEVINFO_DATA();
                devInfoData.cbSize = (uint)Marshal.SizeOf(devInfoData);

                interfaceDetail = new SetupDI.SP_DEVICE_INTERFACE_DETAIL_DATA();
                interfaceDetail.cbSize = IntPtr.Size == 8 ? 8 : (uint)(4 + Marshal.SystemDefaultCharSize);

                success = SetupDI.SetupDiGetDeviceInterfaceDetail(
                    hDevInfo,
                    ref interfaceData, ref interfaceDetail, 256, out size, ref devInfoData);

                if (success == false) { continue; }

                // Open the device as a file so that we can query it with HID and read/write to it.
                IntPtr devHandle = Kernel32.CreateFile(
                    interfaceDetail.DevicePath,
                    (uint)(Kernel32.AccessRights.GENERIC_READ | Kernel32.AccessRights.GENERIC_WRITE),
                    (uint)(Kernel32.ShareModes.FILE_SHARE_READ | Kernel32.ShareModes.FILE_SHARE_WRITE),
                    IntPtr.Zero,
                    (uint)(Kernel32.CreationDispositions.OPEN_EXISTING),
                    Kernel32.Overlapped, IntPtr.Zero
                );

                Win32Hid.HIDD_ATTRIBUTES hidAttribs = new Win32Hid.HIDD_ATTRIBUTES();
                success = Win32Hid.HidD_GetAttributes(devHandle, ref hidAttribs);

                if (success && hidAttribs.VendorID == VendorId && hidAttribs.ProductID == ProductId)
                {
                    IntPtr preparsedDataPtr = new IntPtr();
                    Win32Hid.HIDP_CAPS caps = new Win32Hid.HIDP_CAPS();

                    Win32Hid.HidD_GetPreparsedData(devHandle, ref preparsedDataPtr);
                    Win32Hid.HidP_GetCaps(preparsedDataPtr, ref caps);
                    Win32Hid.HidD_FreePreparsedData(ref preparsedDataPtr);

                    if ((caps.Usage == 0) && (caps.UsagePage == 1))
                    {
                        goodDevice = true;
                        hDevHandle = devHandle;
                    }
                }
            }

            success = SetupDI.SetupDiDestroyDeviceInfoList(hDevInfo);
            hDevInfo = IntPtr.Zero;

            if (goodDevice == false)
            {
                return 1;
            }

            return 0;
        }
    }
}