# AudioSwitcher

A lightweight Windows 11 application for quickly switching between your favorite audio devices.

## Why did I create this?

Windows 11 does not provide a convenient way to reorder audio devices in its built-in sound output selector. 
When many audio devices are installed, for example virtual devices from Voicemeeter alongside physical audio hardware, so finding the right device every time can become frustrating.
AudioSwitcher aims to make this process faster and more convenient.

## Features

- **Quick audio switching:** Switch between your favorite audio devices from a single window.
- **Custom favorites list:** Select the devices you use most often and keep them readily accessible.
- **Persistent settings:** Your favorite devices are saved locally and restored when you start the application again.

## Requirements

- Windows 11
- .NET 9 SDK (to build the project)
- VSCode or Visual Studio

## Building from source

1. Clone this repository.
2. Open `AudioSwitcher.csproj` in Visual Studio or VSCode, or navigate to the project directory in a terminal.
3. Restore the NuGet packages and build the project.

Alternatively, run:

```bash
dotnet restore
dotnet build
```

## Where is user data stored?

Your favorite devices and application settings are stored locally at:

`%LOCALAPPDATA%\AudioSwitcher\settings.json`

## Dependencies

Audio device discovery is implemented using [NAudio](https://github.com/naudio/NAudio), a .NET audio library that provides access to Windows audio APIs.
Some functionality relies on Windows audio interfaces and behavior that may change in future Windows updates. I will do my best to maintain compatibility, 
but I cannot guarantee that every future Windows version will be supported.

## Contributing

Contributions, bug reports, and suggestions are welcome.
If you would like to contribute code, please open a pull request. I will review contributions as time permits.

## License

This project is licensed under the **PolyForm Noncommercial License 1.0.0**.
Commercial use is not permitted under this license without separate permission from the copyright holder.
See the [`LICENSE`](LICENSE) file for the complete license terms.
For commercial licensing inquiries, please contact the copyright holder via GitHub.