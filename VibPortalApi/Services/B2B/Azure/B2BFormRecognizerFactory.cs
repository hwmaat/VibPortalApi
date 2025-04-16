namespace VibPortalApi.Services.B2B.Azure
{


    public class B2BFormRecognizerFactory : IB2BFormRecognizerFactory
    {
        private readonly IServiceProvider _provider;

        public B2BFormRecognizerFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public IB2BFormRecognizerMapper GetMapper(string supplierCode)
        {
            return supplierCode.ToLower() switch
            {
                "aludium" => _provider.GetRequiredService<FormRecognizerAludiumMapper>(),
                "beckers" => _provider.GetRequiredService<FormRecognizerBeckersMapper>(),
                _ => throw new NotSupportedException($"No mapper available for supplier: {supplierCode}")
            };
        }
    }
}
