using OrchardCore.Modules.Manifest;
using static Lombiq.EmailClient.Constants.FeatureIds;

[assembly: Module(
    Name = "Lombiq Email Client - Samples",
    Author = "Lombiq Technologies",
    Website = "https://github.com/Lombiq/Orchard-Email-Client",
    Version = "0.0.1",
    Description = "Samples for Lombiq Email Client.",
    Category = "Email",
    Dependencies = [
        Imap,
        EmailSync
    ]
)]
