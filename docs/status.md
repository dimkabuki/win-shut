# Project status / session hand-off

## Current state

- Bootstrap PR adds product specification, architecture, agent instructions, workflows and minimal build/test scaffolding.
- App is only a placeholder form. Timer, tray countdown, shutdown process integration and abort watcher are not implemented.
- No test result has been observed while preparing these files; GitHub PR CI is the source of truth once it runs.

## Decisions

- Windows 10 x64 first; Windows only.
- C# / .NET 10 / WinForms.
- Windows `shutdown.exe /s /f /t <seconds>` owns shutdown execution.
- Presets 15/30/45 minutes, +/-5-minute step, limits 5-180 minutes.
- Countdown shows whole minutes above three minutes and `MM:SS` at three minutes or less.
- No in-app cancel; app never invokes `shutdown /a`.
- Candidate abort signal is System provider `User32`, event ID `1075`; Windows 10 validation is a hard gate.

## Open risk

Confirm that the event is emitted and readable by a standard Windows 10 user. If not, do not silently switch to undocumented APIs or localized output parsing.

## Next action

1. Review/merge the bootstrap PR after checks pass.
2. Configure branch protection for `main`, requiring `CI / verify`.
3. In a separate PR, implement duration model and pure countdown formatter (PR-002).

## Test evidence

No local Windows test was run preparing these files. A placeholder smoke test does not prove timer behaviour or OS integration.
