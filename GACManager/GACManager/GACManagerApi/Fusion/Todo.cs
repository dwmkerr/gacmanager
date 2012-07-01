using System;
using System.Runtime.InteropServices;
using System.Text;

//  Thanks to Junfeng Zhang for sharing the original code that this was based on!
//  http://blogs.msdn.com/b/junfeng/archive/2004/09/14/229649.aspx

namespace GACManagerApi.Fusion
{
// IAssemblyEnum

    

// IInstallReferenceItem

    

// IInstallReferenceEnum

    public enum AssemblyCommitFlags
    {
        Default = 1,
        Force = 2
    }

// enum AssemblyCommitFlags

    public enum AssemblyCacheUninstallDisposition
    {
        Unknown = 0,
        Uninstalled = 1,
        StillInUse = 2,
        AlreadyUninstalled = 3,
        DeletePending = 4,
        HasInstallReference = 5,
        ReferenceNotFound = 6
    }


    internal enum CREATE_ASM_NAME_OBJ_FLAGS
    {
        CANOF_DEFAULT = 0,
        CANOF_PARSE_DISPLAY_NAME = 1,
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public class InstallReference
    {
        public InstallReference(Guid guid, String id, String data)
        {
            cbSize = (int) (2*IntPtr.Size + 16 + (id.Length + data.Length)*2);
            flags = 0;
            // quiet compiler warning 
            if (flags == 0)
            {
            }
            guidScheme = guid;
            identifier = id;
            description = data;
        }

        public Guid GuidScheme
        {
            get { return guidScheme; }
        }

        public String Identifier
        {
            get { return identifier; }
        }

        public String Description
        {
            get { return description; }
        }

        private int cbSize;
        private int flags;
        private Guid guidScheme;
        [MarshalAs(UnmanagedType.LPWStr)] private String identifier;
        [MarshalAs(UnmanagedType.LPWStr)] private String description;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AssemblyInfo
    {
        public int cbAssemblyInfo; // size of this structure for future expansion
        public int assemblyFlags;
        public long assemblySizeInKB;
        [MarshalAs(UnmanagedType.LPWStr)] public String currentAssemblyPath;
        public int cchBuf; // size of path buf.
    }

    [ComVisible(false)]
    public class InstallReferenceGuid
    {
        public static bool IsValidGuidScheme(Guid guid)
        {
            return (guid.Equals(UninstallSubkeyGuid) ||
                    guid.Equals(FilePathGuid) ||
                    guid.Equals(OpaqueGuid) ||
                    guid.Equals(Guid.Empty));
        }

        public static readonly Guid UninstallSubkeyGuid = new Guid("8cedc215-ac4b-488b-93c0-a50a49cb2fb8");
        public static readonly Guid FilePathGuid = new Guid("b02f9d65-fb77-4f7a-afa5-b391309f11c9");
        public static readonly Guid OpaqueGuid = new Guid("2ec93463-b0c3-45e1-8364-327e96aea856");
        // these GUID cannot be used for installing into GAC.
        public static readonly Guid MsiGuid = new Guid("25df0fc1-7f97-4070-add7-4b13bbfd7cb8");
        public static readonly Guid OsInstallGuid = new Guid("d16d444c-56d8-11d5-882d-0080c847b195");
    }

    [ComVisible(false)]
    public static class AssemblyCache
    {
        public static void InstallAssembly(String assemblyPath, InstallReference reference, AssemblyCommitFlags flags)
        {
            if (reference != null)
            {
                if (!InstallReferenceGuid.IsValidGuidScheme(reference.GuidScheme))
                    throw new ArgumentException("Invalid reference guid.", "guid");
            }

            IAssemblyCache ac = null;

            int hr = 0;

            hr = FusionImports.CreateAssemblyCache(out ac, 0);
            if (hr >= 0)
            {
                hr = ac.InstallAssembly((int) flags, assemblyPath, reference);
            }

            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
        }

        // assemblyName has to be fully specified name. 
        // A.k.a, for v1.0/v1.1 assemblies, it should be "name, Version=xx, Culture=xx, PublicKeyToken=xx".
        // For v2.0 assemblies, it should be "name, Version=xx, Culture=xx, PublicKeyToken=xx, ProcessorArchitecture=xx".
        // If assemblyName is not fully specified, a random matching assembly will be uninstalled. 
        public static void UninstallAssembly(String assemblyName, InstallReference reference,
                                             out AssemblyCacheUninstallDisposition disp)
        {
            AssemblyCacheUninstallDisposition dispResult = AssemblyCacheUninstallDisposition.Uninstalled;
            if (reference != null)
            {
                if (!InstallReferenceGuid.IsValidGuidScheme(reference.GuidScheme))
                    throw new ArgumentException("Invalid reference guid.", "guid");
            }

            IAssemblyCache ac = null;

            int hr = FusionImports.CreateAssemblyCache(out ac, 0);
            if (hr >= 0)
            {
                hr = ac.UninstallAssembly(0, assemblyName, reference, out dispResult);
            }

            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            disp = dispResult;
        }

        // See comments in UninstallAssembly
        public static String QueryAssemblyInfo(String assemblyName)
        {
            if (assemblyName == null)
            {
                throw new ArgumentException("Invalid name", "assemblyName");
            }

            AssemblyInfo aInfo = new AssemblyInfo();

            aInfo.cchBuf = 1024;
            // Get a string with the desired length
            aInfo.currentAssemblyPath = new String('\0', aInfo.cchBuf);

            IAssemblyCache ac = null;
            int hr = FusionImports.CreateAssemblyCache(out ac, 0);
            if (hr >= 0)
            {
                hr = ac.QueryAssemblyInfo(0, assemblyName, ref aInfo);
            }
            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            return aInfo.currentAssemblyPath;
        }
    }

    

// class AssemblyCacheEnum

    public class AssemblyCacheInstallReferenceEnum
    {
        public AssemblyCacheInstallReferenceEnum(String assemblyName)
        {
            IAssemblyName fusionName = null;

            int hr = FusionImports.CreateAssemblyNameObject(
                out fusionName,
                assemblyName,
                CreateAssemblyNameObjectFlags.CANOF_PARSE_DISPLAY_NAME,
                IntPtr.Zero);

            if (hr >= 0)
            {
                hr = FusionImports.CreateInstallReferenceEnum(out refEnum, fusionName, 0, IntPtr.Zero);
            }

            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
        }

        public InstallReference GetNextReference()
        {
            IInstallReferenceItem item = null;
            int hr = refEnum.GetNextInstallReferenceItem(out item, 0, IntPtr.Zero);
            if ((uint) hr == 0x80070103)
            {
                // ERROR_NO_MORE_ITEMS
                return null;
            }

            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            IntPtr refData;
            InstallReference instRef = new InstallReference(Guid.Empty, String.Empty, String.Empty);

            hr = item.GetReference(out refData, 0, IntPtr.Zero);
            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            Marshal.PtrToStructure(refData, instRef);
            return instRef;
        }

        private IInstallReferenceEnum refEnum;
    }
}