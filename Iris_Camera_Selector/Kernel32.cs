using System;
using System.Runtime.InteropServices;

namespace Iris_Camera_Selector
{
    static public class Kernel32
    {
        public const uint Overlapped = 0x40000000;

        public const int ERROR_IO_PENDING = 997;

        public const uint INFINITE = 0xFFFFFFFF;
        public const uint WAIT_ABANDONED = 0x00000080;
        public const uint WAIT_OBJECT_0 = 0x00000000;
        public const uint WAIT_TIMEOUT = 0x00000102;

        [Flags]
        public enum AccessRights : uint
        {
            GENERIC_READ = (0x80000000),
            GENERIC_WRITE = (0x40000000),
            GENERIC_EXECUTE = (0x20000000),
            GENERIC_ALL = (0x10000000)
        }

        [Flags]
        public enum ShareModes : uint
        {
            FILE_SHARE_READ = 0x00000001,
            FILE_SHARE_WRITE = 0x00000002,
            FILE_SHARE_DELETE = 0x00000004
        }

        public enum CreationDispositions
        {
            CREATE_NEW = 1,
            CREATE_ALWAYS = 2,
            OPEN_EXISTING = 3,
            OPEN_ALWAYS = 4,
            TRUNCATE_EXISTING = 5
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CancelIo(IntPtr hFile);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

        [DllImport("kernel32.dll")]
        public static extern bool ResetEvent(IntPtr hEvent);

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        public static extern int WaitForSingleObject(IntPtr handle, int milliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int GetOverlappedResult(
          IntPtr hFile,
          IntPtr OverlappedData,
          out uint lpNumberOfBytesTransferred,
          int bWait);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateEvent(
          IntPtr lpEventAttributes,
          int bManualReset,
          int bInitialState,
          string lpName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int ReadFile(IntPtr hFile,
            //IntPtr lpBuffer,
            ref byte lpBuffer,
            uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead,
            ref OVERLAPPED OverlappedData
            //IntPtr OverlappedData
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int WriteFile(
          IntPtr Handle,
             //IntPtr lpBuffer,
             ref byte lpBuffer,
          uint NumberOfBytesToWrite,
          out uint NumberOfBytesWritten,
            //IntPtr OverlappedData
            ref OVERLAPPED OverlappedData
            );

        [StructLayout(LayoutKind.Sequential)]
        public struct OVERLAPPED
        {
            public UIntPtr Internal;
            public UIntPtr InternalHigh;
            public uint Offset;
            public uint OffsetHigh;
            public IntPtr EventHandle;
        }
    }
}