# CI, build and repository settings

## PR verification workflow

`.github/workflows/ci.yml` triggers only on `pull_request` targeting `main` (open, synchronize, reopen, ready for review). Do not add `push`, `schedule`, or `workflow_dispatch` triggers. It runs on `windows-latest` with `contents: read`; restores, checks format, builds Release and runs tests. Do not add a paths filter, since a skipped required workflow can leave a required check pending. CI never executes the app or real shutdown. Older runs for the same PR are cancelled when new commits arrive.

## Optional EXE build

`.github/workflows/build.yml` is triggered only by `workflow_dispatch`. It publishes a self-contained single-file `win-x64` executable and uploads a seven-day artifact. It never executes the produced binary. The Run button becomes available from the default branch after this workflow merges to `main`.

Actions use stable major tags; review official release notes when updating them.

## Trigger matrix

| Event | Verification | EXE artifact |
|---|---:|---:|
| PR opened/reopened/updated toward `main` | Yes | No |
| Push to feature branch without PR | No | No |
| Merge/push to `main` | No additional run | No |
| Manual `workflow_dispatch` | No PR check | Yes |

## Current main ruleset

The repository currently has an active ruleset named `protect-main` targeting the default branch. It requires a pull request and the `verify` status check, blocks branch deletion and force pushes, allows squash merge only, and has no bypass actors. Required approvals are set to zero for solo maintenance.

Review it at [Settings / Rulesets](https://github.com/dimkabuki/win-shut/rules/25879952). Repository rulesets are configuration, not something workflow YAML can enforce. If repository ownership or team structure changes, revisit approval requirements and review-thread resolution.
