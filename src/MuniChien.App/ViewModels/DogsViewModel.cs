using MuniChien.App.Navigation;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MuniChien.App.ViewModels;

public class DogsViewModel : ViewModelBase
{
    private readonly OwnerDogViewModel? _dog;
    private readonly Action<int>? _openOwnerAction;

    private bool _isEditing;


    // ==================================================
    // CONSTRUCTEUR
    // ==================================================

    public DogsViewModel(
        OwnerDogViewModel? dog = null,
        Action<int>? openOwnerAction = null)
    {
        _dog = dog;
        _openOwnerAction = openOwnerAction;

        ModifyCommand =
            new RelayCommand(StartEditing);

        SaveCommand =
            new RelayCommand(SaveChanges);

        CancelCommand =
            new RelayCommand(CancelChanges);

        RenewLicenseCommand =
            new RelayCommand(RenewLicense);

        OpenOwnerCommand =
            new RelayCommand(OpenOwner);


        if (_dog is not null)
        {
            LicenseHistory.Add(
                new DogLicenseHistoryViewModel
                {
                    LicenseNumber = LicenseNumber,
                    IssueDate = "2026-01-15",
                    ExpirationDate = "2027-01-15",
                    Status = "Valide"
                });
        }
    }


    // ==================================================
    // ÉTAT
    // ==================================================

    public bool HasSelectedDog =>
        _dog is not null;


    public bool IsEditing
    {
        get => _isEditing;

        private set
        {
            if (_isEditing == value)
            {
                return;
            }

            _isEditing = value;
            OnPropertyChanged();
        }
    }


    // ==================================================
    // CHIEN
    // ==================================================

    public int DogId =>
        _dog?.DogId ?? 0;


    public string DogName =>
        _dog?.DogName
        ?? "Aucun chien sélectionné";


    public string Breed =>
        _dog?.Breed
        ?? "—";


    public string Color =>
        _dog?.Color
        ?? "—";


    public string Sex =>
        _dog?.Sex switch
        {
            "M" => "Mâle",
            "F" => "Femelle",
            _ => "—"
        };


    public string Sterilized =>
        _dog?.Sterilized
        ?? "—";


    public string BirthDate =>
        _dog is null
            ? "—"
            : "2020-05-12";


    public string Age =>
        _dog?.AgeDisplay
        ?? "—";


    public string Weight =>
        _dog?.WeightDisplay
        ?? "—";


    public string Comments =>
        _dog is null
            ? "—"
            : "Chien calme et sociable.";


    // ==================================================
    // PROPRIÉTAIRE
    // ==================================================

    public int OwnerId =>
        _dog?.OwnerId ?? 0;


    public string OwnerName =>
        _dog?.OwnerName
        ?? "—";


    public string OwnerFileNumber =>
        _dog is null
            ? "—"
            : "#12345";


    public string OwnerAddress =>
        _dog is null
            ? "—"
            : "123, boulevard Exemple, Roberval, QC, G8H 2M9";


    // ==================================================
    // STATUT
    // ==================================================

    public string Status =>
        _dog?.Status
        ?? "—";


    public string DeactivationDate =>
        Status == "Inactif"
            ? "2026-08-20"
            : "—";


    public string DeactivationReason =>
        Status == "Inactif"
            ? "Dossier désactivé"
            : "—";


    // ==================================================
    // LICENCE
    // ==================================================

    public string LicenseNumber =>
        string.IsNullOrWhiteSpace(_dog?.LicenseNumber)
            ? "—"
            : _dog!.LicenseNumber;


    public string LicenseStatus =>
        _dog is null
            ? "—"
            : "Valide";


    public string LicenseIssueDate =>
        _dog is null
            ? "—"
            : "2026-01-15";


    public string LicenseExpirationDate =>
        _dog is null
            ? "—"
            : "2027-01-15";


    public ObservableCollection<DogLicenseHistoryViewModel>
        LicenseHistory
    { get; } = [];


    // ==================================================
    // COMMANDES
    // ==================================================

    public ICommand ModifyCommand { get; }

    public ICommand SaveCommand { get; }

    public ICommand CancelCommand { get; }

    public ICommand RenewLicenseCommand { get; }

    public ICommand OpenOwnerCommand { get; }


    // ==================================================
    // ACTIONS
    // ==================================================

    private void StartEditing()
    {
        IsEditing = true;
    }


    private void SaveChanges()
    {
        IsEditing = false;

        // Plus tard : API.
    }


    private void CancelChanges()
    {
        IsEditing = false;
    }


    private void RenewLicense()
    {
        // Plus tard : processus de renouvellement.
    }


    private void OpenOwner()
    {
        if (OwnerId <= 0)
        {
            return;
        }

        _openOwnerAction?.Invoke(OwnerId);
    }
}