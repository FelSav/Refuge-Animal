namespace MuniChien.App.ViewModels;

public class DogsViewModel : ViewModelBase
{
    private readonly OwnerDogViewModel? _dog;


    // ==================================================
    // CONSTRUCTEUR
    // ==================================================

    public DogsViewModel(
        OwnerDogViewModel? dog = null)
    {
        _dog = dog;
    }


    // ==================================================
    // ÉTAT
    // ==================================================

    public bool HasSelectedDog =>
        _dog is not null;


    // ==================================================
    // INFORMATIONS
    // ==================================================

    public string DogName =>
        _dog?.DogName
        ?? "Aucun chien sélectionné";


    public string OwnerName =>
        _dog?.OwnerName
        ?? "—";


    public string Breed =>
        _dog?.Breed
        ?? "—";


    public string Age =>
        _dog?.AgeDisplay
        ?? "—";


    public string Weight =>
        _dog?.WeightDisplay
        ?? "—";


    public string Color =>
        _dog?.Color
        ?? "—";


    public string Sex =>
        _dog?.Sex
        ?? "—";


    public string Status =>
        _dog?.Status
        ?? "—";


    public string Sterilized =>
        _dog?.Sterilized
        ?? "—";


    public string LicenseNumber =>
        string.IsNullOrWhiteSpace(
            _dog?.LicenseNumber)
            ? "—"
            : _dog!.LicenseNumber;
}