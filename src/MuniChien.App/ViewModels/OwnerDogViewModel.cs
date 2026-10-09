namespace MuniChien.App.ViewModels;

public class OwnerDogViewModel
{
    public int DogId { get; init; }

    public int OwnerId { get; init; }

    public string DogName { get; init; } = string.Empty;

    public string OwnerName { get; init; } = string.Empty;

    public string Breed { get; init; } = string.Empty;

    public int AgeMonths { get; init; }

    public double WeightKg { get; init; }

    public string Color { get; init; } = string.Empty;

    public string Sex { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string Sterilized { get; init; } = string.Empty;

    public string LicenseNumber { get; init; } = string.Empty;


    // ==================================================
    // AFFICHAGE DE L'ÂGE
    // ==================================================

    public string SelectionDisplay => $"{DogName} — {OwnerName}";

    public override string ToString() => SelectionDisplay;

    public string AgeDisplay
    {
        get
        {
            if (AgeMonths < 12)
            {
                return $"{AgeMonths} mois";
            }

            int years =
                AgeMonths / 12;

            int months =
                AgeMonths % 12;

            if (months == 0)
            {
                return years == 1
                    ? "1 an"
                    : $"{years} ans";
            }

            return years == 1
                ? $"1 an {months} mois"
                : $"{years} ans {months} mois";
        }
    }


    // ==================================================
    // AFFICHAGE DU POIDS
    // ==================================================

    public string WeightDisplay =>
        $"{WeightKg:0.##} kg";
}