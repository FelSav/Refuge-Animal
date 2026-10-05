using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

    private DashboardMetricDefinition? _selectedCustomMetric;
    private string _editCustomMetricTitle = string.Empty;
    private string _editCustomMetricValue = string.Empty;

    private string _customizationMessage = string.Empty;


    // ==================================================
    // CONSTRUCTEUR
    // ==================================================

    public HomeViewModel()
    {
        AvailableMetrics = CreateAvailableMetrics();

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

        UpdateManualMetricCommand =
            new RelayCommand(UpdateManualMetric);

        DeleteManualMetricCommand =
            new RelayCommand(DeleteManualMetric);

        ApplySavedLayout();
    }


    // ==================================================
    // COLLECTIONS
    // ==================================================

    public ObservableCollection<DashboardCardViewModel> DashboardCards { get; }

    public ObservableCollection<DashboardMetricDefinition> AvailableMetrics { get; }


    // ==================================================
    // LISTES FILTRÉES POUR L'INTERFACE
    // ==================================================

    public IEnumerable<DashboardMetricDefinition> AvailableReplacementMetrics
    {
        get
        {
            string? selectedKey = SelectedCard?.Key;

            HashSet<string> unavailableKeys =
                DashboardCards
                    .Where(card => card.Key != selectedKey)
                    .Select(card => card.Key)
                    .ToHashSet();

            return AvailableMetrics
                .Where(metric =>
                    !unavailableKeys.Contains(metric.Key));
        }
    }

    public IEnumerable<DashboardMetricDefinition> CustomMetrics =>
        AvailableMetrics.Where(metric => metric.IsManual);


    // ==================================================
    // ÉTAT DU PANNEAU
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


    // ==================================================
    // CARTE / STATISTIQUE SÉLECTIONNÉE
    // ==================================================

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
            OnPropertyChanged(nameof(AvailableReplacementMetrics));

            if (_selectedCard is not null)
            {
                SelectedMetric =
                    AvailableMetrics.FirstOrDefault(metric =>
                        metric.Key == _selectedCard.Key);
            }
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


    // ==================================================
    // CRÉATION D'UNE STATISTIQUE PERSONNALISÉE
    // ==================================================

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


    // ==================================================
    // MODIFICATION D'UNE STATISTIQUE PERSONNALISÉE
    // ==================================================

    public DashboardMetricDefinition? SelectedCustomMetric
    {
        get => _selectedCustomMetric;

        set
        {
            if (_selectedCustomMetric == value)
            {
                return;
            }

            _selectedCustomMetric = value;
            OnPropertyChanged();

            EditCustomMetricTitle =
                value?.Title ?? string.Empty;

            EditCustomMetricValue =
                value?.ManualValue ?? string.Empty;
        }
    }

    public string EditCustomMetricTitle
    {
        get => _editCustomMetricTitle;

        set
        {
            if (_editCustomMetricTitle == value)
            {
                return;
            }

            _editCustomMetricTitle = value;
            OnPropertyChanged();
        }
    }

    public string EditCustomMetricValue
    {
        get => _editCustomMetricValue;

        set
        {
            if (_editCustomMetricValue == value)
            {
                return;
            }

            _editCustomMetricValue = value;
            OnPropertyChanged();
        }
    }


    // ==================================================
    // MESSAGE UTILISATEUR
    // ==================================================

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

    public ICommand UpdateManualMetricCommand { get; }

    public ICommand DeleteManualMetricCommand { get; }


    // ==================================================
    // OUVERTURE / FERMETURE DU PANNEAU
    // ==================================================

    private void OpenCustomization()
    {
        CustomizationMessage = string.Empty;

        if (SelectedCard is null &&
            DashboardCards.Count > 0)
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
            if (item.Source != DashboardCardSource.Manual ||
                string.IsNullOrWhiteSpace(item.Title))
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
    // DASHBOARD PAR DÉFAUT
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
    // CRÉER UNE STATISTIQUE PERSONNALISÉE
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

        AvailableMetrics.Add(customMetric);

        SaveCustomMetrics();

        SelectedMetric = customMetric;
        SelectedCustomMetric = customMetric;

        ManualCardTitle = string.Empty;
        ManualCardValue = string.Empty;

        RefreshMetricLists();

        CustomizationMessage =
            "La statistique personnalisée a été ajoutée.";
    }


    // ==================================================
    // MODIFIER UNE STATISTIQUE PERSONNALISÉE
    // ==================================================

    private void UpdateManualMetric()
    {
        CustomizationMessage = string.Empty;

        if (SelectedCustomMetric is null)
        {
            CustomizationMessage =
                "Sélectionnez une statistique personnalisée.";

            return;
        }

        if (string.IsNullOrWhiteSpace(EditCustomMetricTitle))
        {
            CustomizationMessage =
                "Entrez un nom.";

            return;
        }

        if (string.IsNullOrWhiteSpace(EditCustomMetricValue))
        {
            CustomizationMessage =
                "Entrez une valeur.";

            return;
        }

        string title = EditCustomMetricTitle.Trim();
        string value = EditCustomMetricValue.Trim();

        bool duplicateTitle =
            AvailableMetrics.Any(metric =>
                metric.Key != SelectedCustomMetric.Key &&
                string.Equals(
                    metric.Title,
                    title,
                    StringComparison.OrdinalIgnoreCase));

        if (duplicateTitle)
        {
            CustomizationMessage =
                "Une statistique portant ce nom existe déjà.";

            return;
        }

        int metricIndex =
            AvailableMetrics.IndexOf(
                SelectedCustomMetric);

        if (metricIndex < 0)
        {
            return;
        }

        DashboardMetricDefinition updatedMetric =
            new(
                SelectedCustomMetric.Key,
                title,
                DashboardCardSource.Manual,
                value);

        AvailableMetrics[metricIndex] =
            updatedMetric;

        DashboardCardViewModel? displayedCard =
            DashboardCards.FirstOrDefault(card =>
                card.Key == updatedMetric.Key);

        if (displayedCard is not null)
        {
            displayedCard.Title = title;
            displayedCard.Value = value;

            SaveDashboardLayout();
        }

        if (SelectedMetric?.Key ==
            updatedMetric.Key)
        {
            SelectedMetric = updatedMetric;
        }

        SelectedCustomMetric = updatedMetric;

        SaveCustomMetrics();
        RefreshMetricLists();

        CustomizationMessage =
            "La statistique personnalisée a été modifiée.";
    }


    // ==================================================
    // SUPPRIMER UNE STATISTIQUE PERSONNALISÉE
    // ==================================================

    private void DeleteManualMetric()
    {
        CustomizationMessage = string.Empty;

        if (SelectedCustomMetric is null)
        {
            CustomizationMessage =
                "Sélectionnez une statistique personnalisée.";

            return;
        }

        DashboardMetricDefinition metricToDelete =
            SelectedCustomMetric;

        DashboardCardViewModel? displayedCard =
            DashboardCards.FirstOrDefault(card =>
                card.Key == metricToDelete.Key);

        DashboardCardViewModel? replacementCard = null;

        if (displayedCard is not null)
        {
            DashboardMetricDefinition? replacementMetric =
                FindUnusedAutomaticMetric(
                    metricToDelete.Key);

            if (replacementMetric is null)
            {
                CustomizationMessage =
                    "Impossible de supprimer cette statistique pour le moment.";

                return;
            }

            int index =
                DashboardCards.IndexOf(displayedCard);

            replacementCard =
                CreateCardFromMetricDefinition(
                    replacementMetric);

            DashboardCards[index] =
                replacementCard;

            if (SelectedCard?.Key ==
                metricToDelete.Key)
            {
                SelectedCard =
                    replacementCard;
            }
        }

        AvailableMetrics.Remove(metricToDelete);

        if (SelectedMetric?.Key ==
            metricToDelete.Key)
        {
            SelectedMetric =
                replacementCard is null
                    ? null
                    : AvailableMetrics.FirstOrDefault(metric =>
                        metric.Key == replacementCard.Key);
        }

        SelectedCustomMetric = null;

        SaveCustomMetrics();
        SaveDashboardLayout();

        RefreshMetricLists();

        CustomizationMessage =
            "La statistique personnalisée a été supprimée.";
    }

    private DashboardMetricDefinition? FindUnusedAutomaticMetric(
        string ignoredKey)
    {
        HashSet<string> displayedKeys =
            DashboardCards
                .Where(card =>
                    card.Key != ignoredKey)
                .Select(card => card.Key)
                .ToHashSet();

        return AvailableMetrics
            .FirstOrDefault(metric =>
                !metric.IsManual &&
                !displayedKeys.Contains(metric.Key));
    }


    // ==================================================
    // REMPLACER UNE CARTE
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

        int index =
            DashboardCards.IndexOf(SelectedCard);

        if (index < 0)
        {
            return;
        }

        DashboardCardViewModel newCard =
            CreateCardFromMetricDefinition(
                SelectedMetric);

        DashboardCards[index] =
            newCard;

        SelectedCard = newCard;

        SaveDashboardLayout();

        RefreshMetricLists();

        CustomizationMessage =
            "La statistique a été remplacée.";
    }


    // ==================================================
    // RAFRAÎCHIR LES LISTES CALCULÉES
    // ==================================================

    private void RefreshMetricLists()
    {
        OnPropertyChanged(
            nameof(AvailableReplacementMetrics));

        OnPropertyChanged(
            nameof(CustomMetrics));
    }


    // ==================================================
    // SAUVEGARDES
    // ==================================================

    private void SaveCustomMetrics()
    {
        _layoutService.SaveCustomMetrics(
            AvailableMetrics);
    }

    public void SaveDashboardLayout()
    {
        _layoutService.SaveLayout(
            DashboardCards);
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
            if (restoredCards.Count >=
                DashboardCardCount)
            {
                break;
            }

            if (item.Source ==
                DashboardCardSource.Manual)
            {
                customMetricsRecovered |=
                    EnsureManualMetricExistsInCatalog(
                        item);
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

        FillMissingDefaultCards(
            restoredCards);

        DashboardCards.Clear();

        foreach (DashboardCardViewModel card
                 in restoredCards)
        {
            DashboardCards.Add(card);
        }

        if (customMetricsRecovered)
        {
            SaveCustomMetrics();
        }

        RefreshMetricLists();
    }

    private DashboardCardViewModel? CreateCardFromSavedLayout(
        DashboardCardLayoutItem item)
    {
        if (item.Source ==
            DashboardCardSource.Manual)
        {
            return new DashboardCardViewModel(
                item.Key,
                item.Title ?? "Valeur personnalisée",
                item.Value ?? string.Empty,
                DashboardCardSource.Manual);
        }

        return CreateMetricCard(
            item.Key);
    }

    private bool EnsureManualMetricExistsInCatalog(
        DashboardCardLayoutItem item)
    {
        if (item.Source !=
            DashboardCardSource.Manual)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(
                item.Title))
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
                cards.Any(card =>
                    card.Key == key);

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
    // PLUS TARD : API DE MAËL
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