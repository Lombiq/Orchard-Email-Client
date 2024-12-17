using System;
using System.Threading.Tasks;

namespace Lombiq.EmailClient.Models;

public class EmailFilterParameters
{
    public string Folder { get; set; }
    public string Subject { get; set; }
    public uint AfterImapUniqueId { get; set; }
    public Func<EmailMessage, Task<bool>> ShouldDownloadBodyAsync { get; set; }
    public Func<EmailMessage, AttachmentMetadata, Task<bool>> ShouldDownloadAttachmentAsync { get; set; }
}
