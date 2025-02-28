# Lombiq Email Client

## About

An email client for Orchard Core that can fetch emails (including attachments) from a server. Right now it supports IMAP only. There's also a feature that can sync emails periodically from a background task in an extensible way; you can create your content items from emails for example.

Do you want to quickly try out this project and see it in action? Check it out in our [Open-Source Orchard Core Extensions](https://github.com/Lombiq/Open-Source-Orchard-Core-Extensions) full Orchard Core solution and also see our other useful Orchard Core-related open-source projects!

## Documentation

For detailed, documented examples, check out the [samples project](Lombiq.EmailClient.Samples).

### Fetching emails from an IMAP server

To fetch emails from an IMAP server, enable the `Lombiq.EmailClient.Imap` feature. Then you can configure the IMAP server settings in the admin dashboard under Configuration -> Settings -> IMAP. You can also configure the IMAP settings in the `appsettings.json` file:

```json
{
    "Lombiq_EmailClient_Imap": {
        "Host": "imap.example.com",
        "Port": 993,
        "UseSsl": true, 
        "RequireAuthentication": true,
        "Username": "username",
        "Password": "password"
    }
}
```

Then, inject the `IEmailClient` service and use it to fetch emails or process attachments. It's also possible to parameterize the email fetching process by using the `EmailFilterParameters` class; you can filter emails by subject for example.

### Syncing emails periodically

To sync emails periodically, enable the `Lombiq.EmailClient.EmailSync` feature. Then you can configure the sync settings in the admin dashboard under Configuration -> Settings -> Email Sync. You can also configure the sync settings in the `appsettings.json` file:

```json
{
    "Lombiq_EmailClient_EmailSync": {
        "SubjectFilter": "important"
    }
}
```

The email syncing is happening automatically in the `EmailSyncBackgroundTask` once per day by default. You can also trigger the sync manually by using the `IEmailSyncService` service. Check out the sample project for more details.

## Contributing and support

Bug reports, feature requests, comments, questions, code contributions and love letters are warmly welcome. You can send them to us via GitHub issues and pull requests. Please adhere to our [open-source guidelines](https://lombiq.com/open-source-guidelines) while doing so.

This project is developed by [Lombiq Technologies](https://lombiq.com/). Commercial-grade support is available through Lombiq.
