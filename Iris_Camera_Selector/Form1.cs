using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Threading;
using System.Windows.Forms;

namespace Iris_Camera_Selector
{
    public partial class Form1 : Form
    {
        private bool bIris2, bIrisHD, bIrisX80, bAppRunning, bThreadComplete;
        private NotifyIcon ni_TrayIcon;
        private ContextMenu cm_TrayIcon;

        public Form1()
        {
            InitializeComponent();
            bIrisX80 = false;
            bIris2 = false;
            bIrisHD = false;
            bAppRunning = true;

            //Tray Icon Setup
            ni_TrayIcon = new NotifyIcon();
            ni_TrayIcon.Icon = Iris_Camera_Selector.Properties.Resources.SwitcherIcon2;
            ni_TrayIcon.Text = "Digital Doc Iris Switch Tool";
            ni_TrayIcon.Visible = true;

            cm_TrayIcon = new ContextMenu();
            cm_TrayIcon.MenuItems.Add("Exit", OnExit);

            ni_TrayIcon.ContextMenu = cm_TrayIcon;
            Thread thread = new Thread(CheckThread);
            thread.Start();
        }

        //Iris X80 HWID
        //USB\VID_20F1&PID_0404&MI_00\6&21FDE103&0&0000
        //Iris 2
        //USB\VID_EB1A&PID_2860\5&350590A7&0&6
        //Iris HD
        //USB\VID_20F1&PID_0104\5&350590A7&0&21

        private void Form1_Load(object sender, EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
        }

        private void OnExit(object sender, EventArgs e)
        {
            bAppRunning = false;
            if (bThreadComplete)
            {
                ni_TrayIcon.Dispose();
                this.Close();
            }
            else
            {
                Thread.Sleep(250);
                OnExit(sender, e);
            }
        }

        private void CheckThread()
        {
            while (bAppRunning)
            {
                bThreadComplete = false;
                List<USBDeviceInfo> USBDevices = GetUSBDevices();
                foreach (var USBDevice in USBDevices)
                {
                    bIrisHD = false;
                    bIris2 = false;
                    bIrisX80 = false;
                    
                    //ignore REV and MI codes incase hardware revisions are needed.
                    if (USBDevice.PnpDeviceID.ToString().Contains(@"VID_20F1&PID_0404"))//Iris x80 Connected
                    {
                        Console.WriteLine(USBDevice.PnpDeviceID);
                        bIrisX80 = true;
                        EnableCamera("X80");
                        bIris2 = false;
                        bIrisHD = false;
                        ni_TrayIcon.Icon = Iris_Camera_Selector.Properties.Resources.X80ToothIcon;
                        ni_TrayIcon.Text = "Iris X80 Connected, Ready to use";
                        break;
                    }
                    if (USBDevice.PnpDeviceID.ToString().Contains(@"VID_20F1&PID_0104"))//Iris HD Connected
                    {
                        Console.WriteLine(USBDevice.PnpDeviceID);
                        bIrisHD = true;
                        EnableCamera("HD");
                        bIris2 = false;
                        bIrisX80 = false;
                        ni_TrayIcon.Icon = Iris_Camera_Selector.Properties.Resources.IrisHD_Oldie;
                        ni_TrayIcon.Text = "Iris HD Connected, Ready to use";
                        break;
                    }
                    if (USBDevice.PnpDeviceID.ToString().Contains(@"VID_EB1A&PID_2860"))//Iris 2.0 Connected
                    {
                        Console.WriteLine(USBDevice.PnpDeviceID);
                        bIris2 = true;
                        EnableCamera("2.0");
                        bIrisX80 = false;
                        bIrisHD = false;
                        ni_TrayIcon.Icon = Iris_Camera_Selector.Properties.Resources.Iris2_Oldie;
                        ni_TrayIcon.Text = "Iris 2.0 Connected, Ready to use";
                        break;
                    }
                    
                }
                if (!bIrisX80 && !bIrisHD && !bIris2)
                {
                    bIrisHD = false;
                    bIris2 = false;
                    bIrisX80 = false;
                    ni_TrayIcon.Icon = Iris_Camera_Selector.Properties.Resources.SwitcherIcon2;
                    ni_TrayIcon.Text = "Digital Doc Iris Switch Tool";
                    try
                    {
                        foreach (Process proc in Process.GetProcessesByName("DEXvideo"))
                        {
                            proc.Kill();
                        }
                    }
                    catch { }
                }
                bThreadComplete = true;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Thread.Sleep(5000);
            }
        }

        private static List<USBDeviceInfo> GetUSBDevices()
        {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_PnPEntity"))
            {
                collection = searcher.Get();
            }

            foreach (var device in collection)
            {
                devices.Add(new USBDeviceInfo((string)device.GetPropertyValue("PNPDeviceID")));
            }

            collection.Dispose();
            return devices;
        }

        private static void EnableCamera(string camera)
        {
            byte[] setDefault = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            byte[] setUnused = new byte[] { 0x01, 0x00, 0x00, 0x00 };

            RegistryKey EaglesoftKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Eaglesoft\Chairside\Video\WDM");
            RegistryKey X80GameConKey = Registry.CurrentUser.CreateSubKey(@"System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\VID_20F1&PID_0404\Calibration\1");
            RegistryKey NetVirtGPKey = Registry.CurrentUser.CreateSubKey(@"System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\VID_BEEF&PID_FEED\Calibration\0");
            if (Environment.Is64BitOperatingSystem)
            {
                RegistryKey FilterKey = Registry.LocalMachine.CreateSubKey(@"Software\Classes\WOW6432Node\CLSID\{860BB310-5D01-11d0-BD3B-00A0C911CE86}\Instance");
            }
            else
            {
                RegistryKey FilterKey = Registry.LocalMachine.CreateSubKey(@"Software\Classes\CLSID\{860BB310-5D01-11d0-BD3B-00A0C911CE86}\Instance");
            }
            RegistryKey ESVideoFilter = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Eaglesoft\Chairside\Video");
            ESVideoFilter.SetValue("UseDriverDlg", 1, RegistryValueKind.DWord);

            byte[] iris_FilterData = new byte[] {0x02,0x00,0x00,0x00,0x00,0x00,0x60,0x00,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x30,0x70,0x69,0x33,
                    0x08,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x30,0x74,0x79,0x33,0x00,
                    0x00,0x00,0x00,0x38,0x00,0x00,0x00,0x48,0x00,0x00,0x00,0x76,0x69,0x64,0x73,0x00,0x00,0x10,0x00,0x80,0x00,0x00,0xaa,0x00,0x38,
                    0x9b,0x71,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00};

            byte[] hd_FilterData = new byte[] {0x02,0x00,0x00,0x00,0x00,0x00,0x60,0x00,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x30,0x70,0x69,0x33,
                    0x08,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x30,0x74,0x79,0x33,0x00,
                    0x00,0x00,0x00,0x38,0x00,0x00,0x00,0x48,0x00,0x00,0x00,0x76,0x69,0x64,0x73,0x00,0x00,0x10,0x00,0x80,0x00,0x00,0xaa,0x00,0x38,
                    0x9b,0x71,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00};
            if (camera == "X80")
            {
                //Default Game Con Settings
                X80GameConKey.SetValue("Joystick Id", setDefault, RegistryValueKind.Binary);
                NetVirtGPKey.SetValue("Joystick Id", setUnused, RegistryValueKind.Binary);

                byte[] x80_VidFormat = new byte[] {0x76,0x69,0x64,0x73,0x00,0x00,0x10,0x00,0x80,0x00,0x00,0xaa,0x00,0x38,0x9b,0x71,0x4d,0x4a,0x50,0x47,0x00,0x00,0x10,0x00,0x80,0x00,0x00,0xaa,
                    0x00,0x38,0x9b,0x71,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x31,0x47,0x00,0x80,0x9f,0x58,0x05,0x56,0xc3,0xce,0x11,0xbf,0x01,0x00,0xaa,0x00,0x55,0x59,0x5a,0x00,0x00,0x00,0x00,0x58,0x00,
                    0x00,0x00,0x08,0x73,0xdd,0x0e};

                byte[] x80_Vidinfo = new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xf0,0xbd,
                    0x42,0x00,0x00,0x00,0x00,0x15,0x16,0x05,0x00,0x00,0x00,0x00,0x00,0x28,0x00,0x00,0x00,0xa0,0x05,0x00,0x00,0x38,
                    0x04,0x00,0x00,0x01,0x00,0x18,0x00,0x4d,0x4a,0x50,0x47,0x00,0x31,0x47,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                    0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00};

                try
                {
                    EaglesoftKey.SetValue("VideoDeviceDesc", "IRIS X80 ", RegistryValueKind.String);
                    EaglesoftKey.DeleteValue("Brightness", false);
                    EaglesoftKey.DeleteValue("Contrast", false);
                    EaglesoftKey.DeleteValue("Saturation", false);
                    EaglesoftKey.DeleteValue("Hue", false);
                    EaglesoftKey.DeleteValue("Sharpness", false);
                    EaglesoftKey.DeleteValue("VideoFormat", false);
                    EaglesoftKey.DeleteValue("VideoInfo", false);
                    EaglesoftKey.SetValue("USBCam Button", 0, RegistryValueKind.DWord);
                    EaglesoftKey.SetValue("VideoFormat", x80_VidFormat, RegistryValueKind.Binary);
                    EaglesoftKey.SetValue("VideoInfo", x80_Vidinfo, RegistryValueKind.Binary);
                }
                catch { Console.WriteLine("Eaglesoft Not Installed"); }
                try { DisableOtherCameras(camera); } catch { Console.WriteLine("Couldnt unregister other cameras"); }
            }
            if (camera == "HD")
            {
                X80GameConKey.SetValue("Joystick Id", setUnused, RegistryValueKind.Binary);
                NetVirtGPKey.SetValue("Joystick Id", setDefault, RegistryValueKind.Binary);

                //Default ES Camera
                try
                {
                    EaglesoftKey.SetValue("VideoDeviceDesc", "Iris HD", RegistryValueKind.String);
                    EaglesoftKey.SetValue("UseVMR", 0, RegistryValueKind.DWord);
                    EaglesoftKey.SetValue("NoPreview", 0, RegistryValueKind.DWord);
                    EaglesoftKey.SetValue("Brightness", 55, RegistryValueKind.DWord);
                    EaglesoftKey.SetValue("OutputEnable", 1, RegistryValueKind.DWord);
                    EaglesoftKey.SetValue("Contrast", 0, RegistryValueKind.DWord);
                    EaglesoftKey.SetValue("Hue", 0, RegistryValueKind.DWord);
                    EaglesoftKey.SetValue("Saturation", 7, RegistryValueKind.DWord);
                    EaglesoftKey.SetValue("Sharpness", 44, RegistryValueKind.DWord);
                    EaglesoftKey.DeleteValue("VideoFormat", false);
                    EaglesoftKey.DeleteValue("VideoInfo", false);
                }
                catch { }
                try
                {
                    Process p = new Process();
                    p.StartInfo.FileName = "regsvr32.exe";
                    p.StartInfo.Arguments = "-s \"" + Environment.GetFolderPath(Environment.SpecialFolder.SystemX86) + "\\HD_Dental.ax\"";
                    p.StartInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);
                    p.Start();

                    DisableOtherCameras(camera);
                }
                catch { }
            }
            if (camera == "2.0")
            {
                NetVirtGPKey.SetValue("Joystick Id", setDefault, RegistryValueKind.Binary);
                X80GameConKey.SetValue("Joystick Id", setUnused, RegistryValueKind.Binary);

                try
                {
                    EaglesoftKey.SetValue("VideoDeviceDesc", "Iris", RegistryValueKind.String);
                    EaglesoftKey.SetValue("UseVMR", 0, RegistryValueKind.DWord);
                    EaglesoftKey.SetValue("NoPreview", 0, RegistryValueKind.DWord);
                    EaglesoftKey.SetValue("Brightness", 128, RegistryValueKind.DWord);
                    EaglesoftKey.SetValue("OutputEnable", 1, RegistryValueKind.DWord);
                    EaglesoftKey.SetValue("Contast", 128, RegistryValueKind.DWord);
                    EaglesoftKey.SetValue("Hue", 0, RegistryValueKind.DWord);
                    EaglesoftKey.SetValue("Saturation", 255, RegistryValueKind.DWord);
                    EaglesoftKey.SetValue("Sharpness", 0, RegistryValueKind.DWord);
                    EaglesoftKey.DeleteValue("VideoFormat", false);
                    EaglesoftKey.DeleteValue("VideoInfo", false);
                }
                catch { Console.WriteLine("Eaglesoft Not Installed"); }
                try
                {
                    Process p = new Process();
                    p.StartInfo.FileName = "regsvr32.exe";
                    p.StartInfo.Arguments = "-s \"" + Environment.GetFolderPath(Environment.SpecialFolder.SystemX86) + "\\netvecam.ax\"";
                    p.StartInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);
                    p.Start();
                    DisableOtherCameras(camera);
                }
                catch { }
            }
        }

        private static void DisableOtherCameras(string cameraName)
        {
            RegistryKey iris2key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\WOW6432Node\CLSID\{860BB310-5D01-11D0-BD3B-00A0C911CE86}\Instance", true);
            RegistryKey irisHDkey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\WOW6432Node\CLSID\{860BB310-5D01-11D0-BD3B-00A0C911CE86}\Instance", true);

            Process p_UnregHD = new Process();
            p_UnregHD.StartInfo.FileName = "regsvr32.exe";
            p_UnregHD.StartInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);
            p_UnregHD.StartInfo.Arguments = "-s -u \"" + Environment.GetFolderPath(Environment.SpecialFolder.SystemX86) + "\\HD_Dental.ax\"";
            p_UnregHD.Start();
            Process p_UnregIris2 = new Process();
            p_UnregIris2.StartInfo.FileName = "regsvr32.exe";
            p_UnregIris2.StartInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);
            p_UnregIris2.StartInfo.Arguments = "-s -u \"" + Environment.GetFolderPath(Environment.SpecialFolder.SystemX86) + "\\netvecam.ax\"";

            if (cameraName == "2.0")
            {
                //disable only HD other filter will vanish on disconnect
                try
                { irisHDkey.DeleteSubKey("{806FE53D-C34B-40D6-A767-308F2B260D2A}"); }
                catch
                { }
            }
            if (cameraName == "HD")
            {
                //disable only 2.0 other filter will vanish on disconnect
                try
                { iris2key.DeleteSubKeyTree("{4EB9D747-65C6-4850-8972-01C6C039FFBC}"); }
                catch
                { }
            }
            if (cameraName == "X80")
            {
                //disable HD & 2.0
                try
                { irisHDkey.DeleteSubKeyTree("{806FE53D-C34B-40D6-A767-308F2B260D2A}"); }
                catch
                { }
                try
                { iris2key.DeleteSubKeyTree("{4EB9D747-65C6-4850-8972-01C6C039FFBC}"); }
                catch
                { }
            }
        }

        private class USBDeviceInfo
        {
            public USBDeviceInfo(string pnpDeviceID)
            {
                this.PnpDeviceID = pnpDeviceID;
            }

            public string PnpDeviceID { get; private set; }
        }
    }
}