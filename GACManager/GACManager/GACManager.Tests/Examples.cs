using System.Runtime.InteropServices;
using GACManagerApi;
using GACManagerApi.Fusion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GACManager.Tests
{
    [TestClass()]
    public class Examples
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        ///A test for CanFindExecutable
        ///</summary>
        [TestMethod()]
        public void EnumerateAssemblies()
        {
            //  Create an assembly enumerator.
            var assemblyEnumerator = new AssemblyCacheEnumerator();

            //  Get the first assembly.
            var assemblyName = assemblyEnumerator.GetNextAssembly();

            //  Start to loop through the assemblies.
            while(assemblyName != null)
            {
                //  The 'assemblyName' object is a COM interface, if we create an 
                //  AssemblyDescription from it, we will have access to more information.
                var assemblyDescription = new AssemblyDescription(assemblyName);

                //  Show the display name.
                System.Diagnostics.Trace.WriteLine("Display Name: " + assemblyDescription.DisplayName);

                //  Move to the next assembly.
                assemblyName = assemblyEnumerator.GetNextAssembly();
            }
        }

        [TestMethod()]
        public void InstallAssembly()
        {
            //  We'll need a path to the assembly to install.
            var path = @"c:/MyAssembly.dll";

            //  Install the assembly, without an install reference.
            AssemblyCache.InstallAssembly(path, null, AssemblyCommitFlags.Default);
        }

        [TestMethod()]
        public void UninstallAssembly()
        {
            //  We'll need a display name of the assembly to uninstall.
            var displayName = @"Apex, Version=1.4.0.0, Culture=neutral, PublicKeyToken=98d06957926c086d, processorArchitecture=MSIL";

            //  When we try to uninstall an assembly, an uninstall disposition will be
            //  set to indicate the success of the operation.
            var uninstallDisposition = IASSEMBLYCACHE_UNINSTALL_DISPOSITION.Unknown;

            //  Install the assembly, without an install reference.
            try
            {
                AssemblyCache.UninstallAssembly(displayName, null, out uninstallDisposition);
            }
            catch (Exception exception)
            {
                //  We've failed to uninstall the assembly.
                throw new InvalidOperationException("Failed to uninstall the assembly.", exception);
            }

            //  Did we succeed?
            if (uninstallDisposition == IASSEMBLYCACHE_UNINSTALL_DISPOSITION.IASSEMBLYCACHE_UNINSTALL_DISPOSITION_UNINSTALLED)
            {
                //  Hooray!   
            }
        }

        [TestMethod()]
        public void FusionProperties()
        {
            //  Get the first assembly in the global assembly cache.
            var someAssembly = new AssemblyDescription(new AssemblyCacheEnumerator().GetNextAssembly());

            //  Show the install references.
            foreach(var installReference in someAssembly.FusionProperties.InstallReferences)
                System.Diagnostics.Trace.WriteLine(installReference.Description);
        }

        [TestMethod()]
        public void ReflectionProperties()
        {
            //  Get the first assembly in the global assembly cache.
            var someAssembly = new AssemblyDescription(new AssemblyCacheEnumerator().GetNextAssembly());

            //  Show the Runtime Version.
            System.Diagnostics.Trace.WriteLine(someAssembly.ReflectionProperties.RuntimeVersion);
        }
    }
}
