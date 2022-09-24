using System.Reflection;

namespace X39.Hosting.Modularization.Data;

/// <summary>
/// <see cref="EventArgs"/> passed when <see cref="ModuleLoader.ModuleAssemblyLoaded"/> is raised.
/// </summary>
[PublicAPI]
public class AssemblyLoadedEventArgs : EventArgs
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContextBase"/> that has the <see cref="System.Reflection.Assembly"/> loaded.
    /// </summary>
    public ModuleContextBase ModuleContext { get; }

    /// <summary>
    /// The <see cref="System.Reflection.Assembly"/> loaded.
    /// </summary>
    public Assembly Assembly { get; }

    internal AssemblyLoadedEventArgs(ModuleContextBase moduleContext, Assembly assembly)
    {
        ModuleContext = moduleContext;
        Assembly      = assembly;
    }
}