using MuniChien.App.Services;
namespace MuniChien.App.ViewModels;

/// <summary>Une ligne du tableau Clients. On conserve l'identifiant réel du dossier.</summary>
public sealed class ClientListRow
{
    public required OwnerDirectoryItem Client { get; init; }
    public string FileNumber => Client.FileNumber;
    public string Name => Client.FullName;
    public string Phone => string.IsNullOrWhiteSpace(Client.Phone) ? Client.CellPhone : Client.Phone;
    public string Municipality => string.IsNullOrWhiteSpace(Client.Municipality) ? "—" : Client.Municipality;
    public string Status => Client.Status;
    public int DogCount { get; init; }
    public string Loyalty => HasLoyaltyPurchases ? "Achats en cours" : "Aucun achat";
    public bool HasLoyaltyPurchases { get; init; }
}
