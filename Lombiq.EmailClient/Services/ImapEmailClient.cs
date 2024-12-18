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

        var uids = await folder.SearchAsync(searchQuery ?? SearchQuery.All);
        if (!uids.Any()) return [];

        var messageSummaries = await folder.FetchAsync(
            uids,
            MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.BodyStructure);

        var emailMessages = new List<EmailMessage>();

        foreach (var summary in messageSummaries)
        {
            var emailMessage = await CreateEmailMessageAsync(parameters.DownloadOptions, summary, folder);

            emailMessages.Add(emailMessage);
        }

        return emailMessages;
    }

    public async Task DownloadEmailAsync(EmailMessage emailMessage, EmailDownloadOptions downloadOptions)
    {
        ArgumentNullException.ThrowIfNull(emailMessage);
        ArgumentNullException.ThrowIfNull(downloadOptions);

        if (emailMessage.Metadata.Protocol != Protocols.Imap)
        {
            throw new InvalidOperationException("The email message protocol is not IMAP.");
        }

        await InitializeImapClientAndConnectIfNeededAsync();
        var folder = await OpenFolderAsync(emailMessage.Metadata.FolderName);

        var uniqueId = new UniqueId(emailMessage.Metadata.ImapUniqueId);
        var message = await folder.GetMessageAsync(uniqueId);

        await DownloadEmailBodyIfNeededAsync(emailMessage, downloadOptions, folder, uniqueId, fullMessage: message);
        await DownloadEmailAttachmentsIfNeededAsync(
            emailMessage,
            downloadOptions,
            folder,
            uniqueId,
            mimeAttachments: message.Attachments);
    }

    private async Task<EmailMessage> CreateEmailMessageAsync(
        EmailDownloadOptions downloadOptions,
        IMessageSummary summary,
        IMailFolder folder)
    {
        var emailMessage = new EmailMessage
        {
            Metadata = new EmailMetadata
            {
                GlobalMessageId = summary.Envelope?.MessageId,
                Protocol = Protocols.Imap,
                ImapUniqueId = summary.UniqueId.Id,
                FolderName = folder.FullName,
                IsReply = summary.Envelope?.InReplyTo != null,
            },
            Header = new EmailHeader
            {
                Subject = summary.Envelope?.Subject,
                Sender = CreateEmailAddresses(summary.Envelope?.From?.Mailboxes).FirstOrDefault(),
                SentDateUtc = summary.Envelope?.Date?.UtcDateTime,
                ReceivedDateUtc = summary.InternalDate?.UtcDateTime,
            },
            Content = new EmailContent { IsBodyDownloaded = false, AreAttachmentsDownloaded = false, Body = null, },
        };

        emailMessage.Header.To.AddRange(CreateEmailAddresses(summary.Envelope?.To?.Mailboxes));
        emailMessage.Header.Cc.AddRange(CreateEmailAddresses(summary.Envelope?.Cc?.Mailboxes));
        emailMessage.Header.Bcc.AddRange(CreateEmailAddresses(summary.Envelope?.Bcc?.Mailboxes));

        await DownloadEmailBodyIfNeededAsync(emailMessage, downloadOptions, folder, summary.UniqueId, summary: summary);
        await DownloadEmailAttachmentsIfNeededAsync(
            emailMessage,
            downloadOptions,
            folder,
            summary.UniqueId,
            summary.Attachments);

        return emailMessage;
    }

    private static async Task DownloadEmailBodyIfNeededAsync(
        EmailMessage emailMessage,
        EmailDownloadOptions downloadOptions,
        IMailFolder folder,
        UniqueId uniqueId,
        IMessageSummary summary = null,
        MimeMessage fullMessage = null)
    {
        if (downloadOptions.ShouldDownloadBodyAsync == null ||
            await downloadOptions.ShouldDownloadBodyAsync(emailMessage))
        {
            if (summary != null)
            {
                var bodyPart = summary.TextBody ?? summary.HtmlBody;
                if (bodyPart != null)
                {
                    var body = await folder.GetBodyPartAsync(uniqueId, bodyPart);
                    emailMessage.Content.Body = new EmailBody
                    {
                        Body = body.ToString(), IsHtml = summary.HtmlBody != null,
                    };
                    emailMessage.Content.IsBodyDownloaded = true;
                }
            }
            else if (fullMessage != null)
            {
                emailMessage.Content.Body = new EmailBody
                {
                    Body = fullMessage.TextBody ?? fullMessage.HtmlBody, IsHtml = fullMessage.HtmlBody != null,
                };
                emailMessage.Content.IsBodyDownloaded = true;
            }
        }
    }

    private async Task DownloadEmailAttachmentsIfNeededAsync(
        EmailMessage emailMessage,
        EmailDownloadOptions downloadOptions,
        IMailFolder folder,
        UniqueId uniqueId,
        IEnumerable<BodyPartBasic> attachmentSummaries = null,
        IEnumerable<MimeEntity> mimeAttachments = null)
    {
        if (attachmentSummaries != null)
        {
            foreach (var attachmentSummary in attachmentSummaries)
            {
                var attachmentMetadata = new AttachmentMetadata
                {
                    FileName = attachmentSummary.FileName,
                    MimeType = attachmentSummary.ContentType.MimeType,
                    Size = attachmentSummary.Octets,
                };

                if (downloadOptions.ShouldDownloadAttachmentAsync == null ||
                    await downloadOptions.ShouldDownloadAttachmentAsync(emailMessage, attachmentMetadata))
                {
                    var attachment = await folder.GetBodyPartAsync(uniqueId, attachmentSummary);
                    var filePath = SaveAttachmentToFile(attachmentSummary.FileName, attachment);
                    attachmentMetadata.DownloadedFilePath = filePath;
                    emailMessage.Content.AreAttachmentsDownloaded = true;
                }

                emailMessage.Content.Attachments.Add(attachmentMetadata);
            }
        }
        else if (mimeAttachments != null)
        {
            foreach (var attachment in mimeAttachments.OfType<MimePart>())
            {
                var attachmentMetadata = new AttachmentMetadata
                {
                    FileName = attachment.FileName,
                    MimeType = attachment.ContentType.MimeType,
                    Size = attachment.Content.Stream.Length,
                };

                if (downloadOptions.ShouldDownloadAttachmentAsync == null ||
                    await downloadOptions.ShouldDownloadAttachmentAsync(emailMessage, attachmentMetadata))
                {
                    var filePath = SaveAttachmentToFile(attachment.FileName, attachment);
                    attachmentMetadata.DownloadedFilePath = filePath;
                    emailMessage.Content.AreAttachmentsDownloaded = true;
                }

                emailMessage.Content.Attachments.Add(attachmentMetadata);
            }
        }
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

    private static string SaveAttachmentToFile(string fileName, MimeEntity attachment)
    {
        // Temporary code here.
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
