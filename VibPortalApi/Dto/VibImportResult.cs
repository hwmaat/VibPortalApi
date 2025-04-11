namespace VibPortalApi.Dtos
{
    public class VibImportResult
    {
        public bool Success { get; set; }
        public string? ExtractedText { get; set; }
        public string? ErrorMessage { get; set; }
        public int? VibId { get; set; }

    }
}
