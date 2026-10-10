using System;
using System.Collections.Generic;

namespace MuniChien.App.Services;

// Contrat temporaire. L'API déterminera les vrais impayés par année/municipalité.
public interface INoticeService
{
    IReadOnlyList<NoticeCandidateData> GetCandidates(int year, string campaign);
    bool WasSent(int ownerId, int year, string campaign);
    DateTime? GetLastSentDate(int ownerId, int year, string campaign);
    void RecordSent(int ownerId, int year, string campaign, DateTime sentAt);

    // Historique de démonstration indépendant du solde actuel et de la campagne affichée.
    IReadOnlyList<NoticeSentHistoryEntry> GetSentHistory();
    void ClearDemoHistory();
}

public sealed class NoticeDogData
{
    public string Name { get; init; } = string.Empty;
    public string Breed { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public string Sex { get; init; } = string.Empty;
    public string Sterilized { get; init; } = string.Empty;
    public string LicenseNumber { get; init; } = string.Empty;
}

public sealed class NoticeCandidateData
{
    public int OwnerId { get; init; }
    public string FileNumber { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public string Municipality { get; init; } = string.Empty;
    public string DogsDescription { get; init; } = string.Empty;
    public decimal Balance { get; init; }
    public decimal AmountInvoiced { get; init; }
    public decimal PaymentsReceived { get; init; }
    public decimal LateFees { get; init; }
    public string Address { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Telephone { get; init; } = string.Empty;
    public string CellPhone { get; init; } = string.Empty;
    public IReadOnlyList<NoticeDogData> Dogs { get; init; } = Array.Empty<NoticeDogData>();
}
