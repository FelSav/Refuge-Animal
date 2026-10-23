using MuniChien.App.Services;

namespace MuniChien.Tests;

public class LocalLicenseStatusRulesTests
{
    [Theory]
    [InlineData("Valide", "2026-12-31", "2027-01-01", "Expirée")]
    [InlineData("À renouveler", "2026-12-31", "2027-01-01", "Expirée")]
    [InlineData("Valide", "2026-12-31", "2026-12-31", "Valide")]
    [InlineData("À renouveler", "2026-12-31", "2026-10-23", "À renouveler")]
    [InlineData("Expirée", "2027-12-31", "2027-01-01", "Valide")]
    [InlineData("Suspendue", "2025-12-31", "2026-10-23", "Suspendue")]
    public void ResolvesStatusWithoutInventingRenewalThreshold(
        string original, string expiry, string today, string expected)
    {
        var expirationDate = DateTime.ParseExact(expiry, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        var currentDate = DateTime.ParseExact(today, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(expected, LocalLicenseStatusRules.Resolve(expirationDate, original, currentDate));
    }
}
