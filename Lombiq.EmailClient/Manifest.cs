using OrchardCore.Modules.Manifest;
using static Lombiq.EmailClient.Constants.FeatureIds;

[assembly: Module(
    Name = "Lombiq Email Client",
    Author = "Lombiq Technologies",
    Website = "https://github.com/Lombiq/Orchard-Email-Client",
    Version = "0.0.1"
)]

[assembly: Feature(
    Id = Default,
    Name = "Lombiq Email Client - Base",
    Category = "Email",
    Description = "Base functionality for the email client such as content types. Should be used along with a " +
        "specific email provider feature (e.g., IMAP)."
)]

[assembly: Feature(
    Id = Imap,
    Name = "Lombiq Email Client - IMAP",
    Category = "Email",
    Description = "IMAP email provider for the email client.",
    Dependencies =
    [
        Default,
    ]
)]

[assembly: Feature(
    Id = EmailSync,
    Name = "Lombiq Email Client - Email Sync",
    Category = "Email",
    Description = "Syncs emails from an email provider periodically.",
    Dependencies =
    [
        Default,
    ]
)]
