# Project status / session hand-off

## Current state

- PR #1 is open: https://github.com/dimkabuki/win-shut/pull/1
- PR #1 contains the initial specification, agent instructions, architecture, roadmap, CI workflows and minimal build/test scaffolding. This PR also clarifies the countdown as informational and removes external shutdown-state monitoring from scope.
- App is only a placeholder form. Timer, tray countdown and shutdown process integration are not implemented.
- The ruleset `protect-main` is active on the default branch, requires PRs and the `verify` status check, and blocks deletion/force-push. There are no bypass actors.
- The most recent CI run before this documentation revision passed restore, format verification, Release build and tests: https://github.com/dimkabuki/win-shut/actions/runs/40324976343
- The docs change should trigger a new PR CI run. Do not treat earlier CI results as validation of the new head until the new run passes.
- The test suite currently contains only a smoke test; it is not evidence of timer behaviour or Windows integration.
- No local build or manual Windows validation has been performed in this environment.

## Decisions

- Windows 10 x64 first; C# / .NET 10 / WinForms.
- Windows `shutdown.exe /s /f /t <seconds>` is the sole shutdown mechanism.
- Presets 15/30/45 minutes, +/-5-minute step, limits 5-180 minutes.
- The local countdown is approximate and starts only after the shutdown command returns success.
- Above three minutes, display whole minutes rounded up; at three minutes or less, display `MM:SS`.
- During seconds 1-30, pulse the text subtly in red; at `00:00`, show a static display.
- No in-app cancel, no app-side `shutdown /a`, and no monitoring/detection of external cancellation or actual Windows shutdown state.
- Closing/crashing the app leaves the OS shutdown scheduled but removes the local display; restart recovery is out of scope.
- Automated tests and CI must never invoke real shutdown.

## Open limitations

- External `shutdown /a` is intentionally not detected. The countdown may continue to zero after cancellation.
- `/f` may cause unsaved work to be lost.
- The app is not implemented yet and existing tests do not validate product behaviour.

## Next action

1. Review and merge PR #1 only after the latest CI run passes.
2. Start PR-002 from the earliest roadmap item: implement the duration model and pure countdown presentation logic, with boundary tests.
3. Keep one roadmap PR in flight at a time.

## Test evidence

- CI run #40324796883 passed on commit `77d9f94baf454f9671373aa61ef30fc103db0de4`.
- CI run #40324976343 passed on commit `8c0f473b45715ea861503172106f36db5fc7337c`.
- These runs predate this documentation revision; the latest head must pass CI before PR #1 is merged.
