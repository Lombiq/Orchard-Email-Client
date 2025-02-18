using Lombiq.HelpfulLibraries.OrchardCore.Users;
using OrchardCore.Security.Permissions;
using System.Collections.Generic;

namespace Lombiq.EmailClient.Permissions;

public class ImapPermissions : AdminPermissionBase
{
    public static readonly Permission ManageImapSettings = new(nameof(ManageImapSettings), "Manage IMAP settings");

    private static readonly IReadOnlyList<Permission> _adminPermissions =
    [
        ManageImapSettings,
    ];

    protected override IEnumerable<Permission> AdminPermissions => _adminPermissions;
}
