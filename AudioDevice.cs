using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AudioSwitcher;

public sealed class AudioDevice : INotifyPropertyChanged
{
    private bool _isFavorite;
    private int _favoriteOrder;

    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Direction { get; init; } = "Playback";
    public string State { get; init; } = "";
    public bool IsActive { get; init; }

    public bool IsFavorite
    {
        get => _isFavorite;
        set { 
            if (_isFavorite != value) 
            { 
                _isFavorite = value; OnPropertyChanged(); 
            } 
        }
    }

    public int FavoriteOrder
    {
        get => _favoriteOrder;
        set { 
            if (_favoriteOrder != value) 
            { _favoriteOrder = value; OnPropertyChanged(); 
            } 
        }
    }

    public string DirectionLabel => Direction == "Playback" ? "WIEDERGABE" : "AUFNAHME";
    public string StateLabel => IsActive ? "Verfügbar" : State;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
