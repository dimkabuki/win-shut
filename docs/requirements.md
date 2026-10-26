# Product requirements and acceptance criteria

## Goal and scope

Build a small Windows-only utility that requests a forced shutdown through the built-in Windows `shutdown.exe` command and displays an **approximate local countdown**.

Windows owns the actual shutdown timer. The app does not query, monitor or synchronize with Windows shutdown state. It does not detect an external cancellation such as `shutdown /a`. Therefore, the countdown may continue after the OS timer has been canceled, and it may reach `00:00` without the machine shutting down. This limitation must be explained in the UI.

This is a convenience utility, not a tamper-resistant parental-control or security boundary.

In scope: Windows 10 x64 initially; C#/.NET 10/WinForms; duration presets and adjustments; a Start action; tray icon; draggable always-on-top countdown overlay; safe process invocation; deterministic tests; PR-only CI; manually triggered EXE publishing.

Out of scope: shutdown-state detection, event-log watching, external-cancellation synchronization, in-app cancel/stop, remote management, accounts, cloud, telemetry, network, per-program limits, allow/block lists, PIN/password, kiosk mode, tamper resistance, service/task, admin install, registry policies, auto-start and non-Windows platforms.

## Setup UI

- Preset buttons `15`, `30`, `45` set that exact value, not an increment.
- Initial selection is 30 minutes.
- `-5 min` and `+5 min` change the selected value in 5-minute steps.
- Minimum is 5 minutes and initial maximum is 180 minutes; define limits as named constants.
- Disable the corresponding step button at bounds; never wrap.
- Disable/reject repeated Start while the scheduling command is being submitted or a local countdown is active.
- Keep a clear warning visible before Start: forced shutdown may close apps without save prompts and unsaved work can be lost.
- Explain before Start that the countdown is approximate and external cancellation is not detected.

## Active UI

After the scheduling command succeeds, hide the setup window to the notification area and show a small borderless, draggable, always-on-top overlay. The overlay displays the countdown and may have a hide-only control. Never expose Cancel/Stop in the app. Hiding or closing the overlay does not cancel shutdown.

Tray menu: `Show setup`, `Show countdown`, `Exit application`. Exiting the app closes the UI but leaves the Windows shutdown scheduled. If the app exits or crashes, the countdown display is lost; the app does not restore or infer a previous Windows timer on restart.

The overlay/tray tooltip must make it clear that the displayed countdown is only an estimate, not the confirmed Windows shutdown state. If shutdown is canceled externally, the local countdown continues; the UI does not react to that cancellation.

## Countdown timing and appearance

- Derive remaining time from monotonic elapsed time, not by decrementing a counter once per timer tick.
- Above 180 seconds remaining, show whole minutes rounded up. Example: 181 seconds => `4 min`.
- At 180 seconds or less, show `MM:SS`. Examples: 180 seconds => `03:00`; 59 seconds => `00:59`.
- During seconds 1-30 inclusive, apply a subtle red text-colour pulse, approximately once per second.
- The pulse changes the text colour only. Do not flash the whole overlay, toggle window visibility, or animate the background.
- At zero, show static `00:00`; stop the pulse. Do not imply that shutdown has completed or is still pending in Windows.
- Never show negative time.

## Shutdown behaviour

- Resolve the executable as `Path.Combine(Environment.SystemDirectory, "shutdown.exe")`.
- Use `ProcessStartInfo` with `UseShellExecute = false`, `CreateNoWindow = true`, and individual `ArgumentList` entries.
- Exact args: `/s`, `/f`, `/t`, integer seconds. Presets map to 900, 1800 and 2700 seconds.
- Do not invoke cmd.exe/PowerShell, shell-concatenate arguments, use `UseShellExecute = true`, or elevate.
- Handle launch exceptions and non-zero exit status. On failure, remain idle and show an actionable error. Start the local countdown only after the command has completed successfully.
- Closing/crashing the UI must not cancel an already scheduled shutdown.
- `/f` is mandatory and intentionally closes apps without save prompts; unsaved work can be lost.
- Never veto, block or delay OS shutdown notifications.
- The app does not execute `shutdown /a` or attempt to detect whether a user ran that command elsewhere.

## Lifecycle and privacy

Handle duplicate launches, duplicate Start requests, process-start failures, non-zero exit, disposal and tray/icon cleanup without unhandled exceptions. Use a single-instance guard so a second process does not silently schedule another timer. Restart recovery/persistence is not part of v1. Dispose timers, tray icon and handlers. No network, accounts, telemetry, usage history, elevation, service, task, registry policy or auto-start. Saving only the overlay position in current-user local app data is optional.

## Packaging and CI

Target `net10.0-windows`, publish self-contained single-file `win-x64`. PR CI triggers only on `pull_request` targeting `main`; publishing is a separate `workflow_dispatch` workflow. Neither workflow executes the built application or a real shutdown.

## Acceptance criteria

- **AC-01 Presets:** 15/30/45 select exact value; +/- changes by five; bounds do not wrap.
- **AC-02 Command:** 15/30/45 map to `/s /f /t 900`, `/s /f /t 1800`, `/s /f /t 2700`. Tests use a fake launcher.
- **AC-03 Start/error:** local countdown starts only after a successful command exit; launch failure/non-zero exit leaves the app idle.
- **AC-04 Display:** 181 s => `4 min`; 180 s => `03:00`; 59 s => `00:59`; zero/negative => `00:00`.
- **AC-05 Warning:** seconds 1-30 show a subtle red text pulse; zero is static and not red/pulsing.
- **AC-06 Approximation:** the app does not monitor Windows shutdown or detect external `shutdown /a`; the local countdown can continue to zero after external cancellation.
- **AC-07 No in-app cancellation:** no UI action calls `shutdown /a`; hide/exit do not cancel.
- **AC-08 Forced close:** manual destructive test confirms `/f` closes a disposable test app without a save prompt.
- **AC-09 No shutdown blocking:** no rejected `WM_QUERYENDSESSION`, shutdown-block reason or other delay.
- **AC-10 CI safety:** PR CI builds/tests but never executes real shutdown; EXE publishing is manual-only.
