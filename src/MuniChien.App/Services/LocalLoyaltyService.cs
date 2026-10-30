using System;
using System.Collections.Generic;
using System.Linq;

namespace MuniChien.App.Services;

// Données de démonstration en mémoire uniquement. L'API remplacera ultérieurement ce magasin.
// La même carte est réutilisée après chaque cycle terminé; les cycles archivés sont immuables.
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

    public static IReadOnlyList<LoyaltyCardRecord> GetCards(int ownerId)
    {
        if (ownerId <= 0) return [];
        lock (Sync)
        {
            var cards = EnsureInitialCardsLocked(ownerId);
            var order = cards.Select(c => c.ProgramId).Distinct().ToList();
            return cards.Select(Clone).OrderBy(c => GetProgramOrder(c.ProgramId, order)).ToList();
        }
    }

    // Liste ciblée pour le centre de notifications : ne génère pas de cartes
    // pour les clients qui n'ont jamais ouvert leur page de fidélité.
    public static IReadOnlyList<LoyaltyReadyReward> GetReadyRewards()
    {
        lock (Sync)
        {
            return CardsByOwner.Values
                .SelectMany(cards => cards)
                .Where(card => card.HasCompletePurchaseDetails)
                .Select(card => new LoyaltyReadyReward(
                    card.OwnerId, card.Id, card.ProgramName, card.CompletedCycles + 1))
                .ToList();
        }
    }

    public static IReadOnlyList<LoyaltyRewardRecord> GetRewardHistory(int ownerId)
    {
        if (ownerId <= 0) return [];
        lock (Sync)
        {
            return RewardsByOwner.TryGetValue(ownerId, out var rewards)
                ? rewards.OrderByDescending(r => r.RewardGivenAt).Select(CloneReward).ToList()
                : [];
        }
    }

    private static List<LoyaltyCardRecord> EnsureInitialCardsLocked(int ownerId)
    {
        if (!CardsByOwner.TryGetValue(ownerId, out var cards))
            CardsByOwner[ownerId] = cards = [];
        foreach (var program in Programs)
            if (!cards.Any(c => c.ProgramId == program.Id))
                cards.Add(NewCard(ownerId, program));
        return cards;
    }

    private static LoyaltyCardRecord NewCard(int ownerId, LoyaltyProgramDefinition program)
    {
        var now = DateTime.Now;
        return new LoyaltyCardRecord
        {
            Id = Guid.NewGuid(), OwnerId = ownerId, ProgramId = program.Id,
            ProgramName = program.Name, CardNumber = 1, CreatedAt = now,
            CycleStartedAt = now
        };
    }

    private static int GetProgramOrder(string programId, IReadOnlyList<string> order)
    {
        for (int i = 0; i < Programs.Count; i++)
            if (Programs[i].Id == programId) return i;
        for (int i = 0; i < order.Count; i++)
            if (order[i] == programId) return Programs.Count + i;
        return Programs.Count + order.Count;
    }

    public static LoyaltyCardRecord CreateCard(int ownerId, string programId)
    {
        if (ownerId <= 0) throw new ArgumentException("Un client doit être sélectionné.");
        lock (Sync)
        {
            var cards = EnsureInitialCardsLocked(ownerId);
            var program = Programs.FirstOrDefault(p => p.Id == programId)
                ?? throw new ArgumentException("Programme de fidélité invalide.");
            if (cards.Any(c => c.ProgramId == programId))
                throw new InvalidOperationException($"Une carte {program.Name} existe déjà pour ce client.");
            var card = NewCard(ownerId, program);
            cards.Add(card);
            return Clone(card);
        }
    }

    public static LoyaltyCardRecord CreateCustomCard(int ownerId, string companyName)
    {
        if (ownerId <= 0) throw new ArgumentException("Un client doit être sélectionné.");
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Inscrivez le nom de la nouvelle compagnie.");
        var name = string.Join(" ", companyName.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (name.Length > 80)
            throw new ArgumentException("Le nom de la compagnie ne peut pas dépasser 80 caractères.");
        lock (Sync)
        {
            var cards = EnsureInitialCardsLocked(ownerId);
            if (cards.Any(c => string.Equals(c.ProgramName, name, StringComparison.CurrentCultureIgnoreCase)))
                throw new InvalidOperationException($"La carte « {name} » existe déjà pour ce client.");
            var card = NewCard(ownerId, new LoyaltyProgramDefinition($"custom-{Guid.NewGuid():N}", name));
            cards.Add(card);
            return Clone(card);
        }
    }

    // Compatibilité avec les anciennes coches: elles restent visibles mais ne sont PAS
    // admissibles à la remise d'un cadeau tant que leurs détails ne sont pas complétés.
    public static void SetPurchase(int ownerId, Guid cardId, int index, bool selected)
    {
        ValidateIndex(index);
        lock (Sync)
        {
            EnsureInitialCardsLocked(ownerId);
            var card = GetEditableCardLocked(ownerId, cardId);
            if (selected)
                card.PurchaseDates[index] ??= DateTime.Now;
            else
            {
                if (card.PurchaseDates.Skip(index + 1).Any(x => x.HasValue))
                    throw new InvalidOperationException("Retirez d'abord les achats suivants pour conserver leur ordre.");
                card.PurchaseDates[index] = null;
                card.Purchases[index] = null;
            }
        }
    }

    public static LoyaltyPurchaseRecord? GetPurchase(int ownerId, Guid cardId, int index)
    {
        ValidateIndex(index);
        lock (Sync)
        {
            EnsureInitialCardsLocked(ownerId);
            return ClonePurchase(GetEditableCardLocked(ownerId, cardId).Purchases[index]);
        }
    }

    public static void SaveDetailedPurchase(int ownerId, Guid cardId, int index, LoyaltyPurchaseRecord entry)
    {
        ValidateIndex(index);
        if (entry is null) throw new ArgumentNullException(nameof(entry));
        ValidatePurchase(entry);
        lock (Sync)
        {
            EnsureInitialCardsLocked(ownerId);
            var card = GetEditableCardLocked(ownerId, cardId);
            if (!card.PurchaseDates[index].HasValue && card.PurchaseDates.Take(index).Any(d => !d.HasValue))
                throw new InvalidOperationException("Enregistrez les achats dans l'ordre, à partir de la première case libre.");
            // Une même ligne de facture Acomba ne peut être importée deux fois dans le dossier.
            if (!string.IsNullOrWhiteSpace(entry.ExternalTransactionKey) &&
                CardsByOwner[ownerId].SelectMany(c => c.Purchases)
                    .Any(p => p is not null && p.Id != card.Purchases[index]?.Id &&
                              string.Equals(p.ExternalTransactionKey, entry.ExternalTransactionKey, StringComparison.Ordinal)))
                throw new InvalidOperationException("Cette unité de facture Acomba a déjà été enregistrée.");
            var saved = ClonePurchase(entry)!;
            saved.Id = card.Purchases[index]?.Id ?? Guid.NewGuid();
            card.Purchases[index] = saved;
            card.PurchaseDates[index] = saved.PurchasedAt;
        }
    }

    public static bool HasPotentialManualDuplicate(int ownerId, Guid cardId, int index, LoyaltyPurchaseRecord entry)
    {
        if (string.IsNullOrWhiteSpace(entry.InvoiceNumber)) return false;
        lock (Sync)
        {
            EnsureInitialCardsLocked(ownerId);
            var card = GetEditableCardLocked(ownerId, cardId);
            return card.Purchases.Where((_, i) => i != index).Any(p =>
                p is not null && p.PurchasedAt.Date == entry.PurchasedAt.Date &&
                string.Equals(p.InvoiceNumber.Trim(), entry.InvoiceNumber.Trim(), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(p.ProductReference.Trim(), entry.ProductReference.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }

    public static void RemoveDetailedPurchase(int ownerId, Guid cardId, int index)
    {
        SetPurchase(ownerId, cardId, index, false);
    }

    // Détermine le SAC EXACT à remettre, jamais une simple valeur d'achat.
    // En cas de prix égaux, la première occurrence dans les 12 achats fait foi.
    public static LoyaltyRewardCandidate GetRewardCandidate(int ownerId, Guid cardId)
    {
        lock (Sync)
        {
            EnsureInitialCardsLocked(ownerId);
            return GetRewardCandidateLocked(GetEditableCardLocked(ownerId, cardId));
        }
    }

    private static LoyaltyRewardCandidate GetRewardCandidateLocked(LoyaltyCardRecord card)
    {
        if (!card.HasCompletePurchaseDetails)
            throw new InvalidOperationException("Les 12 achats doivent comporter un sac, une référence, un format, un prix et une date avant la remise. Complétez les anciennes coches.");
        var cheapest = card.Purchases.Select((p, index) => new { Purchase = p!, Index = index })
            .OrderBy(x => x.Purchase.PricePaid).ThenBy(x => x.Index).First();
        return new LoyaltyRewardCandidate(cheapest.Index, ClonePurchase(cheapest.Purchase)!);
    }

    public static LoyaltyCardRecord GiveRewardAndStartNext(int ownerId, Guid cardId)
    {
        lock (Sync)
        {
            EnsureInitialCardsLocked(ownerId);
            var card = GetEditableCardLocked(ownerId, cardId);
            var reward = GetRewardCandidateLocked(card);
            var now = DateTime.Now;
            if (!RewardsByOwner.TryGetValue(ownerId, out var history))
                RewardsByOwner[ownerId] = history = [];
            history.Add(new LoyaltyRewardRecord
            {
                Id = Guid.NewGuid(), OwnerId = ownerId, CardId = card.Id,
                ProgramId = card.ProgramId, ProgramName = card.ProgramName,
                CycleNumber = card.CompletedCycles + 1, CycleStartedAt = card.CycleStartedAt,
                RewardGivenAt = now, PurchaseDates = (DateTime?[])card.PurchaseDates.Clone(),
                Purchases = ClonePurchases(card.Purchases), RewardedPurchaseIndex = reward.Index,
                RewardedPurchase = ClonePurchase(reward.Purchase)!
            });
            card.CompletedCycles++;
            card.CycleStartedAt = now;
            card.PurchaseDates = new DateTime?[PurchaseCount];
            card.Purchases = new LoyaltyPurchaseRecord?[PurchaseCount];
            return Clone(card);
        }
    }

    private static void ValidateIndex(int index)
    {
        if (index is < 0 or >= PurchaseCount) throw new ArgumentOutOfRangeException(nameof(index));
    }

    public static void ValidatePurchase(LoyaltyPurchaseRecord p)
    {
        if (string.IsNullOrWhiteSpace(p.ProductName) || p.ProductName.Trim().Length > 160)
            throw new ArgumentException("Inscrivez le nom exact du sac (maximum 160 caractères).");
        if (string.IsNullOrWhiteSpace(p.ProductReference) || p.ProductReference.Trim().Length > 90)
            throw new ArgumentException("Inscrivez la référence SKU ou CUP du sac (maximum 90 caractères).");
        if (string.IsNullOrWhiteSpace(p.Format) || p.Format.Trim().Length > 80)
            throw new ArgumentException("Inscrivez le format exact du sac (ex. : 10 kg).");
        if (p.PricePaid <= 0 || decimal.Round(p.PricePaid, 2) != p.PricePaid)
            throw new ArgumentException("Le prix doit être positif et comporter au maximum deux décimales.");
        if (p.PurchasedAt == default || p.PurchasedAt.Date > DateTime.Today)
            throw new ArgumentException("Choisissez une date d'achat valide (pas dans le futur).");
        if (p.InvoiceNumber?.Length > 100)
            throw new ArgumentException("Le numéro de facture ne peut pas dépasser 100 caractères.");
        if (p.AttachmentPath?.Length > 1024)
            throw new ArgumentException("Le chemin du justificatif est trop long.");
    }

    private static LoyaltyCardRecord GetEditableCardLocked(int ownerId, Guid cardId)
    {
        if (!CardsByOwner.TryGetValue(ownerId, out var cards))
            throw new InvalidOperationException("Aucune carte trouvée.");
        return cards.FirstOrDefault(c => c.Id == cardId)
            ?? throw new InvalidOperationException("Cette carte n'existe plus.");
    }

    private static LoyaltyCardRecord Clone(LoyaltyCardRecord c) => new()
    {
        Id = c.Id, OwnerId = c.OwnerId, ProgramId = c.ProgramId, ProgramName = c.ProgramName,
        CardNumber = c.CardNumber, CreatedAt = c.CreatedAt, CycleStartedAt = c.CycleStartedAt,
        CompletedCycles = c.CompletedCycles, RewardGivenAt = c.RewardGivenAt,
        PurchaseDates = (DateTime?[])c.PurchaseDates.Clone(), Purchases = ClonePurchases(c.Purchases)
    };

    private static LoyaltyRewardRecord CloneReward(LoyaltyRewardRecord r) => new()
    {
        Id = r.Id, OwnerId = r.OwnerId, CardId = r.CardId, ProgramId = r.ProgramId,
        ProgramName = r.ProgramName, CycleNumber = r.CycleNumber, CycleStartedAt = r.CycleStartedAt,
        RewardGivenAt = r.RewardGivenAt, PurchaseDates = (DateTime?[])r.PurchaseDates.Clone(),
        Purchases = ClonePurchases(r.Purchases), RewardedPurchaseIndex = r.RewardedPurchaseIndex,
        RewardedPurchase = ClonePurchase(r.RewardedPurchase)!
    };

    private static LoyaltyPurchaseRecord?[] ClonePurchases(LoyaltyPurchaseRecord?[] purchases) =>
        purchases.Select(ClonePurchase).ToArray();

    private static LoyaltyPurchaseRecord? ClonePurchase(LoyaltyPurchaseRecord? p) => p is null ? null : new()
    {
        Id = p.Id, ProductName = p.ProductName, ProductReference = p.ProductReference,
        Format = p.Format, PricePaid = p.PricePaid, PurchasedAt = p.PurchasedAt,
        InvoiceNumber = p.InvoiceNumber, AttachmentPath = p.AttachmentPath,
        Origin = p.Origin, ExternalTransactionKey = p.ExternalTransactionKey
    };
}

public sealed record LoyaltyProgramDefinition(string Id, string Name)
{
    public override string ToString() => Name;
}

public sealed class LoyaltyPurchaseRecord
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = "";
    public string ProductReference { get; set; } = "";
    public string Format { get; set; } = "";
    public decimal PricePaid { get; set; }
    public DateTime PurchasedAt { get; set; }
    public string InvoiceNumber { get; set; } = "";
    public string AttachmentPath { get; set; } = "";
    public string Origin { get; set; } = "Manuel";
    // Préparation future: identifiant unique transaction+ligne+unité fourni par Acomba.
    public string? ExternalTransactionKey { get; set; }
}

public sealed record LoyaltyRewardCandidate(int Index, LoyaltyPurchaseRecord Purchase);

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
    public DateTime? RewardGivenAt { get; set; }
    // Dates anciennes conservées pour la compatibilité avec la fiche client.
    public DateTime?[] PurchaseDates { get; set; } = new DateTime?[LocalLoyaltyService.PurchaseCount];
    public LoyaltyPurchaseRecord?[] Purchases { get; set; } = new LoyaltyPurchaseRecord?[LocalLoyaltyService.PurchaseCount];
    public int CompletedPurchases => PurchaseDates.Count(d => d.HasValue);
    public int DetailedPurchases => Purchases.Count(p => p is not null);
    public bool HasCompletePurchaseDetails => CompletedPurchases == LocalLoyaltyService.PurchaseCount && DetailedPurchases == LocalLoyaltyService.PurchaseCount;
    public bool IsArchived => RewardGivenAt.HasValue;
    public DateTime LastActivity => PurchaseDates.Where(x => x.HasValue)
        .Select(x => x!.Value).DefaultIfEmpty(CycleStartedAt).Max();
}

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
    public LoyaltyPurchaseRecord?[] Purchases { get; init; } = new LoyaltyPurchaseRecord?[LocalLoyaltyService.PurchaseCount];
    public int RewardedPurchaseIndex { get; init; }
    public LoyaltyPurchaseRecord RewardedPurchase { get; init; } = new();
}

public sealed record LoyaltyReadyReward(int OwnerId, Guid CardId, string ProgramName, int CycleNumber);
