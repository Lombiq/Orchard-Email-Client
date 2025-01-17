namespace Lombiq.EmailClient.Models;

public class EmailSyncSettings
{
    public string SubjectFilter { get; set; }

    public EmailSyncSettings CopyTo(EmailSyncSettings target)
    {
        target.SubjectFilter = SubjectFilter;

        return target;
    }
}
