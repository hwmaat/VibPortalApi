public class PagedResult<T>
{
    public int TotalCount { get; set; }
    public List<T> Data { get; set; } = new();
}
