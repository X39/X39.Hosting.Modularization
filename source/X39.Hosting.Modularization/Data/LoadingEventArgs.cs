namespace X39.Hosting.Modularization.Data;

/// <summary>
/// <see cref="EventArgs"/> passed when <see cref="ModuleLoader.ModuleLoading"/> is raised.
/// </summary>
[PublicAPI]
public class LoadingEventArgs : EventArgs
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContext"/> loading
    /// </summary>
    public ModuleContext ModuleContext { get; }

    internal LoadingEventArgs(ModuleContext moduleContext)
    {
        ModuleContext = moduleContext;
    }
}