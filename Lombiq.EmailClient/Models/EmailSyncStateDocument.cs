using OrchardCore.Data.Documents;
using System;

namespace Lombiq.EmailClient.Models;

public class EmailSyncStateDocument : Document
{
    public uint LastImapUniqueId { get; set; }
    public DateTime? LastSyncedTimeUtc { get; set; }
}
