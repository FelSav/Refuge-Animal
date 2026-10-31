using System;
using System.Collections.Generic;
using System.Linq;

namespace MuniChien.App.Services;

/// <summary>
/// Photographie des données d'un cycle, indépendante de l'impression et de l'API.
/// Aucun prix ou détail n'est inventé pour les anciennes coches.
/// </summary>
public sealed class LoyaltyStatementSnapshot
{
    public required OwnerDirectoryItem Client { get; init; }
    public required string ProgramName { get; init; }
    public required int CycleNumber { get; init; }
    public required DateTime CycleStartedAt { get; init; }
    public DateTime? RewardGivenAt { get; init; }
    public required IReadOnlyList<LoyaltyPurchaseRecord?> Purchases { get; init; }
    public required IReadOnlyList<DateTime?> PurchaseDates { get; init; }
    public LoyaltyRewardCandidate? Gift { get; init; }
    public bool IsArchived => RewardGivenAt.HasValue;
    public int PurchaseCount => Purchases.Count(x => x is not null);
    public int LegacyCount => Enumerable.Range(0, PurchaseDates.Count)
        .Count(i => PurchaseDates[i].HasValue && Purchases[i] is null);
    public bool IsComplete => PurchaseCount == LocalLoyaltyService.PurchaseCount && LegacyCount == 0;
    public bool IsProvisional => !IsArchived && !IsComplete;

    public static LoyaltyStatementSnapshot FromActive(OwnerDirectoryItem client, LoyaltyCardRecord card)
    {
        if (client.OwnerId != card.OwnerId) throw new ArgumentException("Le dossier ne correspond pas à la carte.");
        var gift = card.HasCompletePurchaseDetails ? FindCheapest(card.Purchases) : null;
        return new LoyaltyStatementSnapshot
        {
            Client = client,
            ProgramName = card.ProgramName,
            CycleNumber = card.CompletedCycles + 1,
            CycleStartedAt = card.CycleStartedAt,
            Purchases = card.Purchases.ToArray(),
            PurchaseDates = card.PurchaseDates.ToArray(),
            Gift = gift
        };
    }

    public static LoyaltyStatementSnapshot FromArchive(OwnerDirectoryItem client, LoyaltyRewardRecord reward)
    {
        if (client.OwnerId != reward.OwnerId) throw new ArgumentException("Le dossier ne correspond pas à l'historique.");
        return new LoyaltyStatementSnapshot
        {
            Client = client,
            ProgramName = reward.ProgramName,
            CycleNumber = reward.CycleNumber,
            CycleStartedAt = reward.CycleStartedAt,
            RewardGivenAt = reward.RewardGivenAt,
            Purchases = reward.Purchases.ToArray(),
            PurchaseDates = reward.PurchaseDates.ToArray(),
            Gift = new LoyaltyRewardCandidate(reward.RewardedPurchaseIndex, reward.RewardedPurchase)
        };
    }

    private static LoyaltyRewardCandidate FindCheapest(IReadOnlyList<LoyaltyPurchaseRecord?> purchases)
    {
        if (purchases.Count != LocalLoyaltyService.PurchaseCount || purchases.Any(p => p is null))
            throw new InvalidOperationException("Douze achats détaillés sont requis.");
        var bestIndex = Enumerable.Range(0, purchases.Count)
            .OrderBy(i => purchases[i]!.PricePaid)
            .ThenBy(i => i).First();
        return new LoyaltyRewardCandidate(bestIndex, purchases[bestIndex]!);
    }
}
