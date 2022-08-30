namespace X39.Hosting.Modularization.Data;

/// <summary>
/// <see cref="EventArgs"/> passed when <see cref="ModuleLoader.ModuleUnloaded"/> is raised.
/// </summary>
[PublicAPI]
public class UnloadedEventArgs : EventArgs
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContext"/> unloaded
    /// </summary>
    public ModuleContext ModuleContext { get; }

    internal UnloadedEventArgs(ModuleContext moduleContext)
    {
        ModuleContext = moduleContext;
    }
}