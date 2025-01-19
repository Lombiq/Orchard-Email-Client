using Lombiq.EmailClient.Models;
using Microsoft.Extensions.Options;
using OrchardCore.Documents;
using OrchardCore.Modules;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.EmailClient.Services;

public class EmailSyncService : IEmailSyncService
{
    private readonly IEnumerable<IEmailSyncObserver> _emailSyncObservers;
    private readonly IEmailClient _emailClient;
    private readonly IDocumentManager<EmailSyncStateDocument> _emailSyncStateDocumentManager;
    private readonly IOptionsSnapshot<EmailSyncSettings> _emailSyncSettings;
    private readonly IClock _clock;

    public EmailSyncService(
        IEnumerable<IEmailSyncObserver> emailSyncObservers,
        IEmailClient emailClient,
        IDocumentManager<EmailSyncStateDocument> emailSyncStateDocumentManager,
        IOptionsSnapshot<EmailSyncSettings> emailSyncSettings,
        IClock clock)
    {
        _emailSyncObservers = emailSyncObservers;
        _emailClient = emailClient;
        _emailSyncStateDocumentManager = emailSyncStateDocumentManager;
        _emailSyncSettings = emailSyncSettings;
        _clock = clock;
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

        // 3. For each email, check if the body should be downloaded. Download them if needed. Also, check if the
        // attachments should be processed and process them if needed.
        foreach (var email in emails) await ProcessEmailAsync(email);

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

    private async Task ProcessEmailAsync(EmailMessage email)
    {
        var shouldDownloadBody = false;
        foreach (var observer in _emailSyncObservers)
        {
            if (await observer.ShouldDownloadBodyAsync(email))
            {
                shouldDownloadBody = true;
            }
        }

        if (shouldDownloadBody) await _emailClient.DownloadBodyAsync(email);

        foreach (var attachment in email.Content.Attachments) await ProcessAttachmentAsync(email, attachment);
    }

    private async Task ProcessAttachmentAsync(EmailMessage email, AttachmentMetadata attachment)
    {
        string tempPath = string.Empty;
        foreach (var observer in _emailSyncObservers)
        {
            if (!await observer.ShouldProcessAttachmentAsync(email, attachment)) continue;

            if (string.IsNullOrEmpty(tempPath))
            {
                tempPath = await _emailClient.DownloadAttachmentToTemporaryLocationAsync(email, attachment);
            }

            await observer.ProcessTemporarilyDownloadedAttachmentAsync(email, attachment, tempPath);
        }
    }
}
