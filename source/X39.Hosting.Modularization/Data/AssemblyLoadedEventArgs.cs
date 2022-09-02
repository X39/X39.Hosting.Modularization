using System.Reflection;

namespace X39.Hosting.Modularization.Data;

/// <summary>
/// <see cref="EventArgs"/> passed when <see cref="ModuleLoader.ModuleAssemblyLoaded"/> is raised.
/// </summary>
[PublicAPI]
public class AssemblyLoadedEventArgs : EventArgs
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContext"/> that has the <see cref="System.Reflection.Assembly"/> loaded.
    /// </summary>
    public ModuleContext ModuleContext { get; }

    /// <summary>
    /// The <see cref="System.Reflection.Assembly"/> loaded.
    /// </summary>
    public Assembly Assembly { get; }

    internal AssemblyLoadedEventArgs(ModuleContext moduleContext, Assembly assembly)
    {
        ModuleContext = moduleContext;
        Assembly      = assembly;
    }
}