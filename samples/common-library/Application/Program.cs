// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Hosting;
using X39.Hosting.Modularization;
using X39.Hosting.Modularization.Configuration;

using (var writer = new StreamWriter("config.json"))
{
    writer.Write(ModuleConfiguration.JsonSample);
}
var host = Host.CreateDefaultBuilder(args);
host.UseModularization("modules");