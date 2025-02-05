using Lombiq.EmailClient.Models;
using System.Threading.Tasks;

namespace Lombiq.EmailClient.Services;

/// <summary>
/// Event handler for email sync events.
/// </summary>
public interface IEmailSyncEventHandler
{
    /// <summary>
    /// Called when an email is synced.
    /// </summary>
    Task EmailSyncedAsync(EmailMessage emailMessage);
}
