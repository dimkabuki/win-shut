# Instructions for coding agents

## Mandatory context at the start of each session

1. Read `README.md`, this file, `docs/status.md`, `docs/roadmap.md`, `docs/requirements.md`, and `docs/architecture.md`.
2. Read relevant ADRs under `docs/decisions/`.
3. Inspect `git status --short`, current branch, and latest five commits. Never discard user changes.
4. Select the earliest uncompleted and unblocked roadmap item. Work on one focused item only.
5. State intended outcome, expected files, and tests in the PR description or work log before editing.
6. Read the current PR and review feedback before changing an already proposed implementation.

## Git and PR rules

- `main` is intended to be protected. Never commit directly to it or bypass branch protection.
- Use one branch per task: `feature/<slug>` or `fix/<slug>`.
- All changes after repository initialization must reach `main` through a PR.
- Use imperative Conventional Commit subjects, e.g. `feat: add countdown formatter`.
- Do not amend, reset, or rebase user-owned commits unless explicitly requested.
- Keep PRs small and reviewable. Do not add secrets, credentials, personal data, or machine-specific paths.

## Implementation constraints

- Windows-only, target `net10.0-windows`, C# and WinForms unless an ADR is changed through a PR.
- Use the system `shutdown.exe` as the sole shutdown mechanism.
- Resolve it with `Path.Combine(Environment.SystemDirectory, "shutdown.exe")`.
- Launch without a shell and pass separate arguments: `/s`, `/f`, `/t`, seconds.
- Never expose an in-app cancel action or invoke `shutdown /a` from the app.
- Never veto or delay Windows shutdown, reject `WM_QUERYENDSESSION`, or register a shutdown-block reason.
- Do not require elevation, install a service/task, auto-start, or contact network services.
- Keep process execution and OS event observation behind small injectable interfaces.
- Never run real shutdown in CI or automated tests.
- Candidate abort signal: System event log provider `User32`, event ID `1075`. Validate event semantics and read permissions on Windows 10 before relying on it. If unreliable, document evidence rather than using undocumented APIs or parsing localized console output.

## Validation

Run when applicable:

```powershell
dotnet format ShutdownTimer.sln --verify-no-changes
dotnet build ShutdownTimer.sln --configuration Release
dotnet test ShutdownTimer.sln --configuration Release
```

Do not claim tests passed unless their results were observed. Destructive manual tests must run only after saving work in a disposable/test Windows session.

## Session continuity

- `docs/status.md` is the source of truth for completed work, branch/PR, test evidence, risks, and next action.
- Update `docs/roadmap.md` when a milestone changes.
- Record lasting decisions in ADRs, not only in chat.
- Before ending a meaningful task, update status and report the exact next action.
- If interrupted, leave uncommitted changes intact and report working-tree state.

## Definition of done

- Scope matches the requirements and one roadmap item.
- Relevant build/format/test checks pass and CI is green.
- No automated test can execute real `shutdown.exe`.
- Status/roadmap accurately reflect results.
- PR description lists changes, tests run, known risks, and whether manual Windows validation was performed.
