# Project status / session hand-off

## Current state

- PRs #1-#8 are merged to `main`.
- PR #7 merged as `e7826c3205ad8d072700aa7d12e20750e49b20ac`; its required Windows CI passed format verification, Release build, and 61/61 tests with zero warnings: https://github.com/dimkabuki/win-shut/actions/runs/40386849399
- PR #8 merged the Windows release-validation checklist and initial release hand-off. The manual publishing workflow and clean-machine artifact launch have not yet been verified.
- First user-run feedback: Windows accepted the shutdown request and closed applications/powered off as scheduled, but the app's displayed countdown stayed at its original value. The setup screen also lacked direct duration entry, and the overlay text was too large.
- Code inspection identified the countdown defect: `StopwatchMonotonicClock` exposed an elapsed value but was never started by production code. The UI refresh timer was running, but its clock source remained at zero.
- PR-009 is in progress on `fix/countdown-input-overlay`: start the clock on construction; add an editable whole-minute duration field (5-180 minutes); accept arbitrary supported minutes in the scheduler; reduce overlay typography; add automated regression coverage.
- Manual Windows retest of the fixes is not yet performed. Do not claim the countdown display, input control or font rendering has been manually verified.
- Local .NET format/build/test commands were not run in this authoring environment; the required Windows CI is the validation source of truth for automated checks.

## Preserved constraints

- Windows 10 x64; C# / .NET 10 / WinForms.
- Invoke only the system `shutdown.exe`, without a shell, using separate `/s`, `/f`, `/t`, and seconds arguments.
- Duration is an integer number of minutes from 5 through 180. Presets remain 15/30/45; +/- controls remain five-minute steps.
- Countdown is approximate and does not detect external `shutdown /a`. Do not invoke `shutdown /a` from the app or add in-app cancellation.
- App exit does not cancel a scheduled Windows shutdown. Do not block or delay system/session shutdown.
- Automated tests and CI must never launch a real shutdown command.

## Open limitations

- Manual build workflow execution, EXE artifact inspection and clean-machine launch remain outstanding.
- Manual UI retest is required for countdown progression, direct duration entry, presets/step synchronization, and the smaller overlay font.
- Manual lifecycle, external-cancellation, app-exit, Windows/session-close and forced-close checks remain outstanding.
- No signed binary, installer, automatic update or release hosting is in scope.

## Next action

1. Complete PR-009 and require its Windows `verify` check to pass.
2. Merge PR-009 through the protected-main squash workflow.
3. Run **Actions → Build Windows executable → Run workflow** from `main` and inspect the uploaded `ShutdownTimer-win-x64` artifact.
4. Install/launch that artifact on a clean Windows 10 x64 environment and record the results in `docs/release-validation.md`.
5. Keep the release unannounced until countdown behavior, editable input and overlay font are retested on Windows.
