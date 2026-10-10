using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace MuniChien.App.Services;

// Prototype local. Les seuls éléments persistés sont les deux textes et la date limite.
// Le futur service API pourra remplacer cette classe sans modifier l'impression.
public static class NoticeTemplateSettingsStore
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MuniChien", "modele-avis.json");

    public static NoticeTemplateSettings Load()
    {
        if (!File.Exists(FilePath))
            return new NoticeTemplateSettings();

        string json = File.ReadAllText(FilePath, Encoding.UTF8);
        var settings = JsonSerializer.Deserialize<NoticeTemplateSettings>(json)
            ?? throw new InvalidDataException("Le modèle d'avis enregistré est vide.");

        if (string.IsNullOrWhiteSpace(settings.RegulationText)
            || string.IsNullOrWhiteSpace(settings.PaymentInstructionsText))
            throw new InvalidDataException("Le modèle d'avis enregistré contient un texte vide.");

        return settings;
    }

    public static void Save(NoticeTemplateSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.RegulationText)
            || string.IsNullOrWhiteSpace(settings.PaymentInstructionsText))
            throw new ArgumentException("Les deux textes de l'avis sont obligatoires.");

        string? directory = Path.GetDirectoryName(FilePath);
        if (directory is null)
            throw new IOException("Le dossier de configuration est introuvable.");

        Directory.CreateDirectory(directory);
        string temporaryFile = FilePath + ".tmp";
        string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(temporaryFile, json, new UTF8Encoding(false));
        File.Move(temporaryFile, FilePath, true);
    }
}
