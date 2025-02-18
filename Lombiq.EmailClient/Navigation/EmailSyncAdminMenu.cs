using Lombiq.EmailClient.Drivers;
using Lombiq.EmailClient.Permissions;
using Lombiq.HelpfulLibraries.OrchardCore.Navigation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace Lombiq.EmailClient.Navigation;

public sealed class EmailSyncAdminMenu : AdminMenuNavigationProviderBase
{
    public EmailSyncAdminMenu(IHttpContextAccessor hca, IStringLocalizer<EmailSyncAdminMenu> stringLocalizer)
        : base(hca, stringLocalizer)
    {
    }

    protected override void Build(NavigationBuilder builder) =>
        builder.Add(T["Configuration"], configuration => configuration
            .Add(T["Settings"], settings => settings
                .Add(T["Email Sync"], T["Email Sync"], demo => demo
                    .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = EmailSyncSettingsDisplayDriver.GroupId })
                    .Permission(EmailSyncPermissions.ManageEmailSyncSettings)
                    .LocalNav()
                )));
}
