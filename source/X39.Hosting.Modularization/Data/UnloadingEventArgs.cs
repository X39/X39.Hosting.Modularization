namespace X39.Hosting.Modularization.Data;

/// <summary>
/// <see cref="EventArgs"/> passed when <see cref="ModuleLoader.ModuleUnloading"/> is raised.
/// </summary>
[PublicAPI]
public class UnloadingEventArgs : EventArgs
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContextBase"/> unloading
    /// </summary>
    public ModuleContextBase ModuleContext { get; }

    internal UnloadingEventArgs(ModuleContextBase moduleContext)
    {
        ModuleContext = moduleContext;
    }
}