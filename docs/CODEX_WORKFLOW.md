# Codex Workflow

- Use a branch per stage or coherent vertical slice.
- Run acceptance tests before moving to the next stage.
- Summarize changed files, commands run, test results, blockers, and recommended next step after each prompt.
- Avoid broad rewrites. Prefer small vertical slices that keep the project runnable.
- Keep `Rts.Core` deterministic and independent of UnityEngine.
- Keep source/license implications updated when referencing, copying, or deriving from external code.
- Do not push unless the user explicitly provides a remote and asks for it. The Stage 0 system check says not to push anywhere.
- Keep concept and production assets separate from code.
