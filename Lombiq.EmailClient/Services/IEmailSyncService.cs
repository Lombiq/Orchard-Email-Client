using Lombiq.EmailClient.Models;
using OrchardCore.Documents;
using OrchardCore.Modules;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lombiq.EmailClient.Services;

public class AttachmentProcessingResult
{
    public bool IsProcessed { get; set; }
    public bool ProcessedFilePath { get; set; }
}

public interface IEmailSyncObserver
{
    /// <summary>
    /// Determines whether the body of the email should be downloaded.
    /// </summary>
    /// <param name="emailMessage"></param>
    /// <returns></returns>
    Task<bool> ShouldDownloadBodyAsync(EmailMessage emailMessage);
    Task<bool> ShouldProcessAttachmentAsync(EmailMessage emailMessage, AttachmentMetadata attachmentMetadata);
    Task<AttachmentProcessingResult> ProcessTemporarilyDownloadedAttachmentAsync(
        EmailMessage emailMessage,
        AttachmentMetadata attachmentMetadata,
        string filePath);
}

public class EmailSyncContext
{
    public string EmailMessageContentType { get; set; }
}

public interface IEmailSyncHandler
{
    Task BeforeSyncAsync(EmailSyncContext context);
}

public interface IEmailSyncService
{
    Task SyncEmailsAsync();
}

public class EmailSyncService : IEmailSyncService
{
    private readonly IEnumerable<IEmailSyncObserver> _emailSyncObservers;
    private readonly IEmailClient _emailClient;
    private readonly IDocumentManager<EmailSyncStateDocument> _emailSyncStateDocumentManager;

    public EmailSyncService(
        IEnumerable<IEmailSyncObserver> emailSyncObservers,
        IEmailClient emailClient,
        IDocumentManager<EmailSyncStateDocument> emailSyncStateDocumentManager)
    {
        _emailSyncObservers = emailSyncObservers;
        _emailClient = emailClient;
        _emailSyncStateDocumentManager = emailSyncStateDocumentManager;
    }

    public async Task SyncEmailsAsync()
    {
        var context = new EmailSyncContext();

        // 1. Check the UID of the last email that was synced.
        var emailSyncState = await _emailSyncStateDocumentManager.GetOrCreateMutableAsync();

        // 2. Fetch emails after the last synced UID.
        var emails = await _emailClient.GetEmailsAsync(new EmailFilterParameters
        {
            AfterImapUniqueId = emailSyncState.LastImapUniqueId,
        });

        // 3. For each email, check if the body should be downloaded. Download them if needed. Also, check if the
        // attachments should be processed and process them if needed.
        foreach (var email in emails)
        {
            var shouldDownloadBody = false;
            foreach (var observer in _emailSyncObservers)
            {
                if (await observer.ShouldDownloadBodyAsync(email))
                {
                    shouldDownloadBody = true;
                }
            }

            if (shouldDownloadBody)
            {
                await _emailClient.DownloadBodyAsync(email);
            }

            foreach (var attachment in email.Content.Attachments)
            {
                string tempPath = string.Empty;
                foreach (var observer in _emailSyncObservers)
                {
                    if (await observer.ShouldProcessAttachmentAsync(email, attachment))
                    {
                        if (string.IsNullOrEmpty(tempPath))
                        {
                            tempPath = await _emailClient.DownloadAttachmentToTemporaryLocationAsync(email, attachment);
                        }

                        var processingResult = await observer.ProcessTemporarilyDownloadedAttachmentAsync(email, attachment, filePath);
                        if (processingResult.IsProcessed)
                        {
                            attachment.DownloadedFilePath = processingResult.ProcessedFilePath;
                        }
                    }
                }
            }
        }





        foreach (var observer in _emailSyncObservers)
        {
            await observer.ShouldDownloadBodyAsync();
        }

        var emailFilterParameters = new EmailFilterParameters();
    }
}
