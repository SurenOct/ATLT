using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace CMATLTService
{
    internal class GacInstaller
    {
        [DllImport("fusion.dll")]
        private static extern int CreateAssemblyCache(
        out IAssemblyCache ppAsmCache,
        int reserved);

        public static void Install(string dllPath)
        {
            IAssemblyCache cache;
            int hr = CreateAssemblyCache(out cache, 0);
            if (hr != 0)
                Marshal.ThrowExceptionForHR(hr);

            hr = cache.InstallAssembly(0, dllPath, IntPtr.Zero);
            if (hr != 0)
                Marshal.ThrowExceptionForHR(hr);
        }

        public static void Uninstall(string assemblyFullName)
        {
            IAssemblyCache cache;
            int hr = CreateAssemblyCache(out cache, 0);
            if (hr != 0)
                Marshal.ThrowExceptionForHR(hr);

            int disposition;
            hr = cache.UninstallAssembly(0, assemblyFullName, IntPtr.Zero, out disposition);
            if (hr != 0)
                Marshal.ThrowExceptionForHR(hr);
        }
    }
    [ComImport]
    [Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAssemblyCache
    {
        int UninstallAssembly(
            int dwFlags,
            [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName,
            IntPtr pvReserved,
            out int pulDisposition);

        int QueryAssemblyInfo(
            int dwFlags,
            [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName,
            IntPtr pAsmInfo);

        int CreateAssemblyCacheItem(
            int dwFlags,
            IntPtr pvReserved,
            out IntPtr ppAsmItem,
            [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName);

        int InstallAssembly(
            int dwFlags,
            [MarshalAs(UnmanagedType.LPWStr)] string pszManifestFilePath,
            IntPtr pvReserved);
    }
}

 
 