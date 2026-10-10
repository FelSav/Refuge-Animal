using System.Globalization;
using System.Text.Json.Serialization;

namespace MuniChien.App.Services;

/// <summary>
/// Tarif configuré manuellement pour une municipalité, une année et un libellé.
/// Aucune somme n'est appliquée automatiquement aux comptes de paiement.
/// </summary>
public sealed record LicenseTariffEntry
{
    public Guid Id { get; init; }
    public string Municipality { get; init; } = string.Empty;
    public int Year { get; init; }
    public string Label { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonIgnore]
    public string AmountDisplay => Amount.ToString("N2", CultureInfo.GetCultureInfo("fr-CA")) + " $";
    [JsonIgnore]
    public string UpdatedAtDisplay => UpdatedAt.LocalDateTime.ToString("yyyy-MM-dd HH:mm");
}
