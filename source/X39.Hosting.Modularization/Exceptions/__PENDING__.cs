namespace X39.Hosting.Modularization.Exceptions;

// ToDo: Remove file & class
// ReSharper disable once InconsistentNaming
internal class __PENDING__
{
}

public class ModuleDependantsNotUnloadedException : ModularizationException
{
    public ModuleDependantsNotUnloadedException(
        ModuleContext moduleContext,
        IEnumerable<ModuleContext> loadedModuleContexts)
    {
        throw new NotImplementedException();
    }
}

public class ModuleDependencyNotLoadedException : ModularizationException
{
    public ModuleDependencyNotLoadedException(
        ModuleContext moduleContext,
        IEnumerable<ModuleContext> unloadedModuleContexts)
    {
        throw new NotImplementedException();
    }
}

internal class ModuleMainTypeIsGenericException : ModularizationException
{
    public ModuleMainTypeIsGenericException(ModuleContext moduleContext, string typeName)
    {
        throw new NotImplementedException();
    }
}

public class MultipleMainTypeConstructorsException : ModularizationException
{
    internal MultipleMainTypeConstructorsException(ModuleContext moduleContext, string candidates)
    {
        throw new NotImplementedException();
    }
}

public class NoModuleMainTypeException : ModularizationException
{
    internal NoModuleMainTypeException(ModuleContext moduleContext)
    {
        throw new NotImplementedException();
    }
}

public class MultipleModuleMainTypesException : ModularizationException
{
    internal MultipleModuleMainTypesException(ModuleContext moduleContext, IEnumerable<string> candidates)
    {
        throw new NotImplementedException();
    }
}