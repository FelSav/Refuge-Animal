using MuniChien.App.Navigation;
using MuniChien.App.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MuniChien.App.ViewModels;

public class OwnersViewModel : ViewModelBase
{
    private readonly Action<OwnerDogViewModel>? _openDogAction;
    private readonly IAnimalDirectoryService _directoryService;
    private OwnerDirectoryItem? _selectedOwner;
    private OwnerDogViewModel? _selectedDog;
    private bool _isEditing;

    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _status = string.Empty;
    private string _phone = string.Empty;
    private string _cellPhone = string.Empty;
    private string _email = string.Empty;
    private string _address = string.Empty;
    private string _comments = string.Empty;

    private string _originalFirstName = string.Empty;
    private string _originalLastName = string.Empty;
    private string _originalStatus = string.Empty;
    private string _originalPhone = string.Empty;
    private string _originalCellPhone = string.Empty;
    private string _originalEmail = string.Empty;
    private string _originalAddress = string.Empty;
    private string _originalComments = string.Empty;

    public OwnersViewModel(
        Action<OwnerDogViewModel>? openDogAction = null,
        int ownerId = 1,
        IAnimalDirectoryService? directoryService = null)
    {
        _openDogAction = openDogAction;
        _directoryService = directoryService ?? new LocalAnimalDirectoryService();
        StatusOptions = ["Actif", "Inactif"];
        ModifyCommand = new RelayCommand(StartEditing);
        SaveCommand = new RelayCommand(SaveChanges);
        CancelCommand = new RelayCommand(CancelChanges);

        foreach (var owner in _directoryService.GetOwners())
            OwnerOptions.Add(owner);
        SelectedOwner = OwnerOptions.FirstOrDefault(x => x.OwnerId == ownerId)
                        ?? OwnerOptions.FirstOrDefault();
    }

    public ObservableCollection<OwnerDirectoryItem> OwnerOptions { get; } = [];
    public OwnerDirectoryItem? SelectedOwner
    {
        get => _selectedOwner;
        set
        {
            if (ReferenceEquals(_selectedOwner, value) || IsEditing) return;
            _selectedOwner = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(OwnerId));
            OnPropertyChanged(nameof(FileNumber));
            IsEditing = false;
            LoadOwner();
        }
    }

    public int OwnerId => SelectedOwner?.OwnerId ?? 0;
    public string FileNumber => SelectedOwner?.FileNumber ?? "—";
    public IReadOnlyList<string> StatusOptions { get; }

    public bool IsEditing
    {
        get => _isEditing;
        private set
        {
            if (_isEditing == value) return;
            _isEditing = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotEditing));
        }
    }
    public bool IsNotEditing => !IsEditing;

    public string FirstName { get => _firstName; set { if (_firstName == value) return; _firstName = value; OnPropertyChanged(); } }
    public string LastName { get => _lastName; set { if (_lastName == value) return; _lastName = value; OnPropertyChanged(); } }
    public string Status { get => _status; set { if (_status == value) return; _status = value; OnPropertyChanged(); } }
    public string Phone { get => _phone; set { if (_phone == value) return; _phone = value; OnPropertyChanged(); } }
    public string CellPhone { get => _cellPhone; set { if (_cellPhone == value) return; _cellPhone = value; OnPropertyChanged(); } }
    public string Email { get => _email; set { if (_email == value) return; _email = value; OnPropertyChanged(); } }
    public string Address { get => _address; set { if (_address == value) return; _address = value; OnPropertyChanged(); } }
    public string Comments { get => _comments; set { if (_comments == value) return; _comments = value; OnPropertyChanged(); } }

    public ObservableCollection<OwnerDogViewModel> Dogs { get; } = [];
    public OwnerDogViewModel? SelectedDog
    {
        get => _selectedDog;
        set
        {
            if (ReferenceEquals(_selectedDog, value)) return;
            _selectedDog = value;
            OnPropertyChanged();
            if (value is not null)
                _openDogAction?.Invoke(value);
        }
    }

    public ICommand ModifyCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public int NextDogId => Math.Max(0, _directoryService.GetDogs().Select(x => x.DogId).DefaultIfEmpty(0).Max()) + 1;

    // Le chien ajouté reste disponible dans Propriétaires et Chiens pendant la session.
    public void AddDog(OwnerDogViewModel dog)
    {
        _directoryService.AddDog(dog);
        RefreshDogs();
    }

    private void LoadOwner()
    {
        if (_selectedOwner is null)
        {
            FirstName = LastName = Status = Phone = CellPhone = Email = Address = Comments = string.Empty;
        }
        else
        {
            var owner = _directoryService.GetOwner(_selectedOwner.OwnerId) ?? _selectedOwner;
            FirstName = owner.FirstName;
            LastName = owner.LastName;
            Status = owner.Status;
            Phone = owner.Phone;
            CellPhone = owner.CellPhone;
            Email = owner.Email;
            Address = owner.Address;
            Comments = owner.Comments;
        }
        RefreshDogs();
    }

    private void RefreshDogs()
    {
        _selectedDog = null;
        OnPropertyChanged(nameof(SelectedDog));
        Dogs.Clear();
        if (OwnerId == 0) return;
        foreach (var dog in _directoryService.GetDogsForOwner(OwnerId))
            Dogs.Add(dog);
    }

    private void StartEditing()
    {
        if (_selectedOwner is null) return;
        SaveOriginalValues();
        IsEditing = true;
    }

    private void SaveChanges()
    {
        if (_selectedOwner is null) return;
        var saved = new OwnerDirectoryItem
        {
            OwnerId = OwnerId, FileNumber = FileNumber, Municipality = _selectedOwner.Municipality,
            FirstName = FirstName.Trim(), LastName = LastName.Trim(), Status = Status,
            Phone = Phone, CellPhone = CellPhone, Email = Email,
            Address = Address, Comments = Comments
        };
        _directoryService.SaveOwner(saved);
        IsEditing = false;
        int ownerId = saved.OwnerId;
        OwnerOptions.Clear();
        foreach (var owner in _directoryService.GetOwners()) OwnerOptions.Add(owner);
        // Met à jour le ComboBox même si le nom du propriétaire a changé.
        _selectedOwner = null;
        SelectedOwner = OwnerOptions.FirstOrDefault(x => x.OwnerId == ownerId);
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
}
