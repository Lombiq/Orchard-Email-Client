namespace Lombiq.EmailClient.Models;

/// <summary>
/// Represents metadata for an attachment, such as its filename, MIME type, and size.
/// </summary>
public class AttachmentMetadata
{
    /// <summary>
    /// Gets or sets the filename of the attachment.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the MIME type of the attachment (e.g., "application/pdf", "image/jpeg").
    /// </summary>
    public string MimeType { get; set; }

    /// <summary>
    /// Gets or sets the size of the attachment in bytes.
    /// </summary>
    public long? Size { get; set; }

    /// <summary>
    /// Gets or sets the file path where the attachment has been downloaded.
    /// If null, the attachment has not been downloaded.
    /// </summary>
    public string DownloadedFilePath { get; set; }
}
