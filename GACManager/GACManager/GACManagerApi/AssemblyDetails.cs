using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using GACManagerApi.Fusion;

namespace GACManagerApi
{
    public class AssemblyDetails
    {
        internal void Load(IAssemblyName assemblyName)
        {
            var stringBuilder = new StringBuilder(BufferLength);
            int iLen = BufferLength;
            int hr = assemblyName.GetDisplayName(stringBuilder, ref iLen, ASM_DISPLAY_FLAGS.ALL);
            if (hr < 0)
                Marshal.ThrowExceptionForHR(hr);
            DisplayName = stringBuilder.ToString();

            LoadPropertiesFromDisplayName(DisplayName);

            //  Load the path.
            Path = AssemblyCache.QueryAssemblyInfo(DisplayName);

            /*
            StringBuilder sName = new StringBuilder(BufferLength);
            iLen = BufferLength;
            hr = assemblyName.GetName(ref iLen, sName);
            if (hr < 0)
                Marshal.ThrowExceptionForHR(hr);
            Name = sName.ToString();
*/
        }

        private void LoadPropertiesFromDisplayName(string displayName)
        {
            var properties = displayName.Split(',');

            //  Name should be first.
            try
            {
                Name = properties[0];
            }
            catch (Exception)
            {
                Name = "Unknown";
            }

            //  Then we should have version.
            try
            {
                var versionString = properties[1];
                versionString = versionString.Substring(versionString.IndexOf('=') + 1);
                var versionParts = versionString.Split('.');
                MajorVersion = Convert.ToInt16(versionParts[0]);
                MinorVersion = Convert.ToInt16(versionParts[1]);
                BuildNumber = Convert.ToInt16(versionParts[2]);
                RevisionNumber = Convert.ToInt16(versionParts[3]);
            }
            catch (Exception)
            {
                MajorVersion = 0;
                MinorVersion = 0;
                BuildNumber = 0;
                RevisionNumber = 0;
            }

            //  Then culture.
            try
            {
                var cultureString = properties[2];
                cultureString = cultureString.Substring(cultureString.IndexOf('=') + 1);
                Culture = cultureString;
            }
            catch (Exception)
            {
                Culture = "Unknown";
            }

            //  Then public key token.
            try
            {
                var publicKeyTokenString = properties[3];
                publicKeyTokenString = publicKeyTokenString.Substring(publicKeyTokenString.IndexOf('=') + 1);
                PublicKeyToken = null;//todo
            }
            catch (Exception)
            {
                PublicKeyToken = null;
            }

            //  Then processor architecture.
            try
            {
                var processorArchitectureString = properties[4];
                processorArchitectureString = processorArchitectureString.Substring(processorArchitectureString.IndexOf('=') + 1);
                ProcessorArchitecture = processorArchitectureString;
            }
            catch (Exception)
            {
                ProcessorArchitecture = "Unknown";
            }

            //  Then custom.
            try
            {
                var customString = properties[5];
                customString = customString.Substring(customString.IndexOf('=') + 1);
                Custom = customString;
            }
            catch (Exception)
            {
                Custom = "Unknown";
            }
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

        internal short GetShortProperty(IAssemblyName name, ASM_NAME propertyName)
        {
            uint bufferSize = 512;
            IntPtr buffer = Marshal.AllocHGlobal((int)bufferSize);
            name.GetProperty(propertyName, buffer, ref bufferSize);
            short result = Marshal.ReadInt16(buffer);
            Marshal.FreeHGlobal(buffer);
            return result;
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

        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public byte[] PublicKeyToken { get; private set; }
        public byte[] PublicKey { get; private set; }
        public short MajorVersion { get; private set; }
        public short MinorVersion { get; private set; }
        public short BuildNumber { get; private set; }
        public short RevisionNumber { get; private set; }
        public string Culture { get; private set; }
        public string Path { get; private set; }
        public string ProcessorArchitecture { get; private set; }
        public string Custom { get; private set; }
    }
}
