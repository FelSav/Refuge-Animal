using MuniChien.App.Services;
using System.Collections.ObjectModel;
namespace MuniChien.App.ViewModels;

public class HomeViewModel : ViewModelBase
{
    private readonly DashboardLayoutService _layoutService = new();

    public HomeViewModel()
    {
        DashboardCards =
        [
            new("activeDogs", "Chiens actifs", "1248"),
            new("licensesToRenew", "Licences à renouveler", "37"),
            new("unpaidBalances", "Soldes impayés", "18"),
            new("paymentsToday", "Paiements aujourd'hui", "12"),
            new("dogsWithoutLicense", "Chiens sans licence", "64"),
            new("inactiveDogs", "Chiens inactifs", "95"),
            new("noticesToSend", "Avis à envoyer", "25"),
            new("paymentsThisMonth", "Paiements ce mois-ci", "798")
        ];
        ApplySavedOrder();
    }

    public void SaveDashboardOrder()
    {
        _layoutService.SaveOrder(
            DashboardCards.Select(card => card.Key));
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
            if (cardsByKey.TryGetValue(key, out DashboardCardViewModel? card))
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

    public ObservableCollection<DashboardCardViewModel> DashboardCards { get; }
}