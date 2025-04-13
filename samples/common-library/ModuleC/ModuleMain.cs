using Microsoft.Extensions.DependencyInjection;
using X39.Hosting.Modularization.Abstraction;
using X39.Hosting.Modularization.Samples.CommonLibrary.Common;

namespace X39.Hosting.Modularization.Samples.CommonLibrary.ModuleC;

public class ModuleMain : IModuleMain
{
    public ValueTask ConfigureServicesAsync(IServiceCollection serviceCollection, CancellationToken cancellationToken)
    {
        serviceCollection.AddTransient<DependantClass>();
        return ValueTask.CompletedTask;
    }

    public async ValueTask ConfigureAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var dependant = scope.ServiceProvider.GetRequiredService<DependantClass>();
            Console.WriteLine("MODULEC-MODULEC-MODULEC-MODULEC-MODULEC-MODULEC-MODULEC");
            Console.WriteLine("MODULEC-MODULEC-MODULEC-MODULEC-MODULEC-MODULEC-MODULEC");
            Console.WriteLine("MODULEC-MODULEC-MODULEC-MODULEC-MODULEC-MODULEC-MODULEC");
            Console.WriteLine("MODULEC-MODULEC-MODULEC-MODULEC-MODULEC-MODULEC-MODULEC");
            Console.WriteLine("MODULEC-MODULEC-MODULEC-MODULEC-MODULEC-MODULEC-MODULEC");
        }
    }
}
