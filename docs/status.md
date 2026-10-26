# Project status / session hand-off

## Current state

- PR #1 merged on 2026-10-26.
- PR #2 (duration model and countdown presentation logic) merged by squash auto-merge. Merge commit: `0aba6c23a809b82025703f865a0168a24503944d`. Required Windows CI passed format verification, Release build, and 39/39 tests: https://github.com/dimkabuki/win-shut/actions/runs/40354694642
- PR #3 (safe Windows shutdown command runner) merged by squash auto-merge. Merge commit: `181bc6e4c8becbad5c54ebd1567685d2bd0dab6f`. Required Windows CI passed format verification, Release build, and 51/51 tests: https://github.com/dimkabuki/win-shut/actions/runs/40358333568
- PR #4 implements PR-004 on `feature/countdown-controller`: local countdown controller, injected monotonic clock, and deterministic fake-runner tests. Check the latest required `verify` result on PR #4 before considering validation complete.
- The live `protect-main` ruleset was inspected: active for the default branch, PR required, required check `verify`, zero required approvals, squash-only, no bypass actors, deletion and force-push blocked.
- Local .NET format/build/test commands were not run because the authoring environment has no installed .NET SDK. CI is the validation source of truth.
- PR-004 does not add UI; the application is not yet usable.

## Decisions and preserved constraints

- Windows 10 x64 first; C# / .NET 10 / WinForms.
- Shutdown is requested only through `Path.Combine(Environment.SystemDirectory, "shutdown.exe")`, without shell execution, using separate `/s`, `/f`, `/t`, seconds arguments.
- Selected duration is 5-180 minutes in five-minute increments.
- Countdown is approximate and based on monotonic elapsed time. It starts only after successful scheduling.
- Do not monitor Windows shutdown state, detect external `shutdown /a`, invoke `shutdown /a`, or infer/recover an OS timer after app restart.
- Automated tests and CI must never launch a real shutdown command.

## Open limitations

- External shutdown cancellation is intentionally not detected; local countdown may continue to zero.
- Local .NET checks and manual Windows validation have not been performed in this authoring environment.
- Setup UI, tray/overlay and lifecycle management remain unimplemented.

## Next action

- After PR-004 merges, start **PR-005 - setup window and duration controls** on `feature/setup-ui`. Connect presets, +/- controls and Start to the model/controller; show scheduling failures and the forced-shutdown/approximate-countdown warnings. Keep the UI thin and prevent duplicate Start requests.
