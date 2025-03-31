using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using System.Globalization;
using System.Text.RegularExpressions;
using VibPortalApi.Models;
using VibPortalApi.Services;

public class PdfExtractorService : IPdfExtractorService
{
    public VibImport ExtractData(string filePath)
    {
        using var document = PdfDocument.Open(filePath);
        var text = string.Join("\n", document.GetPages().Select(p => p.Text));

        var result = new VibImport
        {
            RevisionDate = ExtractRevisionDate(text),
            Cas_Number = ExtractCasNumbers(text),
            Cas_Percentages = ExtractCasPercentages(text),
            FlashPoint = ExtractFlashPoint(text),

            // The rest will be filled in later
            SupplierNr = "",
            Dimset = "",
            EntryDate = DateTime.Now,
            H_Number = "",
            H_Cat = "",
            Adr_Un_Nr = "",
            Adr_CargoName = "",
            Adr_TransportHazardClass = "",
            Adr_PackagingGroup = "",
            Adr_EnvironmentHazards = "",
            Adr_ExtraInfo = "",
            Imdg_UnNumber = "",
            Imdg_CargoName = "",
            Imdg_TransportHazardClass = "",
            Imdg_PackagingGroup = "",
            Imdg_EnvironmentHazards = "",
            Imdg_ExtraInfo = "",
            ExtraInfoTunnelCode = "",
            Ems_Fire = "",
            Ems_Spillage = "",
            UserName = "",
            EgNumber = "",
            Status = ""
        };

        return result;
    }

    private DateTime? ExtractRevisionDate(string text)
    {
        var match = Regex.Match(text, @"Herzieningsdatum:\s*(\d{2}\.\d{2}\.\d{4})");
        if (match.Success && DateTime.TryParseExact(match.Groups[1].Value, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }
        return null;
    }

    private string ExtractCasNumbers(string text)
    {
        var lines = GetRubriek3Lines(text);
        var casMatches = lines.SelectMany(line => Regex.Matches(line, @"\b\d{2,7}-\d{2}-\d\b").Cast<Match>())
                               .Select(m => m.Value)
                               .Distinct();
        return string.Join(", ", casMatches);
    }

    private string ExtractCasPercentages(string text)
    {
        var lines = GetRubriek3Lines(text);
        var percentMatches = lines.SelectMany(line => Regex.Matches(line, @">=\s*\d+\s*-\s*<\s*\d+").Cast<Match>())
                                   .Select(m => m.Value)
                                   .Distinct();
        return string.Join(", ", percentMatches);
    }

    private string ExtractFlashPoint(string text)
    {
        var match = Regex.Match(text, @"Zelfontbrandingstemperatu\s*:\s*(.+?)\s*(?:\r|\n|$)");
        return match.Success ? match.Groups[1].Value.Trim() : "";
    }

    private List<string> GetRubriek3Lines(string text)
    {
        var lines = text.Split('\n').ToList();
        var startIndex = lines.FindIndex(l => l.Contains("RUBRIEK 3", StringComparison.OrdinalIgnoreCase));
        if (startIndex == -1) return new();

        var sectionLines = new List<string>();
        for (int i = startIndex + 1; i < lines.Count; i++)
        {
            if (Regex.IsMatch(lines[i], @"RUBRIEK \d+", RegexOptions.IgnoreCase)) break; // stop at next section
            sectionLines.Add(lines[i]);
        }

        return sectionLines;
    }
}