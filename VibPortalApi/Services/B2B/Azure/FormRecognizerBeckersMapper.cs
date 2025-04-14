using Azure.AI.FormRecognizer.DocumentAnalysis;
using VibPortalApi.Models.B2B;

namespace VibPortalApi.Services.B2B.Azure
{
    public class FormRecognizerBeckersMapper : IB2BFormRecognizerMapper
    {
        public B2BParsedOcData Map(AnalyzeResult result)
        {
            var parsed = new B2BParsedOcData
            {
                Supplier_Nr = "BECKERS"
            };

            // Basic fallback parser — you can enhance this based on your Beckers PDF layout
            parsed.OrderNr = GetKeyValue(result, "Order number");
            parsed.CustomerNr = GetKeyValue(result, "Customer number");

            var poNumber = GetKeyValue(result, "Your order no.");
            parsed.EuramaxPo_Nr = poNumber;

            var posValues = result.KeyValuePairs
                .Where(kv => kv.Key?.Content?.Trim() == "Pos")
                .Select(kv => kv.Value?.Content?.Trim())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToList();

            foreach (var pos in posValues)
            {
                parsed.Lines.Add(new B2BParsedOcLine
                {
                    Line = int.TryParse(pos.Split('.').FirstOrDefault(), out var num) ? num : 0,
                    EuramaxPo_Nr = poNumber,
                    SupplierPartNr = GetKeyValue(result, "Our part no."),
                    Dimset = "",
                    Specification = "", // You can enhance this later
                    Quantity_Kg = ParseDecimal(GetKeyValue(result, "Quantity (kg)")),
                    Price_t = ParseDecimal(GetKeyValue(result, "Price EUR/t")),
                    Currency = "EUR"
                });
            }

            return parsed;
        }

        private static string GetKeyValue(AnalyzeResult result, string key)
        {
            return result.KeyValuePairs
                .Where(kv => kv.Key?.Content?.Trim() == key)
                .Select(kv => kv.Value?.Content?.Trim())
                .FirstOrDefault() ?? "";
        }

        private static decimal? ParseDecimal(string value)
        {
            return decimal.TryParse(value, System.Globalization.NumberStyles.Any, new System.Globalization.CultureInfo("nl-NL"), out var dec)
                ? dec
                : null;
        }
    }
}
