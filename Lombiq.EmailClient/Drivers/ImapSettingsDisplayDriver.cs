using Lombiq.EmailClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using System;
using System.Threading.Tasks;

namespace Lombiq.EmailClient.Drivers;

public class ImapSettingsDisplayDriver : SiteDisplayDriver<ImapSettings>
{
    public const string GroupId = nameof(ImapSettings);

    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _hca;

    protected override string SettingsGroupId => GroupId;

    public ImapSettingsDisplayDriver(IAuthorizationService authorizationService, IHttpContextAccessor hca)
    {
        _authorizationService = authorizationService;
        _hca = hca;
    }

    public override async Task<IDisplayResult> EditAsync(ISite model, ImapSettings section, BuildEditorContext context)
    {
        if (!await AuthorizeAsync(context)) return null;

        return Initialize<ImapSettings>($"{nameof(ImapSettings)}_Edit", section.CopyTo)
            .PlaceInContent()
            .OnGroup(GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite model, ImapSettings section, UpdateEditorContext context)
    {
        if (await AuthorizeAsync(context) &&
            await context.CreateModelAsync<ImapSettings>(Prefix) is { } viewModel)
        {
            viewModel.CopyTo(section);
        }

        return await EditAsync(model, section, context);
    }

    private Task<bool> AuthorizeAsync(BuildEditorContext context) =>
        _hca.HttpContext?.User is { } user &&
        context.GroupId.EqualsOrdinalIgnoreCase(GroupId)
            ? _authorizationService.AuthorizeAsync(user, Permissions.ImapPermissions.ManageImapSettings)
            : Task.FromResult(false);
}
