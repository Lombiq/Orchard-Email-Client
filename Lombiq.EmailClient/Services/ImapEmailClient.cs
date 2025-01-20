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
using UniqueId = MailKit.UniqueId;

namespace Lombiq.EmailClient.Services;

public class ImapEmailClient : IEmailClient
{
    private readonly Dictionary<uint, MimeMessage> _downloadedMessages = [];
    private readonly ImapSettings _imapSettings;
    private ImapClient _imapClient;
    private bool _isDisposed;

    public ImapEmailClient(IOptionsSnapshot<ImapSettings> imapSettings) =>
        _imapSettings = imapSettings.Value;

    public async Task<IEnumerable<EmailMessage>> GetEmailsAsync(EmailFilterParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        await InitializeImapClientAndConnectIfNeededAsync();
        var folder = await OpenFolderAsync();

        var searchQuery = BuildSearchQuery(parameters);

        var uids = (await folder.SearchAsync(searchQuery ?? SearchQuery.All)).Select(uniqueId => uniqueId.Id).ToList();
        if (uids.Count == 0) return [];

        var messages = new List<EmailMessage>();
        foreach (var uid in uids)
        {
            var mimeMessage = await GetOrDownloadMessageAsync(uid, folder);

            messages.Add(MapToEmailMessage(mimeMessage, folder.FullName, uid));
        }

        return messages;
    }

    public async Task<Stream> GetAttachmentStreamAsync(
        EmailMessage emailMessage,
        AttachmentMetadata attachmentMetadata)
    {
        ArgumentNullException.ThrowIfNull(emailMessage);
        ArgumentNullException.ThrowIfNull(attachmentMetadata);

        ValidateImapProtocol(emailMessage);
        var message = await GetOrDownloadMessageAsync(emailMessage.Metadata.ImapUniqueId);

        var attachmentEntity = message.Attachments.FirstOrDefault(attachment =>
            attachment.ContentDisposition.FileName == attachmentMetadata.FileName);

        return await SaveAttachmentToMemoryAsync(attachmentEntity);
    }

    private async Task<MimeMessage> GetOrDownloadMessageAsync(uint uid, IMailFolder openedFolder = null)
    {
        if (_downloadedMessages.TryGetValue(uid, out var message))
        {
            return message;
        }

        if (openedFolder == null)
        {
            await InitializeImapClientAndConnectIfNeededAsync();
            openedFolder = await OpenFolderAsync();
        }

        var uniqueId = new UniqueId(uid);
        message = await openedFolder.GetMessageAsync(uniqueId);

        _downloadedMessages.Add(uid, message);

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

    private static EmailMessage MapToEmailMessage(MimeMessage message, string folderName, uint uid)
    {
        var emailMessage = new EmailMessage
        {
            Metadata = new EmailMetadata
            {
                GlobalMessageId = message.MessageId,
                Protocol = Protocols.Imap,
                ImapUniqueId = uid,
                FolderName = folderName,
                IsReply = !string.IsNullOrEmpty(message.InReplyTo),
            },
            Header = new EmailHeader
            {
                Subject = message.Subject,
                Sender = CreateEmailAddresses(message.From.Mailboxes).FirstOrDefault(),
                SentDateUtc = message.Date.UtcDateTime,
            },
            Content = new EmailContent
            {
                Body = new EmailBody
                {
                    Body = message.TextBody ?? message.HtmlBody,
                    IsHtml = !string.IsNullOrEmpty(message.HtmlBody),
                },
            },
        };

        emailMessage.Header.To.AddRange(CreateEmailAddresses(message.To.Mailboxes));
        emailMessage.Header.Cc.AddRange(CreateEmailAddresses(message.Cc.Mailboxes));
        emailMessage.Header.Bcc.AddRange(CreateEmailAddresses(message.Bcc.Mailboxes));

        emailMessage.Content.Attachments.AddRange(message.Attachments
            .OfType<MimePart>()
            .Select(attachment => new AttachmentMetadata
            {
                FileName = attachment.FileName,
                MimeType = attachment.ContentType.MimeType,
                Size = attachment.Content.Stream?.Length ?? 0,
            }));

        return emailMessage;
    }

    private static IEnumerable<EmailAddress> CreateEmailAddresses(IEnumerable<MailboxAddress> mailboxes) =>
        mailboxes.Select(m => new EmailAddress { DisplayName = m.Name, Address = m.Address });

    private async Task<IMailFolder> OpenFolderAsync(string folderName = null)
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

    private static async Task<MemoryStream> SaveAttachmentToMemoryAsync(MimeEntity attachment)
    {
        var stream = new MemoryStream();
        if (attachment is MimePart mimePart)
        {
            await mimePart.Content.DecodeToAsync(stream);
        }

        return stream;
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
