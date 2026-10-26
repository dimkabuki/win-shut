# Roadmap

Keep each PR focused and reviewable.

## PR-001 - specification and CI bootstrap

- [x] Product requirements, acceptance criteria and architecture decision.
- [x] Agent/session continuity guidance.
- [x] Minimal WinForms and test projects so CI compiles a real solution.
- [x] PR-only Windows CI.
- [x] Manual-only single-file EXE workflow.
- [ ] Merge PR, then enable main branch protection.
- App remains a placeholder; no timer, tray countdown or shutdown integration exists yet.

## PR-002 - duration model and formatter

Implement preset selection, +/-5 step, 5-180 bounds and a pure countdown formatter. Add unit tests for values and formatting boundaries. No process execution yet.

## PR-003 - shutdown scheduler

Implement injectable scheduler and safe `ProcessStartInfo` arguments. Test path, arguments, start failures and exit codes through a fake launcher. Never call real shutdown in automated tests.

## PR-004 - UI and tray

Implement setup controls, `NotifyIcon` menu, draggable topmost overlay, show/hide behaviour and single-instance handling. Confirm hide/close/exit never run `shutdown /a`.

## PR-005 - validate external cancellation signal

Verify System/User32/event 1075 semantics and read permissions on Windows 10 as a standard user. If validation fails, record evidence and stop before selecting a supported alternative. Implement the watcher only after this gate passes.

## PR-006 - lifecycle and manual validation

Handle watcher/process failures, duplicate launches, disposal and shutdown notifications. Complete Windows 10 tests, including destructive force-close in a disposable session. Document limitations and usage.

## PR-007 - release hygiene, if needed

Verify startup on a clean Windows 10 x64 system without .NET Desktop Runtime. Do not add signing, installer, auto-update or auto-start without an explicit request.

## Exit criteria

All applicable acceptance criteria pass, PR CI is green, external-abort signal evidence exists, automated tests never execute real shutdown, and status lists known limitations.
