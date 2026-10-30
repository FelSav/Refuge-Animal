using MuniChien.App.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MuniChien.App.Views;

public partial class LoyaltyCardsDialog : Window
{
    private readonly int _ownerId;

    public LoyaltyCardsDialog(int ownerId, string ownerName)
    {
        InitializeComponent();
        _ownerId = ownerId;
        OwnerNameText.Text = $"{ownerName} — fidélités du dossier sélectionné";
        RefreshCards();
    }

    private void RefreshCards(bool keepScrollPosition = false)
    {
        double scrollOffset = CardsScrollViewer.VerticalOffset;
        var active = LocalLoyaltyService.GetCards(_ownerId)
            .Select(record => new LoyaltyCardViewItem(record)).ToList();
        var archived = LocalLoyaltyService.GetRewardHistory(_ownerId)
            .Select(record => new LoyaltyCardViewItem(record)).ToList();

        ActiveCardsList.ItemsSource = active;
        ArchivedCardsList.ItemsSource = archived;
        ActiveCountText.Text = $"{active.Count} compagnie(s)";
        ArchiveCountText.Text = $"{archived.Count} cadeau(x) remis";
        EmptyActiveText.Visibility = active.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        EmptyArchiveText.Visibility = archived.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        if (keepScrollPosition)
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded,
                new Action(() => CardsScrollViewer.ScrollToVerticalOffset(scrollOffset)));
    }

    private void AddCard_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            LocalLoyaltyService.CreateCustomCard(_ownerId, CompanyNameTextBox.Text);
            CompanyNameTextBox.Clear();
            RefreshCards(true);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Ajout impossible", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Stamp_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: LoyaltyStampViewItem stamp }) return;
        try
        {
            var dialog = new LoyaltyPurchaseDialog(stamp.ProgramName, stamp.Index,
                stamp.Purchase, stamp.LegacyDate, stamp.IsArchived) { Owner = this };
            if (dialog.ShowDialog() != true) return;
            if (stamp.IsArchived) return;

            if (dialog.DeleteRequested)
            {
                LocalLoyaltyService.RemoveDetailedPurchase(_ownerId, stamp.CardId, stamp.Index);
                RefreshCards(true);
                return;
            }
            if (dialog.ResultPurchase is not LoyaltyPurchaseRecord entry) return;

            if (LocalLoyaltyService.HasPotentialManualDuplicate(_ownerId, stamp.CardId, stamp.Index, entry))
            {
                var answer = MessageBox.Show(this,
                    "Un sac avec cette référence et ce numéro de facture existe déjà pour cette date.\n\n" +
                    "S'il s'agit de DEUX sacs identiques achetés sur la même facture, vous pouvez continuer. " +
                    "Sinon, annulez pour éviter un achat en double.",
                    "Vérifier un doublon", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (answer != MessageBoxResult.Yes) return;
            }

            var wasComplete = LocalLoyaltyService.GetCards(_ownerId)
                .First(c => c.Id == stamp.CardId).HasCompletePurchaseDetails;
            LocalLoyaltyService.SaveDetailedPurchase(_ownerId, stamp.CardId, stamp.Index, entry);
            var isComplete = LocalLoyaltyService.GetCards(_ownerId)
                .First(c => c.Id == stamp.CardId).HasCompletePurchaseDetails;
            RefreshCards(true);
            if (!wasComplete && isComplete)
                ShowReward(stamp.CardId);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Modification impossible", MessageBoxButton.OK, MessageBoxImage.Warning);
            RefreshCards(true);
        }
    }

    private void Reward_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Guid cardId }) ShowReward(cardId);
    }

    private void ShowReward(Guid cardId)
    {
        try
        {
            var card = LocalLoyaltyService.GetCards(_ownerId).First(c => c.Id == cardId);
            var candidate = LocalLoyaltyService.GetRewardCandidate(_ownerId, cardId);
            var dialog = new LoyaltyRewardDialog(card.ProgramName, candidate) { Owner = this };
            if (dialog.ShowDialog() != true) return;
            LocalLoyaltyService.GiveRewardAndStartNext(_ownerId, cardId);
            RefreshCards(true);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Remise impossible", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}

public sealed class LoyaltyCardViewItem
{
    public LoyaltyCardViewItem(LoyaltyCardRecord record)
    {
        Id = record.Id;
        ProgramName = record.ProgramName;
        CardLabel = record.CompletedCycles == 0
            ? $"Carte active · créée le {record.CreatedAt:dd/MM/yyyy}"
            : $"Carte active · {record.CompletedCycles} cadeau(x) déjà remis";
        ProgressText = $"{record.CompletedPurchases}/{LocalLoyaltyService.PurchaseCount}";
        StatusText = record.HasCompletePurchaseDetails
            ? "Récompense disponible"
            : record.CompletedPurchases == LocalLoyaltyService.PurchaseCount
                ? "Anciennes coches à compléter" : "En cours";
        ActivityText = "Cliquez sur une case pour ajouter ou consulter le sac, son prix et sa facture.";
        CanGiveReward = record.HasCompletePurchaseDetails;
        RewardButtonVisibility = Visibility.Visible;
        Stamps = Enumerable.Range(0, LocalLoyaltyService.PurchaseCount)
            .Select(i => new LoyaltyStampViewItem(record.Id, record.ProgramName, i,
                record.PurchaseDates[i], record.Purchases[i], false)).ToList();
    }

    public LoyaltyCardViewItem(LoyaltyRewardRecord reward)
    {
        Id = reward.CardId;
        ProgramName = reward.ProgramName;
        CardLabel = $"Cycle n° {reward.CycleNumber} · commencé le {reward.CycleStartedAt:dd/MM/yyyy}";
        ProgressText = $"{LocalLoyaltyService.PurchaseCount}/{LocalLoyaltyService.PurchaseCount}";
        StatusText = "Cadeau remis";
        ActivityText = $"Cadeau remis le {reward.RewardGivenAt:dd/MM/yyyy HH:mm} · " +
            $"sac offert : {reward.RewardedPurchase.ProductName} ({reward.RewardedPurchase.Format}) · " +
            $"réf. {reward.RewardedPurchase.ProductReference} · " +
            reward.RewardedPurchase.PricePaid.ToString("C", CultureInfo.GetCultureInfo("fr-CA"));
        CanGiveReward = false;
        RewardButtonVisibility = Visibility.Collapsed;
        Stamps = Enumerable.Range(0, LocalLoyaltyService.PurchaseCount)
            .Select(i => new LoyaltyStampViewItem(reward.CardId, reward.ProgramName, i,
                reward.PurchaseDates[i], reward.Purchases[i], true)).ToList();
    }

    public Guid Id { get; }
    public string ProgramName { get; }
    public string CardLabel { get; }
    public string ProgressText { get; }
    public string StatusText { get; }
    public string ActivityText { get; }
    public bool CanGiveReward { get; }
    public Visibility RewardButtonVisibility { get; }
    public IReadOnlyList<LoyaltyStampViewItem> Stamps { get; }
}

public sealed class LoyaltyStampViewItem
{
    public LoyaltyStampViewItem(Guid cardId, string programName, int index,
        DateTime? date, LoyaltyPurchaseRecord? purchase, bool isArchived)
    {
        CardId = cardId;
        ProgramName = programName;
        Index = index;
        LegacyDate = date;
        Purchase = purchase;
        IsArchived = isArchived;
        DisplayText = purchase is not null ? "✓" : date.HasValue ? "!" : (index + 1).ToString();
        StampBackground = purchase is not null ? "#E87520" : date.HasValue ? "#FFF1D6" : "#F8FAFC";
        StampBorder = purchase is not null ? "#E87520" : date.HasValue ? "#E5B059" : "#CBD5E1";
        StampForeground = purchase is not null ? "#FFFFFF" : date.HasValue ? "#9A5800" : "#667085";
        DateTooltip = purchase is not null
            ? $"Sac n° {index + 1} : {purchase.ProductName} · {purchase.Format} · " +
              $"{purchase.PricePaid:0.00} $ · {purchase.PurchasedAt:dd/MM/yyyy}"
            : date.HasValue
                ? $"Sac n° {index + 1} : ancienne coche à compléter ({date:dd/MM/yyyy})"
                : $"Sac n° {index + 1} : cliquez pour enregistrer l'achat";
    }

    public Guid CardId { get; }
    public string ProgramName { get; }
    public int Index { get; }
    public DateTime? LegacyDate { get; }
    public LoyaltyPurchaseRecord? Purchase { get; }
    public bool IsArchived { get; }
    public string DisplayText { get; }
    public string StampBackground { get; }
    public string StampBorder { get; }
    public string StampForeground { get; }
    public string DateTooltip { get; }
}
