# Project status / session hand-off

## Current state

- PRs #1-#7 are merged to `main`.
- PR #7 merged on 2026-10-26 as `e7826c3205ad8d072700aa7d12e20750e49b20ac`. Required Windows CI passed format verification, Release build, and 61/61 tests with zero warnings: https://github.com/dimkabuki/win-shut/actions/runs/40386849399
- PR-008 is in progress on `test/windows-release-validation`.
- The manual `.github/workflows/build.yml` workflow is configured to publish a self-contained single-file `win-x64` executable and upload `ShutdownTimer-win-x64` for seven days. Workflow execution and artifact contents have not been verified in this session.
- Interactive Windows UI, clean-machine launch, and disposable-VM shutdown checks were not performed. Do not claim them as passed.
- Local .NET format/build/test commands were not run in this authoring environment; CI is the evidence for automated checks.

## Preserved constraints

- Windows 10 x64; C# / .NET 10 / WinForms.
- Invoke only the system `shutdown.exe`, without a shell, using separate `/s`, `/f`, `/t`, and seconds arguments.
- Countdown is approximate and does not detect external `shutdown /a`. Do not invoke `shutdown /a` from the app or add in-app cancellation.
- App exit does not cancel a scheduled Windows shutdown. Do not block or delay system/session shutdown.
- Automated tests and CI must never launch a real shutdown command.

## Open limitations

- Actual manual build workflow run and uploaded EXE have not been inspected.
- Manual validation of setup, tray, overlay, mutex behavior, external cancellation, app exit, Windows/session close and forced close remains outstanding.
- No signed binary, installer, automatic update or release hosting is in scope.

## Next action

1. From `main`, run Actions → Build Windows executable → Run workflow and inspect the uploaded `ShutdownTimer-win-x64` artifact.
2. Launch the EXE on a clean Windows 10 x64 system without a separately installed .NET Desktop Runtime.
3. Complete [the release validation checklist](release-validation.md) in a disposable test session and record Windows build and privilege level.
4. Address any failures in a focused follow-up PR; do not claim unobserved checks passed.
