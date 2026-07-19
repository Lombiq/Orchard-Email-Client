using Lombiq.EmailClient.Drivers;
using Lombiq.EmailClient.Permissions;
using Lombiq.HelpfulLibraries.OrchardCore.Navigation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace Lombiq.EmailClient.Navigation;

public sealed class ImapAdminMenu : AdminMenuNavigationProviderBase
{
    public ImapAdminMenu(IHttpContextAccessor hca, IStringLocalizer<ImapAdminMenu> stringLocalizer)
        : base(hca, stringLocalizer)
    {
    }

    protected override void Build(NavigationBuilder builder) =>
        builder
            .Add(T["Settings"], settings => settings
                .Add(T["IMAP"], T["IMAP"], demo => demo
                    .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = ImapSettingsDisplayDriver.GroupId })
                    .Permission(ImapPermissions.ManageImapSettings)
                    .LocalNav()
                ));
}
