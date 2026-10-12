namespace MuniChien.App.ViewModels;

/// <summary>
/// État de présentation réutilisable pour les pages qui chargent des données.
/// Actuellement alimenté par les services locaux synchrones. Lors de l'intégration
/// API, Begin() précédera l'attente asynchrone et Complete()/Fail() la terminera.
/// Aucun basculement automatique vers des données de démonstration en cas d'erreur.
/// </summary>
public sealed class DataLoadState : ViewModelBase
{
    public enum Kind { NotStarted, Loading, Ready, Empty, Error }

    private Kind _current = Kind.NotStarted;
    private string _errorMessage = string.Empty;
    private string _emptyMessage = string.Empty;

    public Kind Current => _current;
    public bool IsLoading => _current == Kind.Loading;
    public bool HasError => _current == Kind.Error;
    public bool IsEmpty => _current == Kind.Empty;
    public bool HasResults => _current == Kind.Ready;
    public string ErrorMessage => _errorMessage;
    public string EmptyMessage => _emptyMessage;

    public void Begin()
    {
        _errorMessage = string.Empty;
        _emptyMessage = string.Empty;
        ChangeTo(Kind.Loading);
    }

    public void Complete(bool hasResults, string emptyMessage = "Aucune donnée disponible.")
    {
        _errorMessage = string.Empty;
        _emptyMessage = hasResults ? string.Empty : emptyMessage;
        ChangeTo(hasResults ? Kind.Ready : Kind.Empty);
    }

    public void Fail(string message)
    {
        // Message destiné à l'utilisateur, sans détails techniques ou données sensibles.
        _errorMessage = message;
        _emptyMessage = string.Empty;
        ChangeTo(Kind.Error);
    }

    private void ChangeTo(Kind next)
    {
        _current = next;
        OnPropertyChanged(nameof(Current));
        OnPropertyChanged(nameof(IsLoading));
        OnPropertyChanged(nameof(HasError));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(HasResults));
        OnPropertyChanged(nameof(ErrorMessage));
        OnPropertyChanged(nameof(EmptyMessage));
    }
}
