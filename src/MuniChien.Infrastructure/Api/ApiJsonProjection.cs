using System.Text.Json;

namespace MuniChien.Infrastructure.Api;

/// <summary>
/// Projection d'affichage en lecture seule : aucun contrat JSON supposé.
/// Les noms de champs sont conservés tels que renvoyés par l'API.
/// Il ne s'agit PAS d'un DTO métier et ces valeurs ne doivent pas servir à modifier des dossiers.
/// </summary>
public sealed record ApiDisplayField(string Name, string Value);

public sealed record ApiDisplayRecord(IReadOnlyList<ApiDisplayField> Fields);

public sealed record ApiDisplayResult(
    IReadOnlyList<ApiDisplayField> Summary,
    IReadOnlyList<ApiDisplayRecord> Records,
    int RecordCount,
    bool HasRecordCollection,
    string Note);

public static class ApiJsonProjection
{
    public const int MaxVisibleRecords = 100;
    public const int MaxFieldsPerRecord = 24;

    public static ApiDisplayResult Accueil(JsonElement root)
    {
        var fields = new List<ApiDisplayField>();
        AddFields(root, fields, prefix: "", depth: 0, maxFields: 60);
        if (fields.Count == 0 && root.ValueKind == JsonValueKind.Array)
            fields.Add(new ApiDisplayField("Éléments reçus", root.GetArrayLength().ToString()));

        return new ApiDisplayResult(fields, [], 0, false,
            "Champs affichés tels que reçus de /api/accueil. La disposition des cartes sera finalisée après validation du contrat JSON.");
    }

    public static ApiDisplayResult Chiens(JsonElement root)
    {
        if (!FindCollection(root, out var collection))
        {
            var fields = new List<ApiDisplayField>();
            AddFields(root, fields, "", 0, 24);
            return new ApiDisplayResult(fields, [], 0, false,
                "La réponse ne contient pas de liste de chiens identifiable. Vérifier sa structure avec Maël.");
        }

        var count = collection.GetArrayLength();
        var rows = new List<ApiDisplayRecord>(Math.Min(count, MaxVisibleRecords));
        foreach (var item in collection.EnumerateArray())
        {
            if (rows.Count >= MaxVisibleRecords) break;
            var fields = new List<ApiDisplayField>();
            AddFields(item, fields, "", 0, MaxFieldsPerRecord);
            rows.Add(new ApiDisplayRecord(fields));
        }

        var note = count > MaxVisibleRecords
            ? $"Aperçu limité aux {MaxVisibleRecords} premières entrées sur {count} reçues. Recherche et pagination à intégrer avec l'API."
            : "Tableau en lecture seule : noms des colonnes issus directement de la réponse JSON.";

        return new ApiDisplayResult([], rows, count, true, note);
    }

    private static bool FindCollection(JsonElement root, out JsonElement result, int depth = 0)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            result = root;
            return true;
        }

        if (root.ValueKind == JsonValueKind.Object && depth < 3)
        {
            // Préférence pour les contenants habituels, sans les considérer comme un contrat.
            string[] priority = ["chiens", "dogs", "items", "resultats", "résultats", "results", "data", "donnees", "données"];
            foreach (var preferred in priority)
            {
                foreach (var property in root.EnumerateObject())
                {
                    if (!property.Name.Equals(preferred, StringComparison.OrdinalIgnoreCase)) continue;
                    if (FindCollection(property.Value, out result, depth + 1)) return true;
                }
            }

            // Repli : repérer un tableau dans un contenant non standard.
            foreach (var property in root.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    result = property.Value;
                    return true;
                }
            }
            foreach (var property in root.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Object &&
                    FindCollection(property.Value, out result, depth + 1)) return true;
            }
        }
        result = default;
        return false;
    }

    private static void AddFields(JsonElement value, List<ApiDisplayField> fields,
        string prefix, int depth, int maxFields)
    {
        if (fields.Count >= maxFields) return;
        if (value.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in value.EnumerateObject())
            {
                if (fields.Count >= maxFields) return;
                var name = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                if (property.Value.ValueKind == JsonValueKind.Object && depth < 2)
                    AddFields(property.Value, fields, name, depth + 1, maxFields);
                else
                    fields.Add(new ApiDisplayField(name, FormatValue(property.Value)));
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
        {
            fields.Add(new ApiDisplayField(string.IsNullOrEmpty(prefix) ? "Éléments" : prefix,
                $"{value.GetArrayLength()} élément(s)"));
        }
        else
        {
            fields.Add(new ApiDisplayField(string.IsNullOrEmpty(prefix) ? "Valeur" : prefix,
                FormatValue(value)));
        }
    }

    private static string FormatValue(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? "",
        JsonValueKind.Number => value.GetRawText(),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Null or JsonValueKind.Undefined => "—",
        JsonValueKind.Array => $"{value.GetArrayLength()} élément(s)",
        JsonValueKind.Object => "Objet imbriqué",
        _ => value.ToString()
    };
}
