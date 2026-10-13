using MuniChien.App.Services;
using System;
using System.Collections.Generic;
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
        // Le rechargement de la liste peut reinitialiser la position du defilement.
        // On memorise sa position pour eviter les sauts apres un clic sur une case.
        double scrollOffset = CardsScrollViewer.VerticalOffset;
        // Toujours une seule carte active par compagnie, dans un ordre fixe.
        var active = LocalLoyaltyService.GetCards(_ownerId)
            .Select(record => new LoyaltyCardViewItem(record))
            .ToList();
        // Historique des CYCLES terminés, sans ajouter de cartes actives.
        var rewards = LocalLoyaltyService.GetRewardHistory(_ownerId)
            .Select(record => new LoyaltyCardViewItem(record))
            .ToList();

        ActiveCardsList.ItemsSource = active;
        ArchivedCardsList.ItemsSource = rewards;
        ActiveCountText.Text = $"{active.Count} compagnie(s)";
        ArchiveCountText.Text = $"{rewards.Count} cadeau(x) remis";
        EmptyActiveText.Visibility = active.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        EmptyArchiveText.Visibility = rewards.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        if (keepScrollPosition)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded,
                new Action(() => CardsScrollViewer.ScrollToVerticalOffset(scrollOffset)));
        }
    }

    private void AddCard_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            LocalLoyaltyService.CreateCustomCard(_ownerId, CompanyNameTextBox.Text);
            CompanyNameTextBox.Clear();
            RefreshCards(keepScrollPosition: true);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Ajout impossible",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Stamp_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkbox ||
            checkbox.DataContext is not LoyaltyStampViewItem stamp)
            return;

        bool shouldCheck = checkbox.IsChecked == true;
        if (!shouldCheck)
        {
            var choice = MessageBox.Show(this,
                "Retirer cette coche et effacer la date d'achat enregistrée ?",
                "Corriger une coche", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (choice != MessageBoxResult.Yes)
            {
                RefreshCards(keepScrollPosition: true);
                return;
            }
        }

        try
        {
            LocalLoyaltyService.SetPurchase(_ownerId, stamp.CardId, stamp.Index, shouldCheck);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Modification impossible",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        RefreshCards(keepScrollPosition: true);
    }

    private void Reward_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: Guid cardId }) return;

        var answer = MessageBox.Show(this,
            "Confirmer que le cadeau a été remis ?\n\n" +
            "Les 12 dates d'achat seront conservées dans l'historique des cadeaux, " +
            "puis la MÊME carte sera remise à zéro (0/12).",
            "Confirmer la remise du cadeau",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (answer != MessageBoxResult.Yes) return;

        try
        {
            LocalLoyaltyService.GiveRewardAndStartNext(_ownerId, cardId);
            RefreshCards(keepScrollPosition: true);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Remise impossible",
                MessageBoxButton.OK, MessageBoxImage.Warning);
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
        StatusText = record.CompletedPurchases == LocalLoyaltyService.PurchaseCount
            ? "Prête à remettre" : "En cours";
        ActivityText = "Cliquez sur une case pour enregistrer l'achat. La date est visible au survol.";
        CanGiveReward = record.CompletedPurchases == LocalLoyaltyService.PurchaseCount;
        RewardButtonVisibility = Visibility.Visible;
        Stamps = Enumerable.Range(0, LocalLoyaltyService.PurchaseCount)
            .Select(i => new LoyaltyStampViewItem(record.Id, i, record.PurchaseDates[i], true))
            .ToList();
    }

    public LoyaltyCardViewItem(LoyaltyRewardRecord reward)
    {
        Id = reward.CardId;
        ProgramName = reward.ProgramName;
        CardLabel = $"Cycle n° {reward.CycleNumber} · commencé le {reward.CycleStartedAt:dd/MM/yyyy}";
        ProgressText = $"{LocalLoyaltyService.PurchaseCount}/{LocalLoyaltyService.PurchaseCount}";
        StatusText = "Cadeau remis";
        ActivityText = $"Cadeau remis le {reward.RewardGivenAt:dd/MM/yyyy HH:mm} · ancien cycle en lecture seule";
        CanGiveReward = false;
        RewardButtonVisibility = Visibility.Collapsed;
        Stamps = Enumerable.Range(0, LocalLoyaltyService.PurchaseCount)
            .Select(i => new LoyaltyStampViewItem(reward.CardId, i, reward.PurchaseDates[i], false))
            .ToList();
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
    public LoyaltyStampViewItem(Guid cardId, int index, DateTime? date, bool isEditable)
    {
        CardId = cardId;
        Index = index;
        Position = (index + 1).ToString();
        IsChecked = date.HasValue;
        IsEditable = isEditable;
        DateTooltip = date.HasValue
            ? $"Achat n° {index + 1} : {date:dd/MM/yyyy HH:mm}"
            : $"Achat n° {index + 1} : non coché";
    }

    public Guid CardId { get; }
    public int Index { get; }
    public string Position { get; }
    public bool IsChecked { get; }
    public bool IsEditable { get; }
    public string DateTooltip { get; }
}
