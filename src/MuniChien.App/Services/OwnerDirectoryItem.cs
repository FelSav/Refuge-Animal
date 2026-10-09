namespace MuniChien.App.Services;

// Données de démonstration. Le service API remplacera cette source locale.
public sealed class OwnerDirectoryItem
{
    public int OwnerId { get; init; }
    public string FileNumber { get; init; } = string.Empty;
    public string Municipality { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Status { get; init; } = "Actif";
    public string Phone { get; init; } = string.Empty;
    public string CellPhone { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string Comments { get; init; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}".Trim();
    public string SelectionDisplay => $"{FullName} — {Municipality} (#{FileNumber})";
    public override string ToString() => SelectionDisplay;
}

public sealed class DogDirectoryDetails
{
    public string Comments { get; init; } = string.Empty;
    public string DeactivationDate { get; init; } = string.Empty;
    public string DeactivationReason { get; init; } = string.Empty;
}
