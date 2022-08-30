namespace X39.Hosting.Modularization.Data;

/// <summary>
/// <see cref="EventArgs"/> passed when <see cref="ModuleLoader.ModuleUnloading"/> is raised.
/// </summary>
[PublicAPI]
public class UnloadingEventArgs : EventArgs
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContext"/> unloading
    /// </summary>
    public ModuleContext ModuleContext { get; }

    internal UnloadingEventArgs(ModuleContext moduleContext)
    {
        ModuleContext = moduleContext;
    }
}