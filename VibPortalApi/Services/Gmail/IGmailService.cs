using VibPortalApi.Models.Gmail;

namespace VibPortalApi.Services.Gmail;

public interface IGmailService
{
    Task<MailPagedResult<GMessage>> GetMessagesPagedAsync(int page, int pageSize, string? search, string? status);
}