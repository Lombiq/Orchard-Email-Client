using Lombiq.EmailClient.Samples.Services;
using Lombiq.EmailClient.Services;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace Lombiq.EmailClient.Samples;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services) =>
        services.AddScoped<IEmailSyncEventHandler, SampleEmailSyncEventHandler>();
}
