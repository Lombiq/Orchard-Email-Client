using System;
using System.Threading.Tasks;

namespace Lombiq.EmailClient.Models;

public class EmailDownloadOptions
{
    public Func<EmailMessage, Task<bool>> ShouldDownloadBodyAsync { get; set; }
    public Func<EmailMessage, AttachmentMetadata, Task<bool>> ShouldDownloadAttachmentAsync { get; set; }
}
