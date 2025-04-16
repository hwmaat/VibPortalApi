namespace VibPortalApi.Models.Gmail;

public class B2BGMessage
{
    public DateTime? Date { get; set; }
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Attachments { get; set; } = string.Empty;
    public bool IsRead { get; set; }

    // New fields
    public string Status { get; set; } = "new";
    public int ProcessId { get; set; }
    public string GmailId { get; set; } = string.Empty;
}
