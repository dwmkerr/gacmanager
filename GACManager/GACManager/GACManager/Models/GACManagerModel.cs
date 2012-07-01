using System;
using System.Diagnostics;
using Apex.MVVM;
using GACManagerApi;
using GACManagerApi.Fusion;

namespace GACManager.Models
{
    /// <summary>
    /// The <see cref="GACManagerModel"/> Model class.
    /// This class implements the <see cref="IGACManagerModel"/> Model interface.
    /// You can retrieve the model with:
    /// <code>IGACManagerModel model = ApexBroker.GetModel<IGACManagerModel>()</code>
    /// </summary>
    [Model]
    public sealed class GACManagerModel : IGACManagerModel, IModel
    {
        /// <summary>
        /// Called by the Apex framework to initialise the model.
        /// </summary>
        public void OnInitialised()
        {
            //  TODO: Initialise the model.
        }

        public TimeSpan EnumerateAssemblies(Action<AssemblyDetails> onAssemblyEnumerated)
        {
            //  Start a stopwatch.
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            //  Create an assembly cache enumerator.
            var assemblyCacheEnum = new AssemblyCacheEnumerator(null);

            //  Enumerate the assemblies.
            var assemblyName = assemblyCacheEnum.GetNextAssembly();
            while(assemblyName != null)
            {
                //  Create an assembly view model.
                onAssemblyEnumerated(assemblyName);
                assemblyName = assemblyCacheEnum.GetNextAssembly();
            }

            //  Stop the stopwatch.
            stopwatch.Stop();

            //  Return the elapsed time.
            return stopwatch.Elapsed;
        }
    }
}
