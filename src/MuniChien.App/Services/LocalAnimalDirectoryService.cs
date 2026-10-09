using MuniChien.App.ViewModels;

namespace MuniChien.App.Services;

public sealed class LocalAnimalDirectoryService : IAnimalDirectoryService
{
    public IReadOnlyList<OwnerDirectoryItem> GetOwners() => LocalAnimalDirectoryStore.GetOwners();
    public OwnerDirectoryItem? GetOwner(int ownerId) => LocalAnimalDirectoryStore.GetOwner(ownerId);
    public void SaveOwner(OwnerDirectoryItem owner) => LocalAnimalDirectoryStore.SaveOwner(owner);
    public IReadOnlyList<OwnerDogViewModel> GetDogs() => LocalAnimalDirectoryStore.GetDogs();
    public IReadOnlyList<OwnerDogViewModel> GetDogsForOwner(int ownerId) => LocalAnimalDirectoryStore.GetDogsForOwner(ownerId);
    public OwnerDogViewModel? GetDog(int dogId) => LocalAnimalDirectoryStore.GetDog(dogId);
    public DogDirectoryDetails? GetDogDetails(int dogId) => LocalAnimalDirectoryStore.GetDogDetails(dogId);
    public void AddDog(OwnerDogViewModel dog) => LocalAnimalDirectoryStore.AddDog(dog);
    public void SaveDog(OwnerDogViewModel dog, DogDirectoryDetails details) => LocalAnimalDirectoryStore.SaveDog(dog, details);
}
