# win-shut

A small Windows-only desktop utility that schedules a forced shutdown through the built-in Windows `shutdown.exe` and displays an **approximate local countdown**.

**Current implementation:** the setup window can request a forced shutdown and displays an approximate local countdown after the request succeeds. The notification-area icon and separate countdown overlay are not implemented yet; see the roadmap.

## Product constraints

- Windows 10 x64 initially; C# / .NET 10 / Windows Forms.
- No account, remote control, cloud service, telemetry, network requests, service, installer, or elevation.
- Windows owns shutdown timing. The app requests `shutdown.exe /s /f /t <seconds>`.
- The countdown is informational and approximate. The app does not monitor Windows shutdown state or detect external cancellation such as `shutdown /a`.
- The last three minutes use a seconds display; the final 30 seconds use a subtle red text pulse.
- `/f` is intentional: apps may terminate without save prompts and unsaved data can be lost.
- No in-app cancellation button and no app-side invocation of `shutdown /a`.
- CI tests run only for PRs targeting `main`; EXE publishing is manual-only.

## Project documentation

- [Agent instructions](AGENTS.md)
- [Product requirements and acceptance criteria](docs/requirements.md)
- [Architecture](docs/architecture.md)
- [Testing strategy](docs/testing.md)
- [CI and repository settings](docs/ci.md)
- [Implementation roadmap](docs/roadmap.md)
- [Project status / session hand-off](docs/status.md)
- [ADR-0001: stack choice](docs/decisions/ADR-0001-stack.md)

The setup window is implemented, but the tray icon, separate countdown overlay and lifecycle hardening remain outstanding. The countdown is only an estimate and does not detect an external `shutdown /a` cancellation. Use this utility only when forced shutdown and potential loss of unsaved work are understood.
