using Lombiq.EmailClient.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Lombiq.EmailClient.Services;

/// <summary>
/// Service for fetching emails and attachments from an email server.
/// </summary>
public interface IEmailClient : IDisposable
{
    /// <summary>
    /// Gets emails from the email server based on the given parameters.
    /// </summary>
    Task<IEnumerable<EmailMessage>> GetEmailsAsync(EmailFilterParameters parameters);

    /// <summary>
    /// Gets the attachment stream for the given attachment of an email message.
    /// </summary>
    Task<Stream> GetAttachmentStreamAsync(EmailMessage emailMessage, AttachmentMetadata attachmentMetadata);
}
