using MuniChien.App.Navigation;
using MuniChien.App.Services;
using MuniChien.App.Views;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace MuniChien.App.ViewModels;

/// <summary>Chaque rapport propose ses filtres avant la génération.</summary>
public class ReportsViewModel : ViewModelBase
{
    private readonly LocalFinancialReportService _finance = new(new LocalPaymentService());
    private string _financialReceived = "—";
    private string _financialOutstanding = "—";
    private string _financialLateFees = "—";

    public ReportsViewModel()
    {
        GenerateDogsReportCommand = new RelayCommand(() => Configure(ReportKind.Dogs));
        GenerateOwnersReportCommand = new RelayCommand(() => Configure(ReportKind.Clients));
        GenerateBreedReportCommand = new RelayCommand(() => Configure(ReportKind.Breeds));
        GenerateLicensesReportCommand = new RelayCommand(() => Configure(ReportKind.Licenses));
        GenerateBalancesReportCommand = new RelayCommand(() => Configure(ReportKind.UnpaidBalances));
        GenerateLateFeesReportCommand = new RelayCommand(() => Configure(ReportKind.LateFees));
        GeneratePaymentsReportCommand = new RelayCommand(() => Configure(ReportKind.Payments));
        GenerateRevenueReportCommand = new RelayCommand(() => Configure(ReportKind.Revenue));
        GenerateDailyPaymentsReportCommand = new RelayCommand(() => Configure(ReportKind.DailyPayments));
        var overview = _finance.GetOverview(ReportFilterCriteria.AllMunicipalities);
        var culture = CultureInfo.GetCultureInfo("fr-CA");
        FinancialReceived = overview.Received.ToString("C2", culture);
        FinancialOutstanding = overview.Outstanding.ToString("C2", culture);
        FinancialLateFees = overview.LateFees.ToString("C2", culture);
    }

    public string FinancialReceived { get => _financialReceived; private set { _financialReceived = value; OnPropertyChanged(); } }
    public string FinancialOutstanding { get => _financialOutstanding; private set { _financialOutstanding = value; OnPropertyChanged(); } }
    public string FinancialLateFees { get => _financialLateFees; private set { _financialLateFees = value; OnPropertyChanged(); } }

    public ICommand GenerateDogsReportCommand { get; }
    public ICommand GenerateOwnersReportCommand { get; }
    public ICommand GenerateBreedReportCommand { get; }
    public ICommand GenerateLicensesReportCommand { get; }
    public ICommand GenerateBalancesReportCommand { get; }
    public ICommand GenerateLateFeesReportCommand { get; }
    public ICommand GeneratePaymentsReportCommand { get; }
    public ICommand GenerateRevenueReportCommand { get; }
    public ICommand GenerateDailyPaymentsReportCommand { get; }

    private void Configure(ReportKind kind)
    {
        try
        {
            var window = new ReportFiltersDialog(kind);
            if (Application.Current?.MainWindow is Window mainWindow) window.Owner = mainWindow;
            if (window.ShowDialog() != true || window.Result is not { } criteria) return;
            switch (kind)
            {
                case ReportKind.Dogs: GenerateDogs(criteria); break;
                case ReportKind.Clients: GenerateClients(criteria); break;
                case ReportKind.Breeds: GenerateBreeds(criteria); break;
                case ReportKind.Licenses: GenerateLicenses(criteria); break;
                case ReportKind.UnpaidBalances: ShowFinancial(_finance.GenerateUnpaidBalances(criteria), criteria); break;
                case ReportKind.LateFees: ShowFinancial(_finance.GenerateLateFees(criteria), criteria); break;
                case ReportKind.Payments: ShowFinancial(_finance.GeneratePayments(criteria), criteria); break;
                case ReportKind.Revenue: ShowFinancial(_finance.GenerateRevenue(criteria), criteria); break;
                case ReportKind.DailyPayments: ShowFinancial(_finance.GenerateDailyPayments(criteria), criteria); break;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Le rapport n'a pas pu être généré.\n\n{ex.Message}",
                "Rapports", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ShowFinancial(FinancialReportResult report, ReportFilterCriteria c) =>
        OpenPreview(report.Title, report.Description, c.Municipality, report.Period,
            report.Headers, report.Rows, report.Summary);

    // Liste source = répertoire CHIENS, y compris chiens sans licence.
    private static IEnumerable<OwnerDogViewModel> FilterDogs(ReportFilterCriteria c)
    {
        var licenses = LocalLicenseStore.Licenses.GroupBy(x => x.DogId)
            .ToDictionary(x => x.Key, x => x.First());
        return LocalAnimalDirectoryStore.GetDogs()
            .Where(d =>
            {
                var owner = LocalAnimalDirectoryStore.GetOwner(d.OwnerId);
                licenses.TryGetValue(d.DogId, out var license);
                return ReportFilterRules.Choice(owner?.Municipality, c.Municipality,
                           ReportFilterCriteria.AllMunicipalities)
                       && ReportFilterRules.Search(c.SearchText, d.DogName, d.OwnerName,
                           d.Breed, d.LicenseNumber, owner?.FileNumber, owner?.Phone, owner?.CellPhone)
                       && ReportFilterRules.Choice(d.Breed, c.Breed, ReportFilterCriteria.AllBreeds)
                       && ReportFilterRules.Choice(d.Sex, c.Sex, ReportFilterCriteria.AllSexes)
                       && ReportFilterRules.Choice(d.Sterilized, c.Sterilized, ReportFilterCriteria.AllSterilizations)
                       && ReportFilterRules.Choice(license?.Status ?? "Sans licence", c.LicenseStatus,
                           ReportFilterCriteria.AllStatuses);
            });
    }

    private void GenerateDogs(ReportFilterCriteria c)
    {
        var dogs = FilterDogs(c);
        IEnumerable<OwnerDogViewModel> sorted = c.SortBy switch
        {
            "Client" => Sort(dogs, x => x.OwnerName, c.Descending),
            "Race" => Sort(dogs, x => x.Breed, c.Descending),
            "Municipalité" => Sort(dogs, x => LocalAnimalDirectoryStore.GetOwner(x.OwnerId)?.Municipality ?? "", c.Descending),
            "Licence" => Sort(dogs, x => x.LicenseNumber, c.Descending),
            _ => Sort(dogs, x => x.DogName, c.Descending)
        };
        var rows = sorted.Select(x => new ReportPreviewRowViewModel
        {
            Value1 = x.DogName,
            Value2 = LocalAnimalDirectoryStore.GetOwner(x.OwnerId)?.FullName ?? x.OwnerName,
            Value3 = x.Breed,
            Value4 = LocalAnimalDirectoryStore.GetOwner(x.OwnerId)?.Municipality ?? "—",
            Value5 = x.Sterilized,
            Value6 = string.IsNullOrWhiteSpace(x.LicenseNumber) ? "Sans licence" : x.LicenseNumber
        }).ToList();
        OpenPreview("Liste des chiens", "Chiens du répertoire, y compris ceux sans licence.",
            c.Municipality, c.Describe(ReportKind.Dogs),
            ["Chien", "Client", "Race", "Municipalité", "Stérilisé", "Licence"], rows,
            $"{rows.Count} chien(s) dans le rapport");
    }

    private void GenerateClients(ReportFilterCriteria c)
    {
        // Même les clients sans chien sont inclus si aucun filtre sur les chiens n'est demandé.
        var matchingDogs = FilterDogs(c).ToLookup(x => x.OwnerId);
        bool dogCriteria = c.Breed != ReportFilterCriteria.AllBreeds ||
                           c.LicenseStatus != ReportFilterCriteria.AllStatuses;
        var owners = LocalAnimalDirectoryStore.GetOwners()
            .Where(x => ReportFilterRules.Choice(x.Municipality, c.Municipality,
                ReportFilterCriteria.AllMunicipalities))
            .Where(x => ReportFilterRules.Search(c.SearchText, x.FullName, x.FileNumber,
                x.Phone, x.CellPhone) || matchingDogs.Contains(x.OwnerId))
            .Where(x => !dogCriteria || matchingDogs.Contains(x.OwnerId));
        IEnumerable<OwnerDirectoryItem> sorted = c.SortBy switch
        {
            "Municipalité" => Sort(owners, x => x.Municipality, c.Descending),
            "Nombre de chiens" => Sort(owners, x => matchingDogs[x.OwnerId].Count(), c.Descending),
            _ => Sort(owners, x => x.FullName, c.Descending)
        };
        var statuses = LocalLicenseStore.Licenses.ToLookup(x => x.DogId);
        var rows = sorted.Select(x => new ReportPreviewRowViewModel
        {
            Value1 = x.FullName,
            Value2 = string.IsNullOrWhiteSpace(x.Municipality) ? "—" : x.Municipality,
            Value3 = matchingDogs[x.OwnerId].Count().ToString(),
            Value4 = string.Join(", ", matchingDogs[x.OwnerId].Select(d => d.DogName).OrderBy(n => n)),
            Value5 = matchingDogs[x.OwnerId].Sum(d => statuses[d.DogId].Count(l => l.Status == "Valide")).ToString(),
            Value6 = matchingDogs[x.OwnerId].Sum(d => statuses[d.DogId].Count(l => l.Status == "Expirée")).ToString()
        }).ToList();
        OpenPreview("Liste des clients", "Clients du répertoire; chiens correspondants aux filtres.",
            c.Municipality, c.Describe(ReportKind.Clients),
            ["Client", "Municipalité", "Chiens", "Noms des chiens", "Licences valides", "Expirées"], rows,
            $"{rows.Count} client(s) dans le rapport");
    }

    private void GenerateBreeds(ReportFilterCriteria c)
    {
        var dogs = FilterDogs(c).ToList();
        var groups = dogs.GroupBy(x => string.IsNullOrWhiteSpace(x.Breed) ? "Non précisée" : x.Breed).ToList();
        IEnumerable<IGrouping<string, OwnerDogViewModel>> sorted = c.SortBy switch
        {
            "Nombre" or "Pourcentage" => Sort(groups, x => x.Count(), c.Descending),
            _ => Sort(groups, x => x.Key, c.Descending)
        };
        var rows = sorted.Select(group => new ReportPreviewRowViewModel
        {
            Value1 = group.Key,
            Value2 = group.Count().ToString(),
            Value3 = dogs.Count == 0 ? "0 %" : $"{group.Count() * 100.0 / dogs.Count:0.#} %",
            Value4 = string.Join(", ", group.Select(x => x.DogName).OrderBy(x => x))
        }).ToList();
        OpenPreview("Chiens par race", "Répartition des chiens du répertoire après filtrage.",
            c.Municipality, c.Describe(ReportKind.Breeds),
            ["Race", "Nombre", "Pourcentage", "Chiens", "", ""], rows,
            $"{dogs.Count} chien(s) · {groups.Count} race(s)");
    }

    private void GenerateLicenses(ReportFilterCriteria c)
    {
        var selected = LocalLicenseStore.Licenses
            .Where(l =>
            {
                var owner = LocalAnimalDirectoryStore.GetOwner(l.OwnerId);
                return ReportFilterRules.Choice(l.Municipality, c.Municipality,
                           ReportFilterCriteria.AllMunicipalities)
                       && ReportFilterRules.Search(c.SearchText, l.LicenseNumber, l.DogName,
                           l.OwnerName, l.Breed, owner?.FileNumber, owner?.Phone, owner?.CellPhone)
                       && ReportFilterRules.Choice(l.Breed, c.Breed, ReportFilterCriteria.AllBreeds)
                       && ReportFilterRules.Choice(l.Sex, c.Sex, ReportFilterCriteria.AllSexes)
                       && ReportFilterRules.Choice(l.Sterilized, c.Sterilized, ReportFilterCriteria.AllSterilizations)
                       && ReportFilterRules.Choice(l.Status, c.LicenseStatus, ReportFilterCriteria.AllStatuses)
                       && ReportFilterRules.Period(l.IssueDate, c.Period, DateTime.Today);
            });
        IEnumerable<LicenseListItemViewModel> sorted = c.SortBy switch
        {
            "Date d'émission" => Sort(selected, x => x.IssueDate, c.Descending),
            "Chien" => Sort(selected, x => x.DogName, c.Descending),
            "Client" => Sort(selected, x => x.OwnerName, c.Descending),
            "Statut" => Sort(selected, x => x.Status, c.Descending),
            _ => Sort(selected, x => x.ExpirationDate, c.Descending)
        };
        var rows = sorted.Select(x => new ReportPreviewRowViewModel
        {
            Value1 = x.LicenseNumber,
            Value2 = x.DogName,
            Value3 = x.OwnerName,
            Value4 = x.Municipality,
            Value5 = x.ExpirationDate.ToString("yyyy-MM-dd"),
            Value6 = x.Status
        }).ToList();
        OpenPreview("Rapport des licences", "Licences correspondant aux paramètres choisis.",
            c.Municipality, c.Describe(ReportKind.Licenses),
            ["Licence", "Chien", "Client", "Municipalité", "Expiration", "Statut"], rows,
            $"{rows.Count} licence(s) dans le rapport");
    }

    private static IEnumerable<T> Sort<T, TKey>(IEnumerable<T> source, Func<T, TKey> selector,
        bool descending) => descending ? source.OrderByDescending(selector) : source.OrderBy(selector);

    private static void OpenPreview(string title, string description, string municipality,
        string details, IReadOnlyList<string> headers,
        IReadOnlyList<ReportPreviewRowViewModel> rows, string summary)
    {
        var dialog = new ReportPreviewDialog(title, description, municipality, details,
            headers, rows, summary);
        if (Application.Current?.MainWindow is Window mainWindow) dialog.Owner = mainWindow;
        dialog.ShowDialog();
    }
}
