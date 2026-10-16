using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace AudioSwitcher;

public partial class MainWindow : Window
{
    private readonly AudioDeviceService _deviceService = new();
    private readonly SettingsStore _settingsStore = new();
    private SettingsData _settings = new();

    // FxSound flags
    private bool IsFxSoundInstalled => File.Exists(@"C:\Program Files\FxSound LLC\FxSound\FxSound.exe");
    private bool IsFxSoundRunning = Process.GetProcessesByName("FxSound").Length > 0;

    // Audio devices
    public ObservableCollection<AudioDeviceRow> FavoriteAudioDevices { get; } = new();
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

        FavoriteAudioDevices.Clear();

        foreach (var device in filteredDevices)
        {
            FavoriteAudioDevices.Add(new AudioDeviceRow(device, device.Id == defaultId));
        }

        EmptyMessage.Visibility = FavoriteAudioDevices.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        if (FavoriteAudioDevices.Count > 0)
        {
            AudioDevices.SelectedIndex = 0;
        }

        StatusText.Text = $" {devices.Count} aktive Geräte erkannt · {FavoriteAudioDevices.Count} Wiedergabe-Favoriten";
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
        if (AudioDevices.SelectedItem is not AudioDeviceRow selected)
        {
            StatusText.Text = "Bitte zuerst ein Wiedergabegerät auswählen.";
            return;
        }

        try
        {
            var isFxSound = selected.Name.Contains("FxSound");

            // Special case for the FxSound virtual audio device, which will not be started/used when only set as the default device.
            if (isFxSound && !this.IsFxSoundRunning)
            {
                if (this.IsFxSoundInstalled)
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = @"C:\Program Files\FxSound LLC\FxSound\FxSound.exe",
                        UseShellExecute = true
                    };

                    Process.Start(startInfo);
                    this.IsFxSoundRunning = true;
                }
                else
                {
                    StatusText.Text = $"FxSound konnte nicht gestartet werden, da die Datei nicht gefunden wurde. Standardpfad: {@"C:\Program Files\FxSound LLC\FxSound\FxSound.exe"}";
                }
            }

            // Kill FxSound process if it was started and the selected device is not FxSound to avoid issues with other audio devices
            if (!isFxSound && this.IsFxSoundRunning)
            {
                foreach (var process in Process.GetProcessesByName("FxSound"))
                {
                    process.Kill();
                    process.WaitForExit();
                }

                this.IsFxSoundRunning = false;
            }

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
    public AudioDeviceRow(AudioDevice device, bool isDefault) 
    { 
        Device = device; 
        IsDefault = isDefault; 
    }
}
