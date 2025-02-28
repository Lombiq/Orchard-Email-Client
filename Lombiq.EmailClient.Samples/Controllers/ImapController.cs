using Lombiq.EmailClient.Models;
using Lombiq.EmailClient.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Lombiq.EmailClient.Samples.Controllers;

// This controller demonstrates how to fetch emails from an IMAP server.
public class ImapController : Controller
{
    // The IEmailClient service is used to fetch emails. Check the interface to see what you can do with it.
    private readonly IEmailClient _emailClient;

    public ImapController(IEmailClient emailClient) =>
        _emailClient = emailClient;

    public async Task<ActionResult> Index()
    {
        // You can filter emails by subject. For testing purposes we filter by "important".
        var parameters = new EmailFilterParameters
        {
            Subject = "important",
        };

        // Fetch emails from the IMAP server.
        var emails = await _emailClient.GetEmailsAsync(parameters);

        // NEXT STATION: Go to Views/Imap/Index.cshtml.
        return View(emails);
    }
}
