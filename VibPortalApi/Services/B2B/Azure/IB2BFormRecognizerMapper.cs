using Azure.AI.FormRecognizer.DocumentAnalysis;
using VibPortalApi.Models.B2B;

namespace VibPortalApi.Services.B2B.Azure
{
    public interface IB2BFormRecognizerMapper
    {
        B2BParsedOcData Map(AnalyzeResult result);
    }
}
