using MuniChien.App.Services;
using System;
using System.Collections.Generic;

namespace MuniChien.App.ViewModels;

public sealed class NoticeCandidateViewModel : ViewModelBase
{
    private bool _isSelected;

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
    public bool WasSent { get; init; }
    public DateTime? LastSentAt { get; init; }

    public string BalanceDisplay => $"{Balance:0.00} $";
    public string SentStatusDisplay => WasSent && LastSentAt.HasValue
        ? $"Envoi simulé : {LastSentAt.Value:yyyy-MM-dd}"
        : "À préparer";

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
        }
    }
}
