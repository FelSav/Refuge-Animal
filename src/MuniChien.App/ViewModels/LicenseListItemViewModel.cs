using MuniChien.App.Services;

namespace MuniChien.App.ViewModels;

public class LicenseListItemViewModel : ViewModelBase
{
    public int DogId { get; init; }

    public int OwnerId { get; init; }


    public string LicenseNumber { get; init; } =
        string.Empty;


    private string _dogName = string.Empty;
    public string DogName
    {
        get => _dogName;
        set
        {
            if (_dogName == value) return;
            _dogName = value;
            OnPropertyChanged();
        }
    }


    private string _ownerName = string.Empty;
    public string OwnerName
    {
        get => _ownerName;
        set
        {
            if (_ownerName == value) return;
            _ownerName = value;
            OnPropertyChanged();
        }
    }


    private string _municipality = string.Empty;
    public string Municipality
    {
        get => _municipality;
        set
        {
            if (_municipality == value) return;
            _municipality = value;
            OnPropertyChanged();
        }
    }


    private DateTime _issueDate;
    public DateTime IssueDate
    {
        get => _issueDate;
        set
        {
            if (_issueDate == value) return;
            _issueDate = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IssueDateDisplay));
        }
    }

    private DateTime _expirationDate;
    public DateTime ExpirationDate
    {
        get => _expirationDate;
        set
        {
            if (_expirationDate == value) return;
            _expirationDate = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ExpirationDateDisplay));
            OnPropertyChanged(nameof(Status));
        }
    }

    private string _recordedStatus = string.Empty;
    public string Status
    {
        get => LocalLicenseStatusRules.Resolve(ExpirationDate, _recordedStatus, DateTime.Today);
        set
        {
            string next = value ?? string.Empty;
            if (_recordedStatus == next) return;
            _recordedStatus = next;
            OnPropertyChanged();
        }
    }

    // À appeler lors d'un rechargement de page (ex. après minuit).
    public void RefreshCurrentStatus() => OnPropertyChanged(nameof(Status));


    // ==================================================
    // INFORMATIONS DU CHIEN
    // ==================================================

    public string Breed { get; init; } =
        string.Empty;


    public int AgeMonths { get; init; }


    public double WeightKg { get; init; }


    public string Color { get; init; } =
        string.Empty;


    public string Sex { get; init; } =
        string.Empty;


    public string Sterilized { get; init; } =
        string.Empty;


    // ==================================================
    // AFFICHAGE
    // ==================================================

    public string IssueDateDisplay =>
        IssueDate.ToString("yyyy-MM-dd");


    public string ExpirationDateDisplay =>
        ExpirationDate.ToString("yyyy-MM-dd");


    // ==================================================
    // CONVERSION VERS FICHE CHIEN
    // ==================================================

    public OwnerDogViewModel ToDogViewModel()
    {
        return new OwnerDogViewModel
        {
            DogId =
                DogId,

            OwnerId =
                OwnerId,

            DogName =
                DogName,

            OwnerName =
                OwnerName,

            Breed =
                Breed,

            AgeMonths =
                AgeMonths,

            WeightKg =
                WeightKg,

            Color =
                Color,

            Sex =
                Sex,

            Status =
                "Actif",

            Sterilized =
                Sterilized,

            LicenseNumber =
                LicenseNumber
        };
    }
}