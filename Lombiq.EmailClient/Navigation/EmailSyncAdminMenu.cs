using Lombiq.EmailClient.Drivers;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace Lombiq.EmailClient.Navigation;

public sealed class EmailSyncAdminMenu : INavigationProvider
{
    private readonly IStringLocalizer T;

    public EmailSyncAdminMenu(IStringLocalizer<ImapAdminMenu> stringLocalizer) => T = stringLocalizer;

    public ValueTask BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase)) return ValueTask.CompletedTask;

        builder.Add(T["Configuration"], configuration => configuration
            .Add(T["Settings"], settings => settings
                .Add(T["Email Sync"], T["Email Sync"], demo => demo
                    .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = EmailSyncSettingsDisplayDriver.GroupId })
                    .Permission(Permissions.EmailSyncPermissions.ManageEmailSyncSettings)
                    .LocalNav()
                )));

        return ValueTask.CompletedTask;
    }
}
