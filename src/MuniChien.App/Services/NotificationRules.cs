namespace MuniChien.App.Services;

/// <summary>Règles de dates indépendantes de WPF, réutilisables côté API.</summary>
public static class NotificationRules
{
    /// <summary>Classe les alertes du plus récent événement au plus ancien, sans changer la source.</summary>
    public static IOrderedEnumerable<T> OrderNewestFirst<T>(IEnumerable<T> items, Func<T, DateTime> eventDate)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(eventDate);
        return items.OrderByDescending(eventDate);
    }

    public static LicenseAlertKind GetLicenseAlert(DateTime expiration, DateTime today, int warningDays = 30)
    {
        if (warningDays < 0) throw new ArgumentOutOfRangeException(nameof(warningDays));
        int days = (expiration.Date - today.Date).Days;
        if (days < 0) return LicenseAlertKind.Expired;
        return days <= warningDays ? LicenseAlertKind.Upcoming : LicenseAlertKind.None;
    }
}

public enum LicenseAlertKind { None, Upcoming, Expired }
