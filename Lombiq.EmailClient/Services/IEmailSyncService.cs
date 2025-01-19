using System.Threading.Tasks;

namespace Lombiq.EmailClient.Services;

public interface IEmailSyncService
{
    Task SyncNextEmailsAsync();
}
