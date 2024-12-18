namespace Lombiq.EmailClient.Models;

public class EmailFetchParameters
{
    public string MessageId { get; set; }
    public string Folder { get; set; }
    public EmailDownloadOptions DownloadOptions { get; set; }
}
