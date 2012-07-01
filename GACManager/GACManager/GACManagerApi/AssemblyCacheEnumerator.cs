using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using GACManagerApi.Fusion;

namespace GACManagerApi
{
    [ComVisible(false)]
    public class AssemblyCacheEnumerator
    {
        public AssemblyCacheEnumerator()
        {
            Initialise(null);
        }

        public AssemblyCacheEnumerator(string assemblyName)
        {
            Initialise(assemblyName);
        }

        private void Initialise(string assemblyName)
        {
            IAssemblyName fusionName = null;
            int hr = 0;
    
            //  If we have an assembly name, create the assembly name object.
            if (assemblyName != null)
            {
                hr = FusionImports.CreateAssemblyNameObject(out fusionName, assemblyName,
                    CREATE_ASM_NAME_OBJ_FLAGS.CANOF_PARSE_DISPLAY_NAME, IntPtr.Zero);
                if(hr < 0)
                    Marshal.ThrowExceptionForHR(hr);
            }

                
                hr = FusionImports.CreateAssemblyEnum(
                    out assemblyEnumerator,
                    IntPtr.Zero,
                    fusionName,
                    ASM_CACHE_FLAGS.ASM_CACHE_GAC, 
                    IntPtr.Zero);
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);

        }

        public AssemblyDetails GetNextAssembly()
        {
            int hr = 0;
            IAssemblyName fusionName = null;

            if (done)
            {
                return null;
            }

            // Now get next IAssemblyName from m_AssemblyEnum
            hr = assemblyEnumerator.GetNextAssembly((IntPtr)0, out fusionName, 0);

            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            if (fusionName != null)
            {
                var assemblyDetails = new AssemblyDetails();
                assemblyDetails.Load(fusionName);
                return assemblyDetails;
            }
            else
            {
                done = true;
                return null;
            }
        }

        private IAssemblyEnum assemblyEnumerator = null;
        private bool done;
    }
}
