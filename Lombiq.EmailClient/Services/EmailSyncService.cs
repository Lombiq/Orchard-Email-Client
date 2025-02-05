using Lombiq.EmailClient.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Documents;
using OrchardCore.Modules;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.EmailClient.Services;

public class EmailSyncService : IEmailSyncService
{
    private readonly IEnumerable<IEmailSyncEventHandler> _emailSyncEventHandlers;
    private readonly IEmailClient _emailClient;
    private readonly IDocumentManager<EmailSyncStateDocument> _emailSyncStateDocumentManager;
    private readonly IOptionsSnapshot<EmailSyncSettings> _emailSyncSettings;
    private readonly IClock _clock;
    private readonly ILogger<EmailSyncService> _logger;

    public EmailSyncService(
        IEnumerable<IEmailSyncEventHandler> emailSyncEventHandlers,
        IEmailClient emailClient,
        IDocumentManager<EmailSyncStateDocument> emailSyncStateDocumentManager,
        IOptionsSnapshot<EmailSyncSettings> emailSyncSettings,
        IClock clock,
        ILogger<EmailSyncService> logger)
    {
        _emailSyncEventHandlers = emailSyncEventHandlers;
        _emailClient = emailClient;
        _emailSyncStateDocumentManager = emailSyncStateDocumentManager;
        _emailSyncSettings = emailSyncSettings;
        _clock = clock;
        _logger = logger;
    }

    public async Task SyncNextEmailsAsync()
    {
        // 1. Check the UID of the last email that was synced.
        var emailSyncState = await _emailSyncStateDocumentManager.GetOrCreateMutableAsync();

        // 2. Fetch emails after the last synced UID.
        var emails = (await _emailClient.GetEmailsAsync(new EmailFilterParameters
        {
            AfterImapUniqueId = emailSyncState.LastSyncedImapUniqueId,
            Subject = _emailSyncSettings.Value.SubjectFilter,
        })).ToList();

        // 3. Trigger event handlers for each email.
        foreach (var email in emails)
        {
            await _emailSyncEventHandlers.InvokeAsync(handler => handler.EmailSyncedAsync(email), _logger);
        }

        // 4. Update the last synced UID and date.
        await UpdateSyncStateAsync(emails, emailSyncState);
    }

    private Task UpdateSyncStateAsync(List<EmailMessage> emails, EmailSyncStateDocument emailSyncState)
    {
        if (emails.Count > 0)
        {
            emailSyncState.LastSyncedImapUniqueId = emails.Max(email => email.Metadata.ImapUniqueId);
        }

        emailSyncState.LastSyncedDateUtc = _clock.UtcNow;

        return _emailSyncStateDocumentManager.UpdateAsync(emailSyncState);
    }
}
