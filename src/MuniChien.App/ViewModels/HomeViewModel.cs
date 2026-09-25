using System.Collections.ObjectModel;
using MuniChien.App.Services;

namespace MuniChien.App.ViewModels;

public class HomeViewModel : ViewModelBase
{
    private readonly DashboardLayoutService _layoutService = new();

    public HomeViewModel()
    {
        AvailableMetrics =
        [
            new("activeDogs", "Chiens actifs"),
            new("licensesToRenew", "Licences à renouveler"),
            new("unpaidBalances", "Soldes impayés"),
            new("paymentsToday", "Paiements aujourd'hui"),
            new("dogsWithoutLicense", "Chiens sans licence"),
            new("inactiveDogs", "Chiens inactifs"),
            new("noticesToSend", "Avis à envoyer"),
            new("paymentsThisMonth", "Paiements ce mois-ci"),

            // Statistiques disponibles plus tard
            new("activeOwners", "Propriétaires actifs"),
            new("inactiveOwners", "Propriétaires inactifs"),
            new("expiredLicenses", "Licences expirées"),
            new("monthlyRevenue", "Revenus du mois")
        ];

        DashboardCards =
        [
            CreateCard("activeDogs", "1248"),
            CreateCard("licensesToRenew", "37"),
            CreateCard("unpaidBalances", "18"),
            CreateCard("paymentsToday", "12"),
            CreateCard("dogsWithoutLicense", "64"),
            CreateCard("inactiveDogs", "95"),
            CreateCard("noticesToSend", "25"),
            CreateCard("paymentsThisMonth", "798")
        ];

        ApplySavedOrder();
    }

    public ObservableCollection<DashboardCardViewModel> DashboardCards { get; }

    public IReadOnlyList<DashboardMetricDefinition> AvailableMetrics { get; }

    public void SaveDashboardOrder()
    {
        _layoutService.SaveOrder(
            DashboardCards.Select(card => card.Key));
    }

    private DashboardCardViewModel CreateCard(string key, string value)
    {
        DashboardMetricDefinition metric =
            AvailableMetrics.First(metric => metric.Key == key);

        return new DashboardCardViewModel(
            metric.Key,
            metric.Title,
            value);
    }

    private void ApplySavedOrder()
    {
        IReadOnlyList<string> savedOrder = _layoutService.LoadOrder();

        if (savedOrder.Count == 0)
        {
            return;
        }

        Dictionary<string, DashboardCardViewModel> cardsByKey =
            DashboardCards.ToDictionary(card => card.Key);

        List<DashboardCardViewModel> orderedCards = [];

        foreach (string key in savedOrder)
        {
            if (cardsByKey.TryGetValue(
                    key,
                    out DashboardCardViewModel? card))
            {
                orderedCards.Add(card);
            }
        }

        foreach (DashboardCardViewModel card in DashboardCards)
        {
            if (!orderedCards.Contains(card))
            {
                orderedCards.Add(card);
            }
        }

        DashboardCards.Clear();

        foreach (DashboardCardViewModel card in orderedCards)
        {
            DashboardCards.Add(card);
        }
    }
}