namespace MuniChien.App.ViewModels;

/// <summary>Une ligne de propriétaire, même si son dossier n'a aucun chien.</summary>
public sealed class SearchOwnerResultViewModel
{
    public int OwnerId { get; init; }
    public string FileNumber { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public string Municipality { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string CellPhone { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int DogCount { get; init; }
}
