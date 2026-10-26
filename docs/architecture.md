# Architecture

## Technology

- C# / .NET 10 LTS, target `net10.0-windows`.
- Windows Forms, `OutputType=WinExe`, `UseWindowsForms=true`.
- Tray icon: `System.Windows.Forms.NotifyIcon`; countdown: borderless, topmost WinForms form.
- Actual shutdown: system `shutdown.exe`.
- Candidate abort watcher: `System.Diagnostics.Eventing.Reader.EventLogWatcher` on System provider `User32`, event ID `1075`; validate before relying on it.
- Unit tests: xUnit or equivalent. Pure logic and OS side effects must be isolated behind small interfaces.
- No third-party UI framework, MVVM framework, DI container, installer framework or service host.

## Proposed structure

```text
ShutdownTimer.sln
src/ShutdownTimer.App/
  Program.cs
  UI/SetupForm.cs
  UI/CountdownOverlayForm.cs
  UI/TrayController.cs
  Application/TimerController.cs
  Application/TimerState.cs
  Application/CountdownFormatter.cs
  WindowsIntegration/ShutdownScheduler.cs
  WindowsIntegration/ShutdownAbortWatcher.cs
  WindowsIntegration/SingleInstance.cs
tests/ShutdownTimer.Tests/
.github/workflows/ci.yml
.github/workflows/build.yml
AGENTS.md
docs/
```

Keep implementation proportional; merge folders if that reduces complexity, but preserve the boundary between UI/state and OS side effects.

## Component responsibilities

- **SetupForm:** presets, selected minutes, +/- and Start. It does not build process commands or keep time.
- **CountdownOverlayForm:** small, borderless, movable and topmost; displays formatted remaining time. Hiding/closing does not cancel shutdown.
- **TrayController:** owns `NotifyIcon` and show/hide/exit menu. Exiting leaves the OS timer active.
- **TimerController:** state machine and selected duration; coordinates scheduler, countdown and abort notifications. No direct Process/Event Log calls.
- **ShutdownScheduler:** resolves system executable and passes arguments via `ProcessStartInfo.ArgumentList`.
- **ShutdownAbortWatcher:** observes only fresh abort events, starts before scheduling, and raises a typed notification. Never invokes `shutdown /a`.
- **CountdownFormatter:** pure function from remaining duration to a display string.
- **SingleInstance:** named mutex. Add simple activation IPC only if justified; a duplicate must not start a second timer.

## State machine

```text
Idle --Start--> Starting
Starting --watcher ready + schedule succeeds--> Active
Starting --failure--> Idle + error
Active --fresh validated abort event--> Idle
Active --app exits/crashes--> OS shutdown remains scheduled; UI unavailable
Active --OS timer expires--> Windows closes apps and shuts down
```

Window hide/show is not a state transition. No UI transition invokes cancellation.

## Timing and process boundary

Keep requested duration immutable for the active session. Recompute remaining time from `Stopwatch` elapsed time instead of decrementing a counter. Refresh UI about once per second; formatting must tolerate delayed ticks.

Keep process launch behind a small interface. Tests assert executable path and argument vector without launching actual `shutdown.exe`. Use `/s`, `/f`, `/t`, and positive integer seconds. Do not invoke a shell or parse localized console output.

## Failure and shutdown safety

If watcher initialization fails, do not start a timer whose cancellation state cannot be synchronized. If process launch fails or returns non-zero, remain idle. Never intercept/reject OS shutdown notifications, call `ShutdownBlockReasonCreate`, or delay shutdown.

## Distribution

Manual workflow publishes a self-contained single-file `win-x64` EXE and never executes it. No installer, signing, update system or auto-start in initial scope.
