using MuniChien.App.Services;
using System;
using System.Linq;
using System.Threading;

namespace MuniChien.Tests;

public class LoyaltyStatementSnapshotTests
{
    private static int _ownerId = 880000;
    private static (OwnerDirectoryItem client, Guid cardId) Open()
    {
        int id = Interlocked.Increment(ref _ownerId);
        var client = new OwnerDirectoryItem { OwnerId = id, FileNumber = id.ToString(), FirstName = "Exemple", LastName = "Client" };
        return (client, LocalLoyaltyService.GetCards(id).First().Id);
    }

    private static LoyaltyPurchaseRecord Buy(int i, decimal price) => new()
    {
        ProductName = $"Sac exact n°{i}", ProductReference = $"SKU-{i}", Format = "10 kg",
        PricePaid = price, PurchasedAt = new DateTime(2026, 7, 1).AddDays(i), InvoiceNumber = $"FACT-{i}"
    };

    [Fact]
    public void ActiveIncompleteCycleDoesNotInventReward()
    {
        var (client, card) = Open();
        LocalLoyaltyService.SaveDetailedPurchase(client.OwnerId, card, 0, Buy(1, 51.99m));
        var snapshot = LoyaltyStatementSnapshot.FromActive(client, LocalLoyaltyService.GetCards(client.OwnerId).First());
        Assert.Equal(1, snapshot.PurchaseCount);
        Assert.True(snapshot.IsProvisional);
        Assert.Null(snapshot.Gift);
        Assert.Null(snapshot.Purchases[1]);
    }

    [Fact]
    public void ExactLowestPriceProductUsedInCompleteStatement()
    {
        var (client, card) = Open();
        for (int i = 0; i < 12; i++)
            LocalLoyaltyService.SaveDetailedPurchase(client.OwnerId, card, i, Buy(i, i == 8 ? 39.99m : 92.99m));
        var snapshot = LoyaltyStatementSnapshot.FromActive(client, LocalLoyaltyService.GetCards(client.OwnerId).First());
        Assert.True(snapshot.IsComplete);
        Assert.False(snapshot.IsProvisional);
        Assert.Equal(8, snapshot.Gift!.Index);
        Assert.Equal("SKU-8", snapshot.Gift.Purchase.ProductReference);
        Assert.Equal(39.99m, snapshot.Gift.Purchase.PricePaid);
    }

    [Fact]
    public void ArchivedCycleIsNotReplacedByResetActiveCard()
    {
        var (client, card) = Open();
        for (int i = 0; i < 12; i++)
            LocalLoyaltyService.SaveDetailedPurchase(client.OwnerId, card, i, Buy(i, 80m));
        var given = LocalLoyaltyService.GiveRewardAndStartNext(client.OwnerId, card);
        Assert.Equal(0, given.DetailedPurchases);
        var history = LocalLoyaltyService.GetRewardHistory(client.OwnerId).Single();
        var snapshot = LoyaltyStatementSnapshot.FromArchive(client, history);
        Assert.True(snapshot.IsArchived);
        Assert.Equal(12, snapshot.PurchaseCount);
        Assert.Equal("SKU-0", snapshot.Gift!.Purchase.ProductReference);
        Assert.NotNull(snapshot.RewardGivenAt);
    }

    [Fact]
    public void LegacyCheckIsShownAsMissingDetailsNotFictionalPurchase()
    {
        var (client, card) = Open();
        LocalLoyaltyService.SetPurchase(client.OwnerId, card, 0, true);
        var snapshot = LoyaltyStatementSnapshot.FromActive(client, LocalLoyaltyService.GetCards(client.OwnerId).First());
        Assert.Equal(1, snapshot.LegacyCount);
        Assert.Equal(0, snapshot.PurchaseCount);
        Assert.Null(snapshot.Gift);
    }
}
