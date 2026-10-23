namespace MuniChien.App.Services;

/// <summary>
/// La date d'expiration l'emporte sur les statuts de démonstration.
/// "À renouveler" demeure un statut explicitement marqué par le refuge :
/// aucun délai de rappel automatique n'est supposé ici.
/// </summary>
public static class LocalLicenseStatusRules
{
    public static string Resolve(DateTime expirationDate, string? recordedStatus, DateTime currentDate)
    {
        string status = recordedStatus?.Trim() ?? string.Empty;
        bool knownStatus = status is "Valide" or "À renouveler" or "Expirée";
        if (!knownStatus)
            return status; // Ne pas écraser de futurs statuts métier.

        if (expirationDate.Date < currentDate.Date)
            return "Expirée";

        // Une licence encore couverte n'est pas expirée; après renouvellement,
        // le statut redevient Valide sauf si un rappel était déjà demandé.
        return status == "Expirée" ? "Valide" : status;
    }
}
