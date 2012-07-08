using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using GACManagerApi.Fusion;
using System.Reflection;

namespace GACManagerApi
{
    public class AssemblyDetails
    {
        public AssemblyDetails()
        {
                InstallReferences = new List<FUSION_INSTALL_REFERENCE>();
        }
        public void Initialise(IAssemblyName assemblyName)
        {
            //  Get the full name.
            var stringBuilder = new StringBuilder(BufferLength);
            int iLen = BufferLength;
            int hr = assemblyName.GetDisplayName(stringBuilder, ref iLen, ASM_DISPLAY_FLAGS.ALL);
            if (hr < 0)
                Marshal.ThrowExceptionForHR(hr);
            FullName = stringBuilder.ToString();

            //  Get the qualified name.
            stringBuilder = new StringBuilder(BufferLength);
            iLen = BufferLength;
            hr = assemblyName.GetDisplayName(stringBuilder, ref iLen, ASM_DISPLAY_FLAGS.ASM_DISPLAYF_VERSION 
                | ASM_DISPLAY_FLAGS.ASM_DISPLAYF_CULTURE
                | ASM_DISPLAY_FLAGS.ASM_DISPLAYF_PUBLIC_KEY_TOKEN
                | ASM_DISPLAY_FLAGS.ASM_DISPLAYF_PROCESSORARCHITECTURE);
            if (hr < 0)
                Marshal.ThrowExceptionForHR(hr);
            QualifiedAssemblyName = stringBuilder.ToString();


            //  Load properties from the display name.
            LoadPropertiesFromDisplayName(FullName);

            //  Load the path.
            Path = AssemblyCache.QueryAssemblyInfo(FullName);

            //  Load details from via reflection.
         //   LoadAdditionalDetailsViaReflection();

        }

        public void LoadExtendedProperties()
        {
            if(extendedPropertiesLoaded)
                return;
            
            LoadAdditionalDetailsViaReflection();
            LoadInstallReferences();

            extendedPropertiesLoaded = true;
        }

        private void LoadInstallReferences()
        {
            //  Create an install reference enumerator.
            InstallReferenceEnumerator enumerator = new InstallReferenceEnumerator(FullName);
            var reference = enumerator.GetNextReference();
            while(reference != null)
            {
                InstallReferences.Add(reference);
                reference = enumerator.GetNextReference();
            }
        }

        private void LoadPropertiesFromDisplayName(string displayName)
        {
            var properties = displayName.Split(new string[] {", "}, StringSplitOptions.None);

            //  Name should be first.
            try
            {
                Name = properties[0];
            }
            catch (Exception)
            {
                Name = "Unknown";
            }

            var versionString = (from p in properties where p.StartsWith("Version=") select p).FirstOrDefault();
            var cultureString = (from p in properties where p.StartsWith("Culture=") select p).FirstOrDefault();
            var publicKeyTokenString = (from p in properties where p.StartsWith("PublicKeyToken=") select p).FirstOrDefault();
            var processorArchitectureString = (from p in properties where p.StartsWith("processorArchitecture=") select p).FirstOrDefault();
            var customString = (from p in properties where p.StartsWith("Custom=") select p).FirstOrDefault();

            //  Then we should have version.
            if (!string.IsNullOrEmpty(versionString))
            {
                try
                {
                    versionString = versionString.Substring(versionString.IndexOf('=') + 1);
                    var versionParts = versionString.Split('.');
                    MajorVersion = Convert.ToUInt16(versionParts[0]);
                    MinorVersion = Convert.ToUInt16(versionParts[1]);
                    BuildNumber = Convert.ToUInt16(versionParts[2]);
                    RevisionNumber = Convert.ToUInt16(versionParts[3]);
                }
                catch (Exception)
                {
                    MajorVersion = 0;
                    MinorVersion = 0;
                    BuildNumber = 0;
                    RevisionNumber = 0;
                }
            }

            //  Then culture.
            if (!string.IsNullOrEmpty(cultureString))
            {
                try
                {
                    cultureString = cultureString.Substring(cultureString.IndexOf('=') + 1);
                    Culture = cultureString;
                }
                catch (Exception)
                {
                }
            }

            //  Then public key token.
            if (!string.IsNullOrEmpty(publicKeyTokenString))
            {
                try
                {
                    publicKeyTokenString = publicKeyTokenString.Substring(publicKeyTokenString.IndexOf('=') + 1);
                    PublicKeyToken = HexToData(publicKeyTokenString);
                }
                catch (Exception)
                {
                    PublicKeyToken = null;
                }
            }

            //  Then processor architecture.
            if (!string.IsNullOrEmpty(processorArchitectureString))
            {
                try
                {
                    processorArchitectureString =
                        processorArchitectureString.Substring(processorArchitectureString.IndexOf('=') + 1);
                    ProcessorArchitecture = processorArchitectureString;
                }
                catch (Exception)
                {
                }
            }

            if (!string.IsNullOrEmpty(customString))
            {
                //  Then custom.
                try
                {
                    customString = customString.Substring(customString.IndexOf('=') + 1);
                    Custom = customString;
                }
                catch (Exception)
                {
                }
            }
        }
        private static byte[] HexToData(string hexString)
        {
            if (hexString == null)
                return null;

            if (hexString.Length % 2 == 1)
                hexString = '0' + hexString; // Up to you whether to pad the first or last byte

            byte[] data = new byte[hexString.Length / 2];

            for (int i = 0; i < data.Length; i++)
                data[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);

            return data;
        }
        private void LoadPropertiesFromCOMInterface(IAssemblyName assemblyName)
        {
            MajorVersion = GetShortProperty(assemblyName, ASM_NAME.ASM_NAME_MAJOR_VERSION);
            MinorVersion = GetShortProperty(assemblyName, ASM_NAME.ASM_NAME_MINOR_VERSION);
            BuildNumber = GetShortProperty(assemblyName, ASM_NAME.ASM_NAME_BUILD_NUMBER);
            RevisionNumber = GetShortProperty(assemblyName, ASM_NAME.ASM_NAME_REVISION_NUMBER);
            Culture = GetStringProperty(assemblyName, ASM_NAME.ASM_NAME_CULTURE);
            PublicKeyToken = GetByteArrayProperty(assemblyName, ASM_NAME.ASM_NAME_PUBLIC_KEY_TOKEN);
            PublicKey = GetByteArrayProperty(assemblyName, ASM_NAME.ASM_NAME_PUBLIC_KEY);
        }

        private void LoadAdditionalDetailsViaReflection()
        {
            //  Load reflection details.
            try
            {
                if (!loadedAssemblies.ContainsKey(Path))
                    loadedAssemblies[Path] = Assembly.ReflectionOnlyLoad(QualifiedAssemblyName);
                RuntimeVersion = loadedAssemblies[Path].ImageRuntimeVersion;
            }
            catch
            {
            }   
        }

        private static Dictionary<string, Assembly> loadedAssemblies = new Dictionary<string, Assembly>();

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

        private bool extendedPropertiesLoaded;

        public string Name { get; private set; }
        public string FullName { get; private set; }
        public byte[] PublicKeyToken { get; private set; }
        public byte[] PublicKey { get; private set; }
        public ushort MajorVersion { get; private set; }
        public ushort MinorVersion { get; private set; }
        public ushort BuildNumber { get; private set; }
        public ushort RevisionNumber { get; private set; }
        public string Culture { get; private set; }
        public string Path { get; private set; }
        public string ProcessorArchitecture { get; private set; }
        public string Custom { get; private set; }
        public string RuntimeVersion { get; private set; }

        /// <summary>
        /// Gets the qualified name of the assembly. This is useful for Install/Uninstall.
        /// v1.0/v1.1 assemblies: "name, Version=xx, Culture=xx, PublicKeyToken=xx".
        /// v2.0 assemblies: "name, Version=xx, Culture=xx, PublicKeyToken=xx, ProcessorArchitecture=xx".
        /// </summary>
        /// <value>
        /// The name of the qualified assembly.
        /// </value>
        public string QualifiedAssemblyName { get; private set; }

        public List<FUSION_INSTALL_REFERENCE> InstallReferences { get; private set; } 
    }
}
