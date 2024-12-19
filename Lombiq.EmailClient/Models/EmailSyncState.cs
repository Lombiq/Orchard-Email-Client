using OrchardCore.Data.Documents;
using System;

namespace Lombiq.EmailClient.Models;

public class EmailSyncState : Document
{
    public uint LastImapUid { get; set; }
    public DateTime? LastSyncDateUtc { get; set; }
}
