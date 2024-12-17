namespace Lombiq.EmailClient.Models;

/// <summary>
/// Represents protocol-specific metadata for an email.
/// </summary>
public class EmailMetadata
{
    /// <summary>
    /// Gets or sets the unique identifier of the email from the Message-ID header.
    /// This is globally unique across all emails.
    /// </summary>
    public string GlobalMessageId { get; set; }

    /// <summary>
    /// Gets or sets the name of the protocol used to fetch the email (e.g., "IMAP", "JMAP", "GMAIL_API").
    /// </summary>
    public string Protocol { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the email within the protocol (e.g., UID in IMAP).
    /// Note that this identifier is unique only within the context of a specific folder.
    /// </summary>
    public string ProtocolUniqueId { get; set; }

    /// <summary>
    /// Gets or sets the name of the folder where the email is stored (e.g., "INBOX", "Sent").
    /// </summary>
    public string FolderName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the email is a reply to another email.
    /// </summary>
    public bool IsReply { get; set; }
}
