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

    public ImapEmailClient(IOptionsSnapshot<ImapSettings> imapSettings)
    {
        _imapSettings = imapSettings.Value;
    }

    public async Task<IEnumerable<EmailMessage>> GetEmailsAsync(EmailFilterParameters filterParameters)
    {
        await InitializeImapClientAndConnectIfNeededAsync();
        var folder = await OpenFolderAsync(filterParameters.Folder);

        SearchQuery searchQuery = null;

        if (filterParameters.AfterImapUniqueId > 0)
        {
            searchQuery = SearchQuery.Uids(
                new UniqueIdRange(new UniqueId(filterParameters.AfterImapUniqueId + 1), UniqueId.MaxValue));
        }

        if (!string.IsNullOrEmpty(filterParameters.Subject))
        {
            searchQuery = searchQuery == null
                ? SearchQuery.SubjectContains(filterParameters.Subject)
                : searchQuery.And(SearchQuery.SubjectContains(filterParameters.Subject));
        }

        var uids = await folder.SearchAsync(searchQuery ?? SearchQuery.All);
        if (!uids.Any()) return [];

        var messageSummaries = await folder.FetchAsync(
            uids,
            MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.BodyStructure);

        var emailMessages = new List<EmailMessage>();

        foreach (var summary in messageSummaries)
        {
            var emailMessage = await CreateEmailMessageAsync(filterParameters, summary, folder);

            emailMessages.Add(emailMessage);
        }

        return emailMessages;
    }

    private async Task<EmailMessage> CreateEmailMessageAsync(
        EmailFilterParameters filterParameters,
        IMessageSummary summary,
        IMailFolder folder)
    {
        var emailMessage = new EmailMessage
        {
            Metadata = new EmailMetadata
            {
                GlobalMessageId = summary.Envelope?.MessageId,
                Protocol = Protocols.Imap,
                ImapUniqueId = summary.UniqueId.Id.ToTechnicalString(),
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

        if (filterParameters.ShouldDownloadBodyAsync != null &&
            await filterParameters.ShouldDownloadBodyAsync(emailMessage))
        {
            var bodyPart = summary.TextBody ?? summary.HtmlBody;
            if (bodyPart != null)
            {
                var body = await folder.GetBodyPartAsync(summary.UniqueId, bodyPart);
                emailMessage.Content.Body = new EmailBody { Body = body.ToString(), IsHtml = summary.HtmlBody != null };
                emailMessage.Content.IsBodyDownloaded = true;
            }
        }

        if (summary.Attachments?.Any() ?? false)
        {
            foreach (var attachmentSummary in summary.Attachments)
            {
                var attachmentMetadata = new AttachmentMetadata
                {
                    FileName = attachmentSummary.FileName,
                    MimeType = attachmentSummary.ContentType.MimeType,
                    Size = attachmentSummary.Octets,
                };

                if (filterParameters.ShouldDownloadAttachmentAsync != null &&
                    await filterParameters.ShouldDownloadAttachmentAsync(emailMessage, attachmentMetadata))
                {
                    var attachment = await folder.GetBodyPartAsync(summary.UniqueId, attachmentSummary);
                    var filePath = SaveAttachmentToFile(attachmentSummary.FileName, attachment);
                    attachmentMetadata.DownloadedFilePath = filePath;
                    emailMessage.Content.AreAttachmentsDownloaded = true;
                }

                emailMessage.Content.Attachments.Add(attachmentMetadata);
            }
        }

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

    private string SaveAttachmentToFile(string fileName, MimeEntity attachment)
    {
        var filePath = Path.Combine(Path.GetTempPath(), fileName);

        using (var stream = File.Create(filePath))
        {
            if (attachment is MimePart mimePart)
            {
                mimePart.Content.DecodeTo(stream);
            }
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
