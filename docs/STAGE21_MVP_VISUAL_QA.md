# Stage 21 MVP Visual QA

Stage 21 validates the MVP production proxy prefabs as player-facing, 360-degree tabletop miniatures and confirms they are ready for one-at-a-time artist model replacement.

- MVP actors checked: 9
- Passed: 5
- Passed with warnings: 4
- Failed: 0

## Per Actor QA
### fabrication_hub

- Status: Warning
- Visual tier: FirstPassProxy
- Mesh objects: 23
- Materials: 9
- Sockets: 14
- Bounds center: `(0.00, 0.92, 0.38)`
- Bounds size: `(2.88, 1.84, 3.64)`
- Artist import: NoCandidateFound

| Category | Status | Result | Detail |
| --- | --- | --- | --- |
| fallback safety | Pass | Production proxy is preferred and fallback remains assigned. | fabrication_hub |
| footprint scale | Warning | Visual has allowed ramp/socket overhang beyond the fine-grid base. | center (0.00, 0.92, 0.38), size (2.88, 1.84, 3.64), footprint 3x3 |
| pivot/origin | Pass | Root pivot is usable as footprint center/base. | center (0.00, 0.92, 0.38), size (2.88, 1.84, 3.64) |
| 360 readability | Pass | Top, side, rear, roof, and tiering markers are present. | AllAround |
| socket completeness | Pass | Required sockets are present. | 14 sockets checked |
| animation hook readiness | Pass | Motion, combat, and building-state hooks are ready for replacement art. | fabrication_hub |
| LOD/performance readiness | Warning | Proxy is over the recommended Quest budget. | mesh objects 23/48, materials 9/8 |
| top-down readability | Pass | Prefab has modular identity geometry beyond a plain blockout. | renderers 23, materials 9 |
| player-facing visibility | Pass | Prefab has readable rendered volume for live player-facing scenes. | center (0.00, 0.92, 0.38), size (2.88, 1.84, 3.64) |
| artist replacement metadata | Pass | Replacement metadata is present; proxy remains active until real model validation passes. | NoCandidateFound  |

### power_plant

- Status: Pass
- Visual tier: FirstPassProxy
- Mesh objects: 20
- Materials: 7
- Sockets: 10
- Bounds center: `(0.00, 0.85, 0.00)`
- Bounds size: `(1.92, 1.70, 1.92)`
- Artist import: NoCandidateFound

| Category | Status | Result | Detail |
| --- | --- | --- | --- |
| fallback safety | Pass | Production proxy is preferred and fallback remains assigned. | power_plant |
| footprint scale | Pass | Visual scale stays within fine-grid footprint tolerances. | center (0.00, 0.85, 0.00), size (1.92, 1.70, 1.92), footprint 2x2 |
| pivot/origin | Pass | Root pivot is usable as footprint center/base. | center (0.00, 0.85, 0.00), size (1.92, 1.70, 1.92) |
| 360 readability | Pass | Top, side, rear, roof, and tiering markers are present. | AllAround |
| socket completeness | Pass | Required sockets are present. | 10 sockets checked |
| animation hook readiness | Pass | Motion, combat, and building-state hooks are ready for replacement art. | power_plant |
| LOD/performance readiness | Pass | LODGroup and Quest-safe proxy budgets are present. | mesh objects 20/48, materials 7/8 |
| top-down readability | Pass | Prefab has modular identity geometry beyond a plain blockout. | renderers 20, materials 7 |
| player-facing visibility | Pass | Prefab has readable rendered volume for live player-facing scenes. | center (0.00, 0.85, 0.00), size (1.92, 1.70, 1.92) |
| artist replacement metadata | Pass | Replacement metadata is present; proxy remains active until real model validation passes. | NoCandidateFound  |

### refinery

- Status: Warning
- Visual tier: FirstPassProxy
- Mesh objects: 22
- Materials: 9
- Sockets: 13
- Bounds center: `(-0.09, 0.78, 0.00)`
- Bounds size: `(3.06, 1.56, 2.88)`
- Artist import: NoCandidateFound

| Category | Status | Result | Detail |
| --- | --- | --- | --- |
| fallback safety | Pass | Production proxy is preferred and fallback remains assigned. | refinery |
| footprint scale | Pass | Visual scale stays within fine-grid footprint tolerances. | center (-0.09, 0.78, 0.00), size (3.06, 1.56, 2.88), footprint 3x3 |
| pivot/origin | Pass | Root pivot is usable as footprint center/base. | center (-0.09, 0.78, 0.00), size (3.06, 1.56, 2.88) |
| 360 readability | Pass | Top, side, rear, roof, and tiering markers are present. | AllAround |
| socket completeness | Pass | Required sockets are present. | 13 sockets checked |
| animation hook readiness | Pass | Motion, combat, and building-state hooks are ready for replacement art. | refinery |
| LOD/performance readiness | Warning | Proxy is over the recommended Quest budget. | mesh objects 22/48, materials 9/8 |
| top-down readability | Pass | Prefab has modular identity geometry beyond a plain blockout. | renderers 22, materials 9 |
| player-facing visibility | Pass | Prefab has readable rendered volume for live player-facing scenes. | center (-0.09, 0.78, 0.00), size (3.06, 1.56, 2.88) |
| artist replacement metadata | Pass | Replacement metadata is present; proxy remains active until real model validation passes. | NoCandidateFound  |

### barracks

- Status: Pass
- Visual tier: FirstPassProxy
- Mesh objects: 21
- Materials: 8
- Sockets: 13
- Bounds center: `(0.00, 0.61, 0.00)`
- Bounds size: `(1.92, 1.22, 1.92)`
- Artist import: NoCandidateFound

| Category | Status | Result | Detail |
| --- | --- | --- | --- |
| fallback safety | Pass | Production proxy is preferred and fallback remains assigned. | barracks |
| footprint scale | Pass | Visual scale stays within fine-grid footprint tolerances. | center (0.00, 0.61, 0.00), size (1.92, 1.22, 1.92), footprint 2x2 |
| pivot/origin | Pass | Root pivot is usable as footprint center/base. | center (0.00, 0.61, 0.00), size (1.92, 1.22, 1.92) |
| 360 readability | Pass | Top, side, rear, roof, and tiering markers are present. | AllAround |
| socket completeness | Pass | Required sockets are present. | 13 sockets checked |
| animation hook readiness | Pass | Motion, combat, and building-state hooks are ready for replacement art. | barracks |
| LOD/performance readiness | Pass | LODGroup and Quest-safe proxy budgets are present. | mesh objects 21/48, materials 8/8 |
| top-down readability | Pass | Prefab has modular identity geometry beyond a plain blockout. | renderers 21, materials 8 |
| player-facing visibility | Pass | Prefab has readable rendered volume for live player-facing scenes. | center (0.00, 0.61, 0.00), size (1.92, 1.22, 1.92) |
| artist replacement metadata | Pass | Replacement metadata is present; proxy remains active until real model validation passes. | NoCandidateFound  |

### war_factory

- Status: Pass
- Visual tier: FirstPassProxy
- Mesh objects: 22
- Materials: 8
- Sockets: 13
- Bounds center: `(0.00, 0.59, 0.28)`
- Bounds size: `(2.88, 1.26, 2.48)`
- Artist import: NoCandidateFound

| Category | Status | Result | Detail |
| --- | --- | --- | --- |
| fallback safety | Pass | Production proxy is preferred and fallback remains assigned. | war_factory |
| footprint scale | Pass | Visual scale stays within fine-grid footprint tolerances. | center (0.00, 0.59, 0.28), size (2.88, 1.26, 2.48), footprint 3x2 |
| pivot/origin | Pass | Root pivot is usable as footprint center/base. | center (0.00, 0.59, 0.28), size (2.88, 1.26, 2.48) |
| 360 readability | Pass | Top, side, rear, roof, and tiering markers are present. | AllAround |
| socket completeness | Pass | Required sockets are present. | 13 sockets checked |
| animation hook readiness | Pass | Motion, combat, and building-state hooks are ready for replacement art. | war_factory |
| LOD/performance readiness | Pass | LODGroup and Quest-safe proxy budgets are present. | mesh objects 22/48, materials 8/8 |
| top-down readability | Pass | Prefab has modular identity geometry beyond a plain blockout. | renderers 22, materials 8 |
| player-facing visibility | Pass | Prefab has readable rendered volume for live player-facing scenes. | center (0.00, 0.59, 0.28), size (2.88, 1.26, 2.48) |
| artist replacement metadata | Pass | Replacement metadata is present; proxy remains active until real model validation passes. | NoCandidateFound  |

### gun_tower

- Status: Pass
- Visual tier: FirstPassProxy
- Mesh objects: 12
- Materials: 7
- Sockets: 11
- Bounds center: `(0.00, 0.61, 0.18)`
- Bounds size: `(0.94, 1.22, 1.30)`
- Artist import: NoCandidateFound

| Category | Status | Result | Detail |
| --- | --- | --- | --- |
| fallback safety | Pass | Production proxy is preferred and fallback remains assigned. | gun_tower |
| footprint scale | Pass | Visual scale stays within fine-grid footprint tolerances. | center (0.00, 0.61, 0.18), size (0.94, 1.22, 1.30), footprint 1x1 |
| pivot/origin | Pass | Root pivot is usable as footprint center/base. | center (0.00, 0.61, 0.18), size (0.94, 1.22, 1.30) |
| 360 readability | Pass | Top, side, rear, roof, and tiering markers are present. | AllAround |
| socket completeness | Pass | Required sockets are present. | 11 sockets checked |
| animation hook readiness | Pass | Motion, combat, and building-state hooks are ready for replacement art. | gun_tower |
| LOD/performance readiness | Pass | LODGroup and Quest-safe proxy budgets are present. | mesh objects 12/48, materials 7/8 |
| top-down readability | Pass | Prefab has modular identity geometry beyond a plain blockout. | renderers 12, materials 7 |
| player-facing visibility | Pass | Prefab has readable rendered volume for live player-facing scenes. | center (0.00, 0.61, 0.18), size (0.94, 1.22, 1.30) |
| artist replacement metadata | Pass | Replacement metadata is present; proxy remains active until real model validation passes. | NoCandidateFound  |

### rifle_infantry

- Status: Pass
- Visual tier: FirstPassProxy
- Mesh objects: 8
- Materials: 6
- Sockets: 8
- Bounds center: `(0.06, 0.50, 0.27)`
- Bounds size: `(0.55, 1.04, 0.95)`
- Artist import: NoCandidateFound

| Category | Status | Result | Detail |
| --- | --- | --- | --- |
| fallback safety | Pass | Production proxy is preferred and fallback remains assigned. | rifle_infantry |
| footprint scale | Pass | Visual scale stays within fine-grid footprint tolerances. | center (0.06, 0.50, 0.27), size (0.55, 1.04, 0.95), footprint 1x1 |
| pivot/origin | Pass | Root pivot is usable as footprint center/base. | center (0.06, 0.50, 0.27), size (0.55, 1.04, 0.95) |
| 360 readability | Pass | Top, side, rear, roof, and tiering markers are present. | AllAround |
| socket completeness | Pass | Required sockets are present. | 8 sockets checked |
| animation hook readiness | Pass | Motion, combat, and building-state hooks are ready for replacement art. | rifle_infantry |
| LOD/performance readiness | Pass | LODGroup and Quest-safe proxy budgets are present. | mesh objects 8/28, materials 6/6 |
| top-down readability | Pass | Prefab has modular identity geometry beyond a plain blockout. | renderers 8, materials 6 |
| player-facing visibility | Pass | Prefab has readable rendered volume for live player-facing scenes. | center (0.06, 0.50, 0.27), size (0.55, 1.04, 0.95) |
| artist replacement metadata | Pass | Replacement metadata is present; proxy remains active until real model validation passes. | NoCandidateFound  |

### light_tank

- Status: Warning
- Visual tier: FirstPassProxy
- Mesh objects: 12
- Materials: 7
- Sockets: 12
- Bounds center: `(0.00, 0.64, 0.25)`
- Bounds size: `(1.16, 1.12, 1.28)`
- Artist import: NoCandidateFound

| Category | Status | Result | Detail |
| --- | --- | --- | --- |
| fallback safety | Pass | Production proxy is preferred and fallback remains assigned. | light_tank |
| footprint scale | Pass | Visual scale stays within fine-grid footprint tolerances. | center (0.00, 0.64, 0.25), size (1.16, 1.12, 1.28), footprint 1x1 |
| pivot/origin | Pass | Root pivot is usable as footprint center/base. | center (0.00, 0.64, 0.25), size (1.16, 1.12, 1.28) |
| 360 readability | Pass | Top, side, rear, roof, and tiering markers are present. | AllAround |
| socket completeness | Pass | Required sockets are present. | 12 sockets checked |
| animation hook readiness | Pass | Motion, combat, and building-state hooks are ready for replacement art. | light_tank |
| LOD/performance readiness | Warning | Proxy is over the recommended Quest budget. | mesh objects 12/28, materials 7/6 |
| top-down readability | Pass | Prefab has modular identity geometry beyond a plain blockout. | renderers 12, materials 7 |
| player-facing visibility | Pass | Prefab has readable rendered volume for live player-facing scenes. | center (0.00, 0.64, 0.25), size (1.16, 1.12, 1.28) |
| artist replacement metadata | Pass | Replacement metadata is present; proxy remains active until real model validation passes. | NoCandidateFound  |

### harvester

- Status: Warning
- Visual tier: FirstPassProxy
- Mesh objects: 11
- Materials: 7
- Sockets: 10
- Bounds center: `(0.00, 0.64, 0.07)`
- Bounds size: `(1.16, 1.13, 1.34)`
- Artist import: NoCandidateFound

| Category | Status | Result | Detail |
| --- | --- | --- | --- |
| fallback safety | Pass | Production proxy is preferred and fallback remains assigned. | harvester |
| footprint scale | Pass | Visual scale stays within fine-grid footprint tolerances. | center (0.00, 0.64, 0.07), size (1.16, 1.13, 1.34), footprint 1x1 |
| pivot/origin | Pass | Root pivot is usable as footprint center/base. | center (0.00, 0.64, 0.07), size (1.16, 1.13, 1.34) |
| 360 readability | Pass | Top, side, rear, roof, and tiering markers are present. | AllAround |
| socket completeness | Pass | Required sockets are present. | 10 sockets checked |
| animation hook readiness | Pass | Motion, combat, and building-state hooks are ready for replacement art. | harvester |
| LOD/performance readiness | Warning | Proxy is over the recommended Quest budget. | mesh objects 11/28, materials 7/6 |
| top-down readability | Pass | Prefab has modular identity geometry beyond a plain blockout. | renderers 11, materials 7 |
| player-facing visibility | Pass | Prefab has readable rendered volume for live player-facing scenes. | center (0.00, 0.64, 0.07), size (1.16, 1.13, 1.34) |
| artist replacement metadata | Pass | Replacement metadata is present; proxy remains active until real model validation passes. | NoCandidateFound  |

## QA Rule Coverage
- Footprint scale and fine-grid base alignment.
- Pivot/origin near footprint center and base.
- Top-down, side, rear, roof, and tiered silhouette readability.
- Required socket completeness and animation hook readiness.
- LOD/performance, material count, and mesh object count budget.
- Fallback safety and active production proxy assignment.
- Player-facing rendered volume.
- Artist replacement metadata and import scan status.
