namespace VibPortalApi.Services.B2B
{
    public interface IB2bPdfExtractorFactory
    {
        IB2bPdfExtractor GetExtractor(string filePath, string supplierCode);
    }
}
