# Windows release validation

This checklist records evidence; it does not imply validation has passed. Fill in results only after performing the steps on the target machine. Do not perform destructive checks on a machine with valuable unsaved work.

## Build artifact

1. From the default branch, open Actions → Build Windows executable → Run workflow.
2. Confirm the workflow completes successfully.
3. Download the ShutdownTimer-win-x64 artifact before its seven-day retention expires.
4. Confirm the archive contains ShutdownTimer.App.exe. Record the run URL, file size, and SHA-256 hash.
5. Launch the EXE as a standard user on a clean Windows 10 x64 VM without the .NET Desktop Runtime installed.

A successful publish is not proof of runtime behavior.

## Environment record

- Date (ISO 8601):
- Windows edition and build:
- Architecture:
- .NET Desktop Runtime separately installed? (expected: no)
- User privilege: standard / administrator
- Workflow run URL:
- Artifact SHA-256:
- Notes:

## Non-destructive UI checks

Record PASS, FAIL, or NOT RUN and concise evidence.

| Check | Result | Evidence |
|---|---|---|
| Launches without separately installed .NET Desktop Runtime | NOT RUN | Clean-machine/runtime details not captured. |
| Initial duration and 15/30/45 presets | NOT RUN | |
| Editable whole-minute input (5-180) | FAIL (user report) | First run had no direct value-entry control. |
| Direct entry of 7, 37 and 179 minutes; reject values outside 5-180 | NOT RUN | |
| +/-5-minute changes and 5-180 minute bounds, including from custom values | NOT RUN | |
| Start failure is reported and UI remains usable | NOT RUN | Use a controlled test seam. |
| Successful start hides setup to tray and shows overlay | NOT RUN | |
| Overlay movement, topmost behavior, hide/show | NOT RUN | |
| Overlay countdown font is compact/readable at normal desktop scaling | FAIL (user report) | Initial popup countdown font was reported as too large. |
| Tray menu actions | NOT RUN | |
| Windows accepts the shutdown request and powers off at the selected timeout | PASS (user report) | First run; Windows build and privilege level not recorded. |
| Countdown visibly decreases after scheduling; display progresses from rounded minutes to seconds at 180 seconds | FAIL (user report) | Display remained at the originally selected duration. |
| Countdown minute/second formatting | NOT RUN | |
| Final-30-second text pulse and static zero display | NOT RUN | Use a controlled test seam if needed. |
| Duplicate launch is rejected | NOT RUN | |
| App exit leaves the OS timer independent of the app | NOT RUN | Isolated trial below. |
| Windows/session shutdown is not blocked or delayed | NOT RUN | Disposable VM only. |

## Isolated shutdown trials

Only use a disposable VM or test session. Save or discard all work first. The app passes /f, so applications can be force-closed without save prompts.

- **External cancellation:** schedule a timer, keep the app open, run `shutdown /a` from a separate terminal, and observe the app. Expected: the local countdown does not detect cancellation and can continue to zero.
- **App exit:** in a separate trial, schedule another timer, exit the app, then run `shutdown /a` from a separate terminal. Record whether Windows reports a pending shutdown was aborted; do not assume the UI persists after exit.
- **Forced close:** use a disposable test application with deliberately unsaved test data. Confirm the forced-shutdown request closes it without a save prompt. Data loss is intentional.
- **System/session close:** verify the app does not veto, block, or delay normal Windows/session shutdown.

Never run these trials in CI or automated tests.

## Initial user-run observations (2026-10-30)

The first user run confirmed the Windows shutdown request was accepted and the laptop powered off after the selected timeout. The displayed countdown did not change from the initial value, the setup UI had no direct duration input, and the overlay countdown font was considered too large. These are user-observed results; Windows edition/build and privilege level were not captured. PR-009 is intended to address the three reported issues. Re-run all affected checks before treating them as resolved.

## Results summary

- Build workflow/artifact: NOT VERIFIED
- Clean-machine launch: NOT RUN
- Non-destructive UI checks: NOT RUN
- Isolated shutdown trials: NOT RUN
- Known issues:
- Remaining checks:
- Overall assessment: NOT VALIDATED until evidence is filled in

Do not replace NOT RUN with PASS based on code review, unit tests, or publishing alone.
