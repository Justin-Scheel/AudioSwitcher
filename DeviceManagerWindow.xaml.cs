using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace AudioSwitcher;

public partial class DeviceManagerWindow : Window
{
    private readonly AudioDeviceService _service;
    private readonly SettingsStore _store;
    private List<AudioDevice> _allDevices = new();
    public ObservableCollection<AudioDevice> Devices { get; } = new();

    public DeviceManagerWindow(AudioDeviceService service, SettingsStore store)
    {
        InitializeComponent();
        _service = service;
        _store = store;
        DataContext = this;
        LoadDevices();
    }

    private void LoadDevices()
    {
        var settings = _store.Load();
        _allDevices = AudioDeviceService.GetDevices();
        foreach (var device in _allDevices)
        {
            var order = settings.FavoriteDeviceIds.IndexOf(device.Id);
            device.IsFavorite = order >= 0;
            device.FavoriteOrder = order >= 0 ? order : int.MaxValue;
        }

        RenderList();
    }

    private void RenderList()
    {
        var query = SearchBox?.Text?.Trim() ?? "";
        var sorted = _allDevices
            .OrderByDescending(d => d.IsFavorite)
            .ThenBy(d => d.FavoriteOrder)
            .ThenBy(d => d.Direction)
            .ThenBy(d => d.Name);

        Devices.Clear();
        foreach (var device in sorted.Where(d => string.IsNullOrWhiteSpace(query) ||
                                                  d.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                                  d.Direction.Contains(query, StringComparison.OrdinalIgnoreCase)))
            Devices.Add(device);

        if (CountText is not null)
            CountText.Text = $"{_allDevices.Count} Geräte · {_allDevices.Count(d => d.IsFavorite)} Favoriten";
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => RenderList();
    private void Refresh_Click(object sender, RoutedEventArgs e) => LoadDevices();

    private void MoveUp_Click(object sender, RoutedEventArgs e) => Move((sender as Button)?.Tag as AudioDevice, -1);
    private void MoveDown_Click(object sender, RoutedEventArgs e) => Move((sender as Button)?.Tag as AudioDevice, 1);

    private void Move(AudioDevice? item, int offset)
    {
        if (item is null) return;
        var favorites = _allDevices.Where(d => d.IsFavorite && d.Direction == item.Direction)
            .OrderBy(d => d.FavoriteOrder).ThenBy(d => d.Name).ToList();
        var index = favorites.IndexOf(item);
        var target = index + offset;
        if (index < 0 || target < 0 || target >= favorites.Count) return;
        (favorites[index].FavoriteOrder, favorites[target].FavoriteOrder) =
            (favorites[target].FavoriteOrder, favorites[index].FavoriteOrder);
        for (var i = 0; i < favorites.Count; i++) favorites[i].FavoriteOrder = i;
        RenderList();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var orderedFavorites = _allDevices.Where(d => d.IsFavorite)
            .OrderBy(d => d.Direction)
            .ThenBy(d => d.FavoriteOrder)
            .ThenBy(d => d.Name)
            .ToList();

        // Persist ordered IDs; the order in this list is the canonical display order.
        _store.Save(new SettingsData { FavoriteDeviceIds = orderedFavorites.Select(d => d.Id).ToList() });
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
}
