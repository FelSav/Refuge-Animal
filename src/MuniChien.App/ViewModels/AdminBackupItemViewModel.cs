namespace MuniChien.App.ViewModels;

public class AdminBackupItemViewModel
{
    public string Name { get; init; } =
        string.Empty;

    public DateTime CreatedAt { get; init; }

    public string Type { get; init; } =
        string.Empty;


    public string CreatedAtDisplay =>
        CreatedAt.ToString(
            "yyyy-MM-dd HH:mm");


    public string DisplayName =>
        $"{CreatedAtDisplay} — {Type}";


    public override string ToString()
    {
        return DisplayName;
    }
}