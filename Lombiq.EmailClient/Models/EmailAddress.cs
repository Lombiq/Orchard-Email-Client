namespace Lombiq.EmailClient.Models;

/// <summary>
/// Represents an email address with a display name and the actual address.
/// </summary>
public class EmailAddress
{
    /// <summary>
    /// Gets or sets the display name of the email address (e.g., "John Doe").
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the actual email address (e.g., "john.doe@example.com").
    /// </summary>
    public string Address { get; set; }
}
