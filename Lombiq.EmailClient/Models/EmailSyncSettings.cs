using Lombiq.HelpfulLibraries.Common.Utilities;

namespace Lombiq.EmailClient.Models;

public class EmailSyncSettings : ICopier<EmailSyncSettings>
{
    public string SubjectFilter { get; set; }

    public void CopyTo(EmailSyncSettings target) =>
        target.SubjectFilter = SubjectFilter;
}
