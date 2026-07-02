# Stage 28 Feature Regression Matrix

Stage 28 is an integrated player-facing QA pass over the Red Alert-style feature foundation through Stage 27.1. It does not add final art, audio, multiplayer, replay, or campaign scope. It checks whether major systems are exposed, routed, deterministic in `Rts.Core`, visible through Unity feedback, clear to the player, and covered by validation.

| Feature group | Feature | UI exposed | Command route works | Rts.Core behavior works | Unity feedback works | Player-facing status clear | Validation coverage | Known limitations |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Selection | Single select | Yes | Yes | Client-local selection routed through driver | Selection panel and highlights | Yes | Stage22, Stage28 smoke | Final unit portrait art pending |
| Selection | Box select | Yes | Yes | Client-local screen rect selection | Selection panel updates | Yes | Stage22, Stage28 feature HUD | No final marquee styling |
| Selection | Double-click type select | Yes | Yes | Client-local same-type selection | Selection panel updates | Yes | Stage22, Stage28 feature HUD | No advanced filter UI yet |
| Selection | Control groups | Keyboard | Yes | Client-local groups | Status feedback | Yes | Stage22, Stage28 feature HUD | No visible group strip yet |
| Movement/combat | Move | Yes | Yes | Deterministic move orders, including diagonal pathing | Motion/path preview | Yes | Rts.Core tests, Stage22, Stage28 smoke | Final vehicle steering visuals pending |
| Movement/combat | Attack | Yes | Yes | Deterministic attack orders | Combat events/projectiles | Yes | Stage9, Stage22, Stage28 smoke | Placeholder target affordance |
| Movement/combat | Attack-move | Yes | Yes | Deterministic attack-move foundation | Command feedback | Yes | Stage22, Stage28 smoke | Formation behavior pending |
| Movement/combat | Stop | Yes | Yes | Clears current orders | Status feedback | Yes | Stage22, Stage28 smoke | None for current scope |
| Movement/combat | Guard | Yes | Yes | Foundation command accepted | Status feedback | Yes | Stage22, Stage28 feature HUD | Full guard behavior pending |
| Movement/combat | Patrol | Yes | Yes | Foundation command accepted | Status feedback | Yes | Stage22, Stage28 smoke | Full patrol loop pending |
| Movement/combat | Scatter | Yes | Yes | Foundation command accepted | Status feedback | Yes | Stage22, Stage28 feature HUD | Final scatter tuning pending |
| Production/base | Build power | Yes | Yes | Queue, completion, pending placement | Sidebar card/queue/placement | Yes | Stage18.5, Stage19, Stage27.1, Stage28 smoke | Final art pending |
| Production/base | Build refinery | Yes | Yes | Queue, placement, harvester economy | Sidebar and economy visuals | Yes | Stage10, Stage19, Stage28 matrix | Economy balance still prototype |
| Production/base | Build barracks | Yes | Yes | Queue, placement, infantry unlock | Sidebar and unit spawn | Yes | Stage0 tests, Stage19, Stage28 matrix | Final production animation pending |
| Production/base | Build war factory | Yes | Yes | Queue, placement, vehicle unlock | Sidebar and spawn | Yes | Stage0 tests, Stage19, Stage28 matrix | Final factory rollout pending |
| Production/base | Build defenses | Yes | Yes | Gun tower production and combat | Combat/selection feedback | Yes | Stage9, Stage23, Stage28 matrix | Final defense art pending |
| Production/base | Queue/progress | Yes | Yes | Deterministic progress and cancellation | Production queue panel | Yes | Stage0 tests, Stage28 feature HUD | Queue UX is basic |
| Production/base | Pending placement | Yes | Yes | Completed buildings wait for placement | Right-sidebar placement panel | Yes | Stage27.1, Stage28 smoke | No rotate footprint yet |
| Production/base | Fine-grid placement | Yes | Yes | 2x placement grid authoritative in core | Fine footprint preview | Yes | Stage18.5, Stage27.1, Stage28 smoke | Final placement sound/art pending |
| Production/base | Stage27.1 board HUD separation | Yes | Yes | Building placement separate from board setup | Board setup HUD hidden in PC placement | Yes | Stage27.1, Stage28 validators | Manual EXE check still recommended |
| Production/base | Rally point | Yes | Yes | Stored on producer snapshots | Command feedback | Yes | Stage23, Stage28 smoke | No rally flag art yet |
| Base management | Repair | Yes | Yes | Credit-spending repair state | Selection/status feedback | Yes | Stage23, Stage28 smoke | No repair cursor art |
| Base management | Sell | Yes | Yes | Refund/removal route | Status feedback | Yes | Stage23, Stage28 feature HUD | No confirmation dialog |
| Base management | Power toggle | Yes | Yes | Consumption and production pause update | Status/power readout | Yes | Stage23, Stage28 smoke | No final power switch icon |
| Economy | Harvester | Visible | Yes | Harvest/load/unload deterministic | Resource/cargo/refinery visuals | Yes | Stage10, Stage28 feature HUD | Economy tuning still conservative |
| Economy | Resources | Visible | Yes | Resource depletion in core | Resource cells rendered | Yes | Stage10, Stage28 matrix | Final ore visuals pending |
| Economy | Refinery unload | Visible | Yes | Credits awarded on unload | Dock/unload markers | Yes | Stage10, Stage28 feature HUD | No final unload animation |
| Tech/support | Prerequisites | Yes | Yes | Locked/unlocked production data | Disabled reasons | Yes | Stage24, Stage28 matrix | Full tech tree pending |
| Tech/support | Support powers | Yes | Yes | Cooldowns/prerequisites in core | Support panel buttons | Yes | Stage24, Stage28 smoke | Several powers are placeholders |
| Engineer/transport | Capture | Yes | Yes | Engineer capture route | Status feedback | Yes | Stage25, Stage28 smoke | Capture UX art pending |
| Engineer/transport | Repair | Yes | Yes | Engineer repair route | Status feedback | Yes | Stage25, Stage28 matrix | Needs final cursor/FX |
| Engineer/transport | Load | Yes | Yes | Transport load route | Passenger snapshot | Yes | Stage25, Stage28 smoke | Passenger UI is basic |
| Engineer/transport | Unload | Yes | Yes | Transport unload route | Status feedback | Yes | Stage25, Stage28 feature HUD | No unload preview art |
| Air/naval | Aircraft/helipad | Visible | Yes | Airfield/aircraft snapshots | Altitude visual | Yes | Stage26, Stage28 feature HUD | Flight model is foundation only |
| Air/naval | Water/naval | Data-backed | Yes | Naval movement class over water | Terrain debug/minimap context | Partial | Stage26, Stage28 matrix | No player naval production UI yet |
| Visibility | Fog | Yes | Snapshot-driven | Player-perspective visibility | Fog overlay | Yes | Stage11, Stage28 feature HUD | Final fog styling pending |
| Visibility | Radar | Yes | Snapshot-driven | Radar state in core | Minimap/radar readout | Yes | Stage11, Stage24, Stage28 feature HUD | Radar art pending |
| Visibility | Minimap | Yes | Snapshot-driven | Minimap snapshot | Top-right PC minimap | Yes | Stage11, Stage27.1, Stage28 validators | Placeholder dots |
| AI | Production | Internal | Yes | Deterministic AI intents | AI pressure status | Yes | Stage12, Stage27, Stage28 matrix | Strategy variety is limited |
| AI | Attack waves | Visible status | Yes | Timed pressure snapshots | Objective HUD pressure text | Yes | Stage27, Stage28 matrix | Balance is prototype |
| AI | Difficulty | Boot/options | Yes | Easy/Normal/Hard profiles | Options and status | Yes | Stage27, Stage28 matrix | No campaign difficulty storage |
| Mission | Objective checklist | Yes | Snapshot-driven | Match/scenario objectives | Checklist and prompts | Yes | Stage16-19, Stage28 feature HUD | Campaign system pending |
| Mission | Win/loss | Yes | Yes | Match outcome in core | Result HUD | Yes | Stage16, Stage27, Stage28 matrix | Final victory art pending |
| Mission | Restart | Yes | Yes | Scenario reset route | Pause/result UI | Yes | Stage27, Stage28 matrix | No save/load |
| Platform UI | PCDesktop sidebar | Yes | Yes | Commands routed to core | Right docked sidebar/minimap | Yes | Stage19.5, Stage27.1, Stage28 validators | Final icon art pending |
| Platform UI | QuestXR hand controls | Yes | Yes | Uses same command bridge | Left/right hand placeholder UI | Yes | Stage4, Stage5, Stage28 medium | Device testing still manual |
| Platform UI | Pause menu | Yes | Yes | Pauses simulation | Center pause UI | Yes | Stage19.5, Stage27.1, Stage28 smoke | Final settings pending |
| Visuals | MVP proxy readability | Yes | Snapshot-driven | Core unaffected | Proxy visuals and QA | Partial | Stage20, Stage21, Stage28 matrix | Final art pending |
| Visuals | Building animations | Yes | Snapshot-driven | Core snapshot flags | Lights/machinery/doors | Partial | Stage7, Stage20, Stage28 matrix | Final animation clips pending |
| Visuals | Combat/economy feedback | Yes | Snapshot-driven | Core events | Markers and placeholder effects | Partial | Stage9, Stage10, Stage14, Stage28 matrix | Final VFX/audio pending |
| Performance/build | Windows player build | Yes | N/A | N/A | EXE build and launch smoke | Yes | Stage16.5, Stage21.5, Stage28 player-facing | Manual playtest still required |
| Performance/build | Player.log clean | N/A | N/A | N/A | Log inspection tooling | Yes | Stage17+, Stage28 player-facing | Only known red signatures fail |
| Performance/build | Debug panels hidden | Yes | N/A | N/A | Hidden by default, QA overlay on F10 | Yes | Stage16.5, Stage27.1, Stage28 validators | QA overlay is intentionally plain |
