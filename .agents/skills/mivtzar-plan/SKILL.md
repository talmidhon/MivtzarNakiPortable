---
name: mivtzar-plan
description: Plan a Mivtzar Naki task or update its implementation plan using approved USB-to-offline Defender requirements. Use for architecture and multi-step project planning, not generic Codex configuration questions.
---

# Plan a Mivtzar Naki task

Locate the repository containing this skill (`.agents/skills/mivtzar-plan`) and read its root `AGENTS.md`, `docs/STATUS.md`, and `docs/product-decisions.md`.
For architectural work also read `docs/architecture.md`. Read the repository survey only when the task concerns a historical implementation.

Use the requested outcome and approved product behavior to create or update a focused task in root `PLANS.md`.
Record success criteria, dependencies, technical uncertainties, and observable validation. Keep proposed engineering choices distinct from user-approved requirements.

Preserve the two-computer flow: prepare a payload on USB online, then install it locally after a click on the offline computer. App self-update is a separate workflow. Scanning and Windows Updates are outside scope.

If the current phase is infrastructure/planning only, produce the plan and update `docs/STATUS.md`; do not create an app scaffold or run an app build. A later explicit implementation request authorizes the requested coding scope without another confirmation.

Resolve routine technical choices from evidence. Ask the user only when an unresolved choice changes their experience; explain it in Hebrew with concrete outcomes and a recommendation.

Do not mark implementation or checks complete until they have actually happened. End with the concrete plan result and the next task that can be requested.
