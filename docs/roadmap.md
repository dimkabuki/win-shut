# Implementation roadmap

Every numbered item is one focused, agent-authored pull request. Work in order: one task branch and one PR at a time; merge the current PR before starting the next item. Do not stack dependent PRs on unmerged work. Each PR must update this file and `docs/status.md` when its milestone changes.

The app only requests a shutdown and displays an **approximate local countdown**. It does not query, watch, or synchronize with the actual Windows shutdown state. In particular, it does not detect a shutdown canceled externally with `shutdown /a`.

## PR-001 - specification, scaffold and CI bootstrap

**Status:** Merged as PR #1 on 2026-10-26.

Scope:
- Product requirements, architecture decision, agent instructions and session hand-off.
- Minimal WinForms app and test project.
- PR-only Windows CI and manual-only EXE publishing workflow.
- A detailed PR-by-PR roadmap.

Acceptance:
- CI passes on the latest PR commit.
- The ruleset on `main` requires a PR and the `verify` check.
- No workflow runs the app or invokes real `shutdown.exe`.
- The current app is clearly documented as a placeholder.

## PR-002 - duration model and countdown presentation logic

**Suggested branch:** `feature/duration-model`

Scope:
- Implement a UI-independent duration selection model with presets 15/30/45 minutes, initial value 30 minutes, and +/-5-minute steps.
- Enforce 5-180 minute bounds without wrapping.
- Implement pure countdown formatting and a testable visual phase classifier.

Acceptance:
- Presets select exact values; increments/decrements are five minutes; bounds are enforced.
- More than 180 seconds displays whole minutes rounded up: 181 s => `4 min`.
- 180 seconds and below use `MM:SS`: 180 s => `03:00`, 59 s => `00:59`.
- 1-30 seconds are classified as warning; zero displays `00:00` and is not classified as warning.
- Unit tests cover boundaries, rounding, invalid inputs and formatting. No UI or process execution is introduced.

**Implementation delivered by PR #2:**
- `DurationSelectionModel` exposes the selected minutes and step availability, accepts only the 15/30/45 minute presets, and clamps step operations at 5-180 minutes.
- `CountdownFormatter` implements invariant minute rounding and `MM:SS` formatting, clamping zero/negative durations to `00:00`.
- `CountdownVisualState.IsFinalSecondsWarningActive` classifies only remaining durations from 1 through 30 seconds as warning-active.
- xUnit coverage replaces the smoke-only suite with deterministic model, formatter and classifier boundary tests. Tests do not sleep, start processes, or execute `shutdown.exe`.
- Local format/build/test commands could not be run because this authoring environment has no .NET SDK. The PR's required Windows `verify` check is the source of truth for CI validation.

## PR-003 - safe Windows shutdown command runner

**Status:** Implemented in PR #3; see the current required `verify` check on the pull request.

**Suggested branch:** `feature/shutdown-runner`

Scope:
- Add a small injectable process-launch boundary and a shutdown scheduler.
- Resolve `shutdown.exe` from `Environment.SystemDirectory`.
- Pass `/s`, `/f`, `/t`, and integer seconds as separate `ProcessStartInfo.ArgumentList` entries, without a shell or elevation.
- Observe command launch failure and exit code; never invoke `shutdown /a`.

Acceptance:
- Tests assert the exact executable path and argument vector for supported durations.
- Tests cover launch exceptions and non-zero exit codes through a fake runner.
- Automated tests cannot execute the real Windows shutdown command.
- The PR does not add any event-log watcher or shutdown-state detection.

**Implementation delivered by PR #3:**
- `IProcessLauncher` provides a small injectable process boundary; `SystemProcessLauncher` waits for the launched process to exit.
- `ShutdownScheduler` resolves the system `shutdown.exe`, configures `UseShellExecute = false` and `CreateNoWindow = true`, and supplies `/s`, `/f`, `/t`, and invariant integer seconds as separate arguments.
- Durations are validated against the existing 5-180 minute range and five-minute step. A zero exit code is success; launch exceptions and non-zero exits return an explicit failure result.
- Tests use only a fake launcher to verify the executable path, argument vector, invocation flags, invalid durations, launch failures and non-zero exit codes. No test launches a process or executes real shutdown.
- The required `verify` check is the authority for format/build/test state; inspect the latest run linked from PR #3 before merge. Local .NET checks were unavailable in this authoring environment.

## PR-004 - local countdown controller

**Status:** Implemented in PR #4; see the required `verify` result linked from the PR.

**Suggested branch:** `feature/countdown-controller`

Scope:
- Connect duration model, shutdown runner and a local countdown controller.
- Start the local countdown only after the scheduling command returns success.
- Use monotonic elapsed time (for example, `Stopwatch`) rather than decrementing a counter on each UI tick.
- Model start failure, duplicate Start requests, active countdown and zero reached.

Acceptance:
- A failed command leaves the controller idle and reports an error; it does not start the countdown.
- Delayed UI ticks do not accumulate timing drift because remaining time is derived from elapsed time.
- At zero, the display stays at `00:00` without warning animation; the app does not claim that Windows has shut down.
- The controller never polls Windows, watches event logs, attempts to detect external cancellation or calls `shutdown /a`.
- Unit tests use a fake runner and controllable clock; no real shutdown is invoked.

**Implementation delivered by PR #4:**
- Add an injectable monotonic clock contract and a production `Stopwatch` implementation.
- Add a controller state machine: Idle, Starting, ActiveCountdown, and ZeroReached.
- Start the countdown only after the scheduler reports success; scheduling failures leave the controller idle with an error.
- Compute remaining duration from elapsed monotonic time, not refresh/tick counts. Reaching zero is a stable display state and does not assert that Windows has shut down.
- Deterministic tests use a fake clock and fake process launcher. They never start a process or invoke `shutdown.exe`.

## PR-005 - setup window and duration controls

**Status:** Merged as PR #5 on 2026-10-26. Squash merge commit: `30717839b84470943cc355424a02622b8191c029`. Required Windows CI passed format verification, Release build, and 59/59 tests: https://github.com/dimkabuki/win-shut/actions/runs/40376646137

**Suggested branch:** `feature/setup-ui`

Scope:
- Implement the setup form with 15/30/45 presets, +/-5-minute buttons, selected duration and Start.
- Connect the controls to the model and controller.
- Keep the forced-shutdown data-loss warning and countdown limitation visible before Start.

Acceptance:
- The UI reflects the model and disables steps at the bounds.
- Repeated Start clicks cannot schedule duplicate commands while a start is in progress or a countdown is active.
- Scheduling errors are shown in plain language and leave the app usable.
- The UI explains that forced shutdown can lose unsaved work and that the displayed countdown is approximate; external cancellation is not detected.
- Manual UI checks and any automated tests are recorded in the PR.

**Implementation delivered by PR #5:**
- Replace the bootstrap window with a fixed WinForms setup form, exact 15/30/45 presets, selected duration, and bounded +/-5-minute controls.
- Connect Start to the existing countdown controller and Windows shutdown scheduler. Disable duration selection and Start while submission is in progress and after scheduling succeeds.
- Show operational failures in the setup window and keep the form available for retry. On success, display the approximate countdown in the setup window until the tray/overlay step is implemented.
- Keep forced-shutdown/data-loss and external-cancellation/approximation warnings visible at all times.
- No automated test invokes the real shutdown command. Manual UI validation is recorded as not performed in this authoring environment; check PR #5 for the required Windows CI result.

## PR-006 - tray icon and countdown overlay

**Status:** Merged as PR #6 on 2026-10-26. Squash merge commit: `4de4cc87fe6cd74366c7cb5bdf72f567604880a5`. Required Windows CI passed format verification, Release build, and 59/59 tests: https://github.com/dimkabuki/win-shut/actions/runs/40379311632

**Suggested branch:** `feature/countdown-overlay`

Scope:
- Add a notification-area icon and menu: Show setup, Show countdown, Exit application.
- Add a small borderless, draggable, always-on-top countdown overlay.
- Implement the display transitions and the last-30-second visual warning.

Acceptance:
- Setup hides to the tray after a successful schedule; the overlay can be moved and shown/hidden.
- Hiding or closing the overlay never cancels the OS shutdown. Exit closes the app and removes the display but leaves the OS shutdown scheduled.
- Above three minutes, show whole minutes; during the last three minutes, show seconds.
- During seconds 1-30, use a subtle red **text-colour pulse only**, approximately once per second. Do not flash the whole window, toggle visibility, or animate the background. At `00:00`, stop the pulse and keep the display static.
- The overlay and tray tooltip make clear that the countdown is an estimate, not a Windows shutdown status indicator.
- No in-app Cancel/Stop control is added.

**Implementation in PR #6:**
- Add a tray application context with the required Show setup, Show countdown, and Exit application actions. Keep Show countdown disabled until scheduling succeeds.
- Move countdown refresh to one shared timer so the setup status and overlay use the same controller and monotonic clock.
- Add a borderless, draggable, always-on-top overlay. Its hide control and window-close handling hide the UI only; they never cancel Windows shutdown.
- Use the existing formatter and final-30-second classifier. Pulse only the countdown text between muted red tones at a 500 ms phase interval; zero remains static 00:00.
- Keep the tray tooltip and overlay explicit that the countdown is approximate and is not Windows shutdown status. The overlay close control has no cancellation semantics.
- Automated tests continue to use fake process launchers; no test or CI workflow launches shutdown.exe. Required Windows CI and manual UI validation must be checked on PR #6 before proceeding.

## PR-007 - lifecycle, single-instance behaviour and cleanup

**Status:** Merged as PR #7 on 2026-10-26. Squash merge commit: `e7826c3205ad8d072700aa7d12e20750e49b20ac`. Required Windows CI passed format verification, Release build, and 61/61 tests with zero warnings: https://github.com/dimkabuki/win-shut/actions/runs/40386849399

**Suggested branch:** `fix/app-lifecycle`

Scope:
- Handle duplicate launches, timer disposal, tray icon cleanup, close/hide semantics and application shutdown paths.
- Ensure a second launch cannot silently schedule another timer while an instance is already running.
- Keep runtime failures from leaving an unusable or orphaned UI.

Acceptance:
- Duplicate launch is detected and does not issue another shutdown command.
- Timers, event handlers, forms and tray resources are disposed deterministically.
- Closing the app does not invoke `shutdown /a`; the OS shutdown request remains independent of app lifetime.
- Restart recovery and persistence of an existing OS timer are explicitly out of scope; a new process does not infer the state of Windows shutdown.

**Implementation in PR #7:**
- Acquire a named mutex before constructing the UI; a duplicate launch displays an informational message and exits without scheduling shutdown.
- Hold the mutex until the message loop ends and release it on the acquiring thread.
- Centralize tray timer, NotifyIcon, menu, event-handler and form cleanup behind an idempotent exit path.
- On user close, preserve the existing hide-to-tray behavior. On Windows/session/application close reasons, release UI resources and allow closure without vetoing or delaying system shutdown.
- Dispose overlay and setup form-owned fonts/timers even when forms are disposed before being shown.
- Add tests for duplicate mutex acquisition, disposal/reacquisition and invalid names. No automated test starts a process or runs shutdown.exe.
- Manual Windows lifecycle validation remains unperformed; the PR must state that explicitly.

## PR-008 - Windows validation, packaging and release hand-off

**Status:** Merged as PR #8 on 2026-10-26. Squash merge commit: `9011a5a1412a26fb995dea461422749a5d7e032e`. The Windows validation checklist and release hand-off documentation are on `main`; publishing and manual Windows runtime validation remain outstanding.

**Suggested branch:** `test/windows-release-validation`

Scope:
- Verify the manually triggered self-contained single-file `win-x64` artifact.
- Complete the Windows UI, lifecycle and shutdown checklist in [release validation](release-validation.md).
- Keep observed automated evidence separate from unperformed manual tests.

Acceptance:
- The manual build workflow succeeds and uploads the expected EXE.
- The EXE launches on a clean Windows 10 x64 environment without a separately installed .NET Desktop Runtime.
- As a standard user, presets, command failure handling, overlay movement/visibility, tray actions, approximate countdown and last-30-second warning are checked.
- In an isolated disposable VM or test session, confirm external `shutdown /a` is intentionally not detected and the local display continues to zero. Confirm `/f` only in a disposable environment with no valuable unsaved work.
- Manual evidence records Windows edition/build and user privilege level; no result is claimed without observation.
- Any remaining manual-only checks are explicitly listed in the final hand-off.
- CI remains limited to build/tests; neither automated tests nor CI execute real shutdown.

## PR-009 - fix countdown clock, editable duration and overlay typography

**Status:** In progress on `fix/countdown-input-overlay`.

Scope:
- Start the production monotonic clock automatically and add a regression test for clock advancement.
- Add editable whole-minute duration input (5-180 minutes), while retaining 15/30/45 presets and +/-5-minute steps.
- Allow the scheduler to accept every supported whole-minute value and pass the exact seconds to `shutdown.exe`.
- Reduce the countdown overlay font sizes.
- Update requirements, testing, release validation and hand-off documentation.

Acceptance:
- The production countdown visibly advances from the selected duration after a successful schedule; the clock does not depend on UI tick count.
- Direct entry accepts any integer from 5 through 180, including values that are not multiples of five. Out-of-range values are rejected.
- Presets and +/-5 controls stay synchronized with the numeric input.
- Scheduler tests prove exact second conversion for custom values, with no real process execution.
- Overlay countdown typography is visibly smaller without changing shutdown behavior.
- CI passes format verification, Release build and all tests. Manual Windows retest remains explicitly recorded until performed.
- No automated test or workflow executes real shutdown.

## Cross-PR rules

- Keep each PR focused, with a clear summary, acceptance criteria, tests and known limitations.
- Read `AGENTS.md`, `docs/status.md`, this roadmap, requirements, architecture and relevant ADRs at the start of each agent session.
- Do not begin an item until its dependencies are merged.
- Never add shutdown-state monitoring, external-abort detection, in-app cancellation, auto-start, elevation, services, network calls or telemetry without an explicit scope change.
- Update `docs/status.md` with the current PR, latest observed test result, remaining risks and the exact next action.

## Overall exit criteria

The applicable acceptance criteria pass; the latest PR CI is green; manual Windows results and limitations are documented; automated tests never execute real shutdown; and the final single-file artifact works on the declared Windows target.
