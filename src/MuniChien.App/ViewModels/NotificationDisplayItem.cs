using MuniChien.App.Services;

namespace MuniChien.App.ViewModels;

/// <summary>Une ligne d'alerte WPF. Lecture conservée durant la session locale.</summary>
public sealed class NotificationDisplayItem
{
    public NotificationDisplayItem(NotificationEntry entry, bool isRead)
    {
        Entry = entry;
        IsRead = isRead;
    }

    public NotificationEntry Entry { get; }
    public bool IsRead { get; }
    public string Category => Entry.Category;
    public string Title => Entry.Title;
    public string Description => Entry.Description;
    public string Detail => Entry.Detail;
}
