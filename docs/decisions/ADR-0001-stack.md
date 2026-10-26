# ADR-0001: C# / .NET 10 / WinForms

- Status: Accepted for initial implementation
- Date: 2026-10-26

## Context

The app targets Windows only and needs a small desktop UI, tray icon, movable countdown overlay, local process invocation and no remote features. Keep the dependency graph small.

## Decision

Use C# targeting `net10.0-windows` with Windows Forms. Use `NotifyIcon` for the tray and a borderless WinForms form for the countdown overlay. Use Windows `shutdown.exe` for shutdown. Publish a self-contained single-file `win-x64` EXE through a manually triggered GitHub Actions workflow.

## Consequences

- Uses standard Windows/.NET facilities.
- A self-contained EXE does not need a separate .NET Desktop Runtime.
- WinForms is sufficient; WPF/WinUI or third-party UI frameworks add unnecessary complexity here.
- Abort detection needs supported event-log observation and must be validated on Windows 10 before being treated as reliable.
- Keep state/formatting logic and OS side effects behind small interfaces so deterministic tests remain practical.

## Alternatives

- WPF: capable, but unnecessary for this compact UI; tray still requires `NotifyIcon` integration.
- WinUI 3: adds packaging/application-model overhead not justified by this utility.
- PowerShell/AutoHotkey: quick for a one-off script but less suitable for typed tests and the requested agentic PR workflow.
