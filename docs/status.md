# Project status / session hand-off

## Current state

- PR #1 merged on 2026-10-26.
- PR #2 (duration model and countdown presentation logic) merged by squash auto-merge. Merge commit: `0aba6c23a809b82025703f865a0168a24503944d`. Required Windows CI passed format verification, Release build, and 39/39 tests: https://github.com/dimkabuki/win-shut/actions/runs/40354694642
- PR #3 (safe Windows shutdown command runner) merged by squash auto-merge. Merge commit: `181bc6e4c8becbad5c54ebd1567685d2bd0dab6f`. Required Windows CI passed format verification, Release build, and 51/51 tests: https://github.com/dimkabuki/win-shut/actions/runs/40358333568
- PR #4 (local countdown controller) merged by squash auto-merge. Merge commit: `fc3b7a241f1fbaa209f0df4ef326b9adf7f68310`. Required Windows CI passed formatting, Release build, and 58/58 tests: https://github.com/dimkabuki/win-shut/actions/runs/40359474121
- PR #5 (setup window and duration controls) merged by squash auto-merge. Merge commit: `30717839b84470943cc355424a02622b8191c029`. Required Windows CI passed formatting, Release build, and 59/59 tests: https://github.com/dimkabuki/win-shut/actions/runs/40376646137
- PR #6 (tray icon and countdown overlay) merged by squash on 2026-10-26. Merge commit: `4de4cc87fe6cd74366c7cb5bdf72f567604880a5`. Required Windows CI passed formatting, Release build, and 59/59 tests: https://github.com/dimkabuki/win-shut/actions/runs/40379311632
- PR-007 implementation is on `fix/app-lifecycle`: named-mutex single-instance guard, duplicate-launch feedback, and centralized cleanup for forms, tray resources, timers and event handlers. Check PR #7 for its required Windows `verify` result and merge state.
- The live `protect-main` ruleset is active for the default branch, requires a PR and the `verify` check, allows squash-only merges, and blocks force-push/deletion.
- Local .NET format/build/test commands and manual Windows UI/lifecycle validation have not been performed in this authoring environment. CI is the validation source of truth for automated checks.

## Decisions and preserved constraints

- Windows 10 x64 first; C# / .NET 10 / WinForms.
- Shutdown is requested only through `Path.Combine(Environment.SystemDirectory, "shutdown.exe")`, without shell execution, using separate `/s`, `/f`, `/t`, seconds arguments.
- Selected duration is 5-180 minutes in five-minute increments.
- Countdown is approximate and based on monotonic elapsed time. It starts only after successful scheduling.
- The tray controller reuses the existing `CountdownController` and `CountdownFormatter`; one UI timer refreshes both forms.
- A named mutex is held by the initial process until it exits. A duplicate launch displays an informational message and exits; it does not activate or communicate with the first instance.
- Do not monitor Windows shutdown state, detect external `shutdown /a`, invoke `shutdown /a`, or infer/recover an OS timer after app restart.
- Hiding the setup window or overlay and exiting the app do not cancel the already scheduled Windows shutdown. System/session close is not blocked or delayed.
- Automated tests and CI must never launch a real shutdown command.

## Open limitations

- External shutdown cancellation is intentionally not detected; local countdown may continue to zero.
- Manual Windows validation of the tray, overlay, mutex behavior and system shutdown paths remains outstanding.
- Windows/session shutdown notifications were not manually exercised in this authoring environment.

## Next action

- Confirm PR #7's required Windows `verify` check is green, verify the squash merge commit and updated `main` HEAD, then proceed to PR-008 - Windows validation, packaging and release hand-off on `test/windows-release-validation`. Do not begin PR-008 before PR #7 merges.
