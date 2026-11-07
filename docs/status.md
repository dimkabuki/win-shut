# Project status / session hand-off

## Current state

- GitHub PRs #1-#11 are merged to `main`.
- PR #9 merged as `e09d4a8019b2fb498ca27870cda486f81868e5c4`. Required Windows CI run #41042505856 passed format verification, Release build with 0 warnings and 0 errors, and 73/73 tests: https://github.com/dimkabuki/win-shut/actions/runs/41042505856
- PR #10 merged the PR-009 release-readiness documentation as `f5c2e4c4f3504433f2062a2e3f718999e7f13452`.
- PR #11 (append-only shutdown start history) squash-merged as `f5e78009e9f0357e669bfbe3515a66fd80f419d3`.
- Required Windows CI for the final PR head passed in run #42414621515: format verification, Release build with 0 warnings and 0 errors, and 78/78 tests: https://github.com/dimkabuki/win-shut/actions/runs/42414621515
- The writer appends only after the shutdown scheduler succeeds. Each row contains a local ISO 8601 timestamp with offset and the selected duration in whole minutes. Target file: `Documents/win-shut.log`.
- File-write failures are reported but do not undo a Windows shutdown request already accepted or stop the countdown transition.
- Automated tests cover file/parent creation, append semantics, timestamp formatting, default Documents path and duration validation. The PR's final documentation update is also subject to the required `verify` check.
- Manual Windows validation remains outstanding. Local .NET format/build/test commands are not available in this authoring environment; required Windows CI is the automated validation source of truth.

## Preserved constraints

- Windows 10 x64; C# / .NET 10 / WinForms.
- Invoke only the system `shutdown.exe`, without a shell, using separate `/s`, `/f`, `/t`, and seconds arguments.
- Duration is an integer number of minutes from 5 through 180. Presets remain 15/30/45; +/- controls remain five-minute steps.
- Countdown is approximate and does not detect external `shutdown /a`. Do not invoke `shutdown /a` from the app or add in-app cancellation.
- App exit does not cancel a scheduled Windows shutdown. Do not block or delay system/session shutdown.
- Automated tests and CI must never launch a real shutdown command.
- Persist only successful shutdown-start timestamp and duration in `Documents/win-shut.log`. Do not add general-purpose logging, other fields, telemetry, rotation or unrelated activity history.

## Open limitations

- PR-009 fixes passed automated CI, but the countdown, direct entry and overlay typography still need manual Windows retesting.
- The manual publishing workflow, artifact inspection and clean-machine launch remain outstanding.
- Manual validation remains outstanding for UI/tray/lifecycle behavior, history file creation and append behavior, external cancellation, app exit, Windows/session close and forced close.
- No signed binary, installer, automatic update or release hosting is in scope.

## Next action

1. On Windows, press Start with multiple durations and confirm one row per accepted request is appended to `Documents/win-shut.log`; confirm previous rows remain unchanged.
2. Force a controlled scheduling failure and confirm no history row is added. Confirm an unwritable history path reports a warning without cancelling an accepted shutdown request.
3. Continue the outstanding release-validation checklist in `docs/release-validation.md`.
