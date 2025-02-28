using Lombiq.EmailClient.Models;
using Lombiq.EmailClient.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Lombiq.EmailClient.Samples.Services;

// This is a sample implementation of an email sync event handler. It's called when the Email Sync feature is enabled
// and the background task syncs the next batch of emails (i.e., emails that have been received since the last sync).
public class SampleEmailSyncEventHandler : IEmailSyncEventHandler
{
    private readonly ILogger<SampleEmailSyncEventHandler> _logger;

    public SampleEmailSyncEventHandler(ILogger<SampleEmailSyncEventHandler> logger) =>
        _logger = logger;

    public Task EmailSyncedAsync(EmailMessage emailMessage)
    {
        // For testing purposes we just log the email's subject and sender. However, in a real-world scenario you'd most
        // likely want to create a content item that stores the email's data.
        _logger.LogDebug(
            "Email synced (subject: {Subject}, sender: {Sender})",
            emailMessage.Header.Subject,
            emailMessage.Header.Sender.Address);

        return Task.CompletedTask;
    }
}

// END OF TRAINING SECTION: Email sync
