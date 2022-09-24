namespace X39.Hosting.Modularization.Data;

/// <summary>
/// <see cref="EventArgs"/> passed when <see cref="ModuleLoader.ModuleUnloaded"/> is raised.
/// </summary>
[PublicAPI]
public class UnloadedEventArgs : EventArgs
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContextBase"/> unloaded
    /// </summary>
    public ModuleContextBase ModuleContext { get; }

    internal UnloadedEventArgs(ModuleContextBase moduleContext)
    {
        ModuleContext = moduleContext;
    }
}