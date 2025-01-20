using Lombiq.EmailClient.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Lombiq.EmailClient.Services;

public interface IEmailClient : IDisposable
{
    Task<IEnumerable<EmailMessage>> GetEmailsAsync(EmailFilterParameters parameters);
    Task<Stream> GetAttachmentStreamAsync(EmailMessage emailMessage, AttachmentMetadata attachmentMetadata);
}
