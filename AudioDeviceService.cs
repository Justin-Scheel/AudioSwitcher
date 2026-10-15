using NAudio.CoreAudioApi;

namespace AudioSwitcher;

public sealed class AudioDeviceService
{
    public static List<AudioDevice> GetDevices()
    {
        var result = new List<AudioDevice>();
        using var enumerator = new MMDeviceEnumerator();

        AddDevices(enumerator, DataFlow.Render, "Playback", result);
        AddDevices(enumerator, DataFlow.Capture, "Recording", result);

        return result.OrderBy(d => d.Direction).ThenBy(d => d.Name).ToList();
    }

    private static void AddDevices(MMDeviceEnumerator enumerator, DataFlow flow, string direction, List<AudioDevice> result)
    {
        // Active endpoints are the useful set for the first version. Disabled and disconnected
        // endpoints are intentionally excluded to avoid offering unusable devices for switching
        using var devices = enumerator.EnumerateAudioEndPoints(flow, DeviceState.Active);

        foreach (var device in devices)
        {
            try
            {
                if (device == null)
                {
                    continue;
                }

                result.Add(new AudioDevice
                {
                    Id = device.ID,
                    Name = device.FriendlyName,
                    Direction = direction,
                    State = device.State.ToString(),
                    IsActive = device.State == DeviceState.Active
                });
            }
            finally
            {
                device.Dispose();
            }
        }
    }

    public static string? GetDefaultDeviceId(string direction)
    {
        using var enumerator = new MMDeviceEnumerator();
        var flow = direction == "Playback" ? DataFlow.Render : DataFlow.Capture;

        try
        {
            using var device = enumerator.GetDefaultAudioEndpoint(flow, Role.Multimedia);
            return device.ID;
        }
        catch
        {
            return null;
        }
    }

    public static void SetDefaultDevice(string deviceId, string direction)
    {
        var role = direction == "Playback" ? ERole.eMultimedia : ERole.eMultimedia;
        PolicyConfig.SetDefaultEndpoint(deviceId, role);
    }
}
