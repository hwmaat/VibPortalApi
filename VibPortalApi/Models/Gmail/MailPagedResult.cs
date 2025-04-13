namespace VibPortalApi.Models.Gmail;

public class MailPagedResult<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public List<T> Data { get; set; } = new();
    public string Status { get; set; } = "success"; // "success" or "failed"
    public string? Message { get; set; } = "";
}
