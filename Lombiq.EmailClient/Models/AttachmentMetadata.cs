using System.Net.Mime;

namespace Lombiq.EmailClient.Models;

/// <summary>
/// Represents metadata for an attachment, such as its file name, MIME type, and size.
/// </summary>
public class AttachmentMetadata
{
    /// <summary>
    /// Gets or sets the file name of the attachment.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the MIME type of the attachment (e.g., <see cref="MediaTypeNames.Application.Pdf"/>).
    /// </summary>
    public string MimeType { get; set; }

    /// <summary>
    /// Gets or sets the size of the attachment in bytes.
    /// </summary>
    public long? Size { get; set; }
}
