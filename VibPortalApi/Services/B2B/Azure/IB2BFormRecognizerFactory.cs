namespace VibPortalApi.Services.B2B.Azure
{
    public interface IB2BFormRecognizerFactory
    {
        IB2BFormRecognizerMapper GetMapper(string supplierCode);
    }
}
