using MuniChien.App.ViewModels;

namespace MuniChien.App.Services;

// Répertoire commun temporaire (en mémoire, remis à zéro au redémarrage).
// Les chiens existants proviennent des exemples du magasin de licences.
internal static class LocalAnimalDirectoryStore
{
    private static readonly Dictionary<int, OwnerDirectoryItem> Owners = [];
    private static readonly Dictionary<int, OwnerDogViewModel> Dogs = [];
    private static readonly Dictionary<int, DogDirectoryDetails> DogDetails = [];

    static LocalAnimalDirectoryStore()
    {
        foreach (var group in LocalLicenseStore.Licenses.GroupBy(x => x.OwnerId))
        {
            var first = group.First();
            string[] name = first.OwnerName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var owner = new OwnerDirectoryItem
            {
                OwnerId = group.Key,
                FileNumber = group.Key == 1 ? "12345" : (12344 + group.Key).ToString(),
                Municipality = first.Municipality,
                FirstName = name.Length > 0 ? name[0] : string.Empty,
                LastName = name.Length > 1 ? name[1] : string.Empty,
                Status = "Actif",
                // Les autres coordonnées ne sont pas connues; on n'invente pas de données personnelles.
                Phone = group.Key == 1 ? "(418) 123-4567" : string.Empty,
                CellPhone = group.Key == 1 ? "(418) 321-7654" : string.Empty,
                Email = group.Key == 1 ? "exemple@email.com" : string.Empty,
                Address = group.Key == 1 ? "123, boulevard Exemple, Roberval, QC, G8H 2M9" : string.Empty
            };
            Owners[group.Key] = owner;
        }

        foreach (var license in LocalLicenseStore.Licenses)
        {
            var dog = license.ToDogViewModel();
            Dogs[dog.DogId] = dog;
            DogDetails[dog.DogId] = new DogDirectoryDetails
            {
                Comments = dog.DogId == 1 ? "Chien calme et sociable." : string.Empty
            };
        }
    }

    public static IReadOnlyList<OwnerDirectoryItem> GetOwners() =>
        Owners.Values.OrderBy(x => x.FullName).Select(CloneOwner).ToList();

    public static OwnerDirectoryItem? GetOwner(int ownerId) =>
        Owners.TryGetValue(ownerId, out var owner) ? CloneOwner(owner) : null;

    public static void SaveOwner(OwnerDirectoryItem owner)
    {
        if (!Owners.ContainsKey(owner.OwnerId))
            throw new InvalidOperationException("Le propriétaire sélectionné n'existe pas.");

        Owners[owner.OwnerId] = CloneOwner(owner);
        foreach (var dog in Dogs.Values.Where(x => x.OwnerId == owner.OwnerId).ToList())
            Dogs[dog.DogId] = CloneDog(dog, owner.FullName);
    }

    public static IReadOnlyList<OwnerDogViewModel> GetDogs() =>
        Dogs.Values.OrderBy(x => x.DogName).ThenBy(x => x.OwnerName).Select(x => CloneDog(x)).ToList();

    public static IReadOnlyList<OwnerDogViewModel> GetDogsForOwner(int ownerId) =>
        Dogs.Values.Where(x => x.OwnerId == ownerId).OrderBy(x => x.DogName).Select(x => CloneDog(x)).ToList();

    public static OwnerDogViewModel? GetDog(int dogId) =>
        Dogs.TryGetValue(dogId, out var dog) ? CloneDog(dog) : null;

    public static DogDirectoryDetails? GetDogDetails(int dogId) =>
        DogDetails.TryGetValue(dogId, out var details) ? CloneDetails(details) : null;

    public static void AddDog(OwnerDogViewModel dog)
    {
        if (!Owners.TryGetValue(dog.OwnerId, out var owner))
            throw new InvalidOperationException("Le propriétaire sélectionné n'existe pas.");
        if (Dogs.ContainsKey(dog.DogId))
            throw new InvalidOperationException("Ce numéro de chien existe déjà.");
        Dogs[dog.DogId] = CloneDog(dog, owner.FullName);
        DogDetails[dog.DogId] = new DogDirectoryDetails();
    }

    public static void SaveDog(OwnerDogViewModel dog, DogDirectoryDetails details)
    {
        if (!Dogs.ContainsKey(dog.DogId))
            throw new InvalidOperationException("Le chien sélectionné n'existe pas.");
        string? ownerName = Owners.TryGetValue(dog.OwnerId, out var owner) ? owner.FullName : null;
        Dogs[dog.DogId] = CloneDog(dog, ownerName);
        DogDetails[dog.DogId] = CloneDetails(details);
    }

    private static OwnerDirectoryItem CloneOwner(OwnerDirectoryItem x) => new()
    {
        OwnerId = x.OwnerId, FileNumber = x.FileNumber, Municipality = x.Municipality,
        FirstName = x.FirstName, LastName = x.LastName, Status = x.Status,
        Phone = x.Phone, CellPhone = x.CellPhone, Email = x.Email,
        Address = x.Address, Comments = x.Comments
    };

    private static OwnerDogViewModel CloneDog(OwnerDogViewModel x, string? ownerName = null) => new()
    {
        DogId = x.DogId, OwnerId = x.OwnerId, DogName = x.DogName,
        OwnerName = ownerName ?? x.OwnerName, Breed = x.Breed,
        AgeMonths = x.AgeMonths, WeightKg = x.WeightKg, Color = x.Color,
        Sex = x.Sex, Status = x.Status, Sterilized = x.Sterilized,
        LicenseNumber = x.LicenseNumber
    };

    private static DogDirectoryDetails CloneDetails(DogDirectoryDetails x) => new()
    {
        Comments = x.Comments, DeactivationDate = x.DeactivationDate,
        DeactivationReason = x.DeactivationReason
    };
}
