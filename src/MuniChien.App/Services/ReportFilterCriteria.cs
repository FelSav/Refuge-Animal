using System.Globalization;
using System.Text;

namespace MuniChien.App.Services;

/// <summary>Filtres appliqués AVANT la production des lignes et des totaux.</summary>
public enum ReportKind
{
    Dogs, Clients, UnpaidBalances, LateFees, Payments, Revenue,
    DailyPayments, Breeds, Licenses
}

public sealed class ReportFilterCriteria
{
    public const string AllMunicipalities = "Toutes les municipalités";
    public const string AllBreeds = "Toutes les races";
    public const string AllSexes = "Tous les sexes";
    public const string AllStatuses = "Tous les statuts";
    public const string AllSterilizations = "Tous";
    public const string AllMethods = "Tous les modes";

    public string Municipality { get; set; } = AllMunicipalities;
    public string SearchText { get; set; } = string.Empty;
    public string Breed { get; set; } = AllBreeds;
    public string Sex { get; set; } = AllSexes;
    public string Sterilized { get; set; } = AllSterilizations;
    public string LicenseStatus { get; set; } = AllStatuses;
    public string PaymentMethod { get; set; } = AllMethods;
    public string Period { get; set; } = "Toutes";
    public DateTime Day { get; set; } = DateTime.Today;
    public decimal? MinimumAmount { get; set; }
    public string SortBy { get; set; } = string.Empty;
    public bool Descending { get; set; }

    public string Describe(ReportKind kind)
    {
        var items = new List<string>();
        if (!string.IsNullOrWhiteSpace(SearchText)) items.Add($"Recherche : {SearchText.Trim()}");
        if (Breed != AllBreeds && (kind is ReportKind.Dogs or ReportKind.Clients or ReportKind.Breeds or ReportKind.Licenses))
            items.Add($"Race : {Breed}");
        if (Sex != AllSexes && (kind is ReportKind.Dogs or ReportKind.Breeds or ReportKind.Licenses))
            items.Add($"Sexe : {Sex}");
        if (Sterilized != AllSterilizations && (kind is ReportKind.Dogs or ReportKind.Breeds or ReportKind.Licenses))
            items.Add($"Stérilisé : {Sterilized}");
        if (LicenseStatus != AllStatuses && (kind is ReportKind.Dogs or ReportKind.Clients or ReportKind.Breeds or ReportKind.Licenses))
            items.Add($"Licence : {LicenseStatus}");
        if (kind is ReportKind.Payments or ReportKind.Revenue)
            items.Add($"Période : {Period}");
        if (kind == ReportKind.Licenses)
            items.Add($"Émission : {Period}");
        if (kind == ReportKind.DailyPayments)
            items.Add($"Date : {Day:yyyy-MM-dd}");
        if ((kind is ReportKind.Payments or ReportKind.Revenue or ReportKind.DailyPayments) && PaymentMethod != AllMethods)
            items.Add($"Mode : {PaymentMethod}");
        if (MinimumAmount.HasValue)
            items.Add($"Montant minimum : {MinimumAmount.Value:0.00} $");
        if (!string.IsNullOrWhiteSpace(SortBy))
            items.Add($"Tri : {SortBy} ({(Descending ? "décroissant" : "croissant")})");
        return items.Count == 0 ? "Aucun filtre supplémentaire" : string.Join(" · ", items);
    }
}

public static class ReportFilterRules
{
    public static bool Choice(string? value, string? selected, string allLabel) =>
        string.IsNullOrWhiteSpace(selected) || selected == allLabel ||
        string.Equals(value?.Trim(), selected.Trim(), StringComparison.OrdinalIgnoreCase);

    public static bool Search(string? query, params string?[] values)
    {
        if (string.IsNullOrWhiteSpace(query)) return true;
        string text = Normalize(query);
        string digits = Digits(query);
        return values.Any(value => Normalize(value).Contains(text, StringComparison.Ordinal) ||
            (digits.Length >= 3 && Digits(value).Contains(digits, StringComparison.Ordinal)));
    }

    public static bool Period(DateTime date, string? period, DateTime today)
    {
        var day = date.Date;
        today = today.Date;
        return period switch
        {
            "Aujourd'hui" => day == today,
            "7 derniers jours" => day >= today.AddDays(-6) && day <= today,
            "Ce mois" => day.Year == today.Year && day.Month == today.Month,
            "Cette année" => day.Year == today.Year,
            _ => true
        };
    }

    public static bool Method(bool cheque, string? selected) =>
        selected is null or "Tous les modes" ||
        (selected == "Chèque" && cheque) ||
        (selected == "Autre mode" && !cheque);

    public static bool Minimum(decimal amount, decimal? minimum) =>
        minimum is null || amount >= minimum.Value;

    private static string Digits(string? value) =>
        new((value ?? string.Empty).Where(char.IsDigit).ToArray());

    private static string Normalize(string? value)
    {
        var normalized = (value ?? string.Empty).Trim().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (char c in normalized)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(char.ToLowerInvariant(c));
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
