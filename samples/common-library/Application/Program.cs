// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using X39.Hosting.Modularization;
using X39.Hosting.Modularization.Configuration;


var hostBuilder = Host.CreateDefaultBuilder(args);
hostBuilder.UseModularization(Path.GetFullPath("Modules"));
hostBuilder.ConfigureServices(collection => collection.AddHostedService<Worker>());
var host = hostBuilder.Build();
host.Run();

// Write out default config in the end
using var writer = new StreamWriter("config.json");
writer.Write(ModuleConfiguration.JsonSample);