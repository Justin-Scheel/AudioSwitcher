using System.Collections.ObjectModel;
using System.Windows;

namespace AudioSwitcher;

public partial class MainWindow : Window
{
    private readonly AudioDeviceService _deviceService = new();
    private readonly SettingsStore _settingsStore = new();
    private SettingsData _settings = new();

    public ObservableCollection<AudioDeviceRow> PlaybackFavorites { get; } = new();
    public AudioDeviceRow? SelectedDevice { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        Reload();
    }

    private void Reload()
    {
        _settings = _settingsStore.Load();
        var devices = AudioDeviceService.GetDevices();
        var defaultId = AudioDeviceService.GetDefaultDeviceId("Playback");

        var filteredDevices = devices
                     .Where(d => d.Direction == "Playback" && _settings.FavoriteDeviceIds.Contains(d.Id))
                     .OrderBy(d => _settings.FavoriteDeviceIds.IndexOf(d.Id));

        if (filteredDevices == null)
        {
            return;
        }

        PlaybackFavorites.Clear();

        foreach (var device in filteredDevices)
        {
            PlaybackFavorites.Add(new AudioDeviceRow(device, device.Id == defaultId));
        }

        EmptyMessage.Visibility = PlaybackFavorites.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        if (PlaybackFavorites.Count > 0)
        {
            PlaybackList.SelectedIndex = 0;
        }

        StatusText.Text = $" {devices.Count} aktive Geräte erkannt · {PlaybackFavorites.Count} Wiedergabe-Favoriten";
    }

    private void Refresh_Click(object sender, RoutedEventArgs e) => Reload();

    private void ManageDevices_Click(object sender, RoutedEventArgs e)
    {
        var window = new DeviceManagerWindow(_deviceService, _settingsStore) { Owner = this };
        window.ShowDialog();
        Reload();
    }

    private void SetDefault_Click(object sender, RoutedEventArgs e)
    {
        if (PlaybackList.SelectedItem is not AudioDeviceRow selected)
        {
            StatusText.Text = "Bitte zuerst ein Wiedergabegerät auswählen.";
            return;
        }

        try
        {
            AudioDeviceService.SetDefaultDevice(selected.Device.Id, "Playback");
            Reload();
            StatusText.Text = $"Standardgerät gesetzt: {selected.Device.Name}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                "Windows konnte das Standardgerät nicht ändern.\n\n" + ex.Message +
                "\n\nDie verwendete Windows-Schnittstelle ist nicht offiziell dokumentiert.",
                "Umschalten fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

public sealed class AudioDeviceRow
{
    public AudioDevice Device { get; }
    public string Name => Device.Name;
    public string StateLabel => Device.StateLabel;
    public bool IsDefault { get; }
    public AudioDeviceRow(AudioDevice device, bool isDefault) { Device = device; IsDefault = isDefault; }
}
