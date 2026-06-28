# Rts Unity Asset Placeholder

Future Unity client folders:

- `Scripts`: Unity-side snapshot rendering, input adapters, board controls, and command submission.
- `ScriptableObjects`: visual tuning and asset lookup tables, not authoritative rules.
- `Prefabs`: placeholder and production prefabs.
- `Materials`: URP materials.
- `Scenes`: board prototype and later Quest/PC scenes.

Do not add authoritative simulation logic here. Keep gameplay state in `src/Rts.Core`.
