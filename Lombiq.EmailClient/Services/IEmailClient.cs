using Lombiq.EmailClient.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lombiq.EmailClient.Services;

public interface IEmailClient : IDisposable
{
    Task<IEnumerable<EmailMessage>> GetEmailsAsync(EmailFilterParameters parameters);
    Task DownloadBodyAsync(EmailMessage emailMessage);
    Task<string> DownloadAttachmentToTemporaryLocationAsync(EmailMessage emailMessage, AttachmentMetadata attachmentMetadata);
}
