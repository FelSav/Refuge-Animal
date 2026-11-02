using System.Text;
using System.Text.Json;

namespace MuniChien.Infrastructure.Api;

/// <summary>
/// Conserve uniquement l'adresse du serveur pour le poste courant.
/// Ne change pas la source de données utilisée par WPF (toujours Démo à l'étape 2).
/// </summary>
public sealed class ApiConnectionSettingsStore
{
    private readonly string _filePath;

    public ApiConnectionSettingsStore(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MuniChien", "connexion-api.json");
    }

    public string LoadServerUrl()
    {
        if (!File.Exists(_filePath))
            return MuniChienApiClient.DefaultServerUrl;

        string json = File.ReadAllText(_filePath, Encoding.UTF8);
        var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
            ?? throw new InvalidDataException("Le fichier de connexion API est vide.");

        if (!settings.TryGetValue("serverUrl", out var serverUrl)
            || string.IsNullOrWhiteSpace(serverUrl))
            throw new InvalidDataException("L'adresse du serveur enregistrée est vide.");

        try
        {
            using var api = new MuniChienApiClient(serverUrl);
            return api.ServerAddress.ToString();
        }
        catch (ArgumentException ex)
        {
            throw new InvalidDataException("L'adresse du serveur enregistrée est invalide.", ex);
        }
    }

    public void SaveServerUrl(string serverUrl)
    {
        // Utiliser la même validation que le client HTTP : schéma HTTP(S), pas d'identifiants.
        using var api = new MuniChienApiClient(serverUrl);
        string normalizedUrl = api.ServerAddress.ToString();
        string? directory = Path.GetDirectoryName(_filePath);
        if (string.IsNullOrWhiteSpace(directory))
            throw new IOException("Le dossier des paramètres API est introuvable.");

        Directory.CreateDirectory(directory);
        string json = JsonSerializer.Serialize(
            new Dictionary<string, string> { ["serverUrl"] = normalizedUrl },
            new JsonSerializerOptions { WriteIndented = true });
        string temporaryFile = _filePath + ".tmp";
        try
        {
            File.WriteAllText(temporaryFile, json, new UTF8Encoding(false));
            File.Move(temporaryFile, _filePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryFile))
                File.Delete(temporaryFile);
        }
    }

}
