namespace VibPortalApi.Services
{
    public interface IPdfExtractorFactory
    {
        IPdfExtractorService GetExtractor(string supplierCode);
    }

}
