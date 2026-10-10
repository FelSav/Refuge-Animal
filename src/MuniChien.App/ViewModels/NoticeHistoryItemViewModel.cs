using System;
using MuniChien.App.Services;

namespace MuniChien.App.ViewModels;

public sealed class NoticeHistoryItemViewModel
{
    public NoticeHistoryItemViewModel(NoticeSentHistoryEntry entry)
    {
        OwnerId = entry.OwnerId;
        FileNumber = entry.FileNumber;
        OwnerName = entry.OwnerName;
        Municipality = entry.Municipality;
        Year = entry.Year;
        Campaign = entry.Campaign;
        SentAt = entry.SentAt;
    }

    public int OwnerId { get; }
    public string FileNumber { get; }
    public string OwnerName { get; }
    public string Municipality { get; }
    public int Year { get; }
    public string Campaign { get; }
    public DateTime SentAt { get; }
    public string CampaignDisplay => Campaign == NoticesViewModel.JanuaryCampaign
        ? "Premier avis · janvier"
        : Campaign == NoticesViewModel.MarchCampaign
            ? "Rappel · mars"
            : Campaign;
    public string DateDisplay => SentAt.ToString("yyyy-MM-dd HH:mm");
    public string StatusDisplay => "Envoi simulé";
}
