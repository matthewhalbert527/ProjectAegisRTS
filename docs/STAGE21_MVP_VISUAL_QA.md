# Stage 21 MVP Visual QA

Stage 21 validates the MVP production proxy prefabs as player-facing, 360-degree tabletop miniatures and confirms they are ready for one-at-a-time artist model replacement.

- MVP actors checked: 9
- Passed: 0
- Passed with warnings: 9
- Failed: 0

## Per Actor QA
### fabrication_hub

- Status: Warning
- Visual tier: FirstPassProxy
- Mesh objects: 35
- Materials: 14
- Sockets: 14
- Bounds center: `(0.00, 0.91, 0.31)`
- Bounds size: `(3.15, 1.86, 3.78)`
- Artist import: NoCandidateFound

| Category | Status | Result | Detail |
| --- | --- | --- | --- |
| fallback safety | Pass | Production proxy is preferred and fallback remains assigned. | fabrication_hub |
| footprint scale | Warning | Visual has allowed ramp/socket overhang beyond the fine-grid base. | center (0.00, 0.91, 0.31), size (3.15, 1.86, 3.78), footprint 3x3 |
| pivot/origin | Pass | Root pivot is usable as footprint center/base. | center (0.00, 0.91, 0.31), size (3.15, 1.86, 3.78) |
| 360 readability | Pass | Top, side, rear, roof, and tiering markers are present. | AllAround |
| socket completeness | Pass | Required sockets are present. | 14 sockets checked |
| animation hook readiness | Pass | Motion, combat, and building-state hooks are ready for replacement art. | fabrication_hub |
| LOD/performance readiness | Warning | Proxy is over the recommended Quest budget. | mesh objects 35/48, materials 14/8 |
| top-down readability | Pass | Prefab has modular identity geometry beyond a plain blockout. | renderers 35, materials 14 |
| player-facing visibility | Pass | Prefab has readable rendered volume for live player-facing scenes. | center (0.00, 0.91, 0.31), size (3.15, 1.86, 3.78) |
| artist replacement metadata | Pass | Replacement metadata is present; proxy remains active until real model validation passes. | NoCandidateFound  |

### power_plant

- Status: Warning
- Visual tier: FirstPassProxy
- Mesh objects: 31
- Materials: 11
- Sockets: 10
- Bounds center: `(0.00, 0.84, 0.00)`
- Bounds size: `(2.10, 1.72, 2.10)`
- Artist import: NoCandidateFound

| Category | Status | Result | Detail |
| --- | --- | --- | --- |
| fallback safety | Pass | Production proxy is preferred and fallback remains assigned. | power_plant |
| footprint scale | Pass | Visual scale stays within fine-grid footprint tolerances. | center (0.00, 0.84, 0.00), size (2.10, 1.72, 2.10), footprint 2x2 |
| pivot/origin | Pass | Root pivot is usable as footprint center/base. | center (0.00, 0.84, 0.00), size (2.10, 1.72, 2.10) |
| 360 readability | Pass | Top, side, rear, roof, and tiering markers are present. | AllAround |
| socket completeness | Pass | Required sockets are present. | 10 sockets checked |
| animation hook readiness | Pass | Motion, combat, and building-state hooks are ready for replacement art. | power_plant |
| LOD/performance readiness | Warning | Proxy is over the recommended Quest budget. | mesh objects 31/48, materials 11/8 |
| top-down readability | Pass | Prefab has modular identity geometry beyond a plain blockout. | renderers 31, materials 11 |
| player-facing visibility | Pass | Prefab has readable rendered volume for live player-facing scenes. | center (0.00, 0.84, 0.00), size (2.10, 1.72, 2.10) |
| artist replacement metadata | Pass | Replacement metadata is present; proxy remains active until real model validation passes. | NoCandidateFound  |

### refinery

- Status: Warning
- Visual tier: FirstPassProxy
- Mesh objects: 33
- Materials: 15
- Sockets: 13
- Bounds center: `(-0.02, 0.77, 0.00)`
- Bounds size: `(3.20, 1.58, 3.15)`
- Artist import: NoCandidateFound

| Category | Status | Result | Detail |
| --- | --- | --- | --- |
| fallback safety | Pass | Production proxy is preferred and fallback remains assigned. | refinery |
| footprint scale | Pass | Visual scale stays within fine-grid footprint tolerances. | center (-0.02, 0.77, 0.00), size (3.20, 1.58, 3.15), footprint 3x3 |
| pivot/origin | Pass | Root pivot is usable as footprint center/base. | center (-0.02, 0.77, 0.00), size (3.20, 1.58, 3.15) |
| 360 readability | Pass | Top, side, rear, roof, and tiering markers are present. | AllAround |
| socket completeness | Pass | Required sockets are present. | 13 sockets checked |
| animation hook readiness | Pass | Motion, combat, and building-state hooks are ready for replacement art. | refinery |
| LOD/performance readiness | Warning | Proxy is over the recommended Quest budget. | mesh objects 33/48, materials 15/8 |
| top-down readability | Pass | Prefab has modular identity geometry beyond a plain blockout. | renderers 33, materials 15 |
| player-facing visibility | Pass | Prefab has readable rendered volume for live player-facing scenes. | center (-0.02, 0.77, 0.00), size (3.20, 1.58, 3.15) |
| artist replacement metadata | Pass | Replacement metadata is present; proxy remains active until real model validation passes. | NoCandidateFound  |

### barracks

- Status: Warning
- Visual tier: FirstPassProxy
- Mesh objects: 32
- Materials: 13
- Sockets: 13
- Bounds center: `(0.00, 0.60, 0.00)`
- Bounds size: `(2.10, 1.24, 2.10)`
- Artist import: NoCandidateFound

| Category | Status | Result | Detail |
| --- | --- | --- | --- |
| fallback safety | Pass | Production proxy is preferred and fallback remains assigned. | barracks |
| footprint scale | Pass | Visual scale stays within fine-grid footprint tolerances. | center (0.00, 0.60, 0.00), size (2.10, 1.24, 2.10), footprint 2x2 |
| pivot/origin | Pass | Root pivot is usable as footprint center/base. | center (0.00, 0.60, 0.00), size (2.10, 1.24, 2.10) |
| 360 readability | Pass | Top, side, rear, roof, and tiering markers are present. | AllAround |
| socket completeness | Pass | Required sockets are present. | 13 sockets checked |
| animation hook readiness | Pass | Motion, combat, and building-state hooks are ready for replacement art. | barracks |
| LOD/performance readiness | Warning | Proxy is over the recommended Quest budget. | mesh objects 32/48, materials 13/8 |
| top-down readability | Pass | Prefab has modular identity geometry beyond a plain blockout. | renderers 32, materials 13 |
| player-facing visibility | Pass | Prefab has readable rendered volume for live player-facing scenes. | center (0.00, 0.60, 0.00), size (2.10, 1.24, 2.10) |
| artist replacement metadata | Pass | Replacement metadata is present; proxy remains active until real model validation passes. | NoCandidateFound  |

### war_factory

- Status: Warning
- Visual tier: FirstPassProxy
- Mesh objects: 33
- Materials: 13
- Sockets: 13
- Bounds center: `(0.00, 0.60, 0.24)`
- Bounds size: `(3.15, 1.27, 2.57)`
- Artist import: NoCandidateFound

| Category | Status | Result | Detail |
| --- | --- | --- | --- |
| fallback safety | Pass | Production proxy is preferred and fallback remains assigned. | war_factory |
| footprint scale | Pass | Visual scale stays within fine-grid footprint tolerances. | center (0.00, 0.60, 0.24), size (3.15, 1.27, 2.57), footprint 3x2 |
| pivot/origin | Pass | Root pivot is usable as footprint center/base. | center (0.00, 0.60, 0.24), size (3.15, 1.27, 2.57) |
| 360 readability | Pass | Top, side, rear, roof, and tiering markers are present. | AllAround |
| socket completeness | Pass | Required sockets are present. | 13 sockets checked |
| animation hook readiness | Pass | Motion, combat, and building-state hooks are ready for replacement art. | war_factory |
| LOD/performance readiness | Warning | Proxy is over the recommended Quest budget. | mesh objects 33/48, materials 13/8 |
| top-down readability | Pass | Prefab has modular identity geometry beyond a plain blockout. | renderers 33, materials 13 |
| player-facing visibility | Pass | Prefab has readable rendered volume for live player-facing scenes. | center (0.00, 0.60, 0.24), size (3.15, 1.27, 2.57) |
| artist replacement metadata | Pass | Replacement metadata is present; proxy remains active until real model validation passes. | NoCandidateFound  |

### gun_tower

- Status: Warning
- Visual tier: FirstPassProxy
- Mesh objects: 23
- Materials: 12
- Sockets: 11
- Bounds center: `(0.00, 0.60, 0.15)`
- Bounds size: `(1.05, 1.24, 1.36)`
- Artist import: NoCandidateFound

| Category | Status | Result | Detail |
| --- | --- | --- | --- |
| fallback safety | Pass | Production proxy is preferred and fallback remains assigned. | gun_tower |
| footprint scale | Pass | Visual scale stays within fine-grid footprint tolerances. | center (0.00, 0.60, 0.15), size (1.05, 1.24, 1.36), footprint 1x1 |
| pivot/origin | Pass | Root pivot is usable as footprint center/base. | center (0.00, 0.60, 0.15), size (1.05, 1.24, 1.36) |
| 360 readability | Pass | Top, side, rear, roof, and tiering markers are present. | AllAround |
| socket completeness | Pass | Required sockets are present. | 11 sockets checked |
| animation hook readiness | Pass | Motion, combat, and building-state hooks are ready for replacement art. | gun_tower |
| LOD/performance readiness | Warning | Proxy is over the recommended Quest budget. | mesh objects 23/48, materials 12/8 |
| top-down readability | Pass | Prefab has modular identity geometry beyond a plain blockout. | renderers 23, materials 12 |
| player-facing visibility | Pass | Prefab has readable rendered volume for live player-facing scenes. | center (0.00, 0.60, 0.15), size (1.05, 1.24, 1.36) |
| artist replacement metadata | Pass | Replacement metadata is present; proxy remains active until real model validation passes. | NoCandidateFound  |

### rifle_infantry

- Status: Warning
- Visual tier: FirstPassProxy
- Mesh objects: 16
- Materials: 11
- Sockets: 8
- Bounds center: `(0.00, 0.50, 0.11)`
- Bounds size: `(1.05, 1.04, 1.27)`
- Artist import: NoCandidateFound

| Category | Status | Result | Detail |
| --- | --- | --- | --- |
| fallback safety | Pass | Production proxy is preferred and fallback remains assigned. | rifle_infantry |
| footprint scale | Pass | Visual scale stays within fine-grid footprint tolerances. | center (0.00, 0.50, 0.11), size (1.05, 1.04, 1.27), footprint 1x1 |
| pivot/origin | Pass | Root pivot is usable as footprint center/base. | center (0.00, 0.50, 0.11), size (1.05, 1.04, 1.27) |
| 360 readability | Pass | Top, side, rear, roof, and tiering markers are present. | AllAround |
| socket completeness | Pass | Required sockets are present. | 8 sockets checked |
| animation hook readiness | Pass | Motion, combat, and building-state hooks are ready for replacement art. | rifle_infantry |
| LOD/performance readiness | Warning | Proxy is over the recommended Quest budget. | mesh objects 16/28, materials 11/6 |
| top-down readability | Pass | Prefab has modular identity geometry beyond a plain blockout. | renderers 16, materials 11 |
| player-facing visibility | Pass | Prefab has readable rendered volume for live player-facing scenes. | center (0.00, 0.50, 0.11), size (1.05, 1.04, 1.27) |
| artist replacement metadata | Pass | Replacement metadata is present; proxy remains active until real model validation passes. | NoCandidateFound  |

### light_tank

- Status: Warning
- Visual tier: FirstPassProxy
- Mesh objects: 22
- Materials: 7
- Sockets: 12
- Bounds center: `(0.00, 0.59, 0.17)`
- Bounds size: `(1.24, 1.22, 1.45)`
- Artist import: NoCandidateFound

| Category | Status | Result | Detail |
| --- | --- | --- | --- |
| fallback safety | Pass | Production proxy is preferred and fallback remains assigned. | light_tank |
| footprint scale | Pass | Visual scale stays within fine-grid footprint tolerances. | center (0.00, 0.59, 0.17), size (1.24, 1.22, 1.45), footprint 1x1 |
| pivot/origin | Pass | Root pivot is usable as footprint center/base. | center (0.00, 0.59, 0.17), size (1.24, 1.22, 1.45) |
| 360 readability | Pass | Top, side, rear, roof, and tiering markers are present. | AllAround |
| socket completeness | Pass | Required sockets are present. | 12 sockets checked |
| animation hook readiness | Pass | Motion, combat, and building-state hooks are ready for replacement art. | light_tank |
| LOD/performance readiness | Warning | Proxy is over the recommended Quest budget. | mesh objects 22/28, materials 7/6 |
| top-down readability | Pass | Prefab has modular identity geometry beyond a plain blockout. | renderers 22, materials 7 |
| player-facing visibility | Pass | Prefab has readable rendered volume for live player-facing scenes. | center (0.00, 0.59, 0.17), size (1.24, 1.22, 1.45) |
| artist replacement metadata | Pass | Replacement metadata is present; proxy remains active until real model validation passes. | NoCandidateFound  |

### harvester

- Status: Warning
- Visual tier: FirstPassProxy
- Mesh objects: 21
- Materials: 9
- Sockets: 10
- Bounds center: `(0.00, 0.59, 0.13)`
- Bounds size: `(1.24, 1.22, 1.46)`
- Artist import: NoCandidateFound

| Category | Status | Result | Detail |
| --- | --- | --- | --- |
| fallback safety | Pass | Production proxy is preferred and fallback remains assigned. | harvester |
| footprint scale | Pass | Visual scale stays within fine-grid footprint tolerances. | center (0.00, 0.59, 0.13), size (1.24, 1.22, 1.46), footprint 1x1 |
| pivot/origin | Pass | Root pivot is usable as footprint center/base. | center (0.00, 0.59, 0.13), size (1.24, 1.22, 1.46) |
| 360 readability | Pass | Top, side, rear, roof, and tiering markers are present. | AllAround |
| socket completeness | Pass | Required sockets are present. | 10 sockets checked |
| animation hook readiness | Pass | Motion, combat, and building-state hooks are ready for replacement art. | harvester |
| LOD/performance readiness | Warning | Proxy is over the recommended Quest budget. | mesh objects 21/28, materials 9/6 |
| top-down readability | Pass | Prefab has modular identity geometry beyond a plain blockout. | renderers 21, materials 9 |
| player-facing visibility | Pass | Prefab has readable rendered volume for live player-facing scenes. | center (0.00, 0.59, 0.13), size (1.24, 1.22, 1.46) |
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

## Stage 29 Realistic Battlefield Addendum

Stage 29 keeps the Stage 20/21 socket, pivot, LOD, fallback, and artist replacement metadata contract intact while adding an additive material/detail pass for realistic battlefield readability.

- MVP proxies keep their existing descriptors, sockets, production visual validation tags, LOD tags, and Stage 6/7/9 gameplay-facing hooks.
- Added detail must improve grounding, roof identity, top silhouettes, and front/side/rear cues without changing gameplay scale or deterministic `Rts.Core` data.
- The Stage 29 review scene and screenshot capture validate these visual upgrades beside terrain, resource, foundation, lighting, and fine-grid material samples.
