# win-shut

A small Windows-only desktop utility that schedules a forced shutdown through the built-in Windows `shutdown.exe` and displays an **approximate local countdown**.

**Current implementation:** preset and editable whole-minute durations, a notification-area menu, a separate approximate countdown overlay, single-instance protection, and centralized lifecycle cleanup.

## Product constraints

- Windows 10 x64 initially; C# / .NET 10 / Windows Forms.
- No account, remote control, cloud service, telemetry, network requests, service, installer, or elevation.
- Windows owns shutdown timing. The app requests `shutdown.exe /s /f /t <seconds>`.
- Duration input accepts whole minutes from 5 through 180; 15/30/45 presets and +/-5-minute controls are also available.
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
- [Windows release validation checklist](docs/release-validation.md)
- [ADR-0001: stack choice](docs/decisions/ADR-0001-stack.md)

The application provides editable whole-minute durations and a local approximate countdown. The countdown does not monitor Windows shutdown state or detect an external `shutdown /a` cancellation. Forced shutdown can close applications without save prompts and may cause data loss.
