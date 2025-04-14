using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Linq;
using VibPortalApi.Data;
using VibPortalApi.Models.B2B;
using VibPortalApi.Models.Gmail;
using VibPortalApi.Models.Settings;
using VibPortalApi.Services.B2B;
using VibPortalApi.Services.Gmail;

public class GmailService : IGmailService
{
    private readonly GmailSettings _settings;
    private readonly AppSettings _appSettings;
    private readonly IB2bPdfExtractorFactory _b2bPdfExtractorFactory;
    private readonly AppDbContext _appDbContext;
    private readonly IB2BImportOc _b2bImportOc;

    public GmailService(IOptions<GmailSettings> settings, IOptions<AppSettings> appSettings, 
        IB2bPdfExtractorFactory b2bPdfExtractorFactory, AppDbContext appDbContext, IB2BImportOc b2bImportOc)
    {
        _settings = settings.Value;
        _appSettings = appSettings.Value;
        _b2bPdfExtractorFactory = b2bPdfExtractorFactory;
        _appDbContext = appDbContext;
        _b2bImportOc = b2bImportOc;
    }

    public async Task<B2BProcessResult> ProcessB2BEmailAsync(string gmailId, string supplierCode, string attachmentName)
    {
        var result = new B2BProcessResult { GmailId = gmailId };

        try
        {
            var b2bPath = _appSettings.B2BPath;
            if (string.IsNullOrEmpty(b2bPath))
                throw new Exception("B2B path is not configured in AppSettings");

            using var client = new ImapClient();
            await client.ConnectAsync("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(_settings.Email, _settings.Password);

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly);

            MimeMessage? message = null;

            var headerQuery = SearchQuery.HeaderContains("Message-Id", gmailId);
            var headerMatches = await inbox.SearchAsync(headerQuery);

            if (headerMatches.Count > 0)
            {
                message = await inbox.GetMessageAsync(headerMatches[0]);
            }
            else if (uint.TryParse(gmailId, out var uidValue))
            {
                var uid = new UniqueId(uidValue);
                var uidMatches = await inbox.SearchAsync(SearchQuery.Uids(new[] { uid }));
                if (uidMatches.Count > 0)
                {
                    message = await inbox.GetMessageAsync(uid);
                }
            }

            if (message == null)
                throw new Exception($"Email with GmailId '{gmailId}' not found.");

            var part = message.Attachments
                .OfType<MimePart>()
                .FirstOrDefault(a =>
                    a.FileName != null &&
                    a.FileName.Equals(attachmentName, StringComparison.OrdinalIgnoreCase) &&
                    a.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));

            if (part == null)
                throw new Exception($"Attachment '{attachmentName}' not found in message.");


            //----------------------------------------------------------------
            using var memory = new MemoryStream();
            await part.Content.DecodeToAsync(memory);
            memory.Position = 0;

            var b2bResult = await _b2bImportOc.B2BProcessOcAsync(memory, gmailId, attachmentName, supplierCode);

            result.Success = b2bResult.Success;
            result.Status = b2bResult.Status;
            result.ErrorMessage = b2bResult.ErrorMessage;
            //----------------------------------------------------------------

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.Status = "failed";
            return result;
        }
    }


    public async Task<MailPagedResult<GMessage>> GetMessagesPagedAsync(int page, int pageSize, string? search, string? status)
    {
        var result = new MailPagedResult<GMessage>
        {
            PageNumber = page,
            PageSize = pageSize,
            Status = "success"
        };

        using var client = new ImapClient();
        try
        {
            await client.ConnectAsync("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(_settings.Email, _settings.Password);

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly);

            int total = inbox.Count;
            result.TotalCount = total;

            if (total == 0)
                return result;

            int endIndex = total - ((page - 1) * pageSize) - 1;
            int startIndex = Math.Max(0, endIndex - pageSize + 1);

            if (endIndex < 0 || startIndex > endIndex)
                return result;

            var summaries = await inbox.FetchAsync(startIndex, endIndex,
                MessageSummaryItems.Envelope |
                MessageSummaryItems.Flags |
                MessageSummaryItems.InternalDate |
                MessageSummaryItems.BodyStructure);

            var expandedMessages = summaries
                .Reverse()
                .SelectMany(summary =>
                {
                    var messageId = summary.Envelope?.MessageId;
                    var fallbackUid = summary.UniqueId.Id.ToString();

                    var baseMessage = new GMessage
                    {
                        Date = summary.InternalDate?.DateTime,
                        From = summary.Envelope?.From?.Mailboxes?.FirstOrDefault()?.Address ?? string.Empty,
                        To = string.Join(", ", summary.Envelope?.To?.Mailboxes?.Select(m => m.Address) ?? Enumerable.Empty<string>()),
                        Subject = summary.Envelope?.Subject ?? "(no subject)",
                        IsRead = summary.Flags?.HasFlag(MessageFlags.Seen) ?? false,
                        Status = "new",
                        ProcessId = 0,
                        GmailId = messageId ?? fallbackUid
                    };

                    var attachmentList = GetAttachmentNamesList(summary);

                    // No attachments: return one line
                    if (attachmentList.Count == 0)
                    {
                        baseMessage.Attachments = string.Empty;
                        return new[] { baseMessage };
                    }

                    // One row per attachment
                    return attachmentList.Select(name => new GMessage
                    {
                        Date = baseMessage.Date,
                        From = baseMessage.From,
                        To = baseMessage.To,
                        Subject = baseMessage.Subject,
                        IsRead = baseMessage.IsRead,
                        Status = baseMessage.Status,
                        ProcessId = baseMessage.ProcessId,
                        GmailId = baseMessage.GmailId,
                        Attachments = name
                    });
                }).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.ToLowerInvariant();
                expandedMessages = expandedMessages
                   .Where(m => m.Subject.ToLowerInvariant().Contains(keyword) || m.From.ToLowerInvariant().Contains(keyword))
                   .ToList();
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                expandedMessages = expandedMessages.Where(m =>
                    m.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Create a lookup for all attachment/gmail combinations
            var keys = expandedMessages
                .Select(m => new { m.Attachments, m.GmailId })
                .Where(x => !string.IsNullOrWhiteSpace(x.Attachments) && !string.IsNullOrWhiteSpace(x.GmailId))
                .ToList();

            if (keys.Any())
            {
                var attachmentNames = keys.Select(k => k.Attachments!).ToList();
                var gmailIds = keys.Select(k => k.GmailId!).ToList();

                var existingRecords = await _appDbContext.B2BSupplierOcs
                    .Where(x => gmailIds.Contains(x.GmailId!) && attachmentNames.Contains(x.AttachtmentName!))
                    .ToListAsync();

                foreach (var msg in expandedMessages)
                {
                    var match = existingRecords.FirstOrDefault(r =>
                        r.AttachtmentName == msg.Attachments && r.GmailId == msg.GmailId);
                    //if a record is there: seen, otherwise 'todo'
                    msg.Status = match?.Status?? "new";
                }
            }

            result.Data = expandedMessages.ToList();
        }
        catch (Exception ex)
        {
            result.Status = "failed";
            result.Message = ex.Message;
        }
        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true);
        }

        return result;
    }

    private static string GetAttachmentNames(IMessageSummary summary)
    {
        if (summary.BodyParts == null)
            return string.Empty;

        var attachments = summary.BodyParts
            .Where(part => part.IsAttachment)
            .Select(part => part.ContentDisposition?.FileName ?? part.PartSpecifier)
            .ToList();

        return string.Join(", ", attachments);
    }
    private static List<string> GetAttachmentNamesList(IMessageSummary summary)
    {
        if (summary.BodyParts == null)
            return new List<string>();

        return summary.BodyParts
            .Where(part => part.IsAttachment)
            .Select(part => part.ContentDisposition?.FileName ?? part.PartSpecifier)
            .ToList();
    }
}
