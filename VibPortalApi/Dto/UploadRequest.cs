namespace VibPortalApi.Dtos
{
    public class UploadRequest
    {
        public IFormFile File { get; set; } = default!;
        public string? SupplierCode { get; set; }
        public string? ProductCode { get; set; }
        public string? Dimset { get; set; }
        public string? Recipe { get; set; }
    }
}