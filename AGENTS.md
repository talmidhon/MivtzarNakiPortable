# Codex instructions — Mivtzar Naki

## Current phase

The user has authorized proceeding with implementation after completing the Codex
infrastructure (2026-09-28). Implement, build, test, and package the application
within the approved requirements. Routine validation must not install Defender
updates or execute repairs on the development PC; use controlled fixtures/fakes.

## Load the right context

- Read `docs/STATUS.md` and `docs/product-decisions.md` at the start of a project task.
- Read `PLANS.md` for a multi-step task and `docs/architecture.md` when making architectural decisions.
- Read `code_review.md` when reviewing or changing download, trust, installation, or self-update behavior.
- Read `docs/repository-survey-2026-09-28.md` only when old implementations are relevant.
- Latest user instructions override these files. Update recorded decisions when the user changes them.
- Treat approved requirements, proposed designs, old repository behavior, and verified results as distinct.

## Product invariants

- Product: transfer Microsoft Defender security intelligence from an online PC to an offline PC using the same USB.
- C#, WinUI 3, Windows 10/11 x64. Hebrew RTL UI. No Windows Updates and no scanning feature.
- Run the app from USB; keep Defender payloads separately beside the executable.
- Check online metadata automatically at launch; timeout promptly and retain local operation when offline.
- Download from Microsoft only after a user click and when the USB payload needs updating.
- Downloading must not install Defender updates on the online PC.
- Installation is a separate explicit user action. Skip equal versions and block older versions.
- Start without elevation; request administrator privileges only for an operation that needs them.
- Offer repair only for an update-blocking problem, explaining the exact action and consequences before consent.
- Display PC / USB file / server versions in collapsed details; show unknown or unavailable honestly. Keep diagnostic logs on disk, without a log UI.
- Keep settings and logs on USB, with a local fallback when writing there is impossible.
- App self-update: automatic check, notice, user click. Adapt Acer's behavior to portable USB deployment.

## Engineering guidance for future implementation

- Separate UI, workflow/policy, Windows integration, and network/storage concerns. Avoid unnecessary services or databases.
- Discover the USB base path from the running executable; single-file extraction can make AppContext.BaseDirectory unsuitable.
- A new payload replaces the old one only after validation. Preserve the last valid update on failure or cancellation.
- Validate Authenticode trust and Microsoft's publisher identity; SHA-256 alone is not proof of publisher identity.
- Resume/multipart downloads require server range support and a stable file validator. Never combine bytes from different versions.
- Successful process exit alone does not prove installation; verify the installed Defender version against the target.
- Distinguish unavailable Defender data from a real version. Do not represent errors as 0.0.0.0 or assume HEAD contains a version.
- Keep app version, Defender signature version, and payload architecture separate.
- WinUI 3 is selected. Delivery is a self-contained App folder and ZIP; keep all runtime files together. The user reported clean Windows, physical USB/offline and real Defender/UAC acceptance for baseline 0.1.0. Do not carry those results over to a new binary without evidence. Do not change UI frameworks.
- Preserve machine state during routine checks. Installation and repair tests must be identified explicitly before execution.

## Working in this repository

- `.research/` contains read-only reference clones. Do not implement changes there or add those repositories to Git.
- Record verified build/test commands in README.md as implementation progresses. Do not invent successful checks.
- Use the local environment reported by tools; see `docs/STATUS.md` for the latest recorded snapshot.
- For nontrivial work, record scope, completion criteria, and verification in `PLANS.md` before implementing.
- Keep `docs/STATUS.md` current with actual results and the next useful action. Completing a plan does not authorize starting another phase.
- Communicate with the user in Hebrew. Explain behavior in plain language; use short multiple-choice questions with a recommendation only when a product choice is genuinely unresolved.
- Preserve user edits. Use `codex/` for new branches when a branch is needed.
- Follow existing user authorization for Git actions; documentation work does not authorize publishing, merging, or changing remote repositories.
- Do not delegate to subagents unless the user explicitly requests delegation for the task.

## Definition of done

The requested scope is complete, recorded requirements remain consistent, references resolve,
and relevant checks have been run and reported accurately. For documentation-only work,
check document links, skill structure, and workspace scope; do not build the application.

For code work after it is requested, use meaningful behavioral tests and document the actual
build/test commands. Report untested installation, repair, packaging, and offline behavior explicitly.
