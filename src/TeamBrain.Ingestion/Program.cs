using Microsoft.Extensions.Hosting;
using TeamBrain.Application;
using TeamBrain.Infrastructure;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddApplication();
        services.AddInfrastructure(context.Configuration);
    })
    .Build();

host.Run();
