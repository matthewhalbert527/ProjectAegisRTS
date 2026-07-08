# Stage 28 Known Issues

These are known limitations after the integrated playtest stabilization pass. They are not blockers for Stage 28 because they are outside the current stabilization scope.

| Area | Known issue | Current status | Suggested follow-up |
| --- | --- | --- | --- |
| Visuals | Most units/buildings still use MVP proxy/blockout visuals. | Covered by Stage20/21 QA. | Replace with artist-authored production models in a later art pass. |
| Audio/VFX | Combat, economy, support powers, and placement feedback are placeholder-level. | Snapshot-driven hooks exist. | Add final audio/VFX pass after model replacement stabilizes. |
| Guard/patrol/scatter | Commands are exposed and routed, but some are foundation behavior rather than final RTS behavior. | Covered by command matrix validation. | Expand command semantics after the feature surface is stable. |
| Naval | Water passability and naval movement classes exist, but player naval production/combat UI is not final. | Stage26 foundation preserved. | Add a dedicated naval production/content stage. |
| Air | Aircraft/helipad snapshots and altitude visuals exist, but full flight model, banking, ammo, and docking UX are placeholders. | Stage26 foundation preserved. | Add air-unit behavior and visual polish later. |
| Engineer/transport UX | Capture/load/unload routes work, but visual affordances and selection feedback are basic. | Stage25 routing preserved. | Add cursor, preview, and passenger panel polish. |
| Tech/support | Several support powers are intentionally placeholder-gated. | Stage24 validation preserves cooldown/prerequisite behavior. | Expand support power effects once combat balance is firmer. |
| Minimap/radar | Minimap uses placeholder actor dots. | Stage11/27.1/28 validation preserves layout. | Replace with final minimap styling and interaction. |
| Quest | Stage4/5 hand-control scenes and components validate, but headset device QA remains manual. | Stage28 medium keeps hand-control validation. | Run Quest 3S device playtests once OpenXR input package choices settle. |
| Save/replay/multiplayer | No campaign save, replay, checksum stream, or multiplayer system yet. | Explicitly deferred. | Stage29+ can plan replay/checksum work. |
| QA overlay | `FeatureRegressionHud` is plain IMGUI and development-only. | Hidden by default and toggled with F10. | Replace with editor-only tooling if it becomes too visible for testers. |
