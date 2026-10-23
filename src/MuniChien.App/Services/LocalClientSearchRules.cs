using System.Globalization;
using System.Text;

namespace MuniChien.App.Services;

/// <summary>Critères partagés des sélecteurs clients, indépendants de WPF et testables.</summary>
public static class LocalClientSearchRules
{
    public static string DigitsOnly(string? value) =>
        new((value ?? string.Empty).Where(char.IsDigit).ToArray());

    public static string NormalizeWords(string? value)
    {
        var normalized = (value ?? string.Empty).Normalize(NormalizationForm.FormD);
        return new string(normalized
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray()).ToUpperInvariant();
    }

    public static bool Matches(string? query, string name, string? phone, string? cellPhone, string? fileNumber)
    {
        var search = (query ?? string.Empty).Trim();
        if (search.Length < 2) return false;

        var words = NormalizeWords(search);
        var digits = DigitsOnly(search);
        var looksLikePhoneOrNumber = digits.Length >= 2 && search.All(c =>
            char.IsDigit(c) || c is ' ' or '-' or '(' or ')' or '+' or '.');

        if (NormalizeWords(name).Contains(words, StringComparison.OrdinalIgnoreCase)) return true;
        if ((fileNumber ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)) return true;
        if (!looksLikePhoneOrNumber) return false;

        return DigitsOnly(phone).Contains(digits, StringComparison.Ordinal) ||
               DigitsOnly(cellPhone).Contains(digits, StringComparison.Ordinal);
    }
}
