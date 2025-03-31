using Microsoft.AspNetCore.Http;

namespace VibPortalApi.Dtos
{
    public class UploadRequest
    {
        public IFormFile File { get; set; } = default!;
        public int? SupplierId { get; set; }
        public string? Product { get; set; }
    }
}