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
            Rev_Date = ExtractRevisionDate(text),
            Cas_Nr = ExtractCasNumbers(text),
            Cas_Perc = ExtractCasPercentages(text),
            FlashPoint = ExtractFlashPoint(text),

            // The rest will be filled in later
            Suppl_Nr = "",
            Dimset = "",
            Entry_Date = DateTime.Now,
            H_Nr = "",
            H_Cat = "",
            Adr_Un_Nr = "",
            Adr_Cargo_Name = "",
            Adr_TransportHazard_Class = "",
            Adr_Packaging_Group = "",
            Adr_Environment_Hazards = "",
            Adr_ExtraInfo = "",
            Imdg_Un_Nr = "",
            Imdg_Cargo_Name = "",
            Imdg_TransportHazard_Class = "",
            Imdg_Packaging_Group = "",
            Imdg_Environment_Hazards = "",
            Imdg_ExtraInfo = "",
            ExtraInfo_TunnelCode = "",
            Ems_Fire = "",
            Ems_Spillage = "",
            UserName = "",
            Eg_Nr = "",
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