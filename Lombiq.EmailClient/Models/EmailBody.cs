namespace Lombiq.EmailClient.Models;

/// <summary>
/// Represents the body of an email, including its content and format.
/// </summary>
public class EmailBody
{
    /// <summary>
    /// Gets or sets the main body of the email as plain text or HTML.
    /// </summary>
    public string Body { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the body contains HTML content.
    /// </summary>
    public bool IsHtml { get; set; }
}
