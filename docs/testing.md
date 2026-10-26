# Testing strategy

## Automated checks

PR verification runs on a Windows GitHub-hosted runner. Automated tests must never perform a real shutdown, launch the application UI, or depend on the machine's actual Windows shutdown state.

Add coverage as features are implemented:

- Presets, +/-5 increments, min/max boundaries and no wraparound.
- Conversion from minutes to seconds.
- Countdown formatting: 181 s => `4 min`; 180 s => `03:00`; 61 s => `01:01`; 60 s => `01:00`; 59 s => `00:59`; zero/negative => `00:00`.
- Visual-state boundaries: 31 s is not warning; 30 s and 1 s are warning; zero is static/non-warning.
- Scheduler executable path and separated args; process start failure and non-zero exit.
- State transitions, duplicate-start rejection, monotonic elapsed-time calculations and failure handling.
- Hiding/exiting never calls `shutdown /a`.
- Use an injected fake process launcher and controllable clock; tests must not execute the actual `shutdown.exe`.
- No tests should assert that an external OS cancellation is detected. The product intentionally does not monitor that state.

## Manual Windows 10 validation

Perform after saving all work, ideally in a disposable VM/test account with no valuable unsaved data.

1. Launch as a standard non-admin user on Windows 10 x64.
2. Check selected duration, command arguments and error handling.
3. Confirm successful start hides setup to tray and shows a movable, topmost overlay.
4. Verify minute/second formatting and the subtle red text pulse in the final 30 seconds; confirm that animation stops at `00:00`.
5. Move/hide/show the overlay and confirm these actions do not cancel shutdown.
6. Exit the app and verify that the UI closes while the OS shutdown request remains scheduled.
7. In the disposable test environment, run `shutdown /a` from a separate terminal and verify that the app does not detect this cancellation; its local countdown continues to zero by design.
8. Confirm a second launch does not start another timer while the first instance is running.
9. Confirm the app does not veto OS shutdown notifications.
10. For the force-close check, use a disposable session and an app with deliberately unsaved test data; verify that `/f` closes it without prompting. Data loss is expected.

Do not lower the production minimum below five minutes to ease testing. Use an explicit test seam/harness if shorter tests are needed. Do not claim that an external cancellation was detected: detection is explicitly out of scope.

## Evidence in the PR

Record Windows edition/build, privilege level, steps and outcome. Redact usernames/hostnames. Never claim manual validation passed without observing the result.
