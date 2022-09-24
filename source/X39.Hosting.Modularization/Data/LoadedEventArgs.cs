namespace X39.Hosting.Modularization.Data;

/// <summary>
/// <see cref="EventArgs"/> passed when <see cref="ModuleLoader.ModuleLoaded"/> is raised.
/// </summary>
[PublicAPI]
public class LoadedEventArgs : EventArgs
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContextBase"/> loaded
    /// </summary>
    public ModuleContextBase ModuleContext { get; }

    internal LoadedEventArgs(ModuleContextBase moduleContext)
    {
        ModuleContext = moduleContext;
    }
}