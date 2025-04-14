using VibPortalApi.Models.B2B;
using VibPortalApi.Models.Gmail;

namespace VibPortalApi.Services.Gmail;

public interface IGmailService
{
    Task<MailPagedResult<GMessage>> GetMessagesPagedAsync(int page, int pageSize, string? search, string? status);
    Task<B2BProcessResult> ProcessB2BEmailAsync(string gmailId, string supplierCode, string AttachmentName);
}