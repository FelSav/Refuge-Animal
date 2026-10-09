using MuniChien.App.ViewModels;

namespace MuniChien.App.Services;

// Contrat remplaçable par une implémentation basée sur l'API.
public interface IAnimalDirectoryService
{
    IReadOnlyList<OwnerDirectoryItem> GetOwners();
    OwnerDirectoryItem? GetOwner(int ownerId);
    void SaveOwner(OwnerDirectoryItem owner);
    IReadOnlyList<OwnerDogViewModel> GetDogs();
    IReadOnlyList<OwnerDogViewModel> GetDogsForOwner(int ownerId);
    OwnerDogViewModel? GetDog(int dogId);
    DogDirectoryDetails? GetDogDetails(int dogId);
    void AddDog(OwnerDogViewModel dog);
    void SaveDog(OwnerDogViewModel dog, DogDirectoryDetails details);
}
