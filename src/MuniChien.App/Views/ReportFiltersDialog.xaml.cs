using MuniChien.App.Services;
using System.Globalization;
using System.Windows;

namespace MuniChien.App.Views;

/// <summary>Une fenêtre de filtres spécialisée pour chacun des neuf rapports.</summary>
public partial class ReportFiltersDialog : Window
{
    private readonly ReportKind _kind;

    public ReportFiltersDialog(ReportKind kind)
    {
        _kind = kind;
        ReportTitle = kind switch
        {
            ReportKind.Dogs => "Chiens",
            ReportKind.Clients => "Clients et leurs chiens",
            ReportKind.UnpaidBalances => "Soldes impayés",
            ReportKind.LateFees => "Frais de retard",
            ReportKind.Payments => "Paiements",
            ReportKind.Revenue => "Revenus",
            ReportKind.DailyPayments => "Paiements journaliers",
            ReportKind.Breeds => "Chiens par race",
            _ => "Licences"
        };
        MunicipalityOptions = [ReportFilterCriteria.AllMunicipalities, .. LocalLicenseStore.Licenses
            .Select(x => x.Municipality)
            .Concat(LocalAnimalDirectoryStore.GetOwners().Select(x => x.Municipality))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)];
        BreedOptions = [ReportFilterCriteria.AllBreeds, .. LocalAnimalDirectoryStore.GetDogs()
            .Select(x => x.Breed)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)];
        SexOptions = [ReportFilterCriteria.AllSexes, "Mâle", "Femelle"];
        SterilizationOptions = [ReportFilterCriteria.AllSterilizations, "Oui", "Non"];
        StatusOptions = kind == ReportKind.Licenses
            ? [ReportFilterCriteria.AllStatuses, "Valide", "À renouveler", "Expirée"]
            : [ReportFilterCriteria.AllStatuses, "Valide", "À renouveler", "Expirée", "Sans licence"];
        PeriodOptions = ["Toutes", "Aujourd'hui", "7 derniers jours", "Ce mois", "Cette année"];
        PaymentMethodOptions = [ReportFilterCriteria.AllMethods, "Chèque", "Autre mode"];
        DirectionOptions = ["Croissant", "Décroissant"];
        SortOptions = kind switch
        {
            ReportKind.Dogs => ["Chien", "Client", "Race", "Municipalité", "Licence"],
            ReportKind.Clients => ["Client", "Municipalité", "Nombre de chiens"],
            ReportKind.UnpaidBalances => ["Solde", "Client", "Municipalité"],
            ReportKind.LateFees => ["Frais", "Client", "Municipalité"],
            ReportKind.Payments or ReportKind.DailyPayments => ["Date", "Montant", "Client", "Reçu"],
            ReportKind.Revenue => ["Mois", "Montant", "Municipalité"],
            ReportKind.Breeds => ["Race", "Nombre", "Pourcentage"],
            _ => ["Date d'expiration", "Date d'émission", "Chien", "Client", "Statut"]
        };
        SelectedSort = SortOptions[0];
        InitializeComponent();
        DataContext = this;
    }

    public string ReportTitle { get; }
    public IReadOnlyList<string> MunicipalityOptions { get; }
    public IReadOnlyList<string> BreedOptions { get; }
    public IReadOnlyList<string> SexOptions { get; }
    public IReadOnlyList<string> SterilizationOptions { get; }
    public IReadOnlyList<string> StatusOptions { get; }
    public IReadOnlyList<string> PeriodOptions { get; }
    public IReadOnlyList<string> PaymentMethodOptions { get; }
    public IReadOnlyList<string> DirectionOptions { get; }
    public IReadOnlyList<string> SortOptions { get; }

    public bool ShowAnimalFilters => _kind is ReportKind.Dogs or ReportKind.Clients or ReportKind.Breeds or ReportKind.Licenses;
    public bool ShowAnimalDetails => _kind is ReportKind.Dogs or ReportKind.Breeds or ReportKind.Licenses;
    public bool ShowClientExplanation => _kind == ReportKind.Clients;
    public bool ShowDateFilters => _kind is ReportKind.UnpaidBalances or ReportKind.LateFees or ReportKind.Payments or ReportKind.Revenue or ReportKind.DailyPayments or ReportKind.Licenses;
    public bool ShowPeriod => _kind is ReportKind.Payments or ReportKind.Revenue or ReportKind.Licenses;
    public bool ShowLicensePeriodHint => _kind == ReportKind.Licenses;
    public bool ShowDay => _kind == ReportKind.DailyPayments;
    public bool ShowMoneyFilters => _kind is ReportKind.UnpaidBalances or ReportKind.LateFees or ReportKind.Payments or ReportKind.Revenue or ReportKind.DailyPayments;
    public bool ShowPaymentMethod => _kind is ReportKind.Payments or ReportKind.Revenue or ReportKind.DailyPayments;
    public string DateSectionTitle => _kind switch
    {
        ReportKind.Licenses => "2. Période d’émission",
        ReportKind.UnpaidBalances or ReportKind.LateFees => "2. Montant à sélectionner",
        _ => "2. Dates et montants"
    };
    public string MinimumLabel => _kind switch
    {
        ReportKind.UnpaidBalances => "Solde minimum ($)",
        ReportKind.LateFees => "Frais minimum ($)",
        _ => "Paiement minimum ($)"
    };

    public string SelectedMunicipality { get; set; } = ReportFilterCriteria.AllMunicipalities;
    public string SearchText { get; set; } = string.Empty;
    public string SelectedBreed { get; set; } = ReportFilterCriteria.AllBreeds;
    public string SelectedSex { get; set; } = ReportFilterCriteria.AllSexes;
    public string SelectedSterilization { get; set; } = ReportFilterCriteria.AllSterilizations;
    public string SelectedStatus { get; set; } = ReportFilterCriteria.AllStatuses;
    public string SelectedPeriod { get; set; } = "Toutes";
    public DateTime? SelectedDay { get; set; } = DateTime.Today;
    public string SelectedMethod { get; set; } = ReportFilterCriteria.AllMethods;
    public string MinimumText { get; set; } = string.Empty;
    public string SelectedSort { get; set; }
    public string SelectedDirection { get; set; } = "Croissant";
    public ReportFilterCriteria? Result { get; private set; }

    private void Generate_Click(object sender, RoutedEventArgs e)
    {
        decimal? amount = null;
        if (ShowMoneyFilters && !string.IsNullOrWhiteSpace(MinimumText))
        {
            // Les espaces et virgules du français canadien sont acceptés.
            var value = MinimumText.Trim();
            decimal number;
            bool parsed = value.Contains(',')
                ? decimal.TryParse(value, NumberStyles.Number,
                    CultureInfo.GetCultureInfo("fr-CA"), out number)
                : decimal.TryParse(value, NumberStyles.Number,
                    CultureInfo.InvariantCulture, out number);
            if (!parsed || number < 0m || decimal.Round(number, 2) != number)
            {
                MessageBox.Show(this, "Entrez un montant positif avec au maximum deux décimales (ex. 25,50).",
                    "Montant minimum invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            amount = number;
        }
        if (ShowDay && SelectedDay is null)
        {
            MessageBox.Show(this, "Choisissez une date pour les paiements journaliers.",
                "Date requise", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Result = new ReportFilterCriteria
        {
            Municipality = SelectedMunicipality,
            SearchText = SearchText.Trim(),
            Breed = SelectedBreed,
            Sex = SelectedSex,
            Sterilized = SelectedSterilization,
            LicenseStatus = SelectedStatus,
            Period = SelectedPeriod,
            Day = SelectedDay?.Date ?? DateTime.Today,
            PaymentMethod = SelectedMethod,
            MinimumAmount = amount,
            SortBy = SelectedSort,
            Descending = SelectedDirection == "Décroissant"
        };
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
