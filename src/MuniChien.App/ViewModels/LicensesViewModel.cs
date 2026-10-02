using MuniChien.App.Navigation;
using MuniChien.App.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MuniChien.App.ViewModels;

public class LicensesViewModel : ViewModelBase
{
    private readonly Action<OwnerDogViewModel>?
        _openDogAction;


    private readonly IReadOnlyList<LicenseListItemViewModel>
        _allLicenses;


    private string _licenseNumberFilter =
        string.Empty;

    private string _dogNameFilter =
        string.Empty;

    private string _ownerNameFilter =
        string.Empty;

    private string _municipalityFilter =
        string.Empty;

    private string _statusFilter =
        "Tous";

    private string _expirationFilter =
        string.Empty;


    private LicenseListItemViewModel?
        _selectedLicense;


    // ==================================================
    // CONSTRUCTEUR
    // ==================================================

    public LicensesViewModel(
        Action<OwnerDogViewModel>? openDogAction = null)
    {
        _openDogAction =
            openDogAction;


        _allLicenses =
            LocalLicenseStore.Licenses;


        StatusOptions =
        [
            "Tous",
            "Valide",
            "À renouveler",
            "Expirée"
        ];


        SearchCommand =
            new RelayCommand(
                ApplyFilters);

        ResetCommand =
            new RelayCommand(
                ResetFilters);

        ShowValidCommand =
            new RelayCommand(
                ShowValid);

        ShowRenewalCommand =
            new RelayCommand(
                ShowRenewal);

        ShowExpiredCommand =
            new RelayCommand(
                ShowExpired);

        OpenSelectedLicenseCommand =
            new RelayCommand(
                OpenSelectedLicense);


        ApplyFilters();
    }


    public IReadOnlyList<string>
        StatusOptions
    { get; }


    public ObservableCollection<LicenseListItemViewModel>
        Licenses
    { get; } =
        [];


    // ==================================================
    // FILTRES
    // ==================================================

    public string LicenseNumberFilter
    {
        get => _licenseNumberFilter;

        set
        {
            if (_licenseNumberFilter == value)
            {
                return;
            }

            _licenseNumberFilter = value;
            OnPropertyChanged();
        }
    }


    public string DogNameFilter
    {
        get => _dogNameFilter;

        set
        {
            if (_dogNameFilter == value)
            {
                return;
            }

            _dogNameFilter = value;
            OnPropertyChanged();
        }
    }


    public string OwnerNameFilter
    {
        get => _ownerNameFilter;

        set
        {
            if (_ownerNameFilter == value)
            {
                return;
            }

            _ownerNameFilter = value;
            OnPropertyChanged();
        }
    }


    public string MunicipalityFilter
    {
        get => _municipalityFilter;

        set
        {
            if (_municipalityFilter == value)
            {
                return;
            }

            _municipalityFilter = value;
            OnPropertyChanged();
        }
    }


    public string StatusFilter
    {
        get => _statusFilter;

        set
        {
            if (_statusFilter == value)
            {
                return;
            }

            _statusFilter = value;
            OnPropertyChanged();
        }
    }


    public string ExpirationFilter
    {
        get => _expirationFilter;

        set
        {
            if (_expirationFilter == value)
            {
                return;
            }

            _expirationFilter = value;
            OnPropertyChanged();
        }
    }


    // ==================================================
    // STATS
    // ==================================================

    public int ValidCount =>
        _allLicenses.Count(
            x => x.Status == "Valide");


    public int RenewalCount =>
        _allLicenses.Count(
            x => x.Status == "À renouveler");


    public int ExpiredCount =>
        _allLicenses.Count(
            x => x.Status == "Expirée");


    public int ResultCount =>
        Licenses.Count;


    public string ResultCountText =>
        ResultCount == 1
            ? "1 résultat"
            : $"{ResultCount} résultats";


    // ==================================================
    // SÉLECTION
    // ==================================================

    public LicenseListItemViewModel?
        SelectedLicense
    {
        get => _selectedLicense;

        set
        {
            if (_selectedLicense == value)
            {
                return;
            }

            _selectedLicense =
                value;

            OnPropertyChanged();
        }
    }


    // ==================================================
    // COMMANDES
    // ==================================================

    public ICommand SearchCommand { get; }

    public ICommand ResetCommand { get; }

    public ICommand ShowValidCommand { get; }

    public ICommand ShowRenewalCommand { get; }

    public ICommand ShowExpiredCommand { get; }

    public ICommand OpenSelectedLicenseCommand { get; }


    // ==================================================
    // OUVRIR
    // ==================================================

    private void OpenSelectedLicense()
    {
        if (SelectedLicense is null)
        {
            return;
        }


        _openDogAction?.Invoke(
            SelectedLicense.ToDogViewModel());
    }


    // ==================================================
    // FILTRAGE
    // ==================================================

    private void ApplyFilters()
    {
        IEnumerable<LicenseListItemViewModel>
            filtered =
            _allLicenses;


        if (!string.IsNullOrWhiteSpace(
                LicenseNumberFilter))
        {
            filtered =
                filtered.Where(
                    x =>
                        ContainsIgnoreCase(
                            x.LicenseNumber,
                            LicenseNumberFilter));
        }


        if (!string.IsNullOrWhiteSpace(
                DogNameFilter))
        {
            filtered =
                filtered.Where(
                    x =>
                        ContainsIgnoreCase(
                            x.DogName,
                            DogNameFilter));
        }


        if (!string.IsNullOrWhiteSpace(
                OwnerNameFilter))
        {
            filtered =
                filtered.Where(
                    x =>
                        ContainsIgnoreCase(
                            x.OwnerName,
                            OwnerNameFilter));
        }


        if (!string.IsNullOrWhiteSpace(
                MunicipalityFilter))
        {
            filtered =
                filtered.Where(
                    x =>
                        ContainsIgnoreCase(
                            x.Municipality,
                            MunicipalityFilter));
        }


        if (!string.IsNullOrWhiteSpace(
                ExpirationFilter))
        {
            filtered =
                filtered.Where(
                    x =>
                        ContainsIgnoreCase(
                            x.ExpirationDateDisplay,
                            ExpirationFilter));
        }


        if (StatusFilter != "Tous")
        {
            filtered =
                filtered.Where(
                    x =>
                        x.Status ==
                        StatusFilter);
        }


        Licenses.Clear();


        foreach (
            LicenseListItemViewModel item
            in filtered)
        {
            Licenses.Add(
                item);
        }


        SelectedLicense =
            null;


        OnPropertyChanged(
            nameof(ValidCount));

        OnPropertyChanged(
            nameof(RenewalCount));

        OnPropertyChanged(
            nameof(ExpiredCount));

        OnPropertyChanged(
            nameof(ResultCount));

        OnPropertyChanged(
            nameof(ResultCountText));
    }


    private void ResetFilters()
    {
        LicenseNumberFilter =
            string.Empty;

        DogNameFilter =
            string.Empty;

        OwnerNameFilter =
            string.Empty;

        MunicipalityFilter =
            string.Empty;

        StatusFilter =
            "Tous";

        ExpirationFilter =
            string.Empty;


        ApplyFilters();
    }


    private void ShowValid()
    {
        StatusFilter =
            "Valide";

        ApplyFilters();
    }


    private void ShowRenewal()
    {
        StatusFilter =
            "À renouveler";

        ApplyFilters();
    }


    private void ShowExpired()
    {
        StatusFilter =
            "Expirée";

        ApplyFilters();
    }


    private static bool ContainsIgnoreCase(
        string source,
        string filter)
    {
        return source.Contains(
            filter.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }
}