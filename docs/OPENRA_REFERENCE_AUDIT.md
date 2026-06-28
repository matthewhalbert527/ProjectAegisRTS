# OpenRA Reference Audit

OpenRA was inspected as reference only. Stage 0 does not copy large OpenRA code sections and does not port the renderer, SDL/OpenGL platform layer, or chrome YAML UI.

## Production Queues

Key references:

- `external/openra/OpenRA.Mods.Common/Traits/Player/ProductionQueue.cs`
- `external/openra/OpenRA.Mods.Common/Traits/Player/ClassicProductionQueue.cs`
- `external/openra/OpenRA.Mods.Common/Traits/Player/ClassicParallelProductionQueue.cs`
- `external/openra/OpenRA.Mods.Common/Traits/Production.cs`
- `external/openra/OpenRA.Mods.Common/Traits/Buildable.cs`

Relevant ideas: buildable actor metadata, per-player or per-building queues, queue items, low-power production modifiers, build-time and cost modifiers, and completion callbacks. Stage 0 implements a much smaller explicit queue with fixed build ticks, credit deduction, pending building placement, and unit spawning.

## Building Placement

Key references:

- `external/openra/OpenRA.Mods.Common/Orders/PlaceBuildingOrderGenerator.cs`
- `external/openra/OpenRA.Mods.Common/Traits/Player/PlaceBuilding.cs`
- `external/openra/OpenRA.Mods.Common/Traits/Buildings/BuildingUtils.cs`
- `external/openra/OpenRA.Mods.Common/Traits/Buildings/Building.cs`
- `external/openra/OpenRA.Mods.Common/Traits/Buildings/BuildingInfluence.cs`
- `external/openra/OpenRA.Mods.Common/Traits/Buildings/GivesBuildableArea.cs`
- `external/openra/OpenRA.Mods.Common/Traits/Buildings/RequiresBuildableArea.cs`

Relevant ideas: placement previews, footprint validation, map bounds, buildable terrain, occupied cells, build radius/influence, and final place order resolution. Stage 0 implements structured placement errors and construction-radius checks from owned powered fabrication hubs.

## Selection

Key references:

- `external/openra/OpenRA.Mods.Common/Traits/World/Selection.cs`
- `external/openra/OpenRA.Mods.Common/Widgets/SelectionUtils.cs`

Relevant ideas: selection is primarily client/UI state and informs order generation. Stage 0 includes `SelectActorsCommand` as a DTO but keeps selection client-local for later PC and VR/MR clients.

## Order Generation

Key references:

- `external/openra/OpenRA.Game/Network/Order.cs`
- `external/openra/OpenRA.Game/Network/OrderManager.cs`
- `external/openra/OpenRA.Mods.Common/Orders/OrderGenerator.cs`
- `external/openra/OpenRA.Mods.Common/Orders/UnitOrderGenerator.cs`
- `external/openra/OpenRA.Mods.Common/TraitsInterfaces.cs`

Relevant ideas: UI order generation is separate from order resolution, and network/replay systems process explicit orders. Stage 0 follows the separation by exposing DTO commands and deterministic command application.

## Power Management

Key references:

- `external/openra/OpenRA.Mods.Common/Traits/Power/Player/PowerManager.cs`
- `external/openra/OpenRA.Mods.Common/Traits/Power/Power.cs`
- `external/openra/OpenRA.Mods.Common/Traits/Power/AffectedByPowerOutage.cs`
- `external/openra/OpenRA.Mods.Common/Traits/Conditions/GrantConditionOnPowerState.cs`
- `external/openra/OpenRA.Mods.Common/Widgets/Logic/Ingame/IngamePowerBarLogic.cs`
- `external/openra/OpenRA.Mods.Common/Widgets/Logic/Ingame/IngamePowerCounterLogic.cs`

Relevant ideas: player-level power totals, power states, conditional traits, and UI exposure. Stage 0 computes generated/consumed power and exposes Normal, LowPower, and Offline states in snapshots.

## Actor Traits and Components

Key references:

- `external/openra/OpenRA.Game/Actor.cs`
- `external/openra/OpenRA.Mods.Common/TraitsInterfaces.cs`
- `external/openra/OpenRA.Mods.Common/Traits/Conditions/ConditionalTrait.cs`
- `external/openra/OpenRA.Mods.Common/Traits/Mobile.cs`
- `external/openra/OpenRA.Mods.Common/Traits/World/PathFinder.cs`

Relevant ideas: actors are composed from trait-like behavior and data. Stage 0 uses explicit definitions and state classes instead of a full trait loader, leaving a clear extension point for later componentization.

## World Tick and Simulation Loop

Key references:

- `external/openra/OpenRA.Game/World.cs`
- `external/openra/OpenRA.Game/Actor.cs`
- `external/openra/OpenRA.Game/Game.cs`
- `external/openra/OpenRA.Game/Network/OrderManager.cs`

Relevant ideas: fixed simulation steps, actor ticking, order processing, and replay/network determinism. Stage 0 keeps a fixed tick loop and deterministic summary smoke test.

## Renderer, Input, and Platform Boundaries

Key references:

- `external/openra/OpenRA.Game/Graphics/WorldRenderer.cs`
- `external/openra/OpenRA.Game/Graphics/Viewport.cs`
- `external/openra/OpenRA.Game/Renderer.cs`
- `external/openra/OpenRA.Game/Platform.cs`
- `external/openra/OpenRA.Game/Graphics/PlatformInterfaces.cs`
- `external/openra/OpenRA.Game/Input/IInputHandler.cs`

Risk: these systems are deeply tied to OpenRA presentation, SDL-style input, OpenGL-oriented rendering, and UI assumptions. They are intentionally deferred. Unity will provide presentation and input, but not authoritative simulation.

## UI Chrome and Right Sidebar

Key references:

- `external/openra/mods/cnc/chrome/ingame.yaml`
- `external/openra/mods/ra/chrome/ingame-player.yaml`
- `external/openra/OpenRA.Mods.Common/Widgets/ProductionPaletteWidget.cs`
- `external/openra/OpenRA.Mods.Common/Widgets/ProductionTabsWidget.cs`
- `external/openra/OpenRA.Mods.Common/Widgets/Logic/Ingame/ClassicProductionLogic.cs`
- `external/openra/OpenRA.Mods.Common/Widgets/Logic/Ingame/ProductionTabsLogic.cs`

Relevant ideas: production categories, icon palette, tabs, tooltips, power counters, and right-side layout. Stage 0 documents the reference but does not port YAML chrome or widgets.

## Risks

- Copying OpenRA code into production code would bring GPL obligations and must be documented.
- Porting renderer/UI directly would fight Unity and Quest interaction requirements.
- Original game naming and final art must stay IP-safe.
- Future multiplayer/replay work must preserve deterministic command ordering and checksums.
