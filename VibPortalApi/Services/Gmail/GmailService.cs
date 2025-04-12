using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.Options;
using VibPortalApi.Models.Gmail;
using VibPortalApi.Models.Settings;
using VibPortalApi.Services.Gmail;

public class GmailService : IGmailService
{
    private readonly GmailSettings _settings;

    public GmailService(IOptions<GmailSettings> settings)
    {
        _settings = settings.Value;
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

            var messages = summaries
                .Reverse()
                .Select(summary =>
                {
                    var messageId = summary.Envelope?.MessageId;
                    var fallbackUid = summary.UniqueId.Id.ToString();

                    return new GMessage
                    {
                        Date = summary.InternalDate?.DateTime,
                        From = summary.Envelope?.From?.Mailboxes?.FirstOrDefault()?.Address ?? string.Empty,
                        To = string.Join(", ", summary.Envelope?.To?.Mailboxes?.Select(m => m.Address) ?? Enumerable.Empty<string>()),
                        Subject = summary.Envelope?.Subject ?? "(no subject)",
                        IsRead = summary.Flags?.HasFlag(MessageFlags.Seen) ?? false,
                        Attachments = GetAttachmentNames(summary),
                        Status = "unread",
                        ProcessId = 0,
                        GmailId = messageId ?? fallbackUid
                    };
                });

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.ToLowerInvariant();
                messages = messages.Where(m =>
                    m.Subject.ToLowerInvariant().Contains(keyword) ||
                    m.From.ToLowerInvariant().Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                messages = messages.Where(m => m.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            result.Data = messages.ToList();
            result.TotalCount = result.Data.Count;
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
}
