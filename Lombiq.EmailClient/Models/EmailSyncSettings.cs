namespace Lombiq.EmailClient.Models;

public class EmailSyncSettings
{
    public string SubjectFilter { get; set; }

    public void CopyTo(EmailSyncSettings target)
    {
        target.SubjectFilter = SubjectFilter;
    }
}
