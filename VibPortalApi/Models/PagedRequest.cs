namespace VibPortalApi.Models
{
    public class PagedRequest
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SortColumn { get; set; } = string.Empty;
        public string SortDirection { get; set; } = "asc";
        public string? Filter { get; set; }
        public string? Status { get; set; }
    }
}
