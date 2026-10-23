namespace MuniChien.App.Services;

// Validation commune à la création et à la modification d'un client.
// La municipalité est facultative pour les dossiers de fidélité sans chien.
internal static class LocalClientValidation
{
    public static void Validate(OwnerDirectoryItem owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        if (string.IsNullOrWhiteSpace(owner.FirstName))
            throw new ArgumentException("Le prénom du client est obligatoire.");
        if (string.IsNullOrWhiteSpace(owner.LastName))
            throw new ArgumentException("Le nom du client est obligatoire.");

        string municipality = owner.Municipality?.Trim() ?? string.Empty;
        if (municipality.Length > 0 && !MunicipalityCatalog.All.Contains(municipality))
            throw new ArgumentException("La municipalité choisie ne figure pas au catalogue.");

        if (owner.Status is not ("Actif" or "Inactif"))
            throw new ArgumentException("Le statut du client doit être Actif ou Inactif.");

        string email = owner.Email?.Trim() ?? string.Empty;
        if (email.Length > 0 &&
            (!email.Contains('@') || email.StartsWith('@') || email.EndsWith('@')))
            throw new ArgumentException("Veuillez saisir un courriel valide ou laisser ce champ vide.");
    }
}
