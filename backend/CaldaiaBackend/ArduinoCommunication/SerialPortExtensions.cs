using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace ArduinoCommunication
{
    internal static class COMConstants
    {
        internal const int FBINARY = 0;
        internal const int FPARITY = 1;
        internal const int FOUTXCTSFLOW = 2;
        internal const int FOUTXDSRFLOW = 3;
        internal const int FDTRCONTROL = 4;
        internal const int FDSRSENSITIVITY = 6;
        internal const int FTXCONTINUEONXOFF = 7;
        internal const int FOUTX = 8;
        internal const int FINX = 9;
        internal const int FERRORCHAR = 10;
        internal const int FNULL = 11;
        internal const int FRTSCONTROL = 12;
        internal const int FABORTONOERROR = 14;
        internal const int FDUMMY2 = 15;
    }

    internal static class SerialPortExtensions
    {
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void SetField(this SerialPort port, string field, object value)
        {
            if (port == null)
                throw new NullReferenceException();
            if (port.BaseStream == null)
                throw new InvalidOperationException("Cannot change fields until after the port has been opened.");

            try
            {
                object baseStream = port.BaseStream;
                Type baseStreamType = baseStream.GetType();

                FieldInfo dcbFieldInfo = baseStreamType.GetField("dcb", BindingFlags.NonPublic | BindingFlags.Instance);
                object dcbValue = dcbFieldInfo.GetValue(baseStream);

                Type dcbType = dcbValue.GetType();
                dcbType.GetField(field).SetValue(dcbValue, value);
                dcbFieldInfo.SetValue(baseStream, dcbValue);
            }
            catch (SecurityException) { throw; }
            catch (OutOfMemoryException) { throw; }
            catch (Win32Exception) { throw; }
            catch (Exception)
            {
                throw;
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void SetFlag(this SerialPort port, int flag, int value)
        {
            object BaseStream = port.BaseStream;
            Type SerialStream = BaseStream.GetType();
            SerialStream.GetMethod("SetDcbFlag", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(BaseStream, new object[] { flag, value });
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void UpdateComm(this SerialPort port)
        {
            object baseStream = port.BaseStream;
            Type baseStreamType = baseStream.GetType();

            FieldInfo dcbFieldInfo = baseStreamType.GetField("dcb", BindingFlags.NonPublic | BindingFlags.Instance);
            object dcbValue = dcbFieldInfo.GetValue(baseStream);

            SafeFileHandle portFileHandle = (SafeFileHandle)baseStreamType.GetField("_handle", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(baseStream);
            IntPtr hGlobal = Marshal.AllocHGlobal(Marshal.SizeOf(dcbValue));
            try
            {
                Marshal.StructureToPtr(dcbValue, hGlobal, false);

                if (!SetCommState(portFileHandle, hGlobal))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            finally
            {
                if (hGlobal != IntPtr.Zero)
                    Marshal.FreeHGlobal(hGlobal);
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetCommState(SafeFileHandle hFile, IntPtr lpDCB);
    }
}
