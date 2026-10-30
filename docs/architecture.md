# Architecture

## Technology

- C# / .NET 10, target `net10.0-windows`.
- Windows Forms, `OutputType=WinExe`, `UseWindowsForms=true`.
- Tray icon: `System.Windows.Forms.NotifyIcon`; countdown: borderless, topmost WinForms form.
- Actual shutdown request: system `shutdown.exe`.
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
  WindowsIntegration/SingleInstance.cs
tests/ShutdownTimer.Tests/
.github/workflows/ci.yml
.github/workflows/build.yml
AGENTS.md
docs/
```

Keep implementation proportional; merge folders if that reduces complexity, but preserve the boundary between UI/state and OS side effects.

## Component responsibilities

- **SetupForm:** 15/30/45 presets, editable whole-minute duration input, +/-5 and Start. It does not build process commands or keep time.
- **CountdownOverlayForm:** small, borderless, movable and topmost; displays formatted remaining time and warning appearance. Hiding/closing does not cancel shutdown.
- **TrayController:** owns `NotifyIcon` and show/hide/exit menu. Exiting leaves the OS timer active and removes the UI.
- **CountdownController:** local state machine and selected duration; coordinates scheduler and countdown clock. No direct Process calls and no OS shutdown-state monitoring.
- **ShutdownScheduler:** resolves system executable and passes arguments via `ProcessStartInfo.ArgumentList`. Returns success/failure; never invokes `shutdown /a`.
- **StopwatchMonotonicClock:** starts its `Stopwatch` when constructed; the controller captures its elapsed value after the shutdown request succeeds and derives remaining time from the elapsed delta.
- **CountdownFormatter:** pure function from remaining duration to a display string.
- **CountdownVisualState:** pure classification for minute display, normal seconds, last-30-second warning and zero reached.
- **SingleInstanceGuard:** owns a named mutex for the application's lifetime so a second process cannot schedule another shutdown. A duplicate launch shows an informational message and exits; activation IPC is intentionally omitted.

There is deliberately no event-log watcher, polling loop or other detector for Windows shutdown state. If shutdown is canceled externally, the app does not know and the display keeps counting down.

## Local state machine

```text
Idle --Start--> Starting
Starting --command succeeded--> ActiveCountdown
Starting --failure--> Idle + error
ActiveCountdown --elapsed time reaches zero--> ZeroDisplay (`00:00`, static)
ActiveCountdown / ZeroDisplay --app exits--> UI closes; OS shutdown request remains independent
```

Window hide/show is not a state transition. No UI transition invokes cancellation. A new app process does not recover or infer an existing Windows shutdown timer. The local display reaching zero is not evidence that Windows has shut down.

## Timing and process boundary

Keep the requested duration immutable for one active countdown. Start the production monotonic clock at construction and capture its elapsed value only after the shutdown command returns a successful exit code. Recompute remaining time from elapsed monotonic time, not refresh/tick counts. Refresh the text at least once per second; use a separate lightweight visual pulse timer if needed for the final 30 seconds.

Above 180 seconds, display whole minutes rounded up. At 180 seconds and below, display `MM:SS`. For the last 1-30 seconds, pulse only the text between muted and brighter red at approximately 1 Hz; do not flash the window or its background. Use a compact countdown font in the overlay. At zero, stop animation and leave `00:00` static.

Accept any whole-minute duration from 5 through 180. Preset buttons select 15, 30 or 45 minutes; +/- controls move five minutes from the currently selected value. Convert the selected integer minute value to seconds for the OS command.

Keep process launch behind a small interface. Tests assert executable path and argument vector without launching actual `shutdown.exe`. Use `/s`, `/f`, `/t`, and positive integer seconds. Do not invoke a shell or parse localized console output.

## Failure and shutdown safety

If command launch fails or returns a non-zero exit code, remain idle and show an actionable error; do not begin the countdown. Never intercept/reject OS shutdown notifications, call `ShutdownBlockReasonCreate`, delay shutdown, watch event logs or invoke `shutdown /a`.

## Distribution

Manual workflow publishes a self-contained single-file `win-x64` EXE and never executes it. No installer, signing, update system or auto-start in initial scope.
