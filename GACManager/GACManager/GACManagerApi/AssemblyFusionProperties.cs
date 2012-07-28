using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using GACManagerApi.Fusion;

namespace GACManagerApi
{
    /// <summary>
    /// AssemblyFusionProperties represent the properties of an assembly that
    /// are loaded by the Fusion API.
    /// </summary>
    public class AssemblyFusionProperties
    {
        public AssemblyFusionProperties()
        {
            InstallReferences = new List<FUSION_INSTALL_REFERENCE>();
        }

        public void Load(IAssemblyName assemblyName)
        {
            //  Load the properties.
            MajorVersion = GetShortProperty(assemblyName, ASM_NAME.ASM_NAME_MAJOR_VERSION);
            MinorVersion = GetShortProperty(assemblyName, ASM_NAME.ASM_NAME_MINOR_VERSION);
            BuildNumber = GetShortProperty(assemblyName, ASM_NAME.ASM_NAME_BUILD_NUMBER);
            RevisionNumber = GetShortProperty(assemblyName, ASM_NAME.ASM_NAME_REVISION_NUMBER);
            PublicKey = GetByteArrayProperty(assemblyName, ASM_NAME.ASM_NAME_PUBLIC_KEY);

            //  Create an install reference enumerator.
            var enumerator = new InstallReferenceEnumerator(assemblyName);
            var reference = enumerator.GetNextReference();
            while (reference != null)
            {
                InstallReferences.Add(reference);
                reference = enumerator.GetNextReference();
            }
        }
        internal UInt16 GetShortProperty(IAssemblyName name, ASM_NAME propertyName)
        {
            uint bufferSize = 512;
            IntPtr buffer = Marshal.AllocHGlobal((int)bufferSize);
            name.GetProperty(propertyName, buffer, ref bufferSize);
            byte low = Marshal.ReadByte(buffer);
            byte high = Marshal.ReadByte(buffer, 1);
            Marshal.FreeHGlobal(buffer);
            return (UInt16)(low + (high << 8));
        }

        internal string GetStringProperty(IAssemblyName name, ASM_NAME propertyName)
        {
            uint bufferSize = BufferLength;
            IntPtr buffer = Marshal.AllocHGlobal((int)bufferSize);
            name.GetProperty(propertyName, buffer, ref bufferSize);
            var stringVaule = Marshal.PtrToStringUni(buffer, (int)bufferSize);
            Marshal.FreeHGlobal(buffer);
            return stringVaule;
        }

        internal byte[] GetByteArrayProperty(IAssemblyName name, ASM_NAME propertyName)
        {
            uint bufferSize = 512;
            IntPtr buffer = Marshal.AllocHGlobal((int)bufferSize);
            name.GetProperty(propertyName, buffer, ref bufferSize);
            byte[] result = new byte[bufferSize];
            for (int i = 0; i < bufferSize; i++)
                result[i] = Marshal.ReadByte(buffer, i);
            Marshal.FreeHGlobal(buffer);
            return result;
        }

        private const int BufferLength = 65535;

        /// <summary>
        /// Gets or sets the major version.
        /// </summary>
        /// <value>
        /// The major version.
        /// </value>
        public ushort MajorVersion { get; set; }

        /// <summary>
        /// Gets or sets the minor version.
        /// </summary>
        /// <value>
        /// The minor version.
        /// </value>
        public ushort MinorVersion { get; set; }

        /// <summary>
        /// Gets or sets the build number.
        /// </summary>
        /// <value>
        /// The build number.
        /// </value>
        public ushort BuildNumber { get; set; }

        /// <summary>
        /// Gets or sets the revision number.
        /// </summary>
        /// <value>
        /// The revision number.
        /// </value>
        public ushort RevisionNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        /// <value>
        /// The public key.
        /// </value>
        public byte[] PublicKey { get; set; }

        /// <summary>
        /// Gets the install references.
        /// </summary>
        public List<FUSION_INSTALL_REFERENCE> InstallReferences { get; private set; }
    }
}
