using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Iris_Camera_Selector
{
    public static class Win32Hid
    {
        // HidD_GetHidGuid
        [DllImport("hid.dll", SetLastError = true)]
        public static extern void HidD_GetHidGuid(ref Guid hidGuid);

        // HidD_GetAttributes
        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_GetAttributes(
            IntPtr HidDeviceObject,
            ref HIDD_ATTRIBUTES Attributes);

        // HidD_GetSerialNumberString
        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_GetSerialNumberString(
            IntPtr HidDeviceObject,
            [MarshalAs(UnmanagedType.LPWStr)]
        StringBuilder Buffer,
            uint BufferLength);

        // HidD_GetPreparsedData
        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_GetPreparsedData(IntPtr HidDeviceObject, ref IntPtr PreparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_FreePreparsedData(ref IntPtr PreparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern int HidP_GetCaps(IntPtr preparsedData, ref HIDP_CAPS capabilities);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_FlushQueue(IntPtr HidDeviceObject);

        public struct HIDD_ATTRIBUTES
        {
            public int Size;
            public short VendorID;
            public short ProductID;
            public short VersionNumber;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HIDP_CAPS
        {
            public short Usage;
            public short UsagePage;
            public short InputReportByteLength;
            public short OutputReportByteLength;
            public short FeatureReportByteLength;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public short[] Reserved;

            public short NumberLinkCollectionNodes;
            public short NumberInputButtonCaps;
            public short NumberInputValueCaps;
            public short NumberInputDataIndices;
            public short NumberOutputButtonCaps;
            public short NumberOutputValueCaps;
            public short NumberOutputDataIndices;
            public short NumberFeatureButtonCaps;
            public short NumberFeatureValueCaps;
            public short NumberFeatureDataIndices;
        }
    }
}