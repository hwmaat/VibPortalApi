namespace VibPortalApi.Models.Gmail
{
    public class ProcessEmailRequest
    {
        public string MessageId { get; set; } = string.Empty;
        public string SupplierCode { get; set; } = string.Empty;
        public string AttachmentName { get; set; } = string.Empty;
    }
}
