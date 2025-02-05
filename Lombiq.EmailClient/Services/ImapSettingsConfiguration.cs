using Lombiq.EmailClient.Models;
using Microsoft.Extensions.Options;
using OrchardCore.Settings;

namespace Lombiq.EmailClient.Services;

public class ImapSettingsConfiguration : IConfigureOptions<ImapSettings>
{
    private readonly ISiteService _siteService;

    public ImapSettingsConfiguration(ISiteService siteService) => _siteService = siteService;

    public void Configure(ImapSettings options)
    {
        if (!string.IsNullOrEmpty(options.Host)) return;

        var settings = _siteService.GetSettingsAsync<ImapSettings>().GetAwaiter().GetResult();

        settings.CopyTo(options);
    }
}

public class EmailSyncSettingsConfiguration : IConfigureOptions<EmailSyncSettings>
{
    private readonly ISiteService _siteService;

    public EmailSyncSettingsConfiguration(ISiteService siteService) => _siteService = siteService;

    public void Configure(EmailSyncSettings options)
    {
        if (!string.IsNullOrEmpty(options.SubjectFilter)) return;

        var settings = _siteService.GetSettingsAsync<EmailSyncSettings>().GetAwaiter().GetResult();

        settings.CopyTo(options);
    }
}
