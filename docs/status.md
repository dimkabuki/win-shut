# Project status / session hand-off

## Current state

- PRs #1-#9 are merged to `main`.
- PR #7 merged as `e7826c3205ad8d072700aa7d12e20750e49b20ac`; its required Windows CI passed format verification, Release build, and 61/61 tests with zero warnings: https://github.com/dimkabuki/win-shut/actions/runs/40386849399
- PR #8 merged the Windows release-validation checklist and initial release hand-off as `9011a5a1412a26fb995dea461422749a5d7e032e`.
- PR #9 merged as `e09d4a8019b2fb498ca27870cda486f81868e5c4`. Required Windows CI run #41042505856 passed format verification, Release build with 0 warnings and 0 errors, and 73/73 tests: https://github.com/dimkabuki/win-shut/actions/runs/41042505856
- PR #9 fixes the stopped production countdown clock, adds an editable whole-minute input (5-180 minutes), allows non-preset durations through the scheduler, and reduces the overlay countdown font from 25 pt to 18 pt.
- First user-run feedback confirmed Windows accepted the shutdown request and powered off the laptop after the selected timeout; the countdown display did not advance, direct duration entry was missing, and the overlay text was too large.
- Code inspection identified the countdown defect: the production `StopwatchMonotonicClock` was never started. PR #9 starts it at construction and adds a regression test that verifies elapsed time advances.
- Manual Windows retest of the fixes has not been performed. Do not claim countdown progression, direct input or font rendering has been manually verified.
- The manual publishing workflow, artifact inspection and clean-machine launch remain outstanding.

## Preserved constraints

- Windows 10 x64; C# / .NET 10 / WinForms.
- Invoke only the system `shutdown.exe`, without a shell, using separate `/s`, `/f`, `/t`, and seconds arguments.
- Duration is an integer number of minutes from 5 through 180. Presets remain 15/30/45; +/- controls remain five-minute steps.
- Countdown is approximate and does not detect external `shutdown /a`. Do not invoke `shutdown /a` from the app or add in-app cancellation.
- App exit does not cancel a scheduled Windows shutdown. Do not block or delay system/session shutdown.
- Automated tests and CI must never launch a real shutdown command.

## Open limitations

- The PR #9 code changes passed automated CI but still require Windows UI retest.
- Manual validation remains outstanding for clean-machine launch, countdown progression, direct entry/presets/step synchronization, overlay typography, tray/menu behavior, external cancellation, app exit, Windows/session close and forced close.
- No signed binary, installer, automatic update or release hosting is in scope.

## Next action

1. From `main`, run **Actions → Build Windows executable → Run workflow** and inspect the uploaded `ShutdownTimer-win-x64` artifact.
2. Launch the artifact on a clean Windows 10 x64 system without a separately installed .NET Desktop Runtime.
3. Verify the countdown advances (whole minutes above three minutes, then seconds), enter a non-preset duration such as 37 minutes, and inspect the smaller overlay font.
4. Complete the remaining disposable-VM checks in `docs/release-validation.md`; record Windows edition/build and privilege level.
5. Treat the next binary as a release candidate until those manual checks pass.
