namespace Lombiq.EmailClient.Models;

/// <summary>
/// Represents the metadata, headers, and content of an email, including protocol-specific details,
/// sender and recipient information, and message body.
/// </summary>
public class EmailMessage
{
    /// <summary>
    /// Gets or sets the protocol-specific metadata of the email (e.g., Message-ID, protocol name, and folder information).
    /// </summary>
    public EmailMetadata Metadata { get; set; }

    /// <summary>
    /// Gets or sets the headers of the email, including sender, recipients, and subject.
    /// </summary>
    public EmailHeader Header { get; set; }

    /// <summary>
    /// Gets or sets the content of the email, including its body and format details.
    /// This is populated only after the email is downloaded.
    /// </summary>
    public EmailContent Content { get; set; }
}
