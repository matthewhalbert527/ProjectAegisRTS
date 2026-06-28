# Agent Instructions

- Do not run destructive commands. Avoid `git reset --hard`, broad deletes, or force-overwrites unless the user explicitly requests them.
- Keep `src/Rts.Core` deterministic and UnityEngine-free. The simulation must remain authoritative and use integer or fixed-point state.
- Do not introduce protected names, faction labels, art, audio, or game code from Command & Conquer, Red Alert, EA, GDI, Nod, Soviet, Allied, or similar protected IP.
- Treat `external/openra` as reference material. If future work copies or derives code from OpenRA, preserve GPL headers and document GPL obligations in `docs/LICENSE_AND_IP_NOTES.md`.
- Treat `external/redalert_reference` as read-only historical reference. Do not port or derive Stage 0 game code from it.
- Keep concept art and future original assets separate from code so asset licensing can be managed independently.
- Prefer small vertical slices with tests. Do not make large refactors without running or updating the acceptance tests.
- Keep command DTOs explicit and return structured errors for validation failures.
- Update docs when architecture, licensing assumptions, or asset naming changes.
