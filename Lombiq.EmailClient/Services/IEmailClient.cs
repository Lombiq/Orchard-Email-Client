using Lombiq.EmailClient.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lombiq.EmailClient.Services;

public interface IEmailClient : IDisposable
{
    Task<IEnumerable<EmailMessage>> GetEmailsAsync(EmailFilterParameters filterParameters);
}
