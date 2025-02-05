using System;
using System.Collections.Generic;

namespace Lombiq.EmailClient.Models;

/// <summary>
/// Represents the headers of an email, including sender, recipients, subject, and dates.
/// </summary>
public class EmailHeader
{
    /// <summary>
    /// Gets or sets the subject line of the email.
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    /// Gets or sets the sender of the email, including their display name and email address.
    /// </summary>
    public EmailAddress Sender { get; set; }

    /// <summary>
    /// Gets the primary recipients of the email (To field).
    /// </summary>
    public IList<EmailAddress> To { get; private set; } = [];

    /// <summary>
    /// Gets the carbon copy recipients of the email (Cc field).
    /// </summary>
    public IList<EmailAddress> Cc { get; private set; } = [];

    /// <summary>
    /// Gets the blind carbon copy recipients of the email (Bcc field).
    /// </summary>
    public IList<EmailAddress> Bcc { get; private set; } = [];

    /// <summary>
    /// Gets or sets the date and time when the email was sent by the sender's client.
    /// This value is in UTC but reflects the sender's system clock and timezone.
    /// </summary>
    public DateTime? SentDateUtc { get; set; }
}
