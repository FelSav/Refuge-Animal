using System;
using System.Collections.Generic;
using System.Linq;

namespace MuniChien.App.Services;

// Démonstration locale en mémoire. Une seule carte active par compagnie et propriétaire.
// Lors de la remise d'un cadeau, on archive le CYCLE (les dates d'achats),
// mais on conserve la même carte et son identifiant pour les achats suivants.
public static class LocalLoyaltyService
{
    public const int PurchaseCount = 12;

    private static readonly object Sync = new();
    private static readonly Dictionary<int, List<LoyaltyCardRecord>> CardsByOwner = new();
    private static readonly Dictionary<int, List<LoyaltyRewardRecord>> RewardsByOwner = new();

    public static IReadOnlyList<LoyaltyProgramDefinition> Programs { get; } =
    [
        new("recompattes", "Récompattes / VIPaws"),
        new("big-country-raw", "Big Country Raw"),
        new("nutrena", "Nutrena (Loyall)"),
        new("nutrience", "Nutrience"),
        new("go-natural", "Go! Natural")
    ];

    // Les cinq programmes de base sont créés une seule fois, à la première ouverture
    // du dossier. Les compagnies ajoutées suivent dans l'ordre de création.
    public static IReadOnlyList<LoyaltyCardRecord> GetCards(int ownerId)
    {
        if (ownerId <= 0) return [];
        lock (Sync)
        {
            var cards = EnsureInitialCardsLocked(ownerId);
            var creationOrder = cards.Select(c => c.ProgramId).Distinct().ToList();
            return cards.Select(Clone)
                .OrderBy(c => GetProgramOrder(c.ProgramId, creationOrder))
                .ToList();
        }
    }

    // Historique des récompenses : snapshots en lecture seule, et NON des cartes
    // supplémentaires. Une carte peut avoir plusieurs remises dans son historique.
    public static IReadOnlyList<LoyaltyRewardRecord> GetRewardHistory(int ownerId)
    {
        if (ownerId <= 0) return [];
        lock (Sync)
        {
            if (!RewardsByOwner.TryGetValue(ownerId, out var rewards)) return [];
            return rewards.OrderByDescending(r => r.RewardGivenAt)
                .Select(CloneReward).ToList();
        }
    }

    private static List<LoyaltyCardRecord> EnsureInitialCardsLocked(int ownerId)
    {
        if (!CardsByOwner.TryGetValue(ownerId, out var cards))
        {
            cards = [];
            CardsByOwner[ownerId] = cards;
        }

        foreach (var program in Programs)
        {
            if (cards.Any(card => card.ProgramId == program.Id)) continue;
            cards.Add(NewCard(ownerId, program));
        }
        return cards;
    }

    private static LoyaltyCardRecord NewCard(int ownerId, LoyaltyProgramDefinition program)
    {
        var now = DateTime.Now;
        return new LoyaltyCardRecord
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            ProgramId = program.Id,
            ProgramName = program.Name,
            CardNumber = 1,
            CreatedAt = now,
            CycleStartedAt = now,
            PurchaseDates = new DateTime?[PurchaseCount]
        };
    }

    private static int GetProgramOrder(string programId, IReadOnlyList<string> creationOrder)
    {
        for (int i = 0; i < Programs.Count; i++)
        {
            if (Programs[i].Id == programId) return i;
        }

        for (int i = 0; i < creationOrder.Count; i++)
        {
            if (creationOrder[i] == programId) return Programs.Count + i;
        }
        return Programs.Count + creationOrder.Count;
    }

    // Conservé pour la compatibilité des appels précédents, mais la création d'un
    // doublon est désormais interdite, y compris pour les programmes par défaut.
    public static LoyaltyCardRecord CreateCard(int ownerId, string programId)
    {
        if (ownerId <= 0) throw new ArgumentException("Un client doit être sélectionné.");
        lock (Sync)
        {
            var cards = EnsureInitialCardsLocked(ownerId);
            var program = Programs.FirstOrDefault(p => p.Id == programId);
            if (program is null)
                throw new ArgumentException("Programme de fidélité invalide.");
            if (cards.Any(c => c.ProgramId == programId))
                throw new InvalidOperationException($"Une carte {program.Name} existe déjà pour ce client.");

            var newCard = NewCard(ownerId, program);
            cards.Add(newCard);
            return Clone(newCard);
        }
    }

    // Ajout manuel d'une NOUVELLE compagnie, seulement pour le dossier sélectionné.
    // Le nom est comparé sans tenir compte des espaces répétés ni de la casse.
    public static LoyaltyCardRecord CreateCustomCard(int ownerId, string companyName)
    {
        if (ownerId <= 0) throw new ArgumentException("Un client doit être sélectionné.");
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Inscrivez le nom de la nouvelle compagnie.");

        var name = string.Join(" ", companyName.Split(
            (char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (name.Length > 80)
            throw new ArgumentException("Le nom de la compagnie ne peut pas dépasser 80 caractères.");

        lock (Sync)
        {
            var cards = EnsureInitialCardsLocked(ownerId);
            if (cards.Any(c => string.Equals(c.ProgramName, name,
                    StringComparison.CurrentCultureIgnoreCase)))
                throw new InvalidOperationException(
                    $"La carte « {name} » existe déjà. Utilisez sa carte actuelle; elle sera remise à zéro après la remise du cadeau.");

            var card = NewCard(ownerId,
                new LoyaltyProgramDefinition($"custom-{Guid.NewGuid():N}", name));
            cards.Add(card);
            return Clone(card);
        }
    }

    public static void SetPurchase(int ownerId, Guid cardId, int index, bool selected)
    {
        if (index < 0 || index >= PurchaseCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        lock (Sync)
        {
            EnsureInitialCardsLocked(ownerId);
            var card = GetEditableCardLocked(ownerId, cardId);
            if (selected)
                card.PurchaseDates[index] ??= DateTime.Now;
            else
                card.PurchaseDates[index] = null;
        }
    }

    // Conserve le même objet carte, le même Guid et la même position.
    // Les achats du cycle terminé sont copiés dans l'historique avant remise à zéro.
    public static LoyaltyCardRecord GiveRewardAndStartNext(int ownerId, Guid cardId)
    {
        lock (Sync)
        {
            EnsureInitialCardsLocked(ownerId);
            var card = GetEditableCardLocked(ownerId, cardId);
            if (card.CompletedPurchases != PurchaseCount)
                throw new InvalidOperationException("Les 12 achats doivent être cochés avant la remise du cadeau.");

            var now = DateTime.Now;
            if (!RewardsByOwner.TryGetValue(ownerId, out var rewards))
            {
                rewards = [];
                RewardsByOwner[ownerId] = rewards;
            }

            rewards.Add(new LoyaltyRewardRecord
            {
                Id = Guid.NewGuid(),
                OwnerId = ownerId,
                CardId = card.Id,
                ProgramId = card.ProgramId,
                ProgramName = card.ProgramName,
                CycleNumber = card.CompletedCycles + 1,
                CycleStartedAt = card.CycleStartedAt,
                RewardGivenAt = now,
                PurchaseDates = (DateTime?[])card.PurchaseDates.Clone()
            });

            card.CompletedCycles++;
            card.CycleStartedAt = now;
            card.PurchaseDates = new DateTime?[PurchaseCount];
            return Clone(card);
        }
    }

    private static LoyaltyCardRecord GetEditableCardLocked(int ownerId, Guid cardId)
    {
        if (!CardsByOwner.TryGetValue(ownerId, out var cards))
            throw new InvalidOperationException("Aucune carte trouvée.");
        return cards.FirstOrDefault(c => c.Id == cardId)
            ?? throw new InvalidOperationException("Cette carte n'existe plus.");
    }

    private static LoyaltyCardRecord Clone(LoyaltyCardRecord x) => new()
    {
        Id = x.Id,
        OwnerId = x.OwnerId,
        ProgramId = x.ProgramId,
        ProgramName = x.ProgramName,
        CardNumber = x.CardNumber,
        CreatedAt = x.CreatedAt,
        CycleStartedAt = x.CycleStartedAt,
        CompletedCycles = x.CompletedCycles,
        RewardGivenAt = x.RewardGivenAt,
        PurchaseDates = (DateTime?[])x.PurchaseDates.Clone()
    };

    private static LoyaltyRewardRecord CloneReward(LoyaltyRewardRecord x) => new()
    {
        Id = x.Id,
        OwnerId = x.OwnerId,
        CardId = x.CardId,
        ProgramId = x.ProgramId,
        ProgramName = x.ProgramName,
        CycleNumber = x.CycleNumber,
        CycleStartedAt = x.CycleStartedAt,
        RewardGivenAt = x.RewardGivenAt,
        PurchaseDates = (DateTime?[])x.PurchaseDates.Clone()
    };
}

public sealed record LoyaltyProgramDefinition(string Id, string Name)
{
    public override string ToString() => Name;
}

public sealed class LoyaltyCardRecord
{
    public Guid Id { get; init; }
    public int OwnerId { get; init; }
    public string ProgramId { get; init; } = "";
    public string ProgramName { get; init; } = "";
    public int CardNumber { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime CycleStartedAt { get; set; }
    public int CompletedCycles { get; set; }
    // Conservé pour la compatibilité avec la fiche Propriétaire existante.
    public DateTime? RewardGivenAt { get; set; }
    public DateTime?[] PurchaseDates { get; set; } = new DateTime?[LocalLoyaltyService.PurchaseCount];
    public int CompletedPurchases => PurchaseDates.Count(date => date.HasValue);
    public bool IsArchived => RewardGivenAt.HasValue;
    public DateTime LastActivity => PurchaseDates.Where(x => x.HasValue)
        .Select(x => x!.Value).DefaultIfEmpty(CycleStartedAt).Max();
}

// Une récompense archivée représente un cycle terminé, pas une deuxième carte.
public sealed class LoyaltyRewardRecord
{
    public Guid Id { get; init; }
    public int OwnerId { get; init; }
    public Guid CardId { get; init; }
    public string ProgramId { get; init; } = "";
    public string ProgramName { get; init; } = "";
    public int CycleNumber { get; init; }
    public DateTime CycleStartedAt { get; init; }
    public DateTime RewardGivenAt { get; init; }
    public DateTime?[] PurchaseDates { get; init; } = new DateTime?[LocalLoyaltyService.PurchaseCount];
}
