# Quickstart

A simple setup looks as follows:

```csharp
// Program.cs
var hostBuilder = Host.CreateDefaultBuilder(args);
hostBuilder.UseModularization(Path.GetFullPath("Modules"));
hostBuilder.ConfigureServices(collection => collection.AddHostedService<Worker>());
var host = hostBuilder.Build();
host.Run();

// Worker.cs
public class Worker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _moduleLoader.LoadAllAsync(stoppingToken);
    }
}
```

Please do note that plugin directories are provided once at start.

# Features

- **`module.json` configuration file to provide information about the module:**

  The module configuration allows to declare things like a guid for the module to
  uniquely identify them over successive runs, offers a way to declare a module license,
  list the licenses used in the module and much more.
- **Automatic dependency resolution:**

  A module can depend on another module by providing a dependency
  in the `module.json` file.
- **Module unloading:**

  Unloading a module is as simple as calling `Unload()` on the `ModuleContext`.
  Please do note that normal unloading rules apply - as in: even having a type
  reference is enough to prevent proper unloading of the assembly.
  To debug theese, checking which objects are still loaded (Rider -> Memory Tab;
  Visual Studio -> Take .net object snapshot with diagnostics tool; for both check
  the types that live in the loaded module) can be a useful tool in your tookit.
  A common issue eg. is caching that prevents the types from being properly released by
  the garbage collector.
- **Clear entry point:**

  Every module must, for it to properly function, implement the `IModuleMain` interface.
  This allows the module to work more or less like the normal ´void Main()` you know
  and love.
- **Dependency Injection for modules:**

  Every main module class is constructed by dependency injection, using
  `IServiceProvider`. It even supports nullable references so you do not have to
  ever worry about invalid "not null" references ever again. Additionally, every module also
  can offer services to other modules depending on it.