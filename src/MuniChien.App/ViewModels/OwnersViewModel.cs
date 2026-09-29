using MuniChien.App.Navigation;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MuniChien.App.ViewModels;

public class OwnersViewModel : ViewModelBase
{
    // ==================================================
    // VALEURS ACTUELLES
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

    private bool _isEditing;


    // ==================================================
    // SAUVEGARDE TEMPORAIRE POUR ANNULER
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

    public OwnersViewModel()
    {
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

        AddDogCommand =
            new RelayCommand(AddDog);


        Dogs.Add(new OwnerDogViewModel
        {
            DogId = 1,
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


        Dogs.Add(new OwnerDogViewModel
        {
            DogId = 2,
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


    // ==================================================
    // DOSSIER
    // ==================================================

    public string FileNumber { get; } = "12345";


    // ==================================================
    // LISTES
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
    // INFORMATIONS PROPRIÉTAIRE
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

    public ObservableCollection<OwnerDogViewModel> Dogs { get; } = [];


    // ==================================================
    // COMMANDES
    // ==================================================

    public ICommand ModifyCommand { get; }

    public ICommand SaveCommand { get; }

    public ICommand CancelCommand { get; }

    public ICommand AddDogCommand { get; }


    // ==================================================
    // MODIFICATION
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
        // appel API pour enregistrer le propriétaire.
    }


    private void CancelChanges()
    {
        FirstName = _originalFirstName;
        LastName = _originalLastName;
        Status = _originalStatus;

        Phone = _originalPhone;
        CellPhone = _originalCellPhone;
        Email = _originalEmail;

        Address = _originalAddress;
        Comments = _originalComments;

        IsEditing = false;
    }


    private void SaveOriginalValues()
    {
        _originalFirstName = FirstName;
        _originalLastName = LastName;
        _originalStatus = Status;

        _originalPhone = Phone;
        _originalCellPhone = CellPhone;
        _originalEmail = Email;

        _originalAddress = Address;
        _originalComments = Comments;
    }


    // ==================================================
    // MISE À JOUR DU NOM DANS LE TABLEAU
    // ==================================================

    private void UpdateDogOwnerNames()
    {
        string ownerName =
            $"{FirstName} {LastName}".Trim();

        for (int i = 0; i < Dogs.Count; i++)
        {
            OwnerDogViewModel oldDog = Dogs[i];

            Dogs[i] = new OwnerDogViewModel
            {
                DogId = oldDog.DogId,
                DogName = oldDog.DogName,
                OwnerName = ownerName,
                Breed = oldDog.Breed,
                AgeMonths = oldDog.AgeMonths,
                WeightKg = oldDog.WeightKg,
                Color = oldDog.Color,
                Sex = oldDog.Sex,
                Status = oldDog.Status,
                Sterilized = oldDog.Sterilized,
                LicenseNumber = oldDog.LicenseNumber
            };
        }
    }


    // ==================================================
    // AJOUTER UN CHIEN
    // ==================================================

    private void AddDog()
    {
        // Prochaine étape.
    }
}