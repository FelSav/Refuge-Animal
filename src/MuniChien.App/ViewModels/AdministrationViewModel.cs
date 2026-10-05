using MuniChien.App.Navigation;
using MuniChien.App.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace MuniChien.App.ViewModels;

public class AdministrationViewModel : ViewModelBase
{
    // ==================================================
    // ÉTAT
    // ==================================================

    private DateTime _lastBackupAt;

    private AdminBackupItemViewModel?
        _selectedBackup;

    private string _operationMessage =
        string.Empty;


    // ==================================================
    // CONSTRUCTEUR
    // ==================================================

    public AdministrationViewModel()
    {
        BackupRetentionDays =
            14;


        CurrentYear =
            DateTime.Today.Year;


        CreateBackupCommand =
            new RelayCommand(
                CreateBackup);

        RestoreBackupCommand =
            new RelayCommand(
                RestoreBackup);

        OpenSensitiveSettingsCommand =
            new RelayCommand(
                OpenSensitiveSettings);

        ManageArchivesCommand =
            new RelayCommand(
                ManageArchives);

        StartAnnualCloseCommand =
            new RelayCommand(
                StartAnnualClose);

        LogoutCommand =
            new RelayCommand(
                Logout);


        LoadExampleBackups();
    }


    // ==================================================
    // INFORMATIONS SYSTÈME
    // ==================================================

    public int BackupRetentionDays { get; }

    public int CurrentYear { get; }


    public string CurrentYearDisplay =>
        CurrentYear.ToString();


    public string NextYearDisplay =>
        (CurrentYear + 1)
            .ToString();


    public string AdminSessionStatus =>
        "Administrateur";


    public string LastBackupDisplay =>
        _lastBackupAt.ToString(
            "yyyy-MM-dd HH:mm");


    public string BackupRetentionDisplay =>
        $"{BackupRetentionDays} jours";


    // ==================================================
    // SAUVEGARDES
    // ==================================================

    public ObservableCollection<AdminBackupItemViewModel>
        Backups
    { get; } =
        [];


    public AdminBackupItemViewModel?
        SelectedBackup
    {
        get =>
            _selectedBackup;

        set
        {
            if (_selectedBackup == value)
            {
                return;
            }

            _selectedBackup =
                value;

            OnPropertyChanged();
        }
    }


    // ==================================================
    // MESSAGE D'OPÉRATION
    // ==================================================

    public string OperationMessage
    {
        get =>
            _operationMessage;

        private set
        {
            if (_operationMessage == value)
            {
                return;
            }

            _operationMessage =
                value;

            OnPropertyChanged();
        }
    }


    // ==================================================
    // COMMANDES
    // ==================================================

    public ICommand CreateBackupCommand { get; }

    public ICommand RestoreBackupCommand { get; }

    public ICommand OpenSensitiveSettingsCommand { get; }

    public ICommand ManageArchivesCommand { get; }

    public ICommand StartAnnualCloseCommand { get; }

    public ICommand LogoutCommand { get; }


    // ==================================================
    // CRÉER UNE SAUVEGARDE
    // ==================================================

    private void CreateBackup()
    {
        MessageBoxResult confirmation =
            MessageBox.Show(
                "Créer une nouvelle sauvegarde de MuniChien maintenant ?",
                "Créer une sauvegarde",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);


        if (confirmation !=
            MessageBoxResult.Yes)
        {
            return;
        }


        AdminBackupItemViewModel backup =
            CreateBackupEntry(
                "Sauvegarde manuelle");


        Backups.Insert(
            0,
            backup);


        SelectedBackup =
            backup;


        _lastBackupAt =
            backup.CreatedAt;


        OnPropertyChanged(
            nameof(LastBackupDisplay));


        OperationMessage =
            $"Sauvegarde créée avec succès à {backup.CreatedAtDisplay}.";
    }


    // ==================================================
    // RESTAURER
    // ==================================================

    private void RestoreBackup()
    {
        OperationMessage =
            string.Empty;


        if (SelectedBackup is null)
        {
            OperationMessage =
                "Veuillez sélectionner une sauvegarde à restaurer.";

            return;
        }


        MessageBoxResult confirmation =
            MessageBox.Show(
                $"Vous êtes sur le point de restaurer la sauvegarde :\n\n" +
                $"{SelectedBackup.DisplayName}\n\n" +
                "Dans la version finale, cette opération remplacera les données actuelles.\n\n" +
                "Voulez-vous continuer avec la simulation ?",
                "Confirmer la restauration",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);


        if (confirmation !=
            MessageBoxResult.Yes)
        {
            return;
        }


        // ==================================================
        // SIMULATION LOCALE UNIQUEMENT
        //
        // Aucun fichier et aucune donnée ne sont réellement
        // remplacés tant que le backend n'est pas connecté.
        // ==================================================

        OperationMessage =
            $"Restauration simulée de la sauvegarde {SelectedBackup.CreatedAtDisplay}. Aucune donnée réelle n'a été modifiée.";
    }


    // ==================================================
    // PARAMÈTRES SENSIBLES
    // ==================================================

    private void OpenSensitiveSettings()
    {
        MessageBox.Show(
            "La gestion des paramètres sensibles sera reliée aux paramètres réels de la municipalité et à l'API.\n\n" +
            "Cette section pourra notamment contenir les tarifs, frais et règles administratives.",
            "Paramètres sensibles",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }


    // ==================================================
    // ARCHIVES
    // ==================================================

    private void ManageArchives()
    {
        MessageBox.Show(
            "La gestion des données archivées sera disponible lorsque les règles d'archivage et la base de données seront finalisées.",
            "Données archivées",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }


    // ==================================================
    // FERMETURE ANNUELLE
    // ==================================================

    private void StartAnnualClose()
    {
        AnnualCloseDialog dialog =
            new(
                CurrentYear,
                CurrentYear + 1,
                LastBackupDisplay);


        if (Application.Current?.MainWindow
            is Window mainWindow)
        {
            dialog.Owner =
                mainWindow;
        }


        bool? result =
            dialog.ShowDialog();


        if (result != true)
        {
            return;
        }


        // ==================================================
        // Une sauvegarde automatique est simulée avant
        // l'opération, conformément au comportement prévu.
        // ==================================================

        AdminBackupItemViewModel backup =
            CreateBackupEntry(
                "Fermeture annuelle");


        Backups.Insert(
            0,
            backup);


        SelectedBackup =
            backup;


        _lastBackupAt =
            backup.CreatedAt;


        OnPropertyChanged(
            nameof(LastBackupDisplay));


        OperationMessage =
            $"Simulation de la fermeture annuelle {CurrentYear} → {CurrentYear + 1} terminée. " +
            "Une sauvegarde préalable a été créée. Aucune donnée métier n'a encore été modifiée.";
    }


    // ==================================================
    // DÉCONNEXION
    // ==================================================

    private void Logout()
    {
        MessageBox.Show(
            "La déconnexion administrateur sera reliée au système d'authentification lorsqu'il sera branché.\n\n" +
            "Aucune session réelle n'est fermée pour l'instant.",
            "Déconnexion administrateur",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }


    // ==================================================
    // DONNÉES TEMPORAIRES
    // ==================================================

    private void LoadExampleBackups()
    {
        Backups.Clear();


        DateTime todayAtThree =
            DateTime.Today
                .AddHours(3);


        DateTime scheduledBackup =
            DateTime.Now >= todayAtThree
                ? todayAtThree
                : todayAtThree.AddDays(-1);


        Backups.Add(
            new AdminBackupItemViewModel
            {
                Name =
                    $"MuniChien_{scheduledBackup:yyyyMMdd_HHmm}",

                CreatedAt =
                    scheduledBackup,

                Type =
                    "Sauvegarde automatique"
            });


        Backups.Add(
            new AdminBackupItemViewModel
            {
                Name =
                    $"MuniChien_{scheduledBackup.AddDays(-1):yyyyMMdd_HHmm}",

                CreatedAt =
                    scheduledBackup.AddDays(-1),

                Type =
                    "Sauvegarde automatique"
            });


        Backups.Add(
            new AdminBackupItemViewModel
            {
                Name =
                    $"MuniChien_{scheduledBackup.AddDays(-2):yyyyMMdd_HHmm}",

                CreatedAt =
                    scheduledBackup.AddDays(-2),

                Type =
                    "Sauvegarde automatique"
            });


        _lastBackupAt =
            Backups
                .Max(
                    backup =>
                        backup.CreatedAt);


        SelectedBackup =
            Backups.FirstOrDefault();
    }


    // ==================================================
    // CRÉATION D'UNE ENTRÉE
    // ==================================================

    private static AdminBackupItemViewModel
        CreateBackupEntry(
            string type)
    {
        DateTime now =
            DateTime.Now;


        return new AdminBackupItemViewModel
        {
            Name =
                $"MuniChien_{now:yyyyMMdd_HHmmss}",

            CreatedAt =
                now,

            Type =
                type
        };
    }
}