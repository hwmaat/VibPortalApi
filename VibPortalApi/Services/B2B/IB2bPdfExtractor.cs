namespace VibPortalApi.Services.B2B
{
    public interface IB2bPdfExtractor
    {
        Task<string> ExtractTextAsync(string filePath);
    }
}
