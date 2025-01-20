using System.Collections.Generic;

namespace Lombiq.EmailClient.Models;

/// <summary>
/// Represents the content of an email, including its body and attachments.
/// </summary>
public class EmailContent
{
    /// <summary>
    /// Gets or sets the body of the email, including its content and format information.
    /// </summary>
    public EmailBody Body { get; set; }

    /// <summary>
    /// Gets the metadata of the attachments associated with this email.
    /// </summary>
    public IList<AttachmentMetadata> Attachments { get; private set; } = [];
}
