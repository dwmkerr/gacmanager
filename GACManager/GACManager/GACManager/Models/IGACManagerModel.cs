using System;
using GACManagerApi;

namespace GACManager.Models
{
    /// <summary>
    /// The <see cref="IGACManagerModel"/> Model Interface class.
    /// </summary>
    public interface IGACManagerModel
    {
        //  Add functions to the model here.
        TimeSpan EnumerateAssemblies(Action<AssemblyDetails> onAssemblyEnumerated);
    }
}
