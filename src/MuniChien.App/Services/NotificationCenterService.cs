using MuniChien.App.ViewModels;

namespace MuniChien.App.Services;

/// <summary>
/// Centre de notifications de démonstration. Lecture seule des dossiers actuels :
/// aucune licence, carte ou facture n'est modifiée lors d'une consultation.
/// La future API devra produire les alertes côté serveur (avec pagination).
/// </summary>
public sealed class NotificationCenterService
{
    private readonly IApplicationServices _services;
    private readonly HashSet<string> _readKeys = new(StringComparer.Ordinal);
    // Masquées jusqu’à la fermeture de MuniChien : un rafraîchissement ne les réintroduit pas.
    private readonly HashSet<string> _dismissedKeys = new(StringComparer.Ordinal);

    public NotificationCenterService(IApplicationServices services) => _services = services;

    public IReadOnlyList<NotificationEntry> GetNotifications(DateTime today)
    {
        List<NotificationEntry> entries = [];
        foreach (var license in _services.Licenses.GetLicenses())
        {
            var kind = NotificationRules.GetLicenseAlert(license.ExpirationDate, today);
            if (kind == LicenseAlertKind.None) continue;

            bool expired = kind == LicenseAlertKind.Expired;
            entries.Add(new NotificationEntry(
                $"licence:{license.LicenseNumber}:{license.ExpirationDate:yyyyMMdd}",
                "Licence", expired ? "Licence expirée" : "Licence à renouveler bientôt",
                $"{license.DogName} · {license.OwnerName} · {license.LicenseNumber}",
                expired ? $"Expirée le {license.ExpirationDate:dd/MM/yyyy}"
                        : $"Échéance le {license.ExpirationDate:dd/MM/yyyy}",
                expired ? 0 : 2,
                NotificationDestination.Dog, license.OwnerId, license.DogId, Guid.Empty,
                expired ? license.ExpirationDate.Date.AddDays(1)
                        : license.ExpirationDate.Date.AddDays(-30)));
        }

        // Ne parcourt PAS tous les clients et ne crée pas 5 cartes pour chacun.
        // Seules les cartes déjà renseignées et admissibles sont consultées.
        foreach (var ready in LocalLoyaltyService.GetReadyRewards())
        {
            var card = LocalLoyaltyService.GetCards(ready.OwnerId)
                .FirstOrDefault(c => c.Id == ready.CardId);
            string ownerName = _services.Animals.GetOwner(ready.OwnerId)?.FullName ?? $"Client #{ready.OwnerId}";
            entries.Add(new NotificationEntry(
                $"fidelite:{ready.CardId}:{ready.CycleNumber}",
                "Fidélité", "Cadeau prêt à remettre",
                $"{ownerName} · {ready.ProgramName} · 12/12 achats",
                "Ouvrir la carte pour vérifier le sac exact",
                1, NotificationDestination.Loyalty, ready.OwnerId, 0, ready.CardId,
                card?.LastActivity ?? today.Date));
        }

        // Les impayés locaux ne sont pas rattachés à une année : aucune alerte
        // individuelle ne doit prétendre être un véritable avis officiel.
        // On présente donc un seul rappel de contrôle, clairement identifié démo.
        int currentYear = today.Year;
        var candidates = _services.Notices.GetCandidates(currentYear, NoticesViewModel.JanuaryCampaign);
        int toReview = candidates.Count(candidate => !_services.Notices.WasSent(
            candidate.OwnerId, currentYear, NoticesViewModel.JanuaryCampaign));
        if (toReview > 0)
        {
            entries.Add(new NotificationEntry(
                $"avis-demo:{currentYear}:{toReview}", "Avis · démo", "Dossiers d'avis à vérifier",
                $"{toReview} dossier(s) avec un solde local à examiner",
                "Simulation : vérifier l'année et la campagne avant tout envoi",
                3, NotificationDestination.Notices, 0, 0, Guid.Empty,
                new DateTime(currentYear, 1, 1)));
        }

        // Uniquement les alertes encore affichables; la date de l’événement prime sur la catégorie.
        return NotificationRules.OrderNewestFirst(
            entries.Where(e => !_dismissedKeys.Contains(e.Key)), e => e.OccurredAt)
            .ThenBy(e => e.Priority)
            .ThenBy(e => e.Title)
            .ToList();
    }

    public bool IsRead(string key) => _readKeys.Contains(key);

    public void Dismiss(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return;
        _dismissedKeys.Add(key);
        _readKeys.Remove(key);
    }
    public void MarkRead(string key) => _readKeys.Add(key);
    public void MarkAllRead(IEnumerable<NotificationEntry> entries)
    {
        foreach (var entry in entries) _readKeys.Add(entry.Key);
    }
}

public enum NotificationDestination { Dog, Loyalty, Notices }

public sealed record NotificationEntry(
    string Key, string Category, string Title, string Description, string Detail,
    int Priority, NotificationDestination Destination, int OwnerId, int DogId, Guid CardId,
    DateTime OccurredAt);
