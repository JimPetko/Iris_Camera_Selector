using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Iris_Camera_Selector
{
    internal class HD_Dental
    {
        public const string DLL_NAME = "HD_Dental_SDK.dll";

        public const int CH04_SUCCESS = 0;
        public const int CH04_ERROR = 1;

        public const int RYGAIN = 0;
        public const int BYGAIN = 1;

        [DllImport(DLL_NAME)] public static extern int NET_CH04_API_SetCallback(int iCameraIndex, IntPtr CallbackFunc, int pCBContext);

        [DllImport(DLL_NAME)] public static extern void NET_CH04_API_GetApiVersion(StringBuilder cVersion);

        [DllImport(DLL_NAME)] public static extern void NET_CH04_API_GetSdkVersion(StringBuilder cVersion);

        [DllImport(DLL_NAME)] public static extern int NET_CH04_API_GetFirmwareVersion(int iCameraIndex, StringBuilder cVersion);

        [DllImport(DLL_NAME)] public static extern int NET_CH04_API_GetHibVersion(int iCameraIndex, StringBuilder cVersion);

        [DllImport(DLL_NAME)] public static extern int NET_CH04_API_Init(ref int speed);

        [DllImport(DLL_NAME)] public static extern int NET_CH04_API_Destroy(int iCameraIndex);

        [DllImport(DLL_NAME)] public static extern int NET_CH04_API_Open(int iCameraIndex, bool bRollbuf);

        [DllImport(DLL_NAME)] public static extern int NET_CH04_API_Close(int iCameraIndex);

        [DllImport(DLL_NAME)] public static extern int NET_CH04_API_GetUPVersion(int iCameraIndex, byte[] versionData);

        [DllImport(DLL_NAME)] public static extern int NET_CH04_API_GetSerialNumber(int iCameraIndex, StringBuilder cSerial);

        [DllImport(DLL_NAME)] public static extern int NET_CH04_API_Check();

        [DllImport(DLL_NAME)] public static extern int NET_CH04_API_Start(int iCameraIndex);

        [DllImport(DLL_NAME)] public static extern int NET_CH04_API_Stop(int iCameraIndex);

        [DllImport(DLL_NAME)] public static extern int NET_CH04_API_SetChromaHueValue(int iCameraIndex, int iType, int iValue);

        [DllImport(DLL_NAME)] public static extern int NET_CH04_API_GetChromaHueValues(int iCameraIndex, int[] iValues);

        [DllImport(DLL_NAME)] public static extern int NET_CH04_API_GetLightMode(int iCameraIndex, ref bool LightOn);

        [DllImport(DLL_NAME)] public static extern int NET_CH04_API_SetLightMode(int iCameraIndex, bool LightOn);

        [DllImport(DLL_NAME)] public static extern int NET_CH04_API_ForceSleep(int iCameraIndex, bool SleepOn);
    }
}