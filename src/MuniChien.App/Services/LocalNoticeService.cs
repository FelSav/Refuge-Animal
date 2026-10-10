using System;
using System.Collections.Generic;
using System.Linq;

namespace MuniChien.App.Services;

// DÉMONSTRATION UNIQUEMENT — dette locale fictive, non associée à une année.
// Aucune entrée de ce service ne constitue un avis de paiement officiel.
public sealed class LocalNoticeService : INoticeService
{
    private static readonly List<LocalNoticeSentEntry> SentHistory = new();

    public IReadOnlyList<NoticeCandidateData> GetCandidates(int year, string campaign)
    {
        // L'année et la campagne seront utilisées par le futur service API.
        return LocalPaymentStore.GetOwnerOptions()
            .Select(option =>
            {
                var account = LocalPaymentStore.GetAccountSnapshot(option.OwnerId);
                var owner = LocalAnimalDirectoryStore.GetOwner(option.OwnerId);
                var dogs = LocalAnimalDirectoryStore.GetDogsForOwner(option.OwnerId)
                    .Select(dog => new NoticeDogData
                    {
                        Name = dog.DogName,
                        Breed = dog.Breed,
                        Color = dog.Color,
                        Sex = dog.Sex,
                        Sterilized = dog.Sterilized,
                        LicenseNumber = dog.LicenseNumber
                    }).ToList();

                return new NoticeCandidateData
                {
                    OwnerId = option.OwnerId,
                    FileNumber = owner?.FileNumber ?? option.FileNumber,
                    OwnerName = owner?.FullName ?? option.OwnerName,
                    Municipality = owner?.Municipality ?? option.Municipality,
                    Address = owner?.Address ?? string.Empty,
                    Telephone = owner?.Phone ?? string.Empty,
                    CellPhone = owner?.CellPhone ?? string.Empty,
                    Email = owner?.Email ?? string.Empty,
                    Dogs = dogs,
                    DogsDescription = string.Join(", ", dogs.Select(x => $"{x.Name} (#{x.LicenseNumber})")),
                    AmountInvoiced = account.AmountDue,
                    LateFees = account.LateFees,
                    PaymentsReceived = account.TotalPayments,
                    Balance = account.Balance
                };
            })
            .Where(candidate => candidate.Balance > 0m)
            .OrderBy(candidate => candidate.Municipality)
            .ThenBy(candidate => candidate.OwnerName)
            .ToList();
    }

    public bool WasSent(int ownerId, int year, string campaign) =>
        SentHistory.Any(record => record.OwnerId == ownerId
            && record.Year == year && record.Campaign == campaign);

    public DateTime? GetLastSentDate(int ownerId, int year, string campaign) =>
        SentHistory
            .Where(record => record.OwnerId == ownerId
                && record.Year == year && record.Campaign == campaign)
            .Select(record => (DateTime?)record.SentAt)
            .OrderByDescending(date => date)
            .FirstOrDefault();

    public void RecordSent(int ownerId, int year, string campaign, DateTime sentAt)
    {
        if (WasSent(ownerId, year, campaign))
            return;

        SentHistory.Add(new LocalNoticeSentEntry(ownerId, year, campaign, sentAt));
    }

    private sealed record LocalNoticeSentEntry(int OwnerId, int Year, string Campaign, DateTime SentAt);
}
