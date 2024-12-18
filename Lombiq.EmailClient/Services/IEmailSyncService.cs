using Lombiq.EmailClient.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lombiq.EmailClient.Services;

public class EmailSyncContext
{
    public string EmailMessageContentType { get; set; }
}

public interface IEmailSyncHandler
{
    Task BeforeSyncAsync(EmailSyncContext context);
}

public interface IEmailSyncService
{
    Task SyncEmailsAsync();
}

public class EmailSyncService : IEmailSyncService
{
    private readonly IEnumerable<IEmailSyncHandler> _emailSyncHandlers;
    private readonly IEmailClient _emailClient;

    public EmailSyncService(IEnumerable<IEmailSyncHandler> emailSyncHandlers, IEmailClient emailClient)
    {
        _emailSyncHandlers = emailSyncHandlers;
        _emailClient = emailClient;
    }

    public async Task SyncEmailsAsync()
    {
        var context = new EmailSyncContext();

        foreach (var handler in _emailSyncHandlers)
        {
            await handler.BeforeSyncAsync(context);
        }

        var emailFilterParameters = new EmailFilterParameters();
    }
}
