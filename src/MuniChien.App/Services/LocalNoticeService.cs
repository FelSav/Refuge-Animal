using System;
using System.Collections.Generic;
using System.Linq;

namespace MuniChien.App.Services;

// DÉMONSTRATION UNIQUEMENT — dette locale fictive, non associée à une année.
// Aucune entrée de ce service ne constitue un avis de paiement officiel.
public sealed class LocalNoticeService : INoticeService
{
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
        LocalNoticeHistoryStore.GetAll().Any(record => record.OwnerId == ownerId
            && record.Year == year && record.Campaign == campaign);

    public DateTime? GetLastSentDate(int ownerId, int year, string campaign) =>
        LocalNoticeHistoryStore.GetAll()
            .Where(record => record.OwnerId == ownerId
                && record.Year == year && record.Campaign == campaign)
            .Select(record => (DateTime?)record.SentAt)
            .OrderByDescending(date => date)
            .FirstOrDefault();

    public IReadOnlyList<NoticeSentHistoryEntry> GetSentHistory() =>
        LocalNoticeHistoryStore.GetAll();

    public void RecordSent(int ownerId, int year, string campaign, DateTime sentAt)
    {
        // Garder les renseignements tels qu'ils se présentaient à la confirmation.
        // L'API gérera plus tard les versions officielles et l'identité de l'employé.
        var owner = LocalAnimalDirectoryStore.GetOwner(ownerId);
        var paymentOwner = LocalPaymentStore.GetOwnerOptions()
            .FirstOrDefault(item => item.OwnerId == ownerId);

        if (owner is null && paymentOwner is null)
            throw new InvalidOperationException("Le dossier propriétaire n'existe plus.");

        LocalNoticeHistoryStore.TryAdd(new NoticeSentHistoryEntry
        {
            OwnerId = ownerId,
            FileNumber = owner?.FileNumber ?? paymentOwner?.FileNumber ?? string.Empty,
            OwnerName = owner?.FullName ?? paymentOwner?.OwnerName ?? string.Empty,
            Municipality = owner?.Municipality ?? paymentOwner?.Municipality ?? string.Empty,
            Year = year,
            Campaign = campaign,
            SentAt = sentAt
        });
    }

    public void ClearDemoHistory() => LocalNoticeHistoryStore.Clear();
}
