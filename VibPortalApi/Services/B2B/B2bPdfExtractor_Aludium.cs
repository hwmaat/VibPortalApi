using System.Globalization;
using System.Text.RegularExpressions;
using VibPortalApi.Models.B2B;

namespace VibPortalApi.Services.B2B.Extractors
{
    public class B2bPdfExtractor_Aludium : IB2bPdfExtractor
    {
        public Task<B2BParsedOcData> ExtractTextAsync(string filePath)
        {
            var text = PdfTextExtractorUtil.ExtractTextFromPdf(filePath);
            var lines = text.Split('\n').Select(l => l.Trim()).ToList();

            var parsed = new B2BParsedOcData
            {
                Supplier_Nr = " 00116"
            };

            // Extract header-level fields
            foreach (var line in lines)
            {
                if (line.StartsWith("10.1") || line.StartsWith("Pos"))
                {
                    var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length >= 2 && int.TryParse(tokens[1], out _))
                    {
                        parsed.EuramaxPo_Nr = tokens[1];
                        break;
                    }
                }
            }

            var orderMatch = Regex.Match(text, @"Order Confirmation no\.*\s*(\d+)");
            if (orderMatch.Success)
                parsed.OrderNr = orderMatch.Groups[1].Value;

            var customerMatch = Regex.Match(text, @"Customer no\.*\s*(\d+)");
            if (customerMatch.Success)
                parsed.CustomerNr = customerMatch.Groups[1].Value;

            var dateMatch = Regex.Match(text, @"\b(\d{2}/\d{2}/\d{4})\b");
            if (dateMatch.Success && DateTime.TryParseExact(dateMatch.Groups[1].Value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                parsed.OrderDate = date;

            // Extract line items
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].StartsWith("Pos "))
                {
                    var lineMatch = Regex.Match(lines[i], @"Pos\s+(\d+\.\d+)");
                    var quantityMatch = Regex.Match(lines[i], @"\b(\d{1,3}(?:\.\d{3})*,\d{2})\b");
                    var currencyMatch = Regex.Match(lines[i], @"\bEUR|USD\b");

                    string supplierPartNr = "";
                    string dimset = "";
                    string specification = "";

                    for (int j = i + 1; j < Math.Min(lines.Count, i + 10); j++)
                    {
                        if (lines[j].StartsWith("Your part no."))
                        {
                            supplierPartNr = lines[j].Replace("Your part no.", "").Trim();
                            if (j + 1 < lines.Count)
                                dimset = lines[j + 1].Trim();
                        }
                        else if (lines[j].Contains("MF") && lines[j].Contains("|"))
                        {
                            specification = lines[j].Trim();
                        }
                    }

                    if (lineMatch.Success)
                    {
                        var parsedLine = new B2BParsedOcLine
                        {
                            Line = TryParseLineNumber(lineMatch.Groups[1].Value),
                            SupplierPartNr = supplierPartNr,
                            Dimset = dimset,
                            Specification = specification,
                            EuramaxPo_Nr = parsed.EuramaxPo_Nr // <-- set here as well
                        };

                        if (quantityMatch.Success && decimal.TryParse(quantityMatch.Groups[1].Value, NumberStyles.Any, new CultureInfo("nl-NL"), out var qty))
                            parsedLine.Quantity_Kg = qty;

                        if (quantityMatch.Success && decimal.TryParse(quantityMatch.Groups[1].Value, NumberStyles.Any, new CultureInfo("nl-NL"), out var price))
                            parsedLine.Price_t = price;

                        parsedLine.Currency = currencyMatch.Success ? currencyMatch.Value : "EUR";

                        parsed.Lines.Add(parsedLine);
                    }
                }
            }

            return Task.FromResult(parsed);
        }

        private int TryParseLineNumber(string value)
        {
            if (value.Contains('.'))
            {
                var parts = value.Split('.');
                if (int.TryParse(parts[0], out var baseNum))
                {
                    return baseNum;
                }
            }

            return int.TryParse(value, out var num) ? num : 0;
        }
    }
}
