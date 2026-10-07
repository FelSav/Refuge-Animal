using MuniChien.App.Navigation;
using MuniChien.App.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MuniChien.App.ViewModels;

public class LicensesViewModel : ViewModelBase
{
    // ==================================================
    // NAVIGATION
    // ==================================================

    private readonly Action<OwnerDogViewModel>?
        _openDogAction;

    private readonly ILicenseService
        _licenseService;


    // ==================================================
    // DONNÉES
    // ==================================================

    private readonly IReadOnlyList<LicenseListItemViewModel>
        _allLicenses;


    // ==================================================
    // FILTRES
    // ==================================================

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


    // ==================================================
    // SÉLECTION
    // ==================================================

    private LicenseListItemViewModel?
        _selectedLicense;


    // ==================================================
    // CONSTRUCTEUR
    // ==================================================

    public LicensesViewModel(
        Action<OwnerDogViewModel>? openDogAction = null,
        ILicenseService? licenseService = null)
    {
        _openDogAction =
            openDogAction;

        _licenseService =
            licenseService ?? new LocalLicenseService();


        _allLicenses =
            _licenseService.GetLicenses();


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


    // ==================================================
    // OPTIONS
    // ==================================================

    public IReadOnlyList<string>
        StatusOptions
    { get; }


    // ==================================================
    // COLLECTION
    // ==================================================

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

            _licenseNumberFilter =
                value;

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

            _dogNameFilter =
                value;

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

            _ownerNameFilter =
                value;

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

            _municipalityFilter =
                value;

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

            _statusFilter =
                value;

            OnPropertyChanged();

            OnPropertyChanged(
                nameof(IsValidFilterActive));

            OnPropertyChanged(
                nameof(IsRenewalFilterActive));

            OnPropertyChanged(
                nameof(IsExpiredFilterActive));
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

            _expirationFilter =
                value;

            OnPropertyChanged();
        }
    }


    // ==================================================
    // ÉTAT DES CARTES
    // ==================================================

    public bool IsValidFilterActive =>
        StatusFilter == "Valide";


    public bool IsRenewalFilterActive =>
        StatusFilter == "À renouveler";


    public bool IsExpiredFilterActive =>
        StatusFilter == "Expirée";


    // ==================================================
    // STATISTIQUES
    // ==================================================

    public int ValidCount =>
        _allLicenses.Count(
            license =>
                license.Status == "Valide");


    public int RenewalCount =>
        _allLicenses.Count(
            license =>
                license.Status == "À renouveler");


    public int ExpiredCount =>
        _allLicenses.Count(
            license =>
                license.Status == "Expirée");


    // ==================================================
    // RÉSULTATS
    // ==================================================

    public int ResultCount =>
        Licenses.Count;


    public bool HasResults =>
        ResultCount > 0;


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
    // OUVRIR UNE LICENCE
    // ==================================================

    private void OpenSelectedLicense()
    {
        if (SelectedLicense is null)
        {
            return;
        }


        _openDogAction?.Invoke(
            SelectedLicense
                .ToDogViewModel());
    }


    // ==================================================
    // RECHERCHE
    // ==================================================

    private void ApplyFilters()
    {
        IEnumerable<LicenseListItemViewModel>
            filtered =
            _allLicenses;


        // ----------------------------------------------
        // NUMÉRO
        // ----------------------------------------------

        if (!string.IsNullOrWhiteSpace(
                LicenseNumberFilter))
        {
            filtered =
                filtered.Where(
                    license =>
                        ContainsIgnoreCase(
                            license.LicenseNumber,
                            LicenseNumberFilter));
        }


        // ----------------------------------------------
        // CHIEN
        // ----------------------------------------------

        if (!string.IsNullOrWhiteSpace(
                DogNameFilter))
        {
            filtered =
                filtered.Where(
                    license =>
                        ContainsIgnoreCase(
                            license.DogName,
                            DogNameFilter));
        }


        // ----------------------------------------------
        // PROPRIÉTAIRE
        // ----------------------------------------------

        if (!string.IsNullOrWhiteSpace(
                OwnerNameFilter))
        {
            filtered =
                filtered.Where(
                    license =>
                        ContainsIgnoreCase(
                            license.OwnerName,
                            OwnerNameFilter));
        }


        // ----------------------------------------------
        // MUNICIPALITÉ
        // ----------------------------------------------

        if (!string.IsNullOrWhiteSpace(
                MunicipalityFilter))
        {
            filtered =
                filtered.Where(
                    license =>
                        ContainsIgnoreCase(
                            license.Municipality,
                            MunicipalityFilter));
        }


        // ----------------------------------------------
        // EXPIRATION
        // ----------------------------------------------

        if (!string.IsNullOrWhiteSpace(
                ExpirationFilter))
        {
            filtered =
                filtered.Where(
                    license =>
                        ContainsIgnoreCase(
                            license.ExpirationDateDisplay,
                            ExpirationFilter));
        }


        // ----------------------------------------------
        // STATUT
        // ----------------------------------------------

        if (!string.IsNullOrWhiteSpace(
                StatusFilter) &&
            StatusFilter != "Tous")
        {
            filtered =
                filtered.Where(
                    license =>
                        license.Status ==
                        StatusFilter);
        }


        // ----------------------------------------------
        // RECHARGE COLLECTION
        // ----------------------------------------------

        Licenses.Clear();


        foreach (
            LicenseListItemViewModel license
            in filtered)
        {
            Licenses.Add(
                license);
        }


        SelectedLicense =
            null;


        RefreshSummary();
    }


    // ==================================================
    // RESET
    // ==================================================

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


    // ==================================================
    // CARTES
    // ==================================================

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


    // ==================================================
    // RAFRAÎCHISSEMENT
    // ==================================================

    private void RefreshSummary()
    {
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

        OnPropertyChanged(
            nameof(HasResults));
    }


    // ==================================================
    // OUTILS
    // ==================================================

    private static bool ContainsIgnoreCase(
        string source,
        string filter)
    {
        return source.Contains(
            filter.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }
}