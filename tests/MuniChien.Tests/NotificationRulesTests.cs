using MuniChien.App.Services;
using Xunit;

namespace MuniChien.Tests;

public class NotificationRulesTests
{
    [Fact]
    public void Expiration_passee_signale_une_licence_expiree()
        => Assert.Equal(LicenseAlertKind.Expired,
            NotificationRules.GetLicenseAlert(new DateTime(2026, 10, 29), new DateTime(2026, 10, 30)));

    [Fact]
    public void Expiration_dans_30_jours_declenche_une_alerte()
        => Assert.Equal(LicenseAlertKind.Upcoming,
            NotificationRules.GetLicenseAlert(new DateTime(2026, 11, 29), new DateTime(2026, 10, 30)));

    [Fact]
    public void Expiration_dans_31_jours_ne_declenche_aucune_alerte()
        => Assert.Equal(LicenseAlertKind.None,
            NotificationRules.GetLicenseAlert(new DateTime(2026, 11, 30), new DateTime(2026, 10, 30)));

    [Fact]
    public void Licence_expirant_aujourdhui_est_signalee()
        => Assert.Equal(LicenseAlertKind.Upcoming,
            NotificationRules.GetLicenseAlert(new DateTime(2026, 10, 30), new DateTime(2026, 10, 30)));

    [Fact]
    public void Heure_de_la_journee_na_pas_dincidence()
        => Assert.Equal(LicenseAlertKind.Upcoming,
            NotificationRules.GetLicenseAlert(new DateTime(2026, 10, 30, 0, 1, 0),
                new DateTime(2026, 10, 30, 23, 59, 0)));

    [Fact]
    public void Delai_negatif_est_refuse()
        => Assert.Throws<ArgumentOutOfRangeException>(() =>
            NotificationRules.GetLicenseAlert(DateTime.Today, DateTime.Today, -1));
    [Fact]
    public void Notifications_sont_triees_de_la_plus_recente_a_la_plus_ancienne()
    {
        var dates = new[]
        {
            new DateTime(2024, 12, 31),
            new DateTime(2026, 10, 30),
            new DateTime(2026, 1, 1)
        };
        var sorted = NotificationRules.OrderNewestFirst(dates, d => d).ToArray();
        Assert.Equal(new DateTime(2026, 10, 30), sorted[0]);
        Assert.Equal(new DateTime(2026, 1, 1), sorted[1]);
        Assert.Equal(new DateTime(2024, 12, 31), sorted[2]);
    }

    [Fact]
    public void Tri_ne_modifie_pas_la_liste_originale()
    {
        var original = new[] { new DateTime(2025, 1, 1), new DateTime(2026, 1, 1) };
        _ = NotificationRules.OrderNewestFirst(original, d => d).ToArray();
        Assert.Equal(new DateTime(2025, 1, 1), original[0]);
    }
}
