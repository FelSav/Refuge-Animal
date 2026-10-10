using System.Text.Json;
using System.IO;

namespace MuniChien.App.Services;

/// <summary>
/// Répertoire LOCAL de démonstration pour les tarifs de licences.
/// Le tarif est unique par (municipalité, année, libellé).
/// À remplacer par un service API quand les règles officielles seront validées.
/// </summary>
public sealed class LocalLicenseTariffService
{
    private readonly string _filePath;
    private readonly List<LicenseTariffEntry> _tariffs;

    public LocalLicenseTariffService()
    {
        string directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MuniChien");

        _filePath = Path.Combine(directory, "tarifs-licences-demo.json");
        _tariffs = ReadFromDisk();
    }

    public string StorageLocation => _filePath;

    public IReadOnlyList<LicenseTariffEntry> GetAll() =>
        _tariffs.OrderBy(x => x.Municipality, StringComparer.CurrentCultureIgnoreCase)
            .ThenByDescending(x => x.Year)
            .ThenBy(x => x.Label, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

    public void Save(LicenseTariffEntry entry)
    {
        if (entry.Id == Guid.Empty)
            throw new ArgumentException("Le tarif doit avoir un identifiant.", nameof(entry));

        if (!MunicipalityCatalog.All.Contains(entry.Municipality, StringComparer.Ordinal))
            throw new ArgumentException("La municipalité doit appartenir au catalogue.", nameof(entry));

        if (entry.Year < 1900 || entry.Year > 2200)
            throw new ArgumentException("L'année doit être comprise entre 1900 et 2200.", nameof(entry));

        if (string.IsNullOrWhiteSpace(entry.Label) || entry.Label.Length > 80)
            throw new ArgumentException("Le libellé doit contenir de 1 à 80 caractères.", nameof(entry));

        if (entry.Amount < 0 || decimal.Round(entry.Amount, 2) != entry.Amount)
            throw new ArgumentException("Le montant doit être positif ou nul, avec au plus deux décimales.", nameof(entry));

        if (_tariffs.Any(x => x.Id != entry.Id &&
                              x.Year == entry.Year &&
                              string.Equals(x.Municipality, entry.Municipality, StringComparison.OrdinalIgnoreCase) &&
                              string.Equals(x.Label, entry.Label, StringComparison.CurrentCultureIgnoreCase)))
            throw new InvalidOperationException("Un tarif avec ce libellé existe déjà pour cette municipalité et cette année.");

        int index = _tariffs.FindIndex(x => x.Id == entry.Id);
        List<LicenseTariffEntry> updated = [.. _tariffs];
        if (index >= 0)
            updated[index] = entry;
        else
            updated.Add(entry);

        WriteToDisk(updated);
        _tariffs.Clear();
        _tariffs.AddRange(updated);
    }

    public void Delete(Guid id)
    {
        List<LicenseTariffEntry> updated = _tariffs.Where(x => x.Id != id).ToList();
        if (updated.Count == _tariffs.Count)
            return;

        WriteToDisk(updated);
        _tariffs.Clear();
        _tariffs.AddRange(updated);
    }

    private List<LicenseTariffEntry> ReadFromDisk()
    {
        if (!File.Exists(_filePath))
            return [];

        try
        {
            var result = JsonSerializer.Deserialize<List<LicenseTariffEntry>>(
                File.ReadAllText(_filePath));
            return result ?? throw new JsonException("Le fichier ne contient pas de liste de tarifs.");
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            // Ne surtout pas écraser un fichier existant mais illisible.
            throw new InvalidDataException(
                $"Impossible de lire les tarifs locaux : {_filePath}. Le fichier a été préservé.", ex);
        }
    }

    private void WriteToDisk(List<LicenseTariffEntry> updated)
    {
        string directory = Path.GetDirectoryName(_filePath)!;
        Directory.CreateDirectory(directory);
        string temporaryFile = _filePath + "." + Guid.NewGuid().ToString("N") + ".tmp";

        try
        {
            File.WriteAllText(temporaryFile, JsonSerializer.Serialize(updated,
                new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temporaryFile, _filePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryFile))
                File.Delete(temporaryFile);
        }
    }
}
