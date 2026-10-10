using MuniChien.App.Navigation;
using MuniChien.App.Services;
using MuniChien.App.Views;
using System.Windows.Input;
using System.Globalization;

namespace MuniChien.App.ViewModels;

public class ReportsViewModel : ViewModelBase
{
    // ==================================================
    // FILTRES
    // ==================================================

    private readonly LocalFinancialReportService _finance =
        new(new LocalPaymentService());

    private DateTime? _selectedDailyDate = DateTime.Today;
    private string _financialReceived = "—";
    private string _financialOutstanding = "—";
    private string _financialLateFees = "—";

    private string _selectedMunicipality =
        "Toutes les municipalités";

    private string _selectedPeriod =
        "Toutes";


    // ==================================================
    // CONSTRUCTEUR
    // ==================================================

    public ReportsViewModel()
    {
        MunicipalityOptions =
            BuildMunicipalityOptions();


        PeriodOptions =
        [
            "Toutes",
            "Aujourd'hui",
            "7 derniers jours",
            "Ce mois",
            "Cette année"
        ];


        GenerateDogsReportCommand =
            new RelayCommand(
                GenerateDogsReport);

        GenerateOwnersReportCommand =
            new RelayCommand(
                GenerateOwnersReport);

        GenerateBreedReportCommand =
            new RelayCommand(
                GenerateBreedReport);

        GenerateLicensesReportCommand =
            new RelayCommand(
                GenerateLicensesReport);

        GenerateBalancesReportCommand = new RelayCommand(GenerateBalancesReport);
        GenerateLateFeesReportCommand = new RelayCommand(GenerateLateFeesReport);
        GeneratePaymentsReportCommand = new RelayCommand(GeneratePaymentsReport);
        GenerateRevenueReportCommand = new RelayCommand(GenerateRevenueReport);
        GenerateDailyPaymentsReportCommand = new RelayCommand(GenerateDailyPaymentsReport);

        UpdateFinancialOverview();
    }


    // ==================================================
    // OPTIONS
    // ==================================================

    public IReadOnlyList<string>
        MunicipalityOptions
    { get; }


    public IReadOnlyList<string>
        PeriodOptions
    { get; }


    // ==================================================
    // FILTRES
    // ==================================================

    public string SelectedMunicipality
    {
        get => _selectedMunicipality;

        set
        {
            if (_selectedMunicipality == value)
            {
                return;
            }

            _selectedMunicipality =
                value;

            OnPropertyChanged();
            UpdateFinancialOverview();
        }
    }


    public string SelectedPeriod
    {
        get => _selectedPeriod;

        set
        {
            if (_selectedPeriod == value)
            {
                return;
            }

            _selectedPeriod =
                value;

            OnPropertyChanged();
        }
    }


    public DateTime? SelectedDailyDate
    {
        get => _selectedDailyDate;
        set
        {
            DateTime? normalizedDate = value?.Date ?? DateTime.Today;
            if (_selectedDailyDate == normalizedDate) return;
            _selectedDailyDate = normalizedDate;
            OnPropertyChanged();
        }
    }

    public string FinancialReceived
    {
        get => _financialReceived;
        private set { _financialReceived = value; OnPropertyChanged(); }
    }

    public string FinancialOutstanding
    {
        get => _financialOutstanding;
        private set { _financialOutstanding = value; OnPropertyChanged(); }
    }

    public string FinancialLateFees
    {
        get => _financialLateFees;
        private set { _financialLateFees = value; OnPropertyChanged(); }
    }

    private void UpdateFinancialOverview()
    {
        var overview = _finance.GetOverview(SelectedMunicipality);
        var culture = CultureInfo.GetCultureInfo("fr-CA");
        FinancialReceived = overview.Received.ToString("C2", culture);
        FinancialOutstanding = overview.Outstanding.ToString("C2", culture);
        FinancialLateFees = overview.LateFees.ToString("C2", culture);
    }

    // ==================================================
    // COMMANDES
    // ==================================================

    public ICommand GenerateDogsReportCommand { get; }

    public ICommand GenerateOwnersReportCommand { get; }

    public ICommand GenerateBreedReportCommand { get; }

    public ICommand GenerateLicensesReportCommand { get; }
    public ICommand GenerateBalancesReportCommand { get; }
    public ICommand GenerateLateFeesReportCommand { get; }
    public ICommand GeneratePaymentsReportCommand { get; }
    public ICommand GenerateRevenueReportCommand { get; }
    public ICommand GenerateDailyPaymentsReportCommand { get; }

    private void ShowFinancialReport(FinancialReportResult report)
    {
        OpenPreview(report.Title, report.Description, report.Period,
            report.Headers, report.Rows, report.Summary);
    }

    private void GenerateBalancesReport() =>
        ShowFinancialReport(_finance.GenerateUnpaidBalances(SelectedMunicipality));

    private void GenerateLateFeesReport() =>
        ShowFinancialReport(_finance.GenerateLateFees(SelectedMunicipality));

    private void GeneratePaymentsReport() =>
        ShowFinancialReport(_finance.GeneratePayments(SelectedMunicipality, SelectedPeriod));

    private void GenerateRevenueReport() =>
        ShowFinancialReport(_finance.GenerateRevenue(SelectedMunicipality, SelectedPeriod));

    private void GenerateDailyPaymentsReport() =>
        ShowFinancialReport(_finance.GenerateDailyPayments(
            SelectedMunicipality, SelectedDailyDate ?? DateTime.Today));


    // ==================================================
    // RAPPORT : CHIENS
    // ==================================================

    private void GenerateDogsReport()
    {
        IEnumerable<LicenseListItemViewModel> licenses =
            FilterByMunicipality(
                LocalLicenseStore.Licenses);


        List<ReportPreviewRowViewModel> rows =
            licenses
                .OrderBy(
                    license =>
                        license.DogName)
                .Select(
                    license =>
                        new ReportPreviewRowViewModel
                        {
                            Value1 =
                                license.DogName,

                            Value2 =
                                license.OwnerName,

                            Value3 =
                                license.Breed,

                            Value4 =
                                license.Municipality,

                            Value5 =
                                license.Sterilized,

                            Value6 =
                                license.LicenseNumber
                        })
                .ToList();


        OpenPreview(
            title:
                "Liste des chiens",

            description:
                "Liste actuelle des chiens possédant une licence dans MuniChien.",

            periodDisplay:
                "État actuel",

            headers:
            [
                "Chien",
                "Propriétaire",
                "Race",
                "Municipalité",
                "Stérilisé",
                "Licence"
            ],

            rows:
                rows);
    }


    // ==================================================
    // RAPPORT : PROPRIÉTAIRES
    // ==================================================

    private void GenerateOwnersReport()
    {
        IEnumerable<LicenseListItemViewModel> licenses =
            FilterByMunicipality(
                LocalLicenseStore.Licenses);


        var owners =
            licenses
                .GroupBy(
                    license =>
                        new
                        {
                            license.OwnerId,
                            license.OwnerName
                        })
                .OrderBy(
                    group =>
                        group.Key.OwnerName)
                .ToList();


        List<ReportPreviewRowViewModel> rows =
            owners
                .Select(
                    owner =>
                        new ReportPreviewRowViewModel
                        {
                            Value1 =
                                owner.Key.OwnerName,

                            Value2 =
                                owner
                                    .Select(x => x.Municipality)
                                    .FirstOrDefault()
                                ?? "—",

                            Value3 =
                                owner.Count().ToString(),

                            Value4 =
                                string.Join(
                                    ", ",
                                    owner
                                        .Select(x => x.DogName)
                                        .OrderBy(x => x)),

                            Value5 =
                                owner.Count(
                                        x =>
                                            x.Status == "Valide")
                                    .ToString(),

                            Value6 =
                                owner.Count(
                                        x =>
                                            x.Status == "Expirée")
                                    .ToString()
                        })
                .ToList();


        OpenPreview(
            title:
                "Liste des propriétaires",

            description:
                "Vue actuelle des propriétaires présents dans les données de licences.",

            periodDisplay:
                "État actuel",

            headers:
            [
                "Propriétaire",
                "Municipalité",
                "Chiens",
                "Noms des chiens",
                "Licences valides",
                "Expirées"
            ],

            rows:
                rows);
    }


    // ==================================================
    // RAPPORT : CHIENS PAR RACE
    // ==================================================

    private void GenerateBreedReport()
    {
        IEnumerable<LicenseListItemViewModel> licenses =
            FilterByMunicipality(
                LocalLicenseStore.Licenses);


        int totalDogs =
            licenses.Count();


        List<ReportPreviewRowViewModel> rows =
            licenses
                .GroupBy(
                    license =>
                        license.Breed)
                .OrderByDescending(
                    group =>
                        group.Count())
                .ThenBy(
                    group =>
                        group.Key)
                .Select(
                    group =>
                    {
                        double percentage =
                            totalDogs == 0
                                ? 0
                                : group.Count() * 100.0 / totalDogs;


                        return new ReportPreviewRowViewModel
                        {
                            Value1 =
                                group.Key,

                            Value2 =
                                group.Count().ToString(),

                            Value3 =
                                $"{percentage:0.#} %",

                            Value4 =
                                string.Join(
                                    ", ",
                                    group
                                        .Select(x => x.DogName)
                                        .OrderBy(x => x)),

                            Value5 =
                                string.Empty,

                            Value6 =
                                string.Empty
                        };
                    })
                .ToList();


        OpenPreview(
            title:
                "Chiens par race",

            description:
                "Répartition actuelle des chiens selon leur race.",

            periodDisplay:
                "État actuel",

            headers:
            [
                "Race",
                "Nombre",
                "Pourcentage",
                "Chiens",
                "",
                ""
            ],

            rows:
                rows);
    }


    // ==================================================
    // RAPPORT : LICENCES
    // ==================================================

    private void GenerateLicensesReport()
    {
        IEnumerable<LicenseListItemViewModel> licenses =
            FilterByMunicipality(
                LocalLicenseStore.Licenses);


        licenses =
            FilterLicensesByPeriod(
                licenses);


        List<ReportPreviewRowViewModel> rows =
            licenses
                .OrderBy(
                    license =>
                        license.ExpirationDate)
                .Select(
                    license =>
                        new ReportPreviewRowViewModel
                        {
                            Value1 =
                                license.LicenseNumber,

                            Value2 =
                                license.DogName,

                            Value3 =
                                license.OwnerName,

                            Value4 =
                                license.Municipality,

                            Value5 =
                                license.ExpirationDate
                                    .ToString(
                                        "yyyy-MM-dd"),

                            Value6 =
                                license.Status
                        })
                .ToList();


        OpenPreview(
            title:
                "Rapport des licences",

            description:
                "Licences correspondant aux paramètres sélectionnés. La période est appliquée à la date d'émission.",

            periodDisplay:
                SelectedPeriod,

            headers:
            [
                "Licence",
                "Chien",
                "Propriétaire",
                "Municipalité",
                "Expiration",
                "Statut"
            ],

            rows:
                rows);
    }


    // ==================================================
    // FILTRE MUNICIPALITÉ
    // ==================================================

    private IEnumerable<LicenseListItemViewModel>
        FilterByMunicipality(
            IEnumerable<LicenseListItemViewModel> source)
    {
        if (SelectedMunicipality ==
            "Toutes les municipalités")
        {
            return source;
        }


        return source.Where(
            license =>
                license.Municipality.Equals(
                    SelectedMunicipality,
                    StringComparison.OrdinalIgnoreCase));
    }


    // ==================================================
    // FILTRE PÉRIODE
    //
    // Pour l'instant, la période des licences
    // correspond à leur date d'émission.
    // ==================================================

    private IEnumerable<LicenseListItemViewModel>
        FilterLicensesByPeriod(
            IEnumerable<LicenseListItemViewModel> source)
    {
        DateTime today =
            DateTime.Today;


        return SelectedPeriod switch
        {
            "Aujourd'hui" =>
                source.Where(
                    license =>
                        license.IssueDate.Date ==
                        today),

            "7 derniers jours" =>
                source.Where(
                    license =>
                        license.IssueDate.Date >=
                        today.AddDays(-6) &&
                        license.IssueDate.Date <=
                        today),

            "Ce mois" =>
                source.Where(
                    license =>
                        license.IssueDate.Year ==
                        today.Year &&
                        license.IssueDate.Month ==
                        today.Month),

            "Cette année" =>
                source.Where(
                    license =>
                        license.IssueDate.Year ==
                        today.Year),

            _ =>
                source
        };
    }


    // ==================================================
    // MUNICIPALITÉS
    // ==================================================

    private static IReadOnlyList<string>
        BuildMunicipalityOptions()
    {
        List<string> municipalities =
            LocalLicenseStore.Licenses
                .Select(
                    license =>
                        license.Municipality)
                .Where(
                    municipality =>
                        !string.IsNullOrWhiteSpace(
                            municipality))
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    municipality =>
                        municipality)
                .ToList();


        municipalities.Insert(
            0,
            "Toutes les municipalités");


        return municipalities;
    }


    // ==================================================
    // APERÇU
    // ==================================================

    private void OpenPreview(
        string title,
        string description,
        string periodDisplay,
        IReadOnlyList<string> headers,
        IReadOnlyList<ReportPreviewRowViewModel> rows,
        string summary = "")
    {
        ReportPreviewDialog dialog =
            new(
                title,
                description,
                SelectedMunicipality,
                periodDisplay,
                headers,
                rows,
                summary);


        if (System.Windows.Application.Current?.MainWindow
            is System.Windows.Window mainWindow)
        {
            dialog.Owner =
                mainWindow;
        }


        dialog.ShowDialog();
    }
}