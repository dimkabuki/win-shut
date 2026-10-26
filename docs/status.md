# Project status / session hand-off

## Current state

- PR #1 was merged into `main` on 2026-10-26. Merge commit: `3bced90df6fec8e4c0271da2e1a5b7ea790c4bd3`.
- PR #2 implements PR-002: a UI-independent duration selection model, pure countdown formatter, final-seconds warning classifier, and deterministic xUnit tests.
- PR #2 targets `main`: https://github.com/dimkabuki/win-shut/pull/2. Use GitHub squash auto-merge only after the required `verify` check succeeds; the PR page is authoritative for its current check and merge state.
- Local validation was not run: the authoring environment has no installed .NET SDK, and its network cannot reach GitHub for a local clone. Do not report local format/build/test success. The Windows PR CI runs format verification, Release build, and tests.
- The live `protect-main` ruleset was inspected on 2026-10-26: active for the default branch, PR required, required status check `verify`, zero required approvals, squash-only merge, no bypass actors, deletion and force-push blocked.
- No UI, tray icon, overlay, running clock, shutdown process runner, scheduling, or lifecycle management is implemented by PR-002. The app is not yet usable.

## Decisions and preserved constraints

- Windows 10 x64 first; C# / .NET 10 / WinForms.
- Windows `shutdown.exe /s /f /t <seconds>` is the only planned shutdown mechanism.
- Presets are 15/30/45 minutes, adjustment step is 5 minutes, and bounds are 5-180 minutes.
- The future local countdown is approximate. It must not monitor Windows shutdown state or detect external cancellation such as `shutdown /a`.
- Above 180 seconds, formatting rounds up to whole minutes; at or below 180 seconds, it uses `MM:SS`.
- The final-seconds warning is active for 1-30 seconds only; zero and negative remaining durations are not warning-active and format as `00:00`.
- `/f` can close apps without save prompts and lose unsaved data.
- No in-app cancel action or app-side invocation of `shutdown /a`. Closing/crashing the app does not cancel an OS request; restart recovery remains out of scope.
- Automated tests and CI must never invoke real shutdown.

## Open limitations

- External `shutdown /a` is intentionally not detected. A future local countdown may continue to zero after external cancellation.
- No local .NET format/build/test run was possible in the authoring environment. Use the latest `verify` result on PR #2 for observed CI validation.
- Manual Windows validation has not been performed.

## Next action

- After PR-002 merges, start **PR-003 - safe Windows shutdown command runner** on `feature/shutdown-runner`. Add an injectable process-launch boundary and test exact `shutdown.exe` path/arguments with a fake launcher; do not run real shutdown in tests or CI.
