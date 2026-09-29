---
name: mivtzar-review
description: Review Mivtzar Naki changes for USB/offline workflow regressions, payload validation, installation results, and portable self-update behavior. Use for project reviews and sensitive workflow changes, not unrelated repository reviews.
---

# Review Mivtzar Naki changes

Locate the repository containing this skill (`.agents/skills/mivtzar-review`) and read its root `AGENTS.md`, `docs/product-decisions.md`, and `code_review.md`.
Use `docs/STATUS.md` to distinguish documentation-only work from implemented behavior.

Inspect the requested change and relevant callers. Focus on defects triggered by realistic user actions: interrupted USB preparation, a changed remote payload, missing Defender data, installation with insufficient privileges, or portable app replacement.

Require evidence for a finding: triggering condition, observable consequence, and the relevant file/line. Distinguish verified behavior, inference, and untested environments. Use fakes or temporary fixtures when validating logic; the review request alone does not authorize installing signatures or changing Defender state.

For documentation changes, check that requirements match the latest user decisions and that a planning-only phase has not acquired application code or claimed build results.

Report actionable findings in Hebrew by severity with precise file references. If none are found, say so and identify material verification limits. Do not edit implementation or publish feedback externally unless the user requested those actions.
