using Lombiq.EmailClient.Models;
using Microsoft.Extensions.Logging;
using OrchardCore.Documents;
using OrchardCore.Modules;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lombiq.EmailClient.Services;

public class EmailSyncContext
{
    public string EmailMessageContentType { get; set; }
    public EmailFilterParameters FilterParameters { get; set; }
}

public interface IEmailSyncHandler
{
    Task BeforeSyncAsync(EmailSyncContext context);
    Task DownloadingBodyAsync(EmailSyncContext context, EmailMessage emailMessage);
    Task DownloadingAttachmentsAsync(EmailSyncContext context, EmailMessage emailMessage);
    Task DownloadingAttachmentAsync(EmailSyncContext context, EmailMessage emailMessage, AttachmentMetadata emailAttachment);
}

public interface IEmailSyncService
{
    Task SyncEmailsAsync();
}

public class EmailSyncService : IEmailSyncService
{
    private readonly IEmailClient _emailClient;
    private readonly IDocumentManager<EmailSyncState> _syncStateDocumentManager;
    private readonly ILogger<EmailSyncService> _logger;

    public EmailSyncService(
        IEmailClient emailClient,
        IDocumentManager<EmailSyncState> syncStateDocumentManager,
        ILogger<EmailSyncService> logger)
    {
        _emailClient = emailClient;
        _syncStateDocumentManager = syncStateDocumentManager;
        _logger = logger;
    }

    public async Task SyncEmailsAsync()
    {
        var syncState = await _syncStateDocumentManager.GetOrCreateMutableAsync();
        var emailFilterParameters = new EmailFilterParameters { AfterImapUniqueId = syncState.LastImapUid, };

        var context = new EmailSyncContext
        {
            EmailMessageContentType = "EmailMessage",
            FilterParameters = emailFilterParameters,
        };


        var emails = await _emailClient.GetEmailsAsync(emailFilterParameters);
    }
}
