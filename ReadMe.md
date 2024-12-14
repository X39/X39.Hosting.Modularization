<!-- TOC -->
  * [Semantic Versioning](#semantic-versioning)
* [Overview](#overview)
  * [Installing the Template](#installing-the-template)
  * [Quickstart](#quickstart)
  * [Features](#features)
  * [FAQ](#faq)
    * [Why can't I replace or delete a library after unloading my module?](#why-cant-i-replace-or-delete-a-library-after-unloading-my-module)
    * [Why doesn't my renamed assembly load?](#why-doesnt-my-renamed-assembly-load)
    * [Why aren't my dependencies available in the module?](#why-arent-my-dependencies-available-in-the-module)
    * [How can I get the GUID of the module without injecting the `ModuleLoader`?](#how-can-i-get-the-guid-of-the-module-without-injecting-the-moduleloader)
    * [Why is there no `AddModularization(this IServiceCollection)` extension method?](#why-is-there-no-addmodularizationthis-iservicecollection-extension-method)
  * [Examples](#examples)
* [License](#license)
* [Contributing](#contributing)
  * [Code of Conduct](#code-of-conduct)
  * [Contributors Agreement](#contributors-agreement)
<!-- TOC -->

## Semantic Versioning

This library follows the principles of [Semantic Versioning](https://semver.org/). This means that version numbers and
the way they change convey meaning about the underlying changes in the library. For example, if a minor version number
changes (e.g., 1.1 to 1.2), this indicates that new features have been added in a backwards-compatible manner.

# Overview

The `X39.Hosting.Modularization` library simplifies adding modular functionality (plugins) to .NET applications. By
leveraging this library, you can dynamically load, unload, and manage modules that extend the core features of your
software.

## Installing the Template

The library provides a project template to streamline setup. To install the template via the .NET CLI, run:

```bash
dotnet new install X39.Hosting.Modularization.TemplatePackage
```

## Quickstart

To quickly get started, follow the example below:

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

> **Note**: The plugin directories are defined once during application startup.

> **Note**: There are additional overloads and ways to load modules, this is just one of them.
> Follow the code-documentation for further information.

## Features

- **Runtime Module Loading**  
  Dynamically load plugin-based assemblies at runtime.
  Each module can extend your application's functionality.

- **Module Configuration via `module.json`**  
  Every module has a `module.json` file, where you can define a unique module GUID,
  licenses, and other important metadata.

- **Automatic Dependency Resolution**  
  Modules can declare dependencies (on other modules) in the `module.json` file,
  and the library will automatically handle these dependencies during module loading.

- **Module Unloading**  
  To unload a module, simply call `Unload()` on the `ModuleContext`.
  Keep in mind that any references to types from the module will prevent it from fully unloading.

- **Clear Module Entry Point**  
  Every module must implement the `IModuleMain` interface once,
  similar to the `Main()` method in a standard .NET application.
  This provides a clear entry point with dependency injection support.

- **Dependency Injection Support**  
  Modules are instantiated using the `IServiceProvider` via dependency injection.
  Modules can also register services that other dependent modules can consume.

## FAQ

### Why can't I replace or delete a library after unloading my module?

Unloading a module works, but the .NET CLR doesn’t support fully "unloading" assemblies in all
situations.
If any part of your application holds a reference to a type from the unloaded module
(e.g., via caching), the assembly will remain in memory,
preventing you from replacing or deleting it.

To troubleshoot this,
check for objects still in memory that may be holding references to the module
(e.g., in Rider via the Memory Tab, or Visual Studio's .NET object snapshot tools).
Caching is a common culprit here.

### Why doesn't my renamed assembly load?

The library does not automatically detect the module entrypoint for security reasons.
If you rename an assembly, you must also update the `module.json` file to reflect the
new assembly name.

### Why aren't my dependencies available in the module?

Each module is treated as an isolated unit with its own dependency injection container.
This means it won't automatically integrate with the main application's dependency tree.
Ensure that your dependencies are explicitly defined in your module like so:

```csharp
public sealed class ModuleMain : IModuleMain
{
    private readonly IMyFancyService _myFancyService;
    
    public ModuleMain(IMyFancyService myFancyService)
    {
        _myFancyService = myFancyService;
    }
    
    public ValueTask ConfigureServicesAsync(
        IServiceCollection serviceCollection,
        CancellationToken cancellationToken
    )
    {
        serviceCollection.AddSingleton(_myFancyService);
        return ValueTask.CompletedTask;
    }
    
    // [...]
}
```

### How can I get the GUID of the module without injecting the `ModuleLoader`?

Yes, you can access the module's GUID, as specified in the `module.json`,
by injecting the `ModuleGuid` class from the
`X39.Hosting.Modularization.Abstraction` namespace:

```csharp
public sealed class ModuleMain : IModuleMain
{
    private readonly Guid _moduleGuid;
    
    public ModuleMain(ModuleGuid moduleGuid)
    {
        _moduleGuid = moduleGuid;
    }
    
    // [...]
}
```

### Why is there no `AddModularization(this IServiceCollection)` extension method?

It was actively decided against this to not add such an extension method, to prevent
"child" modules from calling this on their respective `IServiceCollection`.

If you must have a `IServiceCollection` overload, please create one your own.
You may call `X39.Hosting.Modularization.ServiceCollectionExtensions.AddModularizationSupport(...)`
using reflection to archive that.

## Examples

Sample projects are available in the
[GitHub repository](https://github.com/X39/X39.Hosting.Modularization/tree/master/samples).
These examples demonstrate how to build and extend applications using the `Modularization` library.
If you feel like there are some examples missing, feel free to create tickets or supply your own.

# License

The entire project, excluding the DotNet template, is released under
the [LGPLv3](https://www.gnu.org/licenses/lgpl-3.0.html) license.
The DotNet template, along with all generated code from it, is considered public domain and may be used or licensed
freely as per your needs.

# Contributing

Contributions are welcome!
Please submit a pull request or create a discussion to discuss any changes you wish to make.

## Code of Conduct

Be excellent to each other.

## Contributors Agreement

First of all, thank you for your interest in contributing to this project!
Please add yourself to the list of contributors in the [CONTRIBUTORS](CONTRIBUTORS.md) file when submitting your
first pull request.
Also, please always add the following to your pull request:

```
By contributing to this project, you agree to the following terms:
- You grant me and any other person who receives a copy of this project the right to use your contribution under the
  terms of the GNU Lesser General Public License v3.0.
- You grant me and any other person who receives a copy of this project the right to relicense your contribution under
  any other license.
- You grant me and any other person who receives a copy of this project the right to change your contribution.
- You waive your right to your contribution and transfer all rights to me and every user of this project.
- You agree that your contribution is free of any third-party rights.
- You agree that your contribution is given without any compensation.
- You agree that I may remove your contribution at any time for any reason.
- You confirm that you have the right to grant the above rights and that you are not violating any third-party rights
  by granting these rights.
- You confirm that your contribution is not subject to any license agreement or other agreement or obligation, which
  conflicts with the above terms.
```

This is necessary to ensure that this project can be licensed under the GNU Lesser General Public License v3.0 and
that a license change is possible in the future if necessary (e.g., to a more permissive license).
It also ensures that I can remove your contribution if necessary (e.g., because it violates third-party rights) and
that I can change your contribution if necessary (e.g., to fix a typo, change implementation details, or improve
performance).
It also shields me and every user of this project from any liability regarding your contribution by deflecting any
potential liability caused by your contribution to you (e.g., if your contribution violates the rights of your
employer).
Feel free to discuss this agreement in the discussions section of this repository, I am open to changes here (as long as
they do not open me or any other user of this project to any liability due to a **malicious contribution**).