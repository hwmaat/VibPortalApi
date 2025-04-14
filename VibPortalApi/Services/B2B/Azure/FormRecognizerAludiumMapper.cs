using Azure.AI.FormRecognizer.DocumentAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using VibPortalApi.Models.B2B;

namespace VibPortalApi.Services.B2B.Azure
{
    public class FormRecognizerAludiumMapper : IB2BFormRecognizerMapper
    {
        public B2BParsedOcData Map(AnalyzeResult result)
        {
            var parsed = new B2BParsedOcData
            {
                Supplier_Nr = "ALUDIUM"
            };

            parsed.OrderNr = GetKeyValue(result, "Order number");
            parsed.CustomerNr = GetKeyValue(result, "Customer number");

            var orderDateStr = GetKeyValue(result, "Order creation date");
            if (DateTime.TryParseExact(orderDateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var orderDate))
                parsed.OrderDate = orderDate;

            var posValues = result.KeyValuePairs
                .Where(kv => kv.Key?.Content?.Trim() == "Pos")
                .Select(kv => kv.Value?.Content?.Trim())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToList();

            foreach (var pos in posValues)
            {
                var line = new B2BParsedOcLine
                {
                    Line = ParseLineNumber(pos),
                    EuramaxPo_Nr = GetKeyValue(result, "Your order no."),
                    SupplierPartNr = GetKeyValue(result, "Our part no."),
                    Dimset = GetDimsetFromSpec(result),
                    Specification = GetGlobalSpecification(result),
                    Quantity_Kg = ParseDecimal(GetKeyValue(result, "Quantity (kg)")),
                    Price_t = ParseDecimal(GetKeyValue(result, "Price EUR/t")),
                    Currency = "EUR"
                };

                parsed.Lines.Add(line);
            }

            parsed.EuramaxPo_Nr = GetKeyValue(result, "Your order no.");
            return parsed;
        }

        private static string GetKeyValue(AnalyzeResult result, string key) =>
            result.KeyValuePairs.FirstOrDefault(kv => kv.Key?.Content?.Trim() == key)?.Value?.Content?.Trim() ?? "";

        private static string GetGlobalSpecification(AnalyzeResult result) =>
            result.KeyValuePairs.FirstOrDefault(kv => kv.Key?.Content?.Contains("Global specification") == true)?.Key?.Content?.Trim() ?? "";

        private static string GetDimsetFromSpec(AnalyzeResult result)
        {
            var spec = GetGlobalSpecification(result);
            var dimMatch = Regex.Match(spec, @"\|\s*([\d.]+\s*x\s*[\d.]+)");
            return dimMatch.Success ? dimMatch.Groups[1].Value.Trim() : "";
        }

        private static decimal? ParseDecimal(string value) =>
            decimal.TryParse(value, NumberStyles.Any, new CultureInfo("nl-NL"), out var dec) ? dec : null;

        private static int ParseLineNumber(string value) =>
            int.TryParse(value.Split('.').FirstOrDefault(), out var num) ? num : 0;
    }
}
