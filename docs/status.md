# Project status / session hand-off

## Current state

- PR #1 was merged into `main` on 2026-10-26. Merge commit: `3bced90df6fec8e4c0271da2e1a5b7ea790c4bd3`.
- PR #2 (PR-002 duration model and countdown presentation logic) was merged using squash auto-merge on 2026-10-26. Merge commit: `0aba6c23a809b82025703f865a0168a24503944d`. Its required Windows CI passed format verification, Release build, and 39/39 tests: https://github.com/dimkabuki/win-shut/actions/runs/40354694642
- PR #3 implements PR-003 on `feature/shutdown-runner`: the safe Windows shutdown command runner, injectable process launcher, and deterministic fake-launcher tests. Its current required `verify` state is tracked on the PR; verify the latest head/check before merge.
- The live `protect-main` ruleset was inspected on 2026-10-26: active for the default branch, PR required, required status check `verify`, zero required approvals, squash-only merge, no bypass actors, deletion and force-push blocked.
- Local `dotnet format`, build and test commands were not run because the authoring environment has no installed .NET SDK. The PR's Windows CI runs format verification, Release build and tests.
- PR-003 adds only the command runner. No setup UI, countdown clock/controller, tray icon, overlay, animation or lifecycle management is implemented. The app is not yet usable.

## Decisions and preserved constraints

- Windows 10 x64 first; C# / .NET 10 / WinForms.
- Use only `Path.Combine(Environment.SystemDirectory, "shutdown.exe")` for the executable.
- Invoke `shutdown.exe` without a shell, with `UseShellExecute = false`, `CreateNoWindow = true`, and distinct arguments `/s`, `/f`, `/t`, seconds.
- Supported selected duration is 5-180 minutes in five-minute steps. The 15/30/45-minute presets map to 900/1800/2700 seconds.
- The future local countdown is approximate. It must not monitor Windows shutdown state or detect external cancellation such as `shutdown /a`.
- No in-app cancel action or app-side invocation of `shutdown /a`. Closing/crashing the app does not cancel an OS request; restart recovery remains out of scope.
- Automated tests and CI must never launch a real shutdown command.

## Open limitations

- External `shutdown /a` is intentionally not detected. The future local countdown may continue to zero after external cancellation.
- Local .NET validation and manual Windows validation were not performed in this authoring environment. Use the latest required `verify` result on PR #3 for observed validation.
- The app is not usable yet; the UI/controller milestones remain unimplemented.

## Next action

- After PR-003 merges, start **PR-004 - local countdown controller** on `feature/countdown-controller`. Coordinate the duration model and shutdown runner, start timing only after the runner returns success, and use an injected clock/fake runner for deterministic tests. Do not monitor Windows shutdown state or invoke `shutdown /a`.
