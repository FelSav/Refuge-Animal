using MuniChien.App.Navigation;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MuniChien.App.ViewModels;

public class OwnersViewModel : ViewModelBase
{
    private string _firstName = "Jean";
    private string _lastName = "Tremblay";
    private string _status = "Actif";

    private string _phone = "(418) 123-4567";
    private string _cellPhone = "(418) 321-7654";
    private string _email = "exemple@email.com";

    private string _address = "123, boulevard Exemple, Roberval, QC, G8H 2M9";

    private string _comments = string.Empty;


    public OwnersViewModel()
    {
        ModifyCommand = new RelayCommand(ModifyOwner);
        AddDogCommand = new RelayCommand(AddDog);

        Dogs.Add(new OwnerDogViewModel
        {
            DogId = 1,
            DogName = "Lucky",
            OwnerName = "Jean Tremblay",
            Breed = "Labrador",
            Age = "6 ans",
            Weight = "28.4 kg",
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
            Age = "2 ans",
            Weight = "20.4 kg",
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

    public ICommand AddDogCommand { get; }


    private void ModifyOwner()
    {
        // À brancher plus tard sur le mode modification.
    }


    private void AddDog()
    {
        // À brancher plus tard sur la création / association d'un chien.
    }
}