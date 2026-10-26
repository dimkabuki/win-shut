# Product requirements and acceptance criteria

## Goal and scope

Build a small Windows-only UI on top of the existing Windows shutdown timer. The app schedules an OS-level shutdown, shows a movable countdown overlay, and returns to idle when external `shutdown /a` cancellation is detected reliably.

This is a convenience timer, not a tamper-resistant parental-control or security boundary.

In scope: Windows 10 x64 initially; setup UI with 15/30/45-minute presets, +/-5-minute step, Start, tray icon, draggable always-on-top countdown overlay, Windows shutdown integration, external-abort synchronization, automated tests, PR-only CI, manual EXE publish.

Out of scope: remote management, accounts, cloud, telemetry, network, per-program limits, allow/block lists, PIN/password, kiosk mode, tamper resistance, service/task, admin install, registry policies, auto-start, in-app cancel, custom process killer, non-Windows platforms.

## Setup UI

- Preset buttons `15`, `30`, `45` set that exact value, not an increment.
- Initial selection is 30 minutes.
- `-5 min` and `+5 min` change the selected value in 5-minute steps.
- Minimum 5 minutes and initial maximum 180 minutes; define limits as named constants.
- Disable the corresponding step button at bounds; never wrap.
- Disable/reject repeated Start while submitting or active.
- Before first use, clearly warn that forced shutdown may lose unsaved data.

## Active UI

After scheduling succeeds, hide the setup window to the notification area and show a small borderless, draggable, always-on-top overlay. The overlay displays only the countdown and may have a hide-only control. Never expose Cancel/Stop in the app. Hiding or closing the overlay does not cancel shutdown.

Tray menu: `Show setup`, `Show countdown`, `Exit application`. Exiting the UI leaves Windows shutdown scheduled and this must be stated clearly. On a valid external abort, stop the countdown, hide overlay and restore idle setup UI.

## Countdown formatting

- More than 180 seconds remaining: whole minutes only, rounded up; 181 s => `4 min`.
- At 180 seconds or less: `MM:SS`; 180 s => `03:00`, 59 s => `00:59`.
- Never show negative time.
- Derive time from monotonic elapsed time (e.g. `Stopwatch`), not a counter decremented on each tick.
- Windows owns the real shutdown timing; the overlay is informational.

## Shutdown behaviour

- Resolve executable as `Path.Combine(Environment.SystemDirectory, "shutdown.exe")`.
- Use `ProcessStartInfo` with `UseShellExecute = false`, `CreateNoWindow = true`, and individual `ArgumentList` entries.
- Exact args: `/s`, `/f`, `/t`, integer seconds. Presets map to 900, 1800 and 2700 seconds.
- Do not invoke cmd.exe/PowerShell, shell-concatenate arguments, use `UseShellExecute = true`, or elevate.
- Inspect launch exceptions and exit status. On failure, remain idle and show actionable error.
- Closing/crashing the UI must not cancel an already scheduled shutdown.
- `/f` is mandatory and intentionally closes apps without save prompts; unsaved work can be lost.
- Never veto, block or delay OS shutdown notifications.

## External cancellation

- User cancels externally, e.g. in Command Prompt/PowerShell using `shutdown /a`.
- The app itself never invokes `shutdown /a`.
- Candidate signal is a new System event-log event from provider `User32`, event ID `1075`.
- Initialize the watcher/bookmark before scheduling and ignore historical events.
- Only a fresh abort event observed while Active can return the UI to Idle.
- Validate signal semantics on Windows 10 and read access as a normal non-admin user.
- If signal is absent/unreliable/inaccessible, record evidence and stop before shipping this feature. Do not use undocumented NT APIs or parse localized console output as the primary signal.
- If watcher initialization fails, do not start a timer that cannot synchronize cancellation state.

## Lifecycle and privacy

Single-instance behaviour should activate the existing instance where practical. Handle process-start failures, non-zero exit, watcher errors, duplicate starts and disposal without unhandled exceptions. Dispose timers, watcher, tray icon and handlers. No network, accounts, telemetry, usage history, elevation, service, task, registry policy, or auto-start. Saving only the overlay position in current-user local app data is optional.

## Packaging and CI

Target `net10.0-windows`, publish self-contained single-file `win-x64`. PR CI triggers only on `pull_request` targeting `main`; publishing is a separate `workflow_dispatch` workflow. Neither workflow executes the built application or a real shutdown.

## Acceptance criteria

- **AC-01 Presets:** 15/30/45 select exact value; +/- changes by five; bounds do not wrap.
- **AC-02 Command:** 15/30/45 map to `/s /f /t 900`, `/s /f /t 1800`, `/s /f /t 2700`. Tests use a fake launcher.
- **AC-03 Active UI:** successful scheduling hides setup to tray and shows a draggable topmost overlay.
- **AC-04 Display:** 181 s => `4 min`; 180 s => `03:00`; 59 s => `00:59`; zero/negative => `00:00`.
- **AC-05 No in-app cancellation:** no UI action calls `shutdown /a`; hide/exit do not cancel.
- **AC-06 External abort:** separate-terminal cancellation returns UI to idle based on a validated event, tested as standard user on Windows 10.
- **AC-07 Forced close:** manual destructive test confirms `/f` closes a disposable unsaved app without a save prompt.
- **AC-08 No shutdown blocking:** no rejected `WM_QUERYENDSESSION`, shutdown-block reason or other delay.
- **AC-09 Errors:** schedule failure leaves app idle and shows an error.
- **AC-10 CI safety:** PR CI builds/tests but never executes real shutdown; EXE publishing is manual-only.
