namespace Lombiq.EmailClient.Models;

public class EmailFilterParameters
{
    public string Folder { get; set; }
    public string Subject { get; set; }
    public uint AfterImapUniqueId { get; set; }
}
