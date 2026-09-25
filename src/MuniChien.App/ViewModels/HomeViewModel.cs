using System.Collections.ObjectModel;
using System.Windows.Input;
using MuniChien.App.Navigation;
using MuniChien.App.Services;

namespace MuniChien.App.ViewModels;

public class HomeViewModel : ViewModelBase
{
    private const int DashboardCardCount = 8;

    private static readonly string[] DefaultMetricKeys =
    [
        "activeDogs",
        "licensesToRenew",
        "unpaidBalances",
        "paymentsToday",
        "dogsWithoutLicense",
        "inactiveDogs",
        "noticesToSend",
        "paymentsThisMonth"
    ];

    private readonly DashboardLayoutService _layoutService = new();

    private bool _isCustomizationOpen;
    private DashboardCardViewModel? _selectedCard;
    private DashboardMetricDefinition? _selectedMetric;
    private string _manualCardTitle = string.Empty;
    private string _manualCardValue = string.Empty;
    private string _customizationMessage = string.Empty;


    // ==================================================
    // CONSTRUCTEUR
    // ==================================================

    public HomeViewModel()
    {
        AvailableMetrics = CreateAvailableMetrics();

        // Charge les statistiques personnalisées créées auparavant.
        LoadCustomMetrics();

        DashboardCards = CreateDefaultDashboard();

        OpenCustomizationCommand =
            new RelayCommand(OpenCustomization);

        CloseCustomizationCommand =
            new RelayCommand(CloseCustomization);

        ReplaceWithMetricCommand =
            new RelayCommand(ReplaceSelectedCardWithMetric);

        CreateManualMetricCommand =
            new RelayCommand(CreateManualMetric);

        ApplySavedLayout();
    }


    // ==================================================
    // COLLECTIONS
    // ==================================================

    public ObservableCollection<DashboardCardViewModel> DashboardCards { get; }

    public ObservableCollection<DashboardMetricDefinition> AvailableMetrics { get; }


    // ==================================================
    // ÉTAT DU PANNEAU DE PERSONNALISATION
    // ==================================================

    public bool IsCustomizationOpen
    {
        get => _isCustomizationOpen;

        set
        {
            if (_isCustomizationOpen == value)
            {
                return;
            }

            _isCustomizationOpen = value;
            OnPropertyChanged();
        }
    }

    public DashboardCardViewModel? SelectedCard
    {
        get => _selectedCard;

        set
        {
            if (_selectedCard == value)
            {
                return;
            }

            _selectedCard = value;
            OnPropertyChanged();
        }
    }

    public DashboardMetricDefinition? SelectedMetric
    {
        get => _selectedMetric;

        set
        {
            if (_selectedMetric == value)
            {
                return;
            }

            _selectedMetric = value;
            OnPropertyChanged();
        }
    }

    public string ManualCardTitle
    {
        get => _manualCardTitle;

        set
        {
            if (_manualCardTitle == value)
            {
                return;
            }

            _manualCardTitle = value;
            OnPropertyChanged();
        }
    }

    public string ManualCardValue
    {
        get => _manualCardValue;

        set
        {
            if (_manualCardValue == value)
            {
                return;
            }

            _manualCardValue = value;
            OnPropertyChanged();
        }
    }

    public string CustomizationMessage
    {
        get => _customizationMessage;

        private set
        {
            if (_customizationMessage == value)
            {
                return;
            }

            _customizationMessage = value;
            OnPropertyChanged();
        }
    }


    // ==================================================
    // COMMANDES
    // ==================================================

    public ICommand OpenCustomizationCommand { get; }

    public ICommand CloseCustomizationCommand { get; }

    public ICommand ReplaceWithMetricCommand { get; }

    public ICommand CreateManualMetricCommand { get; }


    // ==================================================
    // OUVERTURE / FERMETURE DU PANNEAU
    // ==================================================

    private void OpenCustomization()
    {
        CustomizationMessage = string.Empty;

        if (SelectedCard is null && DashboardCards.Count > 0)
        {
            SelectedCard = DashboardCards[0];
        }

        IsCustomizationOpen = true;
    }

    private void CloseCustomization()
    {
        CustomizationMessage = string.Empty;
        IsCustomizationOpen = false;
    }


    // ==================================================
    // CATALOGUE DE STATISTIQUES
    // ==================================================

    private ObservableCollection<DashboardMetricDefinition>
        CreateAvailableMetrics()
    {
        return
        [
            new("activeDogs", "Chiens actifs"),
            new("licensesToRenew", "Licences à renouveler"),
            new("unpaidBalances", "Soldes impayés"),
            new("paymentsToday", "Paiements aujourd'hui"),
            new("dogsWithoutLicense", "Chiens sans licence"),
            new("inactiveDogs", "Chiens inactifs"),
            new("noticesToSend", "Avis à envoyer"),
            new("paymentsThisMonth", "Paiements ce mois-ci"),

            // Statistiques supplémentaires disponibles plus tard via l'API.
            new("activeOwners", "Propriétaires actifs"),
            new("inactiveOwners", "Propriétaires inactifs"),
            new("expiredLicenses", "Licences expirées"),
            new("monthlyRevenue", "Revenus du mois")
        ];
    }

    private void LoadCustomMetrics()
    {
        IReadOnlyList<DashboardCardLayoutItem> savedMetrics =
            _layoutService.LoadCustomMetrics();

        foreach (DashboardCardLayoutItem item in savedMetrics)
        {
            if (item.Source != DashboardCardSource.Manual)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(item.Title))
            {
                continue;
            }

            bool alreadyExists =
                AvailableMetrics.Any(metric =>
                    metric.Key == item.Key);

            if (alreadyExists)
            {
                continue;
            }

            AvailableMetrics.Add(
                new DashboardMetricDefinition(
                    item.Key,
                    item.Title,
                    DashboardCardSource.Manual,
                    item.Value ?? string.Empty));
        }
    }


    // ==================================================
    // CRÉATION DU TABLEAU DE BORD
    // ==================================================

    private ObservableCollection<DashboardCardViewModel>
        CreateDefaultDashboard()
    {
        ObservableCollection<DashboardCardViewModel> cards = [];

        foreach (string key in DefaultMetricKeys)
        {
            DashboardCardViewModel? card =
                CreateMetricCard(key);

            if (card is not null)
            {
                cards.Add(card);
            }
        }

        return cards;
    }

    private DashboardCardViewModel? CreateMetricCard(string key)
    {
        DashboardMetricDefinition? metric =
            AvailableMetrics.FirstOrDefault(metric =>
                metric.Key == key);

        if (metric is null || metric.IsManual)
        {
            return null;
        }

        return new DashboardCardViewModel(
            metric.Key,
            metric.Title,
            GetMetricValue(metric.Key),
            DashboardCardSource.Metric);
    }

    private DashboardCardViewModel CreateCardFromMetricDefinition(
        DashboardMetricDefinition metric)
    {
        if (metric.IsManual)
        {
            return new DashboardCardViewModel(
                metric.Key,
                metric.Title,
                metric.ManualValue ?? string.Empty,
                DashboardCardSource.Manual);
        }

        return new DashboardCardViewModel(
            metric.Key,
            metric.Title,
            GetMetricValue(metric.Key),
            DashboardCardSource.Metric);
    }


    // ==================================================
    // CRÉATION D'UNE STATISTIQUE MANUELLE
    // ==================================================

    private void CreateManualMetric()
    {
        CustomizationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(ManualCardTitle))
        {
            CustomizationMessage =
                "Entrez un nom pour la statistique.";

            return;
        }

        if (string.IsNullOrWhiteSpace(ManualCardValue))
        {
            CustomizationMessage =
                "Entrez une valeur.";

            return;
        }

        string title = ManualCardTitle.Trim();
        string value = ManualCardValue.Trim();

        bool alreadyExists =
            AvailableMetrics.Any(metric =>
                string.Equals(
                    metric.Title,
                    title,
                    StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
        {
            CustomizationMessage =
                "Une statistique portant ce nom existe déjà.";

            return;
        }

        DashboardMetricDefinition customMetric =
            new(
                $"manual-{Guid.NewGuid()}",
                title,
                DashboardCardSource.Manual,
                value);

        // Important :
        // on l'ajoute seulement aux choix disponibles.
        // On ne modifie pas encore le tableau de bord.
        AvailableMetrics.Add(customMetric);

        _layoutService.SaveCustomMetrics(
            AvailableMetrics);

        // On la sélectionne automatiquement dans le ComboBox
        // pour faciliter son utilisation juste après sa création.
        SelectedMetric = customMetric;

        ManualCardTitle = string.Empty;
        ManualCardValue = string.Empty;

        CustomizationMessage =
            "La statistique personnalisée a été ajoutée aux choix.";
    }


    // ==================================================
    // REMPLACEMENT D'UNE CARTE
    // ==================================================

    private void ReplaceSelectedCardWithMetric()
    {
        CustomizationMessage = string.Empty;

        if (SelectedCard is null)
        {
            CustomizationMessage =
                "Sélectionnez une carte à remplacer.";

            return;
        }

        if (SelectedMetric is null)
        {
            CustomizationMessage =
                "Sélectionnez une statistique.";

            return;
        }

        bool alreadyDisplayed =
            DashboardCards.Any(card =>
                card.Key == SelectedMetric.Key &&
                card != SelectedCard);

        if (alreadyDisplayed)
        {
            CustomizationMessage =
                "Cette statistique est déjà affichée.";

            return;
        }

        int index = DashboardCards.IndexOf(SelectedCard);

        if (index < 0)
        {
            return;
        }

        DashboardCardViewModel newCard =
            CreateCardFromMetricDefinition(
                SelectedMetric);

        DashboardCards[index] = newCard;

        SelectedCard = newCard;

        SaveDashboardLayout();

        CustomizationMessage =
            "La statistique a été remplacée.";
    }


    // ==================================================
    // SAUVEGARDE DU TABLEAU DE BORD
    // ==================================================

    public void SaveDashboardLayout()
    {
        _layoutService.SaveLayout(DashboardCards);
    }

    private void ApplySavedLayout()
    {
        IReadOnlyList<DashboardCardLayoutItem> savedLayout =
            _layoutService.LoadLayout();

        if (savedLayout.Count == 0)
        {
            return;
        }

        List<DashboardCardViewModel> restoredCards = [];

        bool customMetricsRecovered = false;

        foreach (DashboardCardLayoutItem item in savedLayout)
        {
            if (restoredCards.Count >= DashboardCardCount)
            {
                break;
            }

            if (item.Source == DashboardCardSource.Manual)
            {
                customMetricsRecovered |=
                    EnsureManualMetricExistsInCatalog(item);
            }

            DashboardCardViewModel? card =
                CreateCardFromSavedLayout(item);

            if (card is null)
            {
                continue;
            }

            bool alreadyAdded =
                restoredCards.Any(existing =>
                    existing.Key == card.Key);

            if (!alreadyAdded)
            {
                restoredCards.Add(card);
            }
        }

        FillMissingDefaultCards(restoredCards);

        DashboardCards.Clear();

        foreach (DashboardCardViewModel card in restoredCards)
        {
            DashboardCards.Add(card);
        }

        // Permet de récupérer automatiquement les anciennes
        // statistiques manuelles créées avant le nouveau catalogue.
        if (customMetricsRecovered)
        {
            _layoutService.SaveCustomMetrics(
                AvailableMetrics);
        }
    }

    private DashboardCardViewModel? CreateCardFromSavedLayout(
        DashboardCardLayoutItem item)
    {
        if (item.Source == DashboardCardSource.Manual)
        {
            return new DashboardCardViewModel(
                item.Key,
                item.Title ?? "Valeur personnalisée",
                item.Value ?? string.Empty,
                DashboardCardSource.Manual);
        }

        return CreateMetricCard(item.Key);
    }

    private bool EnsureManualMetricExistsInCatalog(
        DashboardCardLayoutItem item)
    {
        if (item.Source != DashboardCardSource.Manual)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(item.Title))
        {
            return false;
        }

        bool alreadyExists =
            AvailableMetrics.Any(metric =>
                metric.Key == item.Key);

        if (alreadyExists)
        {
            return false;
        }

        AvailableMetrics.Add(
            new DashboardMetricDefinition(
                item.Key,
                item.Title,
                DashboardCardSource.Manual,
                item.Value ?? string.Empty));

        return true;
    }

    private void FillMissingDefaultCards(
        List<DashboardCardViewModel> cards)
    {
        foreach (string key in DefaultMetricKeys)
        {
            if (cards.Count >= DashboardCardCount)
            {
                return;
            }

            bool alreadyDisplayed =
                cards.Any(card => card.Key == key);

            if (alreadyDisplayed)
            {
                continue;
            }

            DashboardCardViewModel? card =
                CreateMetricCard(key);

            if (card is not null)
            {
                cards.Add(card);
            }
        }
    }


    // ==================================================
    // VALEURS TEMPORAIRES
    // PLUS TARD : REMPLACÉES PAR LES DONNÉES DE L'API
    // ==================================================

    private string GetMetricValue(string key)
    {
        return key switch
        {
            "activeDogs" => "1248",
            "licensesToRenew" => "37",
            "unpaidBalances" => "18",
            "paymentsToday" => "12",
            "dogsWithoutLicense" => "64",
            "inactiveDogs" => "95",
            "noticesToSend" => "25",
            "paymentsThisMonth" => "798",

            "activeOwners" => "—",
            "inactiveOwners" => "—",
            "expiredLicenses" => "—",
            "monthlyRevenue" => "—",

            _ => "—"
        };
    }
}