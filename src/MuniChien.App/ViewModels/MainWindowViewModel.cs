using MuniChien.App.Navigation;
using MuniChien.App.Services;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using MuniChien.App.Views;

namespace MuniChien.App.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    // ==================================================
    // ÉTAT
    // ==================================================

    private ViewModelBase _currentViewModel;

    // Une seule source de services par fenêtre ; fournie au futur bootstrap API.
    private readonly IApplicationServices _services;
    private readonly NotificationCenterService _notificationCenter;
    private bool _isNotificationPanelOpen;
    private string _notificationError = string.Empty;

    // Connexion de démonstration : état conservé uniquement en mémoire.
    // N'accorde aucun droit côté API / serveur.
    private bool _isAdminLoggedIn;

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

    public MainWindowViewModel(IApplicationServices? services = null)
    {
        _services = services ?? new LocalApplicationServices();
        _notificationCenter = new NotificationCenterService(_services);

        _currentViewModel =
            new HomeViewModel(NavigateFromHome, _services.Dashboard);


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

        ToggleAdminLoginCommand = new RelayCommand(ToggleAdminLogin);
        ToggleNotificationsCommand = new RelayCommand(ToggleNotifications);
        RefreshNotificationsCommand = new RelayCommand(RefreshNotifications);
        MarkAllNotificationsReadCommand = new RelayCommand(MarkAllNotificationsRead);
        OpenNotificationCommand = new ParameterRelayCommand<NotificationDisplayItem>(OpenNotification);
        MarkNotificationReadCommand = new ParameterRelayCommand<NotificationDisplayItem>(MarkNotificationRead);
        DeleteNotificationCommand = new ParameterRelayCommand<NotificationDisplayItem>(DeleteNotification);
        RefreshNotifications();
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

    public ICommand ToggleAdminLoginCommand { get; }

    // ==================================================
    // NOTIFICATIONS — DÉMONSTRATION LOCALE
    // ==================================================

    public ObservableCollection<NotificationDisplayItem> Notifications { get; } = new();
    public int NotificationCount => Notifications.Count;
    public int UnreadNotificationCount => Notifications.Count(item => !item.IsRead);
    public string UnreadCountText => UnreadNotificationCount > 99 ? "99+" : UnreadNotificationCount.ToString();
    public Visibility NotificationBadgeVisibility => UnreadNotificationCount > 0 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility EmptyNotificationsVisibility => NotificationCount == 0 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility NotificationErrorVisibility => string.IsNullOrWhiteSpace(_notificationError) ? Visibility.Collapsed : Visibility.Visible;
    public string NotificationError => _notificationError;
    public string NotificationSummary => NotificationCount == 0
        ? "Aucune alerte actuellement"
        : $"{UnreadNotificationCount} non lue(s) sur {NotificationCount}";
    public bool IsNotificationPanelOpen
    {
        get => _isNotificationPanelOpen;
        set
        {
            if (_isNotificationPanelOpen == value) return;
            _isNotificationPanelOpen = value;
            OnPropertyChanged();
        }
    }
    public ICommand ToggleNotificationsCommand { get; }
    public ICommand RefreshNotificationsCommand { get; }
    public ICommand MarkAllNotificationsReadCommand { get; }
    public ICommand OpenNotificationCommand { get; }
    public ICommand MarkNotificationReadCommand { get; }
    public ICommand DeleteNotificationCommand { get; }

    private void ToggleNotifications()
    {
        if (IsNotificationPanelOpen)
        {
            IsNotificationPanelOpen = false;
            return;
        }
        RefreshNotifications();
        IsNotificationPanelOpen = true;
    }

    public void RefreshNotifications()
    {
        try
        {
            var fresh = _notificationCenter.GetNotifications(DateTime.Today);
            Notifications.Clear();
            foreach (var item in fresh)
                Notifications.Add(new NotificationDisplayItem(item, _notificationCenter.IsRead(item.Key)));
            _notificationError = string.Empty;
        }
        catch (Exception)
        {
            // Ne pas inventer un nombre de notifications si une source est inaccessible.
            Notifications.Clear();
            _notificationError = "Impossible de charger les notifications. Réessayez.";
        }
        NotifyNotificationState();
    }

    private void MarkNotificationRead(NotificationDisplayItem item)
    {
        _notificationCenter.MarkRead(item.Entry.Key);
        RefreshNotifications();
    }

    private void DeleteNotification(NotificationDisplayItem item)
    {
        if (item is null) return;

        // Masquer une alerte n’est jamais équivalent à renouveler une licence
        // ou à remettre un cadeau : aucune donnée métier n’est modifiée.
        IsNotificationPanelOpen = false;
        var confirmation = MessageBox.Show(
            $"Supprimer « {item.Title} » de vos notifications ?\n\n" +
            "Le dossier concerné restera inchangé. Cette alerte restera masquée " +
            "jusqu’à la fermeture de l’application.",
            "Supprimer la notification", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirmation == MessageBoxResult.Yes)
        {
            _notificationCenter.Dismiss(item.Entry.Key);
            RefreshNotifications();
        }
        IsNotificationPanelOpen = true;
    }

    private void MarkAllNotificationsRead()
    {
        _notificationCenter.MarkAllRead(Notifications.Select(item => item.Entry));
        RefreshNotifications();
    }

    private void OpenNotification(NotificationDisplayItem item)
    {
        _notificationCenter.MarkRead(item.Entry.Key);
        IsNotificationPanelOpen = false;
        switch (item.Entry.Destination)
        {
            case NotificationDestination.Dog:
                var dog = _services.Animals.GetDog(item.Entry.DogId);
                if (dog is not null)
                    OpenDog(dog);
                else
                    ShowLicenses(); // Dossier modifié ou supprimé depuis la génération.
                break;
            case NotificationDestination.Loyalty:
                OpenLoyaltyNotification(item.Entry.OwnerId);
                break;
            case NotificationDestination.Notices:
                ShowNotices();
                break;
        }
        RefreshNotifications();
    }

    private void OpenLoyaltyNotification(int ownerId)
    {
        var owner = _services.Animals.GetOwner(ownerId);
        if (owner is null) { ShowOwners(); return; }
        OpenOwner(ownerId);
        // Ouvre réellement les cartes correspondantes, pas seulement le répertoire.
        var dialog = new LoyaltyCardsDialog(ownerId, owner.FullName);
        if (Application.Current?.MainWindow is Window window) dialog.Owner = window;
        dialog.ShowDialog();
        if (CurrentViewModel is OwnersViewModel viewModel)
        {
            viewModel.RefreshLoyalty();
            viewModel.RefreshClientList();
        }
    }

    private void NotifyNotificationState()
    {
        OnPropertyChanged(nameof(NotificationCount));
        OnPropertyChanged(nameof(UnreadNotificationCount));
        OnPropertyChanged(nameof(UnreadCountText));
        OnPropertyChanged(nameof(NotificationBadgeVisibility));
        OnPropertyChanged(nameof(EmptyNotificationsVisibility));
        OnPropertyChanged(nameof(NotificationSummary));
        OnPropertyChanged(nameof(NotificationError));
        OnPropertyChanged(nameof(NotificationErrorVisibility));
    }

    public bool IsAdminLoggedIn => _isAdminLoggedIn;

    public string AdminButtonText =>
        IsAdminLoggedIn ? "Admin · Déconnexion" : "Connexion admin";


    // ==================================================
    // PAGES
    // ==================================================

    private void ShowHome()
    {
        NavigateTo(
            new HomeViewModel(NavigateFromHome, _services.Dashboard),
            "Accueil");
    }


    private void ShowSearch()
    {
        NavigateTo(
            new SearchViewModel(OpenDog, OpenOwner, _services.Animals),
            "Recherche");
    }


    private void ShowOwners()
    {
        NavigateTo(
            new OwnersViewModel(
                OpenDog,
                directoryService: _services.Animals),
            "Clients");
    }


    private void ShowDogs()
    {
        NavigateTo(
            new DogsViewModel(
                null,
                OpenOwner,
                _services.Licenses,
                _services.Animals),
            "Chiens");
    }


    private void ShowLicenses()
    {
        NavigateTo(
            new LicensesViewModel(
                OpenDog,
                _services.Licenses),
            "Licences");
    }


    private void ShowPayments()
    {
        NavigateTo(
            new PaymentsViewModel(_services.Payments),
            "Paiements");
    }


    private void ShowNotices()
    {
        NavigateTo(new NoticesViewModel(_services.Notices), "Avis");
    }

    private void ShowReports()
    {
        NavigateTo(
            new ReportsViewModel(),
            "Rapports");
    }


    private void ShowAdministration()
    {
        if (!EnsureAdminLogin())
        {
            // Rétablit la sélection du menu après une connexion annulée.
            SelectNavigation(string.Empty);
            SelectNavigation(PageTitle);
            OnPropertyChanged(nameof(IsAdministrationSelected));
            return;
        }

        NavigateTo(
            new AdministrationViewModel(LogoutAdmin),
            "Administration");
    }


    // ==================================================
    // RACCOURCIS DE L'ACCUEIL
    // ==================================================

    private void NavigateFromHome(string page)
    {
        switch (page)
        {
            case "Recherche": ShowSearch(); break;
            case "Clients":
            case "Propriétaires": ShowOwners(); break;
            case "Chiens": ShowDogs(); break;
            case "Licences": ShowLicenses(); break;
            case "Paiements": ShowPayments(); break;
            case "Avis": ShowNotices(); break;
            case "Rapports": ShowReports(); break;
        }
    }


    // ==================================================
    // CONNEXION ADMINISTRATEUR (DÉMONSTRATION LOCALE)
    // ==================================================

    private bool EnsureAdminLogin()
    {
        if (_isAdminLoggedIn)
            return true;

        var dialog = new AdminLoginDialog();
        if (Application.Current?.MainWindow is Window mainWindow)
            dialog.Owner = mainWindow;

        if (dialog.ShowDialog() != true)
            return false;

        _isAdminLoggedIn = true;
        OnPropertyChanged(nameof(IsAdminLoggedIn));
        OnPropertyChanged(nameof(AdminButtonText));
        return true;
    }

    private void ToggleAdminLogin()
    {
        if (!_isAdminLoggedIn)
        {
            // Se connecter depuis l'en-tête ouvre la page Administration.
            ShowAdministration();
            return;
        }

        if (MessageBox.Show(
                "Fermer la session administrateur de démonstration ?",
                "Déconnexion administrateur",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            LogoutAdmin();
        }
    }

    private void LogoutAdmin()
    {
        _isAdminLoggedIn = false;
        OnPropertyChanged(nameof(IsAdminLoggedIn));
        OnPropertyChanged(nameof(AdminButtonText));

        // Ne laisse pas le panneau Administration ouvert après déconnexion.
        if (IsAdministrationSelected)
            ShowHome();
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
                OpenOwner,
                _services.Licenses,
                _services.Animals),
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
                ownerId,
                _services.Animals,
                openDetail: true),
            "Clients");
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
        RefreshNotifications();
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
            page == "Clients";

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