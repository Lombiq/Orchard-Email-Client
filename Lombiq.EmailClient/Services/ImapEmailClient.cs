using Lombiq.EmailClient.Constants;
using Lombiq.EmailClient.Models;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.EmailClient.Services;

public class ImapEmailClient : IEmailClient
{
    private readonly ImapSettings _imapSettings;
    private ImapClient _imapClient;
    private bool _isDisposed;

    public ImapEmailClient(IOptionsSnapshot<ImapSettings> imapSettings) =>
        _imapSettings = imapSettings.Value;

    public async Task<IEnumerable<EmailMessage>> GetEmailsAsync(EmailFilterParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        await InitializeImapClientAndConnectIfNeededAsync();
        var folder = await OpenFolderAsync(parameters.Folder);

        // Build the search query based on filter parameters
        SearchQuery searchQuery = BuildSearchQuery(parameters);

        // Fetch email summaries
        var uids = await folder.SearchAsync(searchQuery ?? SearchQuery.All);
        if (!uids.Any()) return Enumerable.Empty<EmailMessage>();

        var messageSummaries = await folder.FetchAsync(
            uids,
            MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.BodyStructure);

        // Map summaries to EmailMessage objects
        return messageSummaries.Select(summary => MapToEmailMessage(summary, folder.FullName)).ToList();
    }

    public async Task DownloadBodyAsync(EmailMessage emailMessage)
    {
        ArgumentNullException.ThrowIfNull(emailMessage);
        CanProcessEmailMessage(emailMessage);

        await InitializeImapClientAndConnectIfNeededAsync();
        var folder = await OpenFolderAsync(emailMessage.Metadata.FolderName);

        var uniqueId = new UniqueId(emailMessage.Metadata.ImapUniqueId);
        var message = await folder.GetMessageAsync(uniqueId);

        emailMessage.Content.Body = new EmailBody
        {
            Body = message.TextBody ?? message.HtmlBody,
            IsHtml = message.HtmlBody != null,
        };
        emailMessage.Content.IsBodyDownloaded = true;
    }

    public async Task DownloadAttachmentAsync(EmailMessage emailMessage, AttachmentMetadata attachmentMetadata)
    {
        ArgumentNullException.ThrowIfNull(emailMessage);
        ArgumentNullException.ThrowIfNull(attachmentMetadata);
        CanProcessEmailMessage(emailMessage);

        await InitializeImapClientAndConnectIfNeededAsync();
        var folder = await OpenFolderAsync(emailMessage.Metadata.FolderName);

        var uniqueId = new UniqueId(emailMessage.Metadata.ImapUniqueId);
        var message = await folder.GetMessageAsync(uniqueId);
        var mimeType = message.Attachments.FirstOrDefault(attachment =>
            attachment.ContentDisposition.FileName == attachmentMetadata.FileName);

        var filePath =
        attachmentMetadata.DownloadedFilePath = filePath;
    }

    private static SearchQuery BuildSearchQuery(EmailFilterParameters parameters)
    {
        SearchQuery searchQuery = null;

        if (parameters.AfterImapUniqueId > 0)
        {
            searchQuery = SearchQuery.Uids(
                new UniqueIdRange(new UniqueId(parameters.AfterImapUniqueId + 1), UniqueId.MaxValue));
        }

        if (!string.IsNullOrEmpty(parameters.Subject))
        {
            searchQuery = searchQuery == null
                ? SearchQuery.SubjectContains(parameters.Subject)
                : searchQuery.And(SearchQuery.SubjectContains(parameters.Subject));
        }

        return searchQuery;
    }

    private static EmailMessage MapToEmailMessage(IMessageSummary summary, string folderName)
    {
        var emailMessage = new EmailMessage
        {
            Metadata = new EmailMetadata
            {
                GlobalMessageId = summary.Envelope?.MessageId,
                Protocol = Protocols.Imap,
                ImapUniqueId = summary.UniqueId.Id,
                FolderName = folderName,
                IsReply = summary.Envelope?.InReplyTo != null,
            },
            Header = new EmailHeader
            {
                Subject = summary.Envelope?.Subject,
                Sender = CreateEmailAddresses(summary.Envelope?.From?.Mailboxes).FirstOrDefault(),
                SentDateUtc = summary.Envelope?.Date?.UtcDateTime,
                ReceivedDateUtc = summary.InternalDate?.UtcDateTime,
            },
            Content = new EmailContent
            {
                IsBodyDownloaded = false,
                AreAttachmentsDownloaded = false,
                Body = null,
            },
        };

        emailMessage.Content.Attachments.AddRange(summary.Attachments?.Select(att => new AttachmentMetadata
        {
            FileName = att.FileName, MimeType = att.ContentType.MimeType, Size = att.Octets,
        }).ToList() ?? []);

        return emailMessage;
    }

    private static IEnumerable<EmailAddress> CreateEmailAddresses(IEnumerable<MailboxAddress> mailboxes) =>
        mailboxes.Select(m => new EmailAddress { DisplayName = m.Name, Address = m.Address });

    private async Task<IMailFolder> OpenFolderAsync(string folderName)
    {
        var folder = string.IsNullOrEmpty(folderName)
            ? _imapClient.Inbox
            : await _imapClient.GetFolderAsync(folderName);
        await folder.OpenAsync(FolderAccess.ReadOnly);

        return folder;
    }

    private async Task InitializeImapClientAndConnectIfNeededAsync()
    {
        if (_imapClient?.IsConnected ?? false) return;

        _imapClient = new ImapClient();
        await _imapClient.ConnectAsync(_imapSettings.Server, _imapSettings.Port, _imapSettings.UseSsl);
        await _imapClient.AuthenticateAsync(_imapSettings.Username, _imapSettings.Password);
    }

    private static void CanProcessEmailMessage(EmailMessage emailMessage)
    {
        if (emailMessage.Metadata.Protocol != Protocols.Imap)
        {
            throw new InvalidOperationException("The email message protocol is not IMAP.");
        }
    }

    private static string SaveAttachmentToFile(string fileName, MimeEntity attachment)
    {
        var filePath = Path.Combine(Path.GetTempPath(), fileName);

        using var stream = File.Create(filePath);
        if (attachment is MimePart mimePart)
        {
            mimePart.Content.DecodeTo(stream);
        }

        return filePath;
    }

    #region IDisposable

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            if (_imapClient?.IsConnected ?? false) _imapClient.Disconnect(quit: true);

            _imapClient?.Dispose();
        }

        _isDisposed = true;
    }

    #endregion
}
