using Lombiq.EmailClient.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using System.Threading.Tasks;

namespace Lombiq.EmailClient.Samples.Controllers;

// If the Email Sync feature is enabled, a background task automatically fetches emails from the configured IMAP server.
// An email sync event handler can be implemented to process these emails (you'll see it shortly). You can trigger the
// email sync manually by using the IEmailSyncService service. This controller demonstrates how to do that.
public class EmailSyncController : Controller
{
    public async Task<ActionResult> Index()
    {
        // The email sync operation might take a while so it's recommended to run it after the request.
        await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync(nameof(EmailSyncController), scope =>
        {
            var service = scope.ServiceProvider.GetService<IEmailSyncService>();

            // This method fetches the next batch of emails from the IMAP server. When it syncs an email it calls the
            // EmailSyncedAsync method of the registered email sync event handlers.
            return service.SyncNextEmailsAsync();
        });

        return Ok();
    }
}

// NEXT STATION: Services/SampleEmailSyncEventHandler.cs
