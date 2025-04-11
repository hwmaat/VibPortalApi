using System.Collections.Generic;

namespace VibPortalApi.Models
{
    public class VibPagedResult<T>
    {
        public List<T> Records { get; set; } = new();
        public int TotalRecords { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string Status { get; set; } = "success"; // or "failed"
        public string? Message { get; set; } = "";
    }
}