namespace MuniChien.App.ViewModels;

/// <summary>Une ligne du tableau Chiens issue du répertoire partagé.</summary>
public sealed class DogListRow
{
    public required OwnerDogViewModel Dog { get; init; }
    public int DogId => Dog.DogId;
    public string Name => Dog.DogName;
    public string Owner => Dog.OwnerName;
    public string Breed => Dog.Breed;
    public string Municipality { get; init; } = "—";
    public string Status => Dog.Status;
    public string Sex => Dog.Sex == "M" ? "Mâle" : Dog.Sex == "F" ? "Femelle" : "—";
    public string License => string.IsNullOrWhiteSpace(Dog.LicenseNumber) ? "Sans licence" : Dog.LicenseNumber;
    public bool HasLicense => !string.IsNullOrWhiteSpace(Dog.LicenseNumber);
}
