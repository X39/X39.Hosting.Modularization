namespace X39.Hosting.Modularization.Data;

/// <summary>
/// <see cref="EventArgs"/> passed when <see cref="ModuleLoader.ModuleLoading"/> is raised.
/// </summary>
[PublicAPI]
public class LoadingEventArgs : EventArgs
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContextBase"/> loading
    /// </summary>
    public ModuleContextBase ModuleContext { get; }

    internal LoadingEventArgs(ModuleContextBase moduleContext)
    {
        ModuleContext = moduleContext;
    }
}

/// <summary>
/// <see cref="EventArgs"/> passed when <see cref="ModuleLoader.ModuleDiscovered"/> is raised.
/// </summary>
[PublicAPI]
public class DiscoveredEventArgs : EventArgs
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContextBase"/> loading
    /// </summary>
    public ModuleContextBase ModuleContext { get; }

    internal DiscoveredEventArgs(ModuleContextBase moduleContext)
    {
        ModuleContext = moduleContext;
    }
}