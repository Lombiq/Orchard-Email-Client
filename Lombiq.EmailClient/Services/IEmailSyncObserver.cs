using Lombiq.EmailClient.Models;
using System.Threading.Tasks;

namespace Lombiq.EmailClient.Services;

public interface IEmailSyncObserver
{
    /// <summary>
    /// Determines whether the body of the email should be downloaded.
    /// </summary>
    /// <param name="emailMessage"></param>
    /// <returns></returns>
    Task<bool> ShouldDownloadBodyAsync(EmailMessage emailMessage);
    Task<bool> ShouldProcessAttachmentAsync(EmailMessage emailMessage, AttachmentMetadata attachmentMetadata);
    Task ProcessTemporarilyDownloadedAttachmentAsync(
        EmailMessage emailMessage,
        AttachmentMetadata attachmentMetadata,
        string temporaryFilePath);
}
