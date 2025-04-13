namespace VibPortalApi.Services.Euravib
{
    public interface IPdfExtractorFactory
    {
        IPdfExtractorService GetExtractor(string filePath, string supplierCode);
    }

}
