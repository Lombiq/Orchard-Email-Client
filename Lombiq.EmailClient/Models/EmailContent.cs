using System.Collections.Generic;

namespace Lombiq.EmailClient.Models;

/// <summary>
/// Represents the content of an email, including its body and attachments.
/// </summary>
public class EmailContent
{
    /// <summary>
    /// Gets or sets a value indicating whether the email body has been downloaded.
    /// </summary>
    public bool IsBodyDownloaded { get; set; }

    /// <summary>
    /// Gets or sets the body of the email, including its content and format information.
    /// </summary>
    public EmailBody Body { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the email's attachments have been downloaded.
    /// </summary>
    public bool AreAttachmentsDownloaded { get; set; }

    /// <summary>
    /// Gets the metadata of the attachments associated with this email.
    /// </summary>
    public IList<AttachmentMetadata> Attachments { get; private set; } = [];
}
