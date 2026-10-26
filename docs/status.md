# Project status / session hand-off

## Current state

- PR #1 merged on 2026-10-26.
- PR #2 (duration model and countdown presentation logic) merged by squash auto-merge. Merge commit: 0aba6c23a809b82025703f865a0168a24503944d. Required Windows CI passed format verification, Release build, and 39/39 tests: https://github.com/dimkabuki/win-shut/actions/runs/40354694642
- PR #3 (safe Windows shutdown command runner) merged by squash auto-merge. Merge commit: 181bc6e4c8becbad5c54ebd1567685d2bd0dab6f. Required Windows CI passed format verification, Release build, and 51/51 tests: https://github.com/dimkabuki/win-shut/actions/runs/40358333568
- PR #4 (local countdown controller) merged by squash auto-merge. Merge commit: fc3b7a241f1fbaa209f0df4ef326b9adf7f68310. Required Windows CI passed formatting, Release build, and 58/58 tests: https://github.com/dimkabuki/win-shut/actions/runs/40359474121
- PR #5 (setup window and duration controls) merged by squash auto-merge on 2026-10-26. Merge commit: 30717839b84470943cc355424a02622b8191c029. Required Windows CI passed format verification, Release build, and 59/59 tests: https://github.com/dimkabuki/win-shut/actions/runs/40376646137
- PR-006 implementation is on feature/countdown-overlay: tray menu, shared countdown refresh, and draggable always-on-top overlay. Check PR #6 for the required Windows verify status and merge state before starting PR-007.
- The live protect-main ruleset is active for the default branch, requires a PR and the verify check, allows squash-only merges, and blocks force-push/deletion.
- Local .NET format/build/test commands have not been run in this authoring environment. Manual Windows UI validation has not been performed. CI is the validation source of truth for automated checks.

## Decisions and preserved constraints

- Windows 10 x64 first; C# / .NET 10 / WinForms.
- Shutdown is requested only through Path.Combine(Environment.SystemDirectory, "shutdown.exe"), without shell execution, using separate /s, /f, /t, seconds arguments.
- Selected duration is 5-180 minutes in five-minute increments.
- Countdown is approximate and based on monotonic elapsed time. It starts only after successful scheduling.
- The tray controller reuses the existing CountdownController and CountdownFormatter; one UI timer refreshes both forms.
- Do not monitor Windows shutdown state, detect external shutdown /a, invoke shutdown /a, or infer/recover an OS timer after app restart.
- Hiding the setup window or overlay and exiting the app do not cancel the already scheduled Windows shutdown.
- Automated tests and CI must never launch a real shutdown command.

## Open limitations

- External shutdown cancellation is intentionally not detected; local countdown may continue to zero.
- Local .NET checks and manual Windows UI validation have not been performed in this authoring environment.
- Single-instance behavior and remaining lifecycle hardening are planned for PR-007.

## Next action

- Ensure PR #6's required Windows verify check is green, confirm its actual squash merge and updated main HEAD, then start PR-007 - lifecycle, single-instance behavior and cleanup on fix/app-lifecycle. Do not start PR-007 before PR #6 merges.
