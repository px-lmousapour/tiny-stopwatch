# Tiny Stopwatch

A minimal, always-on-top stopwatch that lives in a small floating strip on your desktop. Sessions are saved to history with editable duration and notes.

![Tiny Stopwatch](stopwatch.png)

## Download

Grab the latest release from the [Releases page](https://github.com/px-lmousapour/tiny-stopwatch/releases/latest).

Extract the zip and run `TinyStopwatch.exe` — no installer required.

**Requirements:** Windows 10/11, x64

## Features

- **Compact strip UI** — sits in a corner of your screen, stays on top
- **Tray icon** — hide the window and keep the stopwatch running in the background; tray tooltip shows the live time
- **Session history** — every reset saves the session; view all past sessions from the ☰ button or the tray right-click menu
- **Inline editing** — click any duration or note in the history list to edit it in place
- **Single instance** — launching the app again when it's already running brings the existing window forward instead of creating a second tray icon
- **Jump List** — right-click the taskbar icon to open History directly

## Usage

| Action | How |
|---|---|
| Start / Pause | Click ▶ / ⏸ or press `Space` |
| Reset & save session | Click ↺ (enabled after first start) |
| Hide window | Click ✕ on the strip (keeps running in tray) |
| Show window again | Double-click the tray icon, or right-click → Show / Hide |
| View history | Click ☰ on the strip, or right-click tray → History |
| Edit a session duration | Click the blue time value in the history list (`MM:SS.d` or `HH:MM:SS.d`) |
| Edit a session note | Click the note field in the history list |
| Commit an edit | Press `Enter` or click away |
| Cancel an edit | Press `Escape` |
| Delete a session | Click ✕ on the right side of the history row |
| Resize the strip | Drag the right edge |

## Building from source

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download) and Windows.

```bash
git clone https://github.com/px-lmousapour/tiny-stopwatch.git
cd tiny-stopwatch
dotnet run
```

To produce a self-contained release build:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Output lands in `bin/Release/net8.0-windows/win-x64/publish/`.

## Data

Session history is stored as JSON at:

```
%APPDATA%\TinyStopwatch\history.json
```
