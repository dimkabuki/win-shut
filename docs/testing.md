# Testing strategy

## Automated checks

PR verification runs on a Windows GitHub-hosted runner. Automated tests must never perform a real shutdown, launch the UI, or depend on the machine's actual System event log.

Add coverage as features are implemented:

- Presets, +/-5 increments, min/max boundaries and no wraparound.
- Conversion from minutes to seconds.
- Countdown formatting: 181 s => `4 min`; 180 s => `03:00`; 61 s => `01:01`; 60 s => `01:00`; 59 s => `00:59`; zero/negative => `00:00`.
- Scheduler executable path and separated args; process start failure and non-zero exit.
- State transitions, duplicate-start rejection and failure handling.
- Only fresh abort events transition Active -> Idle; stale/unrelated events are ignored.
- Hiding/exiting never calls `shutdown /a`.
- Use an injected fake process launcher; tests must not execute the actual `shutdown.exe`.

## Manual Windows 10 validation

Perform after saving all work, ideally in a disposable VM/test account.

1. Launch as a standard non-admin user.
2. Verify selected duration and command arguments without waiting for the full duration.
3. Confirm successful start hides setup to tray and shows a movable topmost overlay.
4. From a separate terminal, run `shutdown /a`; verify fresh System/User32 event 1075 and UI idle reset.
5. Repeat as non-admin to verify event-log read access.
6. Move/hide/show overlay and confirm these do not cancel shutdown.
7. Confirm second launch does not start a second timer.
8. Confirm watcher initialization failure prevents scheduling and shows an actionable error.
9. Confirm app does not veto OS shutdown notifications.
10. For the force-close check, use a disposable session and an app with deliberately unsaved data; verify `/f` closes it without prompting. Data loss is expected.

Do not lower the production minimum below five minutes to ease testing. Use an explicit test seam/harness if shorter tests are needed.

## Evidence in the PR

Record Windows edition/build, privilege level, event provider/ID, outcome and errors. Redact usernames/hostnames. Never claim manual validation passed without observing the result.
