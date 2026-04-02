# Operating System Simulator

An educational Operating System Simulator built with C# (.NET 8) and Uno Platform (Skia). This project visually demonstrates core OS concepts such as Process Management, Context Switching, Mutual Exclusion (File Locking), Memory Allocation, and a custom simulated File System (BKOFS).

## Features

- **Process & Memory Management**: Visual tracking of CPU context switching, process queues, and RAM allocation.
- **Custom File System (BKOFS)**: A virtual file system stored in JSON, supporting file locking (IsBusy), directories, and constraints.
- **Virtual Hardware Monitor**: A dedicated window to observe simulated CPU dispatching, Keystrokes, Disk I/O, Network, and Audio status in real-time.
- **Built-in Apps**: Notepad, File Explorer, Task Manager, and an internal Web Browser (with MP3 audio playback support).

---

## Prerequisites & Installation

To run the application, you need to install the necessary runtimes and dependencies based on your operating system.

### Windows

1. **Required Runtime**: Install the [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (x64).
2. Go to the [Releases](../../releases) page.
3. Download the compiled `OperatingSystemSimulator` release files.
4. Double-click on `OperatingSystemSimulator.exe` to run the simulator.

### Linux

**Option 1: Using the AppImage (Recommended)**
The easiest way to run the application on Linux is using the AppImage, which runs on most distributions without requiring the .NET runtime.

1. **System Dependencies**: The UI is rendered using Skia and requires `fontconfig`. The audio feature (NetCoreAudio) requires `alsa-utils` and `mpg123`. Emojis and other texts also rely on certain fonts. Install them using your package manager (e.g., `sudo apt install -y fonts-noto-color-emoji fontconfig alsa-utils mpg123`).
2. Go to the [Releases](../../releases) page and download the `Operating_System_Simulator-x86_64.AppImage` file.
3. Make the file executable and run it:
   ```bash
   chmod +x Operating_System_Simulator-x86_64.AppImage
   ./Operating_System_Simulator-x86_64.AppImage
   ```

**Option 2: Using the compiled release (Requires .NET 8 Runtime)**
1. **Required Runtime**: Install the [.NET 8 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).
2. **System Dependencies**: The UI is rendered using Skia and requires `fontconfig`. The audio feature (NetCoreAudio) requires `alsa-utils` and `mpg123`. Emojis and other texts also rely on certain fonts.
   Based on your distribution, run the appropriate commands:

   **Ubuntu / Debian / Linux Mint (APT):**
   ```bash
   sudo apt update
   sudo apt install -y fonts-noto-color-emoji fonts-noto-core fonts-ubuntu libfontconfig1 alsa-utils mpg123
   fc-cache -f -v
   ```

   **Fedora / RHEL / CentOS (DNF):**
   ```bash
   sudo dnf install -y fontconfig alsa-utils mpg123 google-noto-color-emoji-fonts google-noto-sans-fonts
   fc-cache -f -v
   ```

   **Arch Linux / Manjaro (Pacman):**
   ```bash
   sudo pacman -S fontconfig alsa-utils mpg123 noto-fonts noto-fonts-emoji
   fc-cache -f -v
   ```
3. Go to the [Releases](../../releases) page and download the compiled `OperatingSystemSimulator` release files.
4. Make the file executable and run it:
   ```bash
   chmod +x OperatingSystemSimulator
   ./OperatingSystemSimulator
   ```

---

## Building from Source

If you want to compile and build the application yourself, you need the **[.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)** installed on your system.

Clone the repository and open a terminal in the root folder, then run the appropriate command for your OS:

### For Windows

```powershell
dotnet publish .\OperatingSystemSimulator\OperatingSystemSimulator.csproj -c Release -f net8.0-desktop -r win-x64 --self-contained false

# To run the application after publishing
cd .\OperatingSystemSimulator\bin\Release\net8.0-desktop\win-x64\publish\
.\OperatingSystemSimulator.exe
```

### For Linux

```bash
dotnet publish ./OperatingSystemSimulator/OperatingSystemSimulator.csproj -c Release -f net8.0-desktop -r linux-x64 --self-contained false

# To run the application after publishing
cd ./OperatingSystemSimulator/bin/Release/net8.0-desktop/linux-x64/publish/
chmod +x OperatingSystemSimulator
./OperatingSystemSimulator
```

You can find the produced output inside:
`OperatingSystemSimulator/bin/Release/net8.0-desktop/win-x64/publish/` (or `linux-x64/publish/` for Linux).

---

## User Guide & Keyboard Shortcuts

The simulator can be controlled using standard mouse interactions and a set of global keyboard shortcuts.

### Global & Desktop Shortcuts

| Shortcut | Action |
| :--- | :--- |
| **Ctrl + Alt + F6** | Open / Bring Virtual Hardware Window to front |
| **Ctrl + Alt + F7** | Open / Bring Page List (Tasks) Window to front |
| **Ctrl + Alt + F11** | Trigger a Manual Kernel BugCheck (Blue Screen) |
| **Ctrl + Q** | Terminate the Currently Focused Application |

### Boot & BIOS Shortcuts

During the initial Boot Animation/Screen, you can interact with the system firmware:

| Shortcut | Action |
| :--- | :--- |
| **F2** | Enter BIOS Settings |
| **Up / Down / Left / Right** | Navigate through BIOS Menus |
| **F5 / F6** | Change selected BIOS Setting Values |
| **F8** | Discard Changes and Exit BIOS |
| **F9** | Save Changes and Exit BIOS |
