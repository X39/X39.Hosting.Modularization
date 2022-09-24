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