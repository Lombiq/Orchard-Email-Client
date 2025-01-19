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

public class EmailSyncSettingsDisplayDriver : SiteDisplayDriver<EmailSyncSettings>
{
    public const string GroupId = nameof(EmailSyncSettings);

    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _hca;

    protected override string SettingsGroupId => GroupId;

    public EmailSyncSettingsDisplayDriver(IAuthorizationService authorizationService, IHttpContextAccessor hca)
    {
        _authorizationService = authorizationService;
        _hca = hca;
    }

    public override async Task<IDisplayResult> EditAsync(ISite model, EmailSyncSettings section, BuildEditorContext context)
    {
        if (!await AuthorizeAsync(context)) return null;

        return Initialize<EmailSyncSettings>($"{nameof(EmailSyncSettings)}_Edit", section.CopyTo)
            .PlaceInContent()
            .OnGroup(GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite model, EmailSyncSettings section, UpdateEditorContext context)
    {
        if (await AuthorizeAsync(context) &&
            await context.CreateModelAsync<EmailSyncSettings>(Prefix) is { } viewModel)
        {
            viewModel.CopyTo(section);
        }

        return await EditAsync(model, section, context);
    }

    private Task<bool> AuthorizeAsync(BuildEditorContext context) =>
        _hca.HttpContext?.User is { } user &&
        context.GroupId.EqualsOrdinalIgnoreCase(GroupId)
            ? _authorizationService.AuthorizeAsync(user, Permissions.EmailSyncPermissions.ManageEmailSyncSettings)
            : Task.FromResult(false);
}
