using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace MuniChien.App.Services;

// Données de test locales, séparées des vrais avis qui proviendront de l'API.
// Enregistrement atomique : une erreur disque n'est jamais présentée comme un envoi confirmé.
public static class LocalNoticeHistoryStore
{
    private static readonly object Sync = new();
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MuniChien", "avis-envois-DEMO.json");

    private static List<NoticeSentHistoryEntry>? _entries;

    public static IReadOnlyList<NoticeSentHistoryEntry> GetAll()
    {
        lock (Sync)
        {
            return Load().OrderByDescending(entry => entry.SentAt).ToArray();
        }
    }

    public static bool TryAdd(NoticeSentHistoryEntry entry)
    {
        lock (Sync)
        {
            var current = Load();
            if (current.Any(existing => existing.OwnerId == entry.OwnerId
                && existing.Year == entry.Year
                && existing.Campaign == entry.Campaign))
                return false;

            var updated = new List<NoticeSentHistoryEntry>(current) { entry };
            Save(updated);
            _entries = updated;
            return true;
        }
    }

    public static void Clear()
    {
        lock (Sync)
        {
            // Conserver un fichier JSON valide même en cas de réinitialisation.
            Save(new List<NoticeSentHistoryEntry>());
            _entries = new List<NoticeSentHistoryEntry>();
        }
    }

    private static List<NoticeSentHistoryEntry> Load()
    {
        if (_entries is not null)
            return _entries;

        if (!File.Exists(FilePath))
        {
            _entries = new List<NoticeSentHistoryEntry>();
            return _entries;
        }

        try
        {
            string json = File.ReadAllText(FilePath, Encoding.UTF8);
            _entries = JsonSerializer.Deserialize<List<NoticeSentHistoryEntry>>(json)
                ?? throw new InvalidDataException("Le fichier d'historique est vide.");
            return _entries;
        }
        catch (Exception ex) when (ex is JsonException || ex is IOException)
        {
            throw new InvalidDataException(
                "Impossible de lire l'historique de démonstration. Aucun envoi ne sera enregistré tant que ce problème n'est pas résolu.",
                ex);
        }
    }

    private static void Save(List<NoticeSentHistoryEntry> entries)
    {
        string? directory = Path.GetDirectoryName(FilePath);
        if (directory is null)
            throw new IOException("Le dossier d'historique est introuvable.");

        Directory.CreateDirectory(directory);
        string temporaryFile = FilePath + ".tmp";
        try
        {
            string json = JsonSerializer.Serialize(entries, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(temporaryFile, json, new UTF8Encoding(false));
            File.Move(temporaryFile, FilePath, true);
        }
        finally
        {
            if (File.Exists(temporaryFile))
                File.Delete(temporaryFile);
        }
    }
}
