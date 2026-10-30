# Testing strategy

## Automated checks

PR verification runs on a Windows GitHub-hosted runner. Automated tests must never perform a real shutdown, launch the application UI, or depend on the machine's actual Windows shutdown state.

Add coverage as features are implemented:

- Presets, direct whole-minute values, +/-5 increments from custom values, 5/180 boundaries and no wraparound.
- Conversion from any supported integer minute value to seconds.
- Countdown formatting: 181 s => `4 min`; 180 s => `03:00`; 61 s => `01:01`; 60 s => `01:00`; 59 s => `00:59`; zero/negative => `00:00`.
- Production monotonic clock starts at construction and its elapsed value advances without an external Start call.
- Visual-state boundaries: 31 s is not warning; 30 s and 1 s are warning; zero is static/non-warning.
- Scheduler executable path and separated args; arbitrary-minute durations, out-of-range values, process-start failure and non-zero exit. Tests use a fake launcher only; no test starts a process.
- PR #4 covers state transitions, duplicate-start rejection, monotonic elapsed-time calculations and failure handling using a fake clock and fake process launcher; no test starts a process.
- The named-mutex single-instance guard rejects a duplicate acquisition and allows reacquisition after disposal.
- Hiding/exiting never calls `shutdown /a`.
- Use an injected fake process launcher and controllable clock; tests must not execute the actual `shutdown.exe`.
- No tests should assert that an external OS cancellation is detected. The product intentionally does not monitor that state.

## Manual Windows 10 validation

Perform in a disposable VM/test account with no valuable unsaved data. External-cancellation and app-exit checks are separate trials.

1. Launch as a standard non-admin user on Windows 10 x64.
2. Check 15/30/45 presets and enter non-preset values such as 7, 37 and 179 minutes. Confirm values outside 5-180 are rejected.
3. Confirm +/-5 changes work from custom values and stop at the 5/180 boundaries.
4. Check the generated duration in seconds and Start/error behavior.
5. Confirm the local display visibly decreases while the scheduled Windows shutdown remains in the future. Check that the display progresses from rounded minutes to seconds at three minutes remaining.
6. Confirm successful start hides setup to tray and shows a movable, topmost overlay with the reduced font size.
7. Verify the subtle red text pulse in the final 30 seconds; confirm animation stops at `00:00`.
8. Move/hide/show the overlay and confirm these actions do not cancel shutdown.
9. **External-cancellation trial:** while the app and countdown remain visible, run `shutdown /a` from a separate terminal. Confirm the app does not detect this cancellation and the local countdown continues by design. Use a controlled test seam if waiting to zero would be impractical.
10. **App-exit trial:** in a fresh run, schedule another timer and exit the app. Then run `shutdown /a` from a separate terminal; a successful abort confirms that exiting the app did not cancel the OS request. Do not rely on an app display after the app has exited.
11. Confirm a second launch displays the already-running message and does not issue another shutdown command.
12. Confirm Windows/session shutdown closes the app without it vetoing, blocking or delaying OS shutdown.
13. For the force-close check, use a disposable session and an app with deliberately unsaved test data; verify that `/f` closes it without prompting. Data loss is expected.

Do not lower the production minimum below five minutes to ease testing. Use an explicit test seam/harness if shorter tests are needed. Do not claim that an external cancellation was detected: detection is explicitly out of scope.

## Evidence in the PR

Record Windows edition/build, privilege level, steps and outcome. Redact usernames/hostnames. Never claim manual validation passed without observing the result.
