using MuniChien.App.Navigation;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MuniChien.App.ViewModels;

public class OwnersViewModel : ViewModelBase
{
    // ==================================================
    // NAVIGATION
    // ==================================================

    private readonly Action<OwnerDogViewModel>? _openDogAction;

    private OwnerDogViewModel? _selectedDog;


    // ==================================================
    // PROPRIÉTAIRE
    // ==================================================

    private string _firstName = "Jean";
    private string _lastName = "Tremblay";
    private string _status = "Actif";

    private string _phone = "(418) 123-4567";
    private string _cellPhone = "(418) 321-7654";
    private string _email = "exemple@email.com";

    private string _address =
        "123, boulevard Exemple, Roberval, QC, G8H 2M9";

    private string _comments = string.Empty;


    // ==================================================
    // MODE ÉDITION
    // ==================================================

    private bool _isEditing;


    // ==================================================
    // VALEURS ORIGINALES
    // ==================================================

    private string _originalFirstName = string.Empty;
    private string _originalLastName = string.Empty;
    private string _originalStatus = string.Empty;

    private string _originalPhone = string.Empty;
    private string _originalCellPhone = string.Empty;
    private string _originalEmail = string.Empty;

    private string _originalAddress = string.Empty;
    private string _originalComments = string.Empty;


    // ==================================================
    // CONSTRUCTEUR
    // ==================================================

    public OwnersViewModel(
        Action<OwnerDogViewModel>? openDogAction = null,
        int ownerId = 1)
    {
        _openDogAction =
            openDogAction;

        OwnerId =
            ownerId;


        StatusOptions =
        [
            "Actif",
            "Inactif"
        ];


        ModifyCommand =
            new RelayCommand(StartEditing);

        SaveCommand =
            new RelayCommand(SaveChanges);

        CancelCommand =
            new RelayCommand(CancelChanges);


        LoadExampleDogs();
    }


    // ==================================================
    // IDENTITÉ DU DOSSIER
    // ==================================================

    public int OwnerId { get; }

    public string FileNumber { get; } =
        "12345";


    // ==================================================
    // OPTIONS
    // ==================================================

    public IReadOnlyList<string> StatusOptions { get; }


    // ==================================================
    // MODE ÉDITION
    // ==================================================

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
    // INFORMATIONS
    // ==================================================

    public string FirstName
    {
        get => _firstName;

        set
        {
            if (_firstName == value)
            {
                return;
            }

            _firstName = value;

            OnPropertyChanged();
        }
    }


    public string LastName
    {
        get => _lastName;

        set
        {
            if (_lastName == value)
            {
                return;
            }

            _lastName = value;

            OnPropertyChanged();
        }
    }


    public string Status
    {
        get => _status;

        set
        {
            if (_status == value)
            {
                return;
            }

            _status = value;

            OnPropertyChanged();
        }
    }


    // ==================================================
    // COORDONNÉES
    // ==================================================

    public string Phone
    {
        get => _phone;

        set
        {
            if (_phone == value)
            {
                return;
            }

            _phone = value;

            OnPropertyChanged();
        }
    }


    public string CellPhone
    {
        get => _cellPhone;

        set
        {
            if (_cellPhone == value)
            {
                return;
            }

            _cellPhone = value;

            OnPropertyChanged();
        }
    }


    public string Email
    {
        get => _email;

        set
        {
            if (_email == value)
            {
                return;
            }

            _email = value;

            OnPropertyChanged();
        }
    }


    // ==================================================
    // ADRESSE
    // ==================================================

    public string Address
    {
        get => _address;

        set
        {
            if (_address == value)
            {
                return;
            }

            _address = value;

            OnPropertyChanged();
        }
    }


    // ==================================================
    // COMMENTAIRES
    // ==================================================

    public string Comments
    {
        get => _comments;

        set
        {
            if (_comments == value)
            {
                return;
            }

            _comments = value;

            OnPropertyChanged();
        }
    }


    // ==================================================
    // CHIENS
    // ==================================================

    public ObservableCollection<OwnerDogViewModel> Dogs { get; } =
        [];


    public OwnerDogViewModel? SelectedDog
    {
        get => _selectedDog;

        set
        {
            if (_selectedDog == value)
            {
                return;
            }

            _selectedDog = value;

            OnPropertyChanged();

            if (_selectedDog is not null)
            {
                _openDogAction?.Invoke(
                    _selectedDog);
            }
        }
    }


    // ==================================================
    // COMMANDES
    // ==================================================

    public ICommand ModifyCommand { get; }

    public ICommand SaveCommand { get; }

    public ICommand CancelCommand { get; }


    // ==================================================
    // MODE ÉDITION
    // ==================================================

    private void StartEditing()
    {
        SaveOriginalValues();

        IsEditing = true;
    }


    private void SaveChanges()
    {
        IsEditing = false;

        UpdateDogOwnerNames();

        // Plus tard :
        // enregistrement via l'API.
    }


    private void CancelChanges()
    {
        FirstName =
            _originalFirstName;

        LastName =
            _originalLastName;

        Status =
            _originalStatus;

        Phone =
            _originalPhone;

        CellPhone =
            _originalCellPhone;

        Email =
            _originalEmail;

        Address =
            _originalAddress;

        Comments =
            _originalComments;

        IsEditing = false;
    }


    private void SaveOriginalValues()
    {
        _originalFirstName =
            FirstName;

        _originalLastName =
            LastName;

        _originalStatus =
            Status;

        _originalPhone =
            Phone;

        _originalCellPhone =
            CellPhone;

        _originalEmail =
            Email;

        _originalAddress =
            Address;

        _originalComments =
            Comments;
    }


    // ==================================================
    // METTRE À JOUR LE NOM DU PROPRIÉTAIRE
    // ==================================================

    private void UpdateDogOwnerNames()
    {
        string ownerName =
            $"{FirstName} {LastName}".Trim();


        for (int i = 0; i < Dogs.Count; i++)
        {
            OwnerDogViewModel oldDog =
                Dogs[i];


            Dogs[i] =
                new OwnerDogViewModel
                {
                    DogId =
                        oldDog.DogId,

                    OwnerId =
                        oldDog.OwnerId,

                    DogName =
                        oldDog.DogName,

                    OwnerName =
                        ownerName,

                    Breed =
                        oldDog.Breed,

                    AgeMonths =
                        oldDog.AgeMonths,

                    WeightKg =
                        oldDog.WeightKg,

                    Color =
                        oldDog.Color,

                    Sex =
                        oldDog.Sex,

                    Status =
                        oldDog.Status,

                    Sterilized =
                        oldDog.Sterilized,

                    LicenseNumber =
                        oldDog.LicenseNumber
                };
        }
    }


    // ==================================================
    // DONNÉES TEMPORAIRES
    // ==================================================

    private void LoadExampleDogs()
    {
        Dogs.Clear();


        Dogs.Add(
            new OwnerDogViewModel
            {
                DogId = 1,
                OwnerId = OwnerId,

                DogName = "Lucky",
                OwnerName = "Jean Tremblay",

                Breed = "Labrador",

                AgeMonths = 72,
                WeightKg = 28.4,

                Color = "Brun",

                Sex = "M",

                Status = "Actif",

                Sterilized = "Oui",

                LicenseNumber = "10452"
            });


        Dogs.Add(
            new OwnerDogViewModel
            {
                DogId = 2,
                OwnerId = OwnerId,

                DogName = "Jack",
                OwnerName = "Jean Tremblay",

                Breed = "Husky",

                AgeMonths = 24,
                WeightKg = 20.4,

                Color = "Blanc",

                Sex = "F",

                Status = "Actif",

                Sterilized = "Non",

                LicenseNumber = "10453"
            });
    }
}