using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.EmailClient.Services;

[BackgroundTask(
    Schedule = "0 1 * * *",
    Description = "Performs email synchronization.")]
public class EmailSyncBackgroundTask : IBackgroundTask
{
    public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var emailSyncService = serviceProvider.GetRequiredService<IEmailSyncService>();

        return emailSyncService.SyncNextEmailsAsync();
    }
}
