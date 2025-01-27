using System.Threading.Tasks;

namespace Lombiq.EmailClient.Services;

/// <summary>
/// Service for syncing emails from an email server.
/// </summary>
public interface IEmailSyncService
{
    /// <summary>
    /// Syncs the next emails from the email server that haven't been synced yet.
    /// </summary>
    Task SyncNextEmailsAsync();
}
