using System;
using GACManagerApi;

namespace GACManager.Models
{
    /// <summary>
    /// The <see cref="IGACManagerModel"/> Model Interface class.
    /// </summary>
    public interface IGACManagerModel
    {
        /// <summary>
        /// Enumerates assemblies from the global assembly cache.
        /// </summary>
        /// <param name="onAssemblyEnumerated">Action to call when each assembly is enumerated.</param>
        /// <returns>The time taken to enumerate all assemblies.</returns>
        TimeSpan EnumerateAssemblies(Action<AssemblyDescription> onAssemblyEnumerated);
    }
}
