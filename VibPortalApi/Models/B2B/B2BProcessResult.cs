namespace VibPortalApi.Models.B2B
{
    public class B2BProcessResult
    {
        public string GmailId { get; set; } = string.Empty;

        public bool Success { get; set; }

        public string? ErrorMessage { get; set; }
        public string? Status { get; set; } = string.Empty;

    }
}
