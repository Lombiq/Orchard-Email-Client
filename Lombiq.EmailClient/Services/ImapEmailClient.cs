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
    private IDictionary<string, MimeMessage> _downloadedMessages = new Dictionary<string, MimeMessage>();

    public ImapEmailClient(IOptionsSnapshot<ImapSettings> imapSettings) =>
        _imapSettings = imapSettings.Value;

    public async Task<IEnumerable<EmailMessage>> GetEmailsAsync(EmailFilterParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        await InitializeImapClientAndConnectIfNeededAsync();
        var folder = await OpenFolderAsync(parameters.Folder);

        var searchQuery = BuildSearchQuery(parameters);

        var uids = await folder.SearchAsync(searchQuery ?? SearchQuery.All);
        if (!uids.Any()) return [];

        var messageSummaries = await folder.FetchAsync(
            uids,
            MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.BodyStructure);

        return messageSummaries.Select(summary => MapToEmailMessage(summary, folder.FullName)).ToList();
    }

    public async Task DownloadBodyAsync(EmailMessage emailMessage)
    {
        ArgumentNullException.ThrowIfNull(emailMessage);

        var message = await GetOrDownloadMessageAsync(emailMessage);

        emailMessage.Content.Body = new EmailBody
        {
            Body = message.TextBody ?? message.HtmlBody,
            IsHtml = message.HtmlBody != null,
        };
        emailMessage.Content.IsBodyDownloaded = true;
    }

    public async Task<string> DownloadAttachmentToTemporaryLocationAsync(EmailMessage emailMessage, AttachmentMetadata attachmentMetadata)
    {
        ArgumentNullException.ThrowIfNull(emailMessage);
        ArgumentNullException.ThrowIfNull(attachmentMetadata);

        var message = await GetOrDownloadMessageAsync(emailMessage);

        var attachment = message.Attachments.FirstOrDefault(att =>
            att.ContentDisposition.FileName == attachmentMetadata.FileName);
        var filePath = await SaveAttachmentToFileAsync(attachmentMetadata.FileName, attachment);
        attachmentMetadata.DownloadedFilePath = filePath;

        return filePath;
    }

    private async Task<MimeMessage> GetOrDownloadMessageAsync(EmailMessage emailMessage)
    {
        ArgumentNullException.ThrowIfNull(emailMessage);
        ValidateImapProtocol(emailMessage);

        if (_downloadedMessages.TryGetValue(emailMessage.Metadata.GlobalMessageId, out var message))
        {
            return message;
        }

        await InitializeImapClientAndConnectIfNeededAsync();
        var folder = await OpenFolderAsync(emailMessage.Metadata.FolderName);

        var uniqueId = new UniqueId(emailMessage.Metadata.ImapUniqueId);
        message = await folder.GetMessageAsync(uniqueId);

        _downloadedMessages.Add(emailMessage.Metadata.GlobalMessageId, message);

        return message;
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
        var message = new EmailMessage
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

        message.Content.Attachments.AddRange(summary.Attachments?.Select(att => new AttachmentMetadata
        {
            FileName = att.FileName,
            MimeType = att.ContentType.MimeType,
            Size = att.Octets,
        }) ?? []);

        return message;
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
        await _imapClient.ConnectAsync(_imapSettings.Host, _imapSettings.Port, _imapSettings.UseSsl);
        await _imapClient.AuthenticateAsync(_imapSettings.Username, _imapSettings.Password);
    }

    private static void ValidateImapProtocol(EmailMessage emailMessage)
    {
        if (emailMessage.Metadata.Protocol != Protocols.Imap)
        {
            throw new InvalidOperationException("The email message protocol is not IMAP.");
        }
    }

    private static async Task<string> SaveAttachmentToFileAsync(string fileName, MimeEntity attachment)
    {
        var filePath = Path.Combine(Path.GetTempPath(), fileName);

        await using var stream = File.Create(filePath);
        if (attachment is MimePart mimePart)
        {
            await mimePart.Content.DecodeToAsync(stream);
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
