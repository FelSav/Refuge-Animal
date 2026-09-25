using System.Collections.ObjectModel;
using MuniChien.App.Services;
using System.Windows.Input;
using MuniChien.App.Navigation;

namespace MuniChien.App.ViewModels;

public class HomeViewModel : ViewModelBase
{
    private readonly DashboardLayoutService _layoutService = new();

    public HomeViewModel()
    {
        OpenCustomizationCommand =
            new RelayCommand(() => IsCustomizationOpen = true);

        CloseCustomizationCommand =
            new RelayCommand(() => IsCustomizationOpen = false);

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
        ApplySavedLayout();

    }

    public ObservableCollection<DashboardCardViewModel> DashboardCards { get; }

    public IReadOnlyList<DashboardMetricDefinition> AvailableMetrics { get; }

    public void SaveDashboardLayout()
    {
        _layoutService.SaveLayout(DashboardCards);
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

    private void ApplySavedLayout()
    {
        IReadOnlyList<DashboardCardLayoutItem> savedLayout =
            _layoutService.LoadLayout();

        if (savedLayout.Count == 0)
        {
            return;
        }

        Dictionary<string, DashboardCardViewModel> metricCards =
            DashboardCards.ToDictionary(card => card.Key);

        List<DashboardCardViewModel> configuredCards = [];

        foreach (DashboardCardLayoutItem item in savedLayout)
        {
            if (item.Source == DashboardCardSource.Manual)
            {
                configuredCards.Add(
                    new DashboardCardViewModel(
                        item.Key,
                        item.Title ?? "Valeur personnalisée",
                        item.Value ?? string.Empty,
                        DashboardCardSource.Manual));

                continue;
            }

            if (metricCards.TryGetValue(
                    item.Key,
                    out DashboardCardViewModel? metricCard))
            {
                configuredCards.Add(metricCard);
            }
        }

        // Pour l'instant on garde également les statistiques
        // par défaut qui ne seraient pas encore dans le fichier.
        foreach (DashboardCardViewModel card in DashboardCards)
        {
            if (!configuredCards.Contains(card))
            {
                configuredCards.Add(card);
            }
        }

        DashboardCards.Clear();

        foreach (DashboardCardViewModel card in configuredCards)
        {
            DashboardCards.Add(card);
        }
    }

    public void AddManualCard(string title, string value)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return;
        }

        string key = $"manual-{Guid.NewGuid()}";

        DashboardCards.Add(
            new DashboardCardViewModel(
                key,
                title.Trim(),
                value.Trim(),
                DashboardCardSource.Manual));

        SaveDashboardLayout();
    }

    private bool _isCustomizationOpen;

    public bool IsCustomizationOpen
    {
        get => _isCustomizationOpen;
        set
        {
            _isCustomizationOpen = value;
            OnPropertyChanged();
        }
    }

    public ICommand OpenCustomizationCommand { get; }
    public ICommand CloseCustomizationCommand { get; }
}