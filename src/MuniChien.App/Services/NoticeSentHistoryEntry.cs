using System;

namespace MuniChien.App.Services;

// Copie des informations visibles au moment de la confirmation de démonstration.
// Ni une preuve d'impression ni une preuve d'envoi postal réel.
public sealed class NoticeSentHistoryEntry
{
    public int OwnerId { get; init; }
    public string FileNumber { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public string Municipality { get; init; } = string.Empty;
    public int Year { get; init; }
    public string Campaign { get; init; } = string.Empty;
    public DateTime SentAt { get; init; }
}
