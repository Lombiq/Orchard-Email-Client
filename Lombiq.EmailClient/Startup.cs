using Lombiq.EmailClient.Constants;
using Lombiq.EmailClient.Drivers;
using Lombiq.EmailClient.Models;
using Lombiq.EmailClient.Navigation;
using Lombiq.EmailClient.Permissions;
using Lombiq.EmailClient.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using System;

namespace Lombiq.EmailClient;

[Feature(FeatureIds.Default)]
public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
    }
}

[Feature(FeatureIds.Imap)]
public class ImapStartup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public ImapStartup(IShellConfiguration shellConfiguration) => _shellConfiguration = shellConfiguration;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IPermissionProvider, ImapPermissions>();
        services.AddScoped<INavigationProvider, ImapAdminMenu>();
        services.AddSiteDisplayDriver<ImapSettingsDisplayDriver>();

        services.Configure<ImapSettings>(_shellConfiguration.GetSection("Lombiq_EmailClient_Imap"));
        services.AddTransient<IConfigureOptions<ImapSettings>, ImapSettingsConfiguration>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
    }
}
