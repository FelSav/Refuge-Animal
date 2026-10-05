namespace MuniChien.App.ViewModels;

public class LicenseListItemViewModel
{
    public int DogId { get; init; }

    public int OwnerId { get; init; }


    public string LicenseNumber { get; init; } =
        string.Empty;


    public string DogName { get; init; } =
        string.Empty;


    public string OwnerName { get; init; } =
        string.Empty;


    public string Municipality { get; init; } =
        string.Empty;


    public DateTime IssueDate { get; set; }

    public DateTime ExpirationDate { get; set; }


    public string Status { get; set; } =
        string.Empty;


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