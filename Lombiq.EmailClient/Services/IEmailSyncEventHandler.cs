using Lombiq.EmailClient.Models;
using System.Threading.Tasks;

namespace Lombiq.EmailClient.Services;

public interface IEmailSyncEventHandler
{
    Task EmailSyncedAsync(EmailMessage emailMessage);
}
