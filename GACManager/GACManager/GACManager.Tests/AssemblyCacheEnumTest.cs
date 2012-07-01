using GACManagerApi;
using GACManagerApi.Fusion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GACManager.Tests
{
    
    
    /// <summary>
    ///This is a test class for AssemblyCacheEnumTest and is intended
    ///to contain all AssemblyCacheEnumTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AssemblyCacheEnumTest
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
        ///A test for AssemblyCacheEnum Constructor
        ///</summary>
        [TestMethod()]
        public void AssemblyCacheEnumConstructorTest()
        {
            var target = new AssemblyCacheEnumerator(null);
            var assembly = target.GetNextAssembly();
            while(assembly != null)
            {
                System.Diagnostics.Trace.WriteLine(assembly);
                assembly = target.GetNextAssembly();
            }

            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
