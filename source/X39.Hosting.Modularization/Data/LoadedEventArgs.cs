namespace X39.Hosting.Modularization.Data;

/// <summary>
/// <see cref="EventArgs"/> passed when <see cref="ModuleLoader.ModuleLoaded"/> is raised.
/// </summary>
[PublicAPI]
public class LoadedEventArgs : EventArgs
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContext"/> loaded
    /// </summary>
    public ModuleContext ModuleContext { get; }

    internal LoadedEventArgs(ModuleContext moduleContext)
    {
        ModuleContext = moduleContext;
    }
}