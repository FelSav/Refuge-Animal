using MuniChien.App.Navigation;
using System.Windows.Input;

namespace MuniChien.App.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    // ==================================================
    // ÉTAT
    // ==================================================

    private ViewModelBase _currentViewModel;

    private string _pageTitle =
        "Accueil";


    // ==================================================
    // SIDEBAR
    // ==================================================

    private bool _isHomeSelected =
        true;

    private bool _isSearchSelected;

    private bool _isOwnersSelected;

    private bool _isDogsSelected;

    private bool _isLicensesSelected;

    private bool _isPaymentsSelected;

    private bool _isNoticesSelected;

    private bool _isReportsSelected;

    private bool _isAdministrationSelected;


    // ==================================================
    // CONSTRUCTEUR
    // ==================================================

    public MainWindowViewModel()
    {
        _currentViewModel =
            new HomeViewModel();


        ShowHomeCommand =
            new RelayCommand(
                ShowHome);

        ShowSearchCommand =
            new RelayCommand(
                ShowSearch);

        ShowOwnersCommand =
            new RelayCommand(
                ShowOwners);

        ShowDogsCommand =
            new RelayCommand(
                ShowDogs);

        ShowLicensesCommand =
            new RelayCommand(
                ShowLicenses);

        ShowPaymentsCommand =
            new RelayCommand(
                ShowPayments);

        ShowNoticesCommand =
            new RelayCommand(ShowNotices);

        ShowReportsCommand =
            new RelayCommand(
                ShowReports);

        ShowAdministrationCommand =
            new RelayCommand(
                ShowAdministration);
    }


    // ==================================================
    // PAGE ACTUELLE
    // ==================================================

    public string PageTitle
    {
        get =>
            _pageTitle;

        private set
        {
            if (_pageTitle == value)
            {
                return;
            }

            _pageTitle =
                value;

            OnPropertyChanged();
        }
    }


    public ViewModelBase CurrentViewModel
    {
        get =>
            _currentViewModel;

        private set
        {
            if (_currentViewModel == value)
            {
                return;
            }

            _currentViewModel =
                value;

            OnPropertyChanged();
        }
    }


    // ==================================================
    // SIDEBAR
    // ==================================================

    public bool IsHomeSelected
    {
        get => _isHomeSelected;

        private set
        {
            if (_isHomeSelected == value)
            {
                return;
            }

            _isHomeSelected = value;

            OnPropertyChanged();
        }
    }


    public bool IsSearchSelected
    {
        get => _isSearchSelected;

        private set
        {
            if (_isSearchSelected == value)
            {
                return;
            }

            _isSearchSelected = value;

            OnPropertyChanged();
        }
    }


    public bool IsOwnersSelected
    {
        get => _isOwnersSelected;

        private set
        {
            if (_isOwnersSelected == value)
            {
                return;
            }

            _isOwnersSelected = value;

            OnPropertyChanged();
        }
    }


    public bool IsDogsSelected
    {
        get => _isDogsSelected;

        private set
        {
            if (_isDogsSelected == value)
            {
                return;
            }

            _isDogsSelected = value;

            OnPropertyChanged();
        }
    }


    public bool IsLicensesSelected
    {
        get => _isLicensesSelected;

        private set
        {
            if (_isLicensesSelected == value)
            {
                return;
            }

            _isLicensesSelected = value;

            OnPropertyChanged();
        }
    }


    public bool IsPaymentsSelected
    {
        get => _isPaymentsSelected;

        private set
        {
            if (_isPaymentsSelected == value)
            {
                return;
            }

            _isPaymentsSelected = value;

            OnPropertyChanged();
        }
    }


    public bool IsNoticesSelected
    {
        get => _isNoticesSelected;
        private set
        {
            if (_isNoticesSelected == value) return;
            _isNoticesSelected = value;
            OnPropertyChanged();
        }
    }

    public bool IsReportsSelected
    {
        get => _isReportsSelected;

        private set
        {
            if (_isReportsSelected == value)
            {
                return;
            }

            _isReportsSelected = value;

            OnPropertyChanged();
        }
    }


    public bool IsAdministrationSelected
    {
        get => _isAdministrationSelected;

        private set
        {
            if (_isAdministrationSelected == value)
            {
                return;
            }

            _isAdministrationSelected = value;

            OnPropertyChanged();
        }
    }


    // ==================================================
    // COMMANDES
    // ==================================================

    public ICommand ShowHomeCommand { get; }

    public ICommand ShowSearchCommand { get; }

    public ICommand ShowOwnersCommand { get; }

    public ICommand ShowDogsCommand { get; }

    public ICommand ShowLicensesCommand { get; }

    public ICommand ShowPaymentsCommand { get; }

    public ICommand ShowNoticesCommand { get; }

    public ICommand ShowReportsCommand { get; }

    public ICommand ShowAdministrationCommand { get; }


    // ==================================================
    // PAGES
    // ==================================================

    private void ShowHome()
    {
        NavigateTo(
            new HomeViewModel(),
            "Accueil");
    }


    private void ShowSearch()
    {
        NavigateTo(
            new SearchViewModel(),
            "Recherche");
    }


    private void ShowOwners()
    {
        NavigateTo(
            new OwnersViewModel(
                OpenDog),
            "Propriétaires");
    }


    private void ShowDogs()
    {
        NavigateTo(
            new DogsViewModel(
                null,
                OpenOwner),
            "Chiens");
    }


    private void ShowLicenses()
    {
        NavigateTo(
            new LicensesViewModel(
                OpenDog),
            "Licences");
    }


    private void ShowPayments()
    {
        NavigateTo(
            new PaymentsViewModel(),
            "Paiements");
    }


    private void ShowNotices()
    {
        NavigateTo(new NoticesViewModel(), "Avis");
    }

    private void ShowReports()
    {
        NavigateTo(
            new ReportsViewModel(),
            "Rapports");
    }


    private void ShowAdministration()
    {
        NavigateTo(
            new AdministrationViewModel(),
            "Administration");
    }


    // ==================================================
    // OUVRIR UN CHIEN
    // ==================================================

    private void OpenDog(
        OwnerDogViewModel dog)
    {
        NavigateTo(
            new DogsViewModel(
                dog,
                OpenOwner),
            "Chiens");
    }


    // ==================================================
    // OUVRIR UN PROPRIÉTAIRE
    // ==================================================

    private void OpenOwner(
        int ownerId)
    {
        NavigateTo(
            new OwnersViewModel(
                OpenDog,
                ownerId),
            "Propriétaires");
    }


    // ==================================================
    // NAVIGATION
    // ==================================================

    private void NavigateTo(
        ViewModelBase viewModel,
        string page)
    {
        CurrentViewModel =
            viewModel;

        PageTitle =
            page;

        SelectNavigation(
            page);
    }


    // ==================================================
    // SIDEBAR
    // ==================================================

    private void SelectNavigation(
        string page)
    {
        IsHomeSelected =
            page == "Accueil";

        IsSearchSelected =
            page == "Recherche";

        IsOwnersSelected =
            page == "Propriétaires";

        IsDogsSelected =
            page == "Chiens";

        IsLicensesSelected =
            page == "Licences";

        IsPaymentsSelected =
            page == "Paiements";

        IsNoticesSelected =
            page == "Avis";

        IsReportsSelected =
            page == "Rapports";

        IsAdministrationSelected =
            page == "Administration";
    }
}