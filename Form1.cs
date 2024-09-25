using Microsoft.Win32.SafeHandles;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows.Forms;


namespace CPLink
{ 
    public partial class Form1 : Form
    {
        private const string SETUPAPI = "setupapi.dll";
        private const int ERROR_INVALID_DATA = 13;
        private const int ERROR_INSUFFICIENT_BUFFER = 122;

        private class SafeDeviceInformationSetHandle : SafeHandleMinusOneIsInvalid
        {
            private SafeDeviceInformationSetHandle() : base(true)
            { }

            private SafeDeviceInformationSetHandle(IntPtr preexistingHandle, bool ownsHandle) : base(ownsHandle)
            {
                SetHandle(preexistingHandle);
            }

            [SecurityCritical]
            protected override bool ReleaseHandle()
            {
                return SetupDiDestroyDeviceInfoList(handle);
            }
        }

        #region Enumerations

        [Flags]
        private enum DIGCF : uint
        {
            DEFAULT = 0x00000001,
            PRESENT = 0x00000002,
            ALLCLASSES = 0x00000004,
            PROFILE = 0x00000008,
            DEVICEINTERFACE = 0x00000010
        }

        private enum SPDRP : uint
        {
            /// <summary>
            /// DeviceDesc (R/W)
            /// </summary>
            DEVICEDESC = 0x00000000,

            /// <summary>
            /// HardwareID (R/W)
            /// </summary>
            HARDWAREID = 0x00000001,

            /// <summary>
            /// CompatibleIDs (R/W)
            /// </summary>
            COMPATIBLEIDS = 0x00000002,

            /// <summary>
            /// unused
            /// </summary>
            UNUSED0 = 0x00000003,

            /// <summary>
            /// Service (R/W)
            /// </summary>
            SERVICE = 0x00000004,

            /// <summary>
            /// unused
            /// </summary>
            UNUSED1 = 0x00000005,

            /// <summary>
            /// unused
            /// </summary>
            UNUSED2 = 0x00000006,

            /// <summary>
            /// Class (R--tied to ClassGUID)
            /// </summary>
            CLASS = 0x00000007,

            /// <summary>
            /// ClassGUID (R/W)
            /// </summary>
            CLASSGUID = 0x00000008,

            /// <summary>
            /// Driver (R/W)
            /// </summary>
            DRIVER = 0x00000009,

            /// <summary>
            /// ConfigFlags (R/W)
            /// </summary>
            CONFIGFLAGS = 0x0000000A,

            /// <summary>
            /// Mfg (R/W)
            /// </summary>
            MFG = 0x0000000B,

            /// <summary>
            /// FriendlyName (R/W)
            /// </summary>
            FRIENDLYNAME = 0x0000000C,

            /// <summary>
            /// LocationInformation (R/W)
            /// </summary>
            LOCATION_INFORMATION = 0x0000000D,

            /// <summary>
            /// PhysicalDeviceObjectName (R)
            /// </summary>
            PHYSICAL_DEVICE_OBJECT_NAME = 0x0000000E,

            /// <summary>
            /// Capabilities (R)
            /// </summary>
            CAPABILITIES = 0x0000000F,

            /// <summary>
            /// UiNumber (R)
            /// </summary>
            UI_NUMBER = 0x00000010,

            /// <summary>
            /// UpperFilters (R/W)
            /// </summary>
            UPPERFILTERS = 0x00000011,

            /// <summary>
            /// LowerFilters (R/W)
            /// </summary>
            LOWERFILTERS = 0x00000012,

            /// <summary>
            /// BusTypeGUID (R)
            /// </summary>
            BUSTYPEGUID = 0x00000013,

            /// <summary>
            /// LegacyBusType (R)
            /// </summary>
            LEGACYBUSTYPE = 0x00000014,

            /// <summary>
            /// BusNumber (R)
            /// </summary>
            BUSNUMBER = 0x00000015,

            /// <summary>
            /// Enumerator Name (R)
            /// </summary>
            ENUMERATOR_NAME = 0x00000016,

            /// <summary>
            /// Security (R/W, binary form)
            /// </summary>
            SECURITY = 0x00000017,

            /// <summary>
            /// Security (W, SDS form)
            /// </summary>
            SECURITY_SDS = 0x00000018,

            /// <summary>
            /// Device Type (R/W)
            /// </summary>
            DEVTYPE = 0x00000019,

            /// <summary>
            /// Device is exclusive-access (R/W)
            /// </summary>
            EXCLUSIVE = 0x0000001A,

            /// <summary>
            /// Device Characteristics (R/W)
            /// </summary>
            CHARACTERISTICS = 0x0000001B,

            /// <summary>
            /// Device Address (R)
            /// </summary>
            ADDRESS = 0x0000001C,

            /// <summary>
            /// UiNumberDescFormat (R/W)
            /// </summary>
            UI_NUMBER_DESC_FORMAT = 0X0000001D,

            /// <summary>
            /// Device Power Data (R)
            /// </summary>
            DEVICE_POWER_DATA = 0x0000001E,

            /// <summary>
            /// Removal Policy (R)
            /// </summary>
            REMOVAL_POLICY = 0x0000001F,

            /// <summary>
            /// Hardware Removal Policy (R)
            /// </summary>
            REMOVAL_POLICY_HW_DEFAULT = 0x00000020,

            /// <summary>
            /// Removal Policy Override (RW)
            /// </summary>
            REMOVAL_POLICY_OVERRIDE = 0x00000021,

            /// <summary>
            /// Device Install State (R)
            /// </summary>
            INSTALL_STATE = 0x00000022,

            /// <summary>
            /// Device Location Paths (R)
            /// </summary>
            LOCATION_PATHS = 0x00000023,
        }

        private enum DIF : uint
        {
            SELECTDEVICE = 0x00000001,
            INSTALLDEVICE = 0x00000002,
            ASSIGNRESOURCES = 0x00000003,
            PROPERTIES = 0x00000004,
            REMOVE = 0x00000005,
            FIRSTTIMESETUP = 0x00000006,
            FOUNDDEVICE = 0x00000007,
            SELECTCLASSDRIVERS = 0x00000008,
            VALIDATECLASSDRIVERS = 0x00000009,
            INSTALLCLASSDRIVERS = 0x0000000A,
            CALCDISKSPACE = 0x0000000B,
            DESTROYPRIVATEDATA = 0x0000000C,
            VALIDATEDRIVER = 0x0000000D,
            DETECT = 0x0000000F,
            INSTALLWIZARD = 0x00000010,
            DESTROYWIZARDDATA = 0x00000011,
            PROPERTYCHANGE = 0x00000012,
            ENABLECLASS = 0x00000013,
            DETECTVERIFY = 0x00000014,
            INSTALLDEVICEFILES = 0x00000015,
            UNREMOVE = 0x00000016,
            SELECTBESTCOMPATDRV = 0x00000017,
            ALLOW_INSTALL = 0x00000018,
            REGISTERDEVICE = 0x00000019,
            NEWDEVICEWIZARD_PRESELECT = 0x0000001A,
            NEWDEVICEWIZARD_SELECT = 0x0000001B,
            NEWDEVICEWIZARD_PREANALYZE = 0x0000001C,
            NEWDEVICEWIZARD_POSTANALYZE = 0x0000001D,
            NEWDEVICEWIZARD_FINISHINSTALL = 0x0000001E,
            UNUSED1 = 0x0000001F,
            INSTALLINTERFACES = 0x00000020,
            DETECTCANCEL = 0x00000021,
            REGISTER_COINSTALLERS = 0x00000022,
            ADDPROPERTYPAGE_ADVANCED = 0x00000023,
            ADDPROPERTYPAGE_BASIC = 0x00000024,
            RESERVED1 = 0x00000025,
            TROUBLESHOOTER = 0x00000026,
            POWERMESSAGEWAKE = 0x00000027,
            ADDREMOTEPROPERTYPAGE_ADVANCED = 0x00000028,
            UPDATEDRIVER_UI = 0x00000029,
            FINISHINSTALL_ACTION = 0x0000002A,
            RESERVED2 = 0x00000030,
        }

        private enum DICS : uint
        {
            ENABLE = 0x00000001,
            DISABLE = 0x00000002,
            PROPCHANGE = 0x00000003,
            START = 0x00000004,
            STOP = 0x00000005,
        }

        [Flags]
        private enum DICS_FLAG : uint
        {
            GLOBAL = 0x00000001,
            CONFIGSPECIFIC = 0x00000002,
            CONFIGGENERAL = 0x00000004,
        }

        #endregion

        #region Structures

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVINFO_DATA
        {
            public UInt32 cbSize;
            public Guid ClassGuid;
            public UInt32 DevInst;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_CLASSINSTALL_HEADER
        {
            public UInt32 cbSize;
            public DIF InstallFunction;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_PROPCHANGE_PARAMS
        {
            public SP_CLASSINSTALL_HEADER ClassInstallHeader;
            public DICS StateChange;
            public DICS_FLAG Scope;
            public UInt32 HwProfile;
        }

        #endregion

        #region P/Invoke Functions

        [DllImport(SETUPAPI, SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern SafeDeviceInformationSetHandle SetupDiGetClassDevs(
            [In] ref Guid ClassGuid,
            [In] string Enumerator,
            IntPtr hwndParent,
            DIGCF Flags
        );

        [DllImport(SETUPAPI, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport(SETUPAPI, SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInfo(
            SafeDeviceInformationSetHandle DeviceInfoSet,
            UInt32 MemberIndex,
            ref SP_DEVINFO_DATA DeviceInfoData
        );

        [DllImport(SETUPAPI, SetLastError = true)]
        private static extern bool SetupDiSetClassInstallParams(
            SafeDeviceInformationSetHandle DeviceInfoSet,
            [In] ref SP_DEVINFO_DATA deviceInfoData,
            [In] ref SP_PROPCHANGE_PARAMS classInstallParams,
            UInt32 ClassInstallParamsSize
        );

        [DllImport(SETUPAPI, SetLastError = true)]
        private static extern bool SetupDiChangeState(
            SafeDeviceInformationSetHandle DeviceInfoSet,
            [In, Out] ref SP_DEVINFO_DATA DeviceInfoData
        );

        [DllImport(SETUPAPI, SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetupDiGetDeviceRegistryProperty(
            SafeDeviceInformationSetHandle DeviceInfoSet,
            [In] ref SP_DEVINFO_DATA DeviceInfoData,
            SPDRP Property,
            out RegistryValueKind PropertyRegDataType,
            [Out] byte[] PropertyBuffer,
            UInt32 PropertyBufferSize,
            out UInt32 RequiredSize
        );

        #endregion

        private static void CheckWin32CallSuccess(bool success)
        {
            if (!success)
            {
                throw new Win32Exception();
            }
        }

        private static string GetStringPropertyForDevice(SafeDeviceInformationSetHandle infoSet, ref SP_DEVINFO_DATA devInfo, SPDRP property)
        {
            RegistryValueKind regType;
            UInt32 requiredSize;

            if (!SetupDiGetDeviceRegistryProperty(infoSet, ref devInfo, property, out regType, null, 0, out requiredSize))
            {
                switch (Marshal.GetLastWin32Error())
                {
                    case ERROR_INSUFFICIENT_BUFFER:
                        break;
                    case ERROR_INVALID_DATA:
                        return string.Empty;
                    default:
                        throw new Win32Exception();
                }
            }

            byte[] propertyBuffer = new byte[requiredSize];
            CheckWin32CallSuccess(SetupDiGetDeviceRegistryProperty(infoSet, ref devInfo, property, out regType, propertyBuffer, (uint)propertyBuffer.Length, out requiredSize));

            return Encoding.Unicode.GetString(propertyBuffer);
        }

        public static void EnableDevice(Func<string, bool> hardwareIdFilter, bool enable)
        {
            Guid nullGuid = Guid.Empty;
            using (SafeDeviceInformationSetHandle infoSet = SetupDiGetClassDevs(ref nullGuid, null, IntPtr.Zero, DIGCF.ALLCLASSES))
            {
                CheckWin32CallSuccess(!infoSet.IsInvalid);

                SP_DEVINFO_DATA devInfo = new SP_DEVINFO_DATA();
                devInfo.cbSize = (UInt32)Marshal.SizeOf(devInfo);

                for (uint index = 0; ; ++index)
                {
                    CheckWin32CallSuccess(SetupDiEnumDeviceInfo(infoSet, index, ref devInfo));

                    string hardwareId = GetStringPropertyForDevice(infoSet, ref devInfo, SPDRP.HARDWAREID);

                    if ((!string.IsNullOrEmpty(hardwareId)) && (hardwareIdFilter(hardwareId)))
                    {
                        break;
                    }
                }

                SP_CLASSINSTALL_HEADER classinstallHeader = new SP_CLASSINSTALL_HEADER();
                classinstallHeader.cbSize = (UInt32)Marshal.SizeOf(classinstallHeader);
                classinstallHeader.InstallFunction = DIF.PROPERTYCHANGE;

                SP_PROPCHANGE_PARAMS propchangeParams = new SP_PROPCHANGE_PARAMS
                {
                    ClassInstallHeader = classinstallHeader,
                    StateChange = enable ? DICS.ENABLE : DICS.DISABLE,
                    Scope = DICS_FLAG.GLOBAL,
                    HwProfile = 0,
                };

                CheckWin32CallSuccess(SetupDiSetClassInstallParams(infoSet, ref devInfo, ref propchangeParams, (UInt32)Marshal.SizeOf(propchangeParams)));

                CheckWin32CallSuccess(SetupDiChangeState(infoSet, ref devInfo));
            }
        }
        //
        public const int WM_DEVICECHANGE = 0x219;
        public const int DBT_DEVICEARRIVAL = 0x8000;
        public const int DBT_CONFIGCHANGECANCELED = 0x0019;
        public const int DBT_CONFIGCHANGED = 0x0018;
        public const int DBT_CUSTOMEVENT = 0x8006;
        public const int DBT_DEVICEQUERYREMOVE = 0x8001;
        public const int DBT_DEVICEQUERYREMOVEFAILED = 0x8002;
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        public const int DBT_DEVICEREMOVEPENDING = 0x8003;
        public const int DBT_DEVICETYPESPECIFIC = 0x8005;
        public const int DBT_DEVNODES_CHANGED = 0x0007;
        public const int DBT_QUERYCHANGECONFIG = 0x0017;
        public const int DBT_USERDEFINED = 0xFFFF;

        private string Xoutput = Application.StartupPath + "\\"+"XOutput\\XOutput.exe";
        private string dir = Application.StartupPath;
        private static readonly string DeviceID = "VID_11FF&PID_A301";
        private bool disablePedals = false;


        public Form1()
        {
            InitializeComponent();
        }

        protected override void WndProc(ref Message m)
        {

            try
            {
                if (m.Msg == WM_DEVICECHANGE)
                {
                    switch (m.WParam.ToInt32())
                    {
                        case WM_DEVICECHANGE:
                            break;
                        case DBT_DEVICEARRIVAL:
                            logBox.AppendText("已插入USB设备\r\n");
                            logBox.ScrollToCaret();
                            DriveInfo[] s = DriveInfo.GetDrives();
                            foreach (DriveInfo drive in s)
                            {
                                if (drive.DriveType == DriveType.Removable)
                                {
                                    break;
                                }
                            }
                            break;
                        case DBT_CONFIGCHANGECANCELED:
                            break;
                        case DBT_CONFIGCHANGED:
                            break;
                        case DBT_CUSTOMEVENT:
                            break;
                        case DBT_DEVICEQUERYREMOVE:
                            break;
                        case DBT_DEVICEQUERYREMOVEFAILED:
                            break;
                        case DBT_DEVICEREMOVECOMPLETE:
                            logBox.AppendText("检测到有USB设备断开连接\r\n");
                            logBox.ScrollToCaret();
                            break;
                        case DBT_DEVICEREMOVEPENDING:                          
                            break;
                        case DBT_DEVICETYPESPECIFIC:
                            break;
                        case DBT_DEVNODES_CHANGED:
                            break;
                        case DBT_QUERYCHANGECONFIG:
                            break;
                        case DBT_USERDEFINED:
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            base.WndProc(ref m);
        }

        public void OpenDirFun()
        {
            try
            {
                System.Diagnostics.Process.Start(dir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }

        private void openDir_Click(object sender, EventArgs e)
        {
            OpenDirFun();           
        }    

        private void pedalsConfig_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Xoutput);
            }
            catch (Exception ex)
            {
                //未找到XOutput.exe
                MessageBox.Show(ex.Message);
                throw;
            }
        }

        private void pedalsSwitch_CheckedChanged(object sender, EventArgs e)
        {  
            if (disablePedals == false)
            {
                EnableDevice(n => n.ToUpperInvariant().Contains(DeviceID),false);
                disablePedals = true;
                pedalsSwitch.Text = "启" + "用踏板";
                logBox.AppendText("踏板已禁用。\r\n");
            }
            else {
                EnableDevice(n => n.ToUpperInvariant().Contains(DeviceID), true);          
                disablePedals = false;
                pedalsSwitch.Text = "禁" + "用踏板";
                logBox.AppendText("踏板已启用。\r\n");
            }
            logBox.ScrollToCaret();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            MessageBox.Show("已适配游戏：飙酷车神2、轰鸣盛典\n已通过测试游戏：欧卡2、美卡", "已适配列表");
        }

        private void deviceManagerLink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("rundll32.exe", @"shell32.dll,Control_RunDLL joy.cpl,,0");

        }

        private void about_Click(object sender, EventArgs e)
        {
            MessageBox.Show("作者：https://github.com/sherryme\r\n版本：0.2.0", "关于");
        }
    }
}
