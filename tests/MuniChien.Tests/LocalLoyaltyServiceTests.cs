using MuniChien.App.Services;
using System;
using System.Linq;
using System.Threading;

namespace MuniChien.Tests;

public class LocalLoyaltyServiceTests
{
    private static int _ownerSeed = 700000;
    private static (int Owner, Guid Card) NewCard()
    {
        int ownerId = Interlocked.Increment(ref _ownerSeed);
        return (ownerId, LocalLoyaltyService.GetCards(ownerId).First().Id);
    }

    private static LoyaltyPurchaseRecord Entry(int i, decimal price = 85.99m) => new()
    {
        ProductName = $"Sac modèle {i}", ProductReference = $"SKU-{i}",
        Format = "10 kg", PricePaid = price,
        PurchasedAt = new DateTime(2026, 9, 1).AddDays(i), InvoiceNumber = $"FAC-{i}"
    };

    [Fact]
    public void TwelveDetailedPurchasesUnlockOnlyExactCheapestProduct()
    {
        var (owner, card) = NewCard();
        for (int i = 0; i < 12; i++)
            LocalLoyaltyService.SaveDetailedPurchase(owner, card, i, Entry(i, i == 7 ? 49.99m : 85.99m));
        var result = LocalLoyaltyService.GetRewardCandidate(owner, card);
        Assert.Equal(7, result.Index);
        Assert.Equal("Sac modèle 7", result.Purchase.ProductName);
        Assert.Equal("SKU-7", result.Purchase.ProductReference);
        Assert.Equal("10 kg", result.Purchase.Format);
        Assert.Equal(49.99m, result.Purchase.PricePaid);
    }

    [Fact]
    public void TieUsesFirstPurchaseWithoutSubstitutingOtherProduct()
    {
        var (owner, card) = NewCard();
        for (int i = 0; i < 12; i++)
            LocalLoyaltyService.SaveDetailedPurchase(owner, card, i, Entry(i, i is 2 or 8 ? 50m : 80m));
        var reward = LocalLoyaltyService.GetRewardCandidate(owner, card);
        Assert.Equal(2, reward.Index);
        Assert.Equal("SKU-2", reward.Purchase.ProductReference);
    }

    [Fact]
    public void RewardArchivesAllPurchasesAndResetsSameCard()
    {
        var (owner, card) = NewCard();
        for (int i = 0; i < 12; i++)
            LocalLoyaltyService.SaveDetailedPurchase(owner, card, i, Entry(i));
        var current = LocalLoyaltyService.GiveRewardAndStartNext(owner, card);
        var history = LocalLoyaltyService.GetRewardHistory(owner).Single();
        Assert.Equal(card, current.Id);
        Assert.Equal(0, current.CompletedPurchases);
        Assert.Equal(0, current.DetailedPurchases);
        Assert.Equal(1, current.CompletedCycles);
        Assert.Equal(12, history.Purchases.Count(p => p != null));
        Assert.Equal("SKU-0", history.RewardedPurchase.ProductReference);
    }

    [Fact]
    public void LegacyCheckboxesDoNotUnlockRewardUntilFullyDocumented()
    {
        var (owner, card) = NewCard();
        for (int i = 0; i < 12; i++)
            LocalLoyaltyService.SetPurchase(owner, card, i, true);
        var snapshot = LocalLoyaltyService.GetCards(owner).First(c => c.Id == card);
        Assert.Equal(12, snapshot.CompletedPurchases);
        Assert.False(snapshot.HasCompletePurchaseDetails);
        Assert.Throws<InvalidOperationException>(() => LocalLoyaltyService.GetRewardCandidate(owner, card));
        LocalLoyaltyService.SaveDetailedPurchase(owner, card, 0, Entry(0));
        Assert.Equal(1, LocalLoyaltyService.GetCards(owner).First(c => c.Id == card).DetailedPurchases);
    }

    [Fact]
    public void RejectsBadPricesAndOutOfOrderEntries()
    {
        var (owner, card) = NewCard();
        Assert.Throws<ArgumentException>(() => LocalLoyaltyService.SaveDetailedPurchase(owner, card, 0, Entry(0, 25.555m)));
        Assert.Throws<ArgumentException>(() => LocalLoyaltyService.SaveDetailedPurchase(owner, card, 0, Entry(0, 0m)));
        Assert.Throws<InvalidOperationException>(() => LocalLoyaltyService.SaveDetailedPurchase(owner, card, 1, Entry(1)));
    }

    [Fact]
    public void DuplicateExternalUnitKeyIsRejectedAcrossPrograms()
    {
        var owner = Interlocked.Increment(ref _ownerSeed);
        var cards = LocalLoyaltyService.GetCards(owner);
        var first = Entry(0);
        first.ExternalTransactionKey = "TRANSACTION-1/LINE-3/UNIT-1";
        LocalLoyaltyService.SaveDetailedPurchase(owner, cards[0].Id, 0, first);
        var second = Entry(1);
        second.ExternalTransactionKey = first.ExternalTransactionKey;
        Assert.Throws<InvalidOperationException>(() => LocalLoyaltyService.SaveDetailedPurchase(owner, cards[1].Id, 0, second));
    }

    [Fact]
    public void RemovingHistoricalPurchaseMustNotShiftLaterOrder()
    {
        var (owner, card) = NewCard();
        LocalLoyaltyService.SaveDetailedPurchase(owner, card, 0, Entry(0));
        LocalLoyaltyService.SaveDetailedPurchase(owner, card, 1, Entry(1));
        Assert.Throws<InvalidOperationException>(() => LocalLoyaltyService.RemoveDetailedPurchase(owner, card, 0));
        LocalLoyaltyService.RemoveDetailedPurchase(owner, card, 1);
        Assert.Equal(1, LocalLoyaltyService.GetCards(owner).First(c => c.Id == card).CompletedPurchases);
    }
}
