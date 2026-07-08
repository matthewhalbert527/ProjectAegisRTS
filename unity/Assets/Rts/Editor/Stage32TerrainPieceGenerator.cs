using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Rendering.TerrainPieces;
using ProjectAegisRTS.UnityClient.Rendering.Visuals;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage32TerrainPieceGenerator
    {
        public const string MaterialFolder = "Assets/Rts/Art/Materials/TerrainPieces";
        public const string PrefabRoot = "Assets/Rts/Art/Prefabs/TerrainPieces";
        public const string DefinitionRoot = "Assets/Rts/ScriptableObjects/Art/TerrainPieces/Definitions";
        public const string LibraryFolder = "Assets/Rts/ScriptableObjects/Art/TerrainPieces";
        public const string TerrainPieceLibraryPath = LibraryFolder + "/stage32_terrain_piece_library.asset";
        public const string MaterialLibraryPath = LibraryFolder + "/stage32_terrain_piece_material_library.asset";
        public const string SetDressingProfilePath = LibraryFolder + "/stage32_player_facing_set_dressing.asset";
        public const string SetDressingLibraryPath = LibraryFolder + "/stage32_terrain_set_dressing_library.asset";

        [MenuItem("ProjectAegisRTS/Stage 32/Generate Terrain Pieces Batch")]
        public static void GenerateStage32TerrainPiecesMenu()
        {
            EnsureStage32TerrainPieces();
        }

        public static void GenerateStage32TerrainPiecesBatch()
        {
            try
            {
                var summary = EnsureStage32TerrainPieces();
                Debug.Log("Stage 32 terrain piece generation completed. Pieces: " + summary.PieceCount);
                if (Application.isBatchMode)
                    EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
                throw;
            }
        }

        public static Stage32TerrainPieceGenerationSummary EnsureStage32TerrainPieces()
        {
            Stage29BattlefieldVisualAssetCreator.EnsureStage29Assets();
            EnsureFolders();

            var materials = CreateMaterialsAndLibrary();
            var specs = BuildSpecs();
            var definitions = new List<TerrainPieceDefinition>();

            for (var i = 0; i < specs.Count; i++)
            {
                var spec = specs[i];
                var prefab = CreatePrefab(spec, materials);
                var definition = CreateDefinition(spec, prefab);
                definitions.Add(definition);
            }

            Stage32_6TerrainArtIntegrationCorrection.ApplyRuntimePrefabsToStage32Definitions(definitions);
            var pieceLibrary = CreateTerrainPieceLibrary(definitions);
            var profile = CreateSetDressingProfile(false);
            CreateSetDressingLibrary(profile);
            Stage32_5TerrainArtBatch01Importer.ApplyImportedBatch01ToTerrainDefinitionsAndProfile();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            pieceLibrary.RebuildLookup();

            var summary = new Stage32TerrainPieceGenerationSummary();
            summary.PieceCount = pieceLibrary.Count;
            summary.GroundCount = pieceLibrary.CountByCategory(TerrainPieceCategory.Ground);
            summary.TransitionCount = pieceLibrary.CountByCategory(TerrainPieceCategory.Transition);
            summary.BaseCount = pieceLibrary.CountByCategory(TerrainPieceCategory.BaseConstruction);
            summary.ObstacleCount = pieceLibrary.CountByCategory(TerrainPieceCategory.Obstacle);
            summary.ResourceCount = pieceLibrary.CountByCategory(TerrainPieceCategory.Resource);
            summary.PropCount = pieceLibrary.CountByCategory(TerrainPieceCategory.Prop);
            return summary;
        }

        public static TerrainPieceLibrary LoadTerrainPieceLibrary()
        {
            return AssetDatabase.LoadAssetAtPath<TerrainPieceLibrary>(TerrainPieceLibraryPath);
        }

        public static Stage32TerrainPieceMaterialLibrary LoadMaterialLibrary()
        {
            return AssetDatabase.LoadAssetAtPath<Stage32TerrainPieceMaterialLibrary>(MaterialLibraryPath);
        }

        public static TerrainSetDressingLibrary LoadSetDressingLibrary()
        {
            return AssetDatabase.LoadAssetAtPath<TerrainSetDressingLibrary>(SetDressingLibraryPath);
        }

        public static TerrainSetDressingProfile LoadPlayerFacingSetDressingProfile()
        {
            return AssetDatabase.LoadAssetAtPath<TerrainSetDressingProfile>(SetDressingProfilePath);
        }

        public static void ConfigureStage16SetDressing(GameObject game)
        {
            if (game == null)
                return;

            EnsureStage32TerrainPieces();
            if (AssetDatabase.IsValidFolder(Stage32TerrainSampleGroundTileIntegrator.SampleRoot))
                Stage32TerrainSampleGroundTileIntegrator.IntegrateGroundTiles();

            var layer = GetOrAdd<TerrainSetDressingRuntimeLayer>(game);
            var resolver = GetOrAdd<TerrainPieceRuntimeResolver>(game);
            var renderer = GetOrAdd<TerrainSetDressingRenderer>(game);

            layer.pieceLibrary = LoadTerrainPieceLibrary();
            layer.materialLibrary = LoadMaterialLibrary();
            layer.setDressingLibrary = LoadSetDressingLibrary();
            layer.activeProfile = LoadPlayerFacingSetDressingProfile();
            layer.resolver = resolver;
            layer.renderer = renderer;
            layer.initializeOnStart = true;
            layer.logStartup = true;

            resolver.pieceLibrary = layer.pieceLibrary;
            resolver.materialLibrary = layer.materialLibrary;
            renderer.resolver = resolver;
            renderer.profile = layer.activeProfile;
            renderer.hardInstanceLimit = 180;
            renderer.renderOnStart = true;
            renderer.rebuildOnRender = true;

            EditorUtility.SetDirty(game);
        }

        static Dictionary<string, Material> CreateMaterialsAndLibrary()
        {
            var profiles = new List<Stage32TerrainPieceMaterialProfile>();
            var materials = new Dictionary<string, Material>();
            AddMaterial(materials, profiles, "grass_dirt", "Grass Dirt", new Color(0.22f, 0.33f, 0.20f, 1f), new Color(0.44f, 0.38f, 0.24f, 1f), 0.13f, 0f, "Mixed field base that sits below the fine grid.");
            AddMaterial(materials, profiles, "compact_soil", "Compacted Soil", new Color(0.30f, 0.28f, 0.22f, 1f), new Color(0.42f, 0.38f, 0.29f, 1f), 0.16f, 0f, "Base approach soil and worn vehicle apron.");
            AddMaterial(materials, profiles, "mud", "Mud", new Color(0.19f, 0.16f, 0.12f, 1f), new Color(0.34f, 0.28f, 0.20f, 1f), 0.20f, 0f, "Darker low-contrast muddy ground.");
            AddMaterial(materials, profiles, "scorch", "Scorched Ground", new Color(0.08f, 0.075f, 0.06f, 1f), new Color(0.33f, 0.22f, 0.15f, 1f), 0.08f, 0f, "Subtle shell/scorch coloration.");
            AddMaterial(materials, profiles, "concrete", "Weathered Concrete", new Color(0.43f, 0.45f, 0.40f, 1f), new Color(0.62f, 0.60f, 0.52f, 1f), 0.21f, 0f, "Not pure white; keeps buildings readable.");
            AddMaterial(materials, profiles, "road", "Road Path", new Color(0.22f, 0.21f, 0.18f, 1f), new Color(0.44f, 0.40f, 0.31f, 1f), 0.10f, 0f, "Worn road/path surface.");
            AddMaterial(materials, profiles, "resource_ground", "Resource Ground", new Color(0.18f, 0.41f, 0.34f, 1f), new Color(0.70f, 0.62f, 0.25f, 1f), 0.26f, 0f, "Distinct harvest-field ground.");
            AddMaterial(materials, profiles, "water", "Water Placeholder", new Color(0.08f, 0.20f, 0.29f, 0.92f), new Color(0.24f, 0.45f, 0.53f, 1f), 0.55f, 0f, "Quest-safe flat water/shore placeholder.");
            AddMaterial(materials, profiles, "rock", "Rock", new Color(0.24f, 0.23f, 0.21f, 1f), new Color(0.46f, 0.43f, 0.36f, 1f), 0.09f, 0f, "Darker rocky blockers and ridge pieces.");
            AddMaterial(materials, profiles, "foundation", "Foundation", new Color(0.36f, 0.37f, 0.33f, 1f), new Color(0.61f, 0.58f, 0.48f, 1f), 0.18f, 0f, "Base pads and hardstand pieces.");
            AddMaterial(materials, profiles, "caution", "Caution Paint", new Color(0.88f, 0.60f, 0.18f, 1f), new Color(0.98f, 0.82f, 0.36f, 1f), 0.24f, 0f, "Rally/exit and industrial markings.");
            AddMaterial(materials, profiles, "mineral", "Resource Mineral", new Color(0.16f, 0.85f, 0.66f, 1f), new Color(0.60f, 1f, 0.86f, 1f), 0.58f, 0f, "Harvestable resource crystals.");
            AddMaterial(materials, profiles, "rich_mineral", "Rich Resource Mineral", new Color(0.20f, 0.96f, 0.76f, 1f), new Color(0.95f, 0.78f, 0.25f, 1f), 0.64f, 0f, "Richer harvest-field highlights.");
            AddMaterial(materials, profiles, "depleted_mineral", "Depleted Resource Mineral", new Color(0.23f, 0.38f, 0.35f, 1f), new Color(0.42f, 0.48f, 0.40f, 1f), 0.30f, 0f, "Depleted resource hints.");
            AddMaterial(materials, profiles, "sandbag", "Sandbag", new Color(0.49f, 0.43f, 0.31f, 1f), new Color(0.64f, 0.56f, 0.40f, 1f), 0.14f, 0f, "Muted defensive bags.");
            AddMaterial(materials, profiles, "metal", "Worn Metal", new Color(0.30f, 0.32f, 0.31f, 1f), new Color(0.48f, 0.50f, 0.47f, 1f), 0.30f, 0.08f, "Crates, barriers, wrecks, and beacon hardware.");
            AddMaterial(materials, profiles, "dark_metal", "Dark Metal", new Color(0.10f, 0.11f, 0.105f, 1f), new Color(0.25f, 0.26f, 0.24f, 1f), 0.18f, 0.05f, "Tank traps and destroyed-vehicle silhouettes.");
            AddMaterial(materials, profiles, "tire", "Tire Track", new Color(0.055f, 0.055f, 0.050f, 1f), new Color(0.19f, 0.17f, 0.14f, 1f), 0.06f, 0f, "Subtle dark track/shell marks.");
            AddMaterial(materials, profiles, "foliage", "Foliage", new Color(0.12f, 0.30f, 0.15f, 1f), new Color(0.28f, 0.45f, 0.22f, 1f), 0.10f, 0f, "Quest-safe bush/tree clusters.");
            AddMaterial(materials, profiles, "beacon", "Beacon Glow", new Color(0.16f, 0.72f, 0.78f, 1f), new Color(0.72f, 0.96f, 1f, 1f), 0.45f, 0f, "Small readable beacon light.");

            var library = AssetDatabase.LoadAssetAtPath<Stage32TerrainPieceMaterialLibrary>(MaterialLibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<Stage32TerrainPieceMaterialLibrary>();
                AssetDatabase.CreateAsset(library, MaterialLibraryPath);
            }

            library.profiles = profiles;
            library.RebuildLookup();
            EditorUtility.SetDirty(library);
            return materials;
        }

        static void AddMaterial(Dictionary<string, Material> materials, List<Stage32TerrainPieceMaterialProfile> profiles, string id, string label, Color baseColor, Color accentColor, float smoothness, float metallic, string notes)
        {
            var material = CreateMaterial(MaterialFolder + "/stage32_" + id + ".mat", baseColor, smoothness, metallic);
            materials[id] = material;
            profiles.Add(new Stage32TerrainPieceMaterialProfile
            {
                profileId = id,
                displayName = label,
                material = material,
                baseColor = baseColor,
                accentColor = accentColor,
                notes = notes
            });
        }

        static List<Stage32PieceSpec> BuildSpecs()
        {
            var specs = new List<Stage32PieceSpec>();
            AddSeries(specs, TerrainPieceCategory.Ground, "Ground", "ground_grass_dirt_patch", "Grass dirt patch", 4, TerrainPieceSizeClass.Patch, 3, 3, "grass_dirt", "Passable visual ground.", "Buildable hint; stays below fine grid.", true, true, false, "Grass/dirt variation.");
            AddSeries(specs, TerrainPieceCategory.Ground, "Ground", "ground_compact_soil_patch", "Compacted soil patch", 3, TerrainPieceSizeClass.Patch, 3, 2, "compact_soil", "Passable visual ground.", "Buildable base approach.", true, true, false, "Worn construction soil.");
            AddSeries(specs, TerrainPieceCategory.Ground, "Ground", "ground_mud_patch", "Mud patch", 2, TerrainPieceSizeClass.Patch, 2, 2, "mud", "Passable visual ground.", "Avoid as primary build cue.", true, true, false, "Low, dark mud variation.");
            AddSeries(specs, TerrainPieceCategory.Ground, "Ground", "ground_scorched_patch", "Scorched ground", 2, TerrainPieceSizeClass.Patch, 2, 2, "scorch", "Passable visual ground.", "Visual-only combat scar.", true, true, false, "Subtle battlefield scorch.");
            AddSeries(specs, TerrainPieceCategory.Ground, "Ground", "ground_concrete_pad", "Concrete pad", 2, TerrainPieceSizeClass.Medium, 4, 3, "concrete", "Passable visual hardstand.", "Buildable hardstand hint.", true, true, false, "Weathered concrete pad.");
            AddSeries(specs, TerrainPieceCategory.Ground, "Ground", "ground_road_path", "Road path", 2, TerrainPieceSizeClass.Strip, 5, 2, "road", "Passable path visual.", "Not a gameplay road authority.", true, true, false, "Road/path strip.");
            AddSeries(specs, TerrainPieceCategory.Ground, "Ground", "ground_resource_field", "Resource ground", 1, TerrainPieceSizeClass.Patch, 4, 4, "resource_ground", "Passable unless core terrain says otherwise.", "No-build visual hint near resources.", true, true, false, "Harvest-field base tint.");
            AddSeries(specs, TerrainPieceCategory.Ground, "Ground", "ground_water_shore_placeholder", "Water shore tile", 1, TerrainPieceSizeClass.Edge, 4, 2, "water", "Blocked only when core terrain says water.", "No-build visual hint.", true, true, true, "Reference-sheet shoreline tile with visible bank detail.");
            AddSeries(specs, TerrainPieceCategory.Ground, "Ground", "ground_rocky_blocked", "Rocky blocked ground", 1, TerrainPieceSizeClass.Medium, 3, 3, "rock", "Blocked visual hint only.", "No-build visual hint.", true, true, true, "Rocky blocked terrain marker.");

            AddSeries(specs, TerrainPieceCategory.Transition, "Transitions", "transition_grass_dirt_edge", "Grass dirt edge", 3, TerrainPieceSizeClass.Edge, 4, 1, "grass_dirt", "Passable visual blend.", "Buildable edge remains visual-only.", true, true, false, "Grass to dirt blend edge.");
            AddSeries(specs, TerrainPieceCategory.Transition, "Transitions", "transition_dirt_road_blend", "Dirt road blend", 3, TerrainPieceSizeClass.Edge, 4, 1, "road", "Passable road blend.", "No build authority.", true, true, false, "Dirt to road transition.");
            AddSeries(specs, TerrainPieceCategory.Transition, "Transitions", "transition_concrete_ground_edge", "Concrete ground edge", 2, TerrainPieceSizeClass.Edge, 4, 1, "concrete", "Passable hardstand edge.", "Buildable edge hint.", true, true, false, "Concrete/ground seam.");
            AddSeries(specs, TerrainPieceCategory.Transition, "Transitions", "transition_resource_edge", "Resource edge", 2, TerrainPieceSizeClass.Edge, 3, 1, "resource_ground", "Harvest-field visual edge.", "No-build resource edge hint.", true, true, false, "Resource field edge.");
            AddSeries(specs, TerrainPieceCategory.Transition, "Transitions", "transition_water_edge", "Water edge", 2, TerrainPieceSizeClass.Edge, 4, 1, "water", "Blocked water edge hint only.", "No-build visual edge.", true, true, true, "Waterline edge.");
            AddSeries(specs, TerrainPieceCategory.Transition, "Transitions", "transition_rock_edge", "Rock edge", 2, TerrainPieceSizeClass.Edge, 3, 1, "rock", "Blocked edge visual hint only.", "No-build visual edge.", true, true, true, "Rock/blocker edge.");
            AddSeries(specs, TerrainPieceCategory.Transition, "Transitions", "transition_buildable_edge", "Buildable edge", 2, TerrainPieceSizeClass.Edge, 4, 1, "foundation", "Passable construction edge.", "Buildable/non-buildable separation hint.", true, true, false, "Buildable edge line.");

            AddSeries(specs, TerrainPieceCategory.BaseConstruction, "Base", "base_foundation_pad", "Foundation pad", 4, TerrainPieceSizeClass.Medium, 4, 4, "foundation", "Passable base pad.", "Buildable footprint guide.", true, true, false, "Starting base hardstand.");
            AddSeries(specs, TerrainPieceCategory.BaseConstruction, "Base", "base_ramp", "Base ramp", 2, TerrainPieceSizeClass.Small, 3, 2, "concrete", "Passable ramp visual.", "Base entrance guide.", true, true, false, "Vehicle ramp/apron.");
            AddSeries(specs, TerrainPieceCategory.BaseConstruction, "Base", "base_production_apron", "Production apron", 2, TerrainPieceSizeClass.Strip, 5, 2, "foundation", "Passable production exit.", "Keep production exits readable.", true, true, false, "Production exit apron.");
            AddSeries(specs, TerrainPieceCategory.BaseConstruction, "Base", "base_road_strip", "Base road strip", 2, TerrainPieceSizeClass.Strip, 5, 1, "road", "Passable road strip.", "No build authority.", true, true, false, "Base road connection.");
            AddSeries(specs, TerrainPieceCategory.BaseConstruction, "Base", "base_concrete_seam", "Concrete seam", 2, TerrainPieceSizeClass.Strip, 4, 1, "foundation", "Passable seam.", "Footprint scale guide.", true, true, false, "Concrete expansion seam.");
            AddSeries(specs, TerrainPieceCategory.BaseConstruction, "Base", "base_footprint_decal", "Footprint decal", 2, TerrainPieceSizeClass.Patch, 3, 3, "caution", "Visual placement footprint.", "Buildable footprint decal.", true, true, false, "Subtle construction footprint.");
            AddSeries(specs, TerrainPieceCategory.BaseConstruction, "Base", "base_rally_exit_marking", "Rally exit marking", 2, TerrainPieceSizeClass.Small, 3, 1, "caution", "Passable rally marking.", "Exit lane indicator.", true, true, false, "Rally/exit floor marking.");

            AddSeries(specs, TerrainPieceCategory.Obstacle, "Obstacles", "obstacle_rock_cluster", "Rock cluster", 4, TerrainPieceSizeClass.Small, 2, 2, "rock", "Blocked visual hint only.", "No-build visual hint.", true, true, true, "Low-poly rock cluster.");
            AddSeries(specs, TerrainPieceCategory.Obstacle, "Obstacles", "obstacle_ridge_piece", "Ridge piece", 3, TerrainPieceSizeClass.Edge, 4, 1, "rock", "Blocked ridge hint only.", "No-build visual edge.", true, true, true, "Low ridge silhouette.");
            AddSeries(specs, TerrainPieceCategory.Obstacle, "Obstacles", "obstacle_cliff_blocker_chunk", "Cliff blocker chunk", 3, TerrainPieceSizeClass.Medium, 3, 2, "rock", "Blocked cliff hint only.", "No-build visual chunk.", true, true, true, "Chunky blocker silhouette.");
            AddSeries(specs, TerrainPieceCategory.Obstacle, "Obstacles", "obstacle_crater", "Crater", 2, TerrainPieceSizeClass.Patch, 3, 3, "scorch", "Passable crater visual unless core says blocked.", "Avoid build visual cue.", true, true, false, "Subtle shell crater.");
            AddSeries(specs, TerrainPieceCategory.Obstacle, "Obstacles", "obstacle_tree_bush_cluster", "Tree bush cluster", 2, TerrainPieceSizeClass.Small, 2, 2, "foliage", "Blocked forest hint only.", "No-build visual foliage.", true, true, true, "Quest-safe foliage cluster.");
            AddSeries(specs, TerrainPieceCategory.Obstacle, "Obstacles", "obstacle_wreckage", "Wreckage", 2, TerrainPieceSizeClass.Small, 3, 2, "dark_metal", "Blocked debris hint only.", "No-build visual wreck.", true, true, true, "Destroyed battlefield wreckage.");
            AddSeries(specs, TerrainPieceCategory.Obstacle, "Obstacles", "obstacle_debris", "Debris", 2, TerrainPieceSizeClass.Small, 2, 2, "metal", "Debris visual hint.", "Keep outside build lanes.", true, true, false, "Small debris scatter.");

            AddSeries(specs, TerrainPieceCategory.Resource, "Resources", "resource_cluster", "Resource cluster", 4, TerrainPieceSizeClass.Small, 2, 2, "mineral", "Harvest visual marker.", "No-build resource hint.", true, true, false, "Standard resource crystal cluster.");
            AddSeries(specs, TerrainPieceCategory.Resource, "Resources", "resource_rich_cluster", "Rich resource cluster", 2, TerrainPieceSizeClass.Small, 2, 2, "rich_mineral", "Rich harvest visual marker.", "No-build resource hint.", true, true, false, "Richer resource variation.");
            AddSeries(specs, TerrainPieceCategory.Resource, "Resources", "resource_depleted_cluster", "Depleted resource cluster", 2, TerrainPieceSizeClass.Small, 2, 2, "depleted_mineral", "Depleted resource visual.", "No-build resource hint.", true, true, false, "Depleted resource variation.");
            AddSeries(specs, TerrainPieceCategory.Resource, "Resources", "resource_decal", "Resource decal", 2, TerrainPieceSizeClass.Patch, 3, 3, "resource_ground", "Harvest-field decal.", "No-build resource ground hint.", true, true, false, "Resource ground decal.");
            AddSeries(specs, TerrainPieceCategory.Resource, "Resources", "resource_harvest_marker", "Harvest marker", 2, TerrainPieceSizeClass.Tiny, 1, 1, "caution", "Harvest-route marker.", "Keep clear of placement lanes.", true, true, false, "Small harvest route marker.");

            AddSeries(specs, TerrainPieceCategory.Prop, "Props", "prop_sandbag", "Sandbag", 3, TerrainPieceSizeClass.Small, 2, 1, "sandbag", "Cover prop visual only.", "No-build prop hint.", true, true, false, "Small defensive sandbags.");
            AddSeries(specs, TerrainPieceCategory.Prop, "Props", "prop_barrier", "Barrier", 2, TerrainPieceSizeClass.Edge, 3, 1, "metal", "Barrier visual only.", "No-build edge prop.", true, true, false, "Light metal barrier.");
            AddSeries(specs, TerrainPieceCategory.Prop, "Props", "prop_tank_trap", "Tank trap", 2, TerrainPieceSizeClass.Small, 1, 1, "dark_metal", "Blocked prop visual hint.", "No-build tank trap.", true, true, true, "Quest-safe tank trap silhouette.");
            AddSeries(specs, TerrainPieceCategory.Prop, "Props", "prop_tire_tracks", "Tire tracks", 2, TerrainPieceSizeClass.Strip, 5, 1, "tire", "Track mark visual.", "Passable visual-only track.", true, true, false, "Subtle tire tracks.");
            AddSeries(specs, TerrainPieceCategory.Prop, "Props", "prop_shell_mark", "Shell mark", 2, TerrainPieceSizeClass.Patch, 2, 2, "tire", "Shell mark visual.", "Passable visual-only scar.", true, true, false, "Small shell mark.");
            AddSeries(specs, TerrainPieceCategory.Prop, "Props", "prop_crates", "Crates", 2, TerrainPieceSizeClass.Small, 2, 2, "metal", "Supply prop visual.", "Keep outside build lanes.", true, true, false, "Small crate stack.");
            AddSeries(specs, TerrainPieceCategory.Prop, "Props", "prop_antenna_beacon", "Antenna beacon", 1, TerrainPieceSizeClass.Tiny, 1, 1, "beacon", "Beacon prop visual.", "Tiny prop outside placement lanes.", true, true, false, "Small antenna/beacon prop.");
            AddSeries(specs, TerrainPieceCategory.Prop, "Props", "prop_destroyed_vehicle_proxy", "Destroyed vehicle proxy", 2, TerrainPieceSizeClass.Small, 3, 2, "dark_metal", "Wreck prop visual hint.", "No-build wreck hint.", true, true, true, "Destroyed vehicle proxy.");
            return specs;
        }

        static void AddSeries(List<Stage32PieceSpec> specs, TerrainPieceCategory category, string folder, string prefix, string label, int count, TerrainPieceSizeClass sizeClass, int footprintWidth, int footprintHeight, string materialProfileId, string passabilityHint, string buildableHint, bool supportsRotation, bool supportsTint, bool blockingVisual, string notes)
        {
            for (var i = 1; i <= count; i++)
            {
                var id = prefix + "_" + i.ToString("00");
                specs.Add(new Stage32PieceSpec
                {
                    Id = id,
                    DisplayName = label + " " + i.ToString("00"),
                    Category = category,
                    Folder = folder,
                    SizeClass = sizeClass,
                    FootprintFineWidth = Mathf.Max(1, footprintWidth + ((i % 3 == 0 && sizeClass != TerrainPieceSizeClass.Tiny) ? 1 : 0)),
                    FootprintFineHeight = Mathf.Max(1, footprintHeight + ((i % 4 == 0 && sizeClass != TerrainPieceSizeClass.Tiny) ? 1 : 0)),
                    MaterialProfileId = materialProfileId,
                    PassabilityHint = passabilityHint,
                    BuildableHint = buildableHint,
                    SupportsRotation = supportsRotation,
                    SupportsTint = supportsTint,
                    BlockingVisualOnly = blockingVisual,
                    Notes = notes,
                    Variant = i,
                    QuestBudgetTag = sizeClass == TerrainPieceSizeClass.Large ? "QuestSafeMedium" : "QuestSafeLow"
                });
            }
        }

        static GameObject CreatePrefab(Stage32PieceSpec spec, Dictionary<string, Material> materials)
        {
            var root = new GameObject(spec.Id);
            root.transform.position = Vector3.zero;
            var primary = MaterialFor(materials, spec.MaterialProfileId);
            var accent = AccentMaterialFor(spec, materials);
            BuildGeometry(root.transform, spec, primary, accent, materials);

            var tag = root.AddComponent<TerrainPieceValidationTag>();
            tag.pieceId = spec.Id;
            tag.displayName = spec.DisplayName;
            tag.category = spec.Category;
            tag.sizeClass = spec.SizeClass;
            tag.footprintFineWidth = spec.FootprintFineWidth;
            tag.footprintFineHeight = spec.FootprintFineHeight;
            tag.materialProfileId = spec.MaterialProfileId;
            tag.passabilityVisualHint = spec.PassabilityHint;
            tag.buildableVisualHint = spec.BuildableHint;
            tag.supportsRotation = spec.SupportsRotation;
            tag.supportsTint = spec.SupportsTint;
            tag.isGameplayBlockingVisualOnly = spec.BlockingVisualOnly;
            tag.questBudgetTag = spec.QuestBudgetTag;
            tag.notes = spec.Notes + " Stage32 visual-only prefab; never gameplay authority.";

            var renderers = root.GetComponentsInChildren<Renderer>(true);
            tag.rendererCount = renderers.Length;
            tag.primitiveCount = renderers.Length;
            var lod = root.AddComponent<LODGroup>();
            if (renderers.Length > 0)
            {
                lod.SetLODs(new[] { new LOD(0.01f, renderers) });
                lod.RecalculateBounds();
            }

            var path = PrefabPath(spec);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        static void BuildGeometry(Transform parent, Stage32PieceSpec spec, Material primary, Material accent, Dictionary<string, Material> materials)
        {
            var width = Mathf.Max(0.35f, spec.FootprintFineWidth * 0.48f);
            var depth = Mathf.Max(0.35f, spec.FootprintFineHeight * 0.48f);

            if (spec.Category == TerrainPieceCategory.Obstacle)
            {
                CreatePrimitive(parent, "Low visual grounding", PrimitiveType.Cube, new Vector3(0f, -0.006f, 0f), new Vector3(width * 0.94f, 0.018f, depth * 0.94f), MaterialFor(materials, "scorch"));
                var count = spec.Id.IndexOf("crater", StringComparison.OrdinalIgnoreCase) >= 0 ? 3 : 5;
                for (var i = 0; i < count; i++)
                {
                    var offset = OffsetFor(i, count, width, depth);
                    var scale = new Vector3(0.22f + 0.05f * ((spec.Variant + i) % 3), 0.12f + 0.05f * (i % 2), 0.18f + 0.04f * ((spec.Variant + i + 1) % 3));
                    var type = spec.Id.IndexOf("ridge", StringComparison.OrdinalIgnoreCase) >= 0 || spec.Id.IndexOf("cliff", StringComparison.OrdinalIgnoreCase) >= 0 ? PrimitiveType.Cube : PrimitiveType.Sphere;
                    CreatePrimitive(parent, "Obstacle mass " + i, type, new Vector3(offset.x, 0.10f + scale.y * 0.25f, offset.y), scale, primary);
                }
                if (spec.Id.IndexOf("tree", StringComparison.OrdinalIgnoreCase) >= 0)
                    CreatePrimitive(parent, "Foliage crown", PrimitiveType.Sphere, new Vector3(0.08f, 0.34f, 0.02f), new Vector3(0.42f, 0.30f, 0.42f), MaterialFor(materials, "foliage"));
                if (spec.Id.IndexOf("rock", StringComparison.OrdinalIgnoreCase) >= 0 || spec.Id.IndexOf("ridge", StringComparison.OrdinalIgnoreCase) >= 0 || spec.Id.IndexOf("cliff", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    CreatePrimitive(parent, "Stage31 moss cap", PrimitiveType.Cube, new Vector3(-width * 0.10f, 0.27f, depth * 0.06f), new Vector3(width * 0.42f, 0.018f, depth * 0.18f), MaterialFor(materials, "foliage"));
                    CreatePrimitive(parent, "Stage31 broken rock face", PrimitiveType.Cube, new Vector3(width * 0.18f, 0.15f, -depth * 0.12f), new Vector3(width * 0.18f, 0.09f, depth * 0.34f), MaterialFor(materials, "rock"));
                }
                return;
            }

            if (spec.Category == TerrainPieceCategory.Resource)
            {
                CreatePrimitive(parent, "Resource ground decal", PrimitiveType.Cube, new Vector3(0f, 0.000f, 0f), new Vector3(width * 0.90f, 0.018f, depth * 0.90f), MaterialFor(materials, "resource_ground"));
                var clusterCount = spec.Id.IndexOf("decal", StringComparison.OrdinalIgnoreCase) >= 0 ? 2 : 5;
                for (var i = 0; i < clusterCount; i++)
                {
                    var offset = OffsetFor(i, clusterCount, width, depth);
                    var h = 0.18f + 0.04f * ((spec.Variant + i) % 4);
                    CreatePrimitive(parent, "Resource shard " + i, PrimitiveType.Cylinder, new Vector3(offset.x, 0.06f + h * 0.5f, offset.y), new Vector3(0.11f, h, 0.11f), primary);
                }
                CreatePrimitive(parent, "Stage31 mineral bed shadow", PrimitiveType.Cube, new Vector3(0f, 0.026f, 0f), new Vector3(width * 0.62f, 0.012f, depth * 0.55f), MaterialFor(materials, "scorch"));
                CreatePrimitive(parent, "Stage31 mineral sparkle", PrimitiveType.Sphere, new Vector3(width * 0.22f, 0.24f, depth * 0.10f), new Vector3(0.08f, 0.08f, 0.08f), MaterialFor(materials, spec.MaterialProfileId == "rich_mineral" ? "rich_mineral" : "mineral"));
                return;
            }

            if (spec.Category == TerrainPieceCategory.Prop)
            {
                CreatePrimitive(parent, "Prop contact patch", PrimitiveType.Cube, new Vector3(0f, -0.004f, 0f), new Vector3(width * 0.82f, 0.014f, depth * 0.82f), MaterialFor(materials, "compact_soil"));
                if (spec.Id.IndexOf("sandbag", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    for (var i = 0; i < 4; i++)
                        CreatePrimitive(parent, "Sandbag " + i, PrimitiveType.Capsule, new Vector3(-0.30f + i * 0.20f, 0.13f, 0f), new Vector3(0.13f, 0.13f, 0.26f), primary);
                }
                else if (spec.Id.IndexOf("tank_trap", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    CreatePrimitive(parent, "Tank trap spine A", PrimitiveType.Cube, new Vector3(0f, 0.18f, 0f), new Vector3(0.10f, 0.10f, 0.58f), primary).transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
                    CreatePrimitive(parent, "Tank trap spine B", PrimitiveType.Cube, new Vector3(0f, 0.18f, 0f), new Vector3(0.10f, 0.10f, 0.58f), primary).transform.localRotation = Quaternion.Euler(0f, -45f, 0f);
                }
                else if (spec.Id.IndexOf("antenna", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    CreatePrimitive(parent, "Beacon mast", PrimitiveType.Cylinder, new Vector3(0f, 0.42f, 0f), new Vector3(0.045f, 0.42f, 0.045f), MaterialFor(materials, "metal"));
                    CreatePrimitive(parent, "Beacon cap", PrimitiveType.Sphere, new Vector3(0f, 0.90f, 0f), new Vector3(0.14f, 0.14f, 0.14f), primary);
                }
                else if (spec.Id.IndexOf("tracks", StringComparison.OrdinalIgnoreCase) >= 0 || spec.Id.IndexOf("shell", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    CreatePrimitive(parent, "Track mark A", PrimitiveType.Cube, new Vector3(-0.16f, 0.018f, 0f), new Vector3(width * 0.78f, 0.016f, 0.045f), primary);
                    CreatePrimitive(parent, "Track mark B", PrimitiveType.Cube, new Vector3(0.16f, 0.019f, 0.12f), new Vector3(width * 0.70f, 0.016f, 0.045f), primary);
                    CreatePrimitive(parent, "Stage31 track cross tread", PrimitiveType.Cube, new Vector3(0f, 0.031f, -depth * 0.16f), new Vector3(width * 0.46f, 0.010f, 0.035f), MaterialFor(materials, "scorch"));
                }
                else
                {
                    for (var i = 0; i < 3; i++)
                    {
                        var offset = OffsetFor(i, 3, width, depth);
                        CreatePrimitive(parent, "Prop mass " + i, PrimitiveType.Cube, new Vector3(offset.x, 0.12f, offset.y), new Vector3(0.22f, 0.20f, 0.22f), primary);
                    }
                }
                return;
            }

            CreatePrimitive(parent, "Low terrain plate", PrimitiveType.Cube, new Vector3(0f, 0.000f, 0f), new Vector3(width, 0.022f, depth), primary);
            var stripeCount = spec.Category == TerrainPieceCategory.BaseConstruction ? 3 : 2;
            for (var i = 0; i < stripeCount; i++)
            {
                var z = -depth * 0.30f + i * depth * 0.30f;
                var stripe = CreatePrimitive(parent, "Readability seam " + i, PrimitiveType.Cube, new Vector3(0f, 0.022f + i * 0.001f, z), new Vector3(width * (0.58f + 0.08f * (i % 2)), 0.010f, 0.030f), accent);
                if (spec.Category == TerrainPieceCategory.Transition || spec.SizeClass == TerrainPieceSizeClass.Edge)
                    stripe.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }

            if (spec.Id.IndexOf("rally", StringComparison.OrdinalIgnoreCase) >= 0 || spec.Id.IndexOf("footprint", StringComparison.OrdinalIgnoreCase) >= 0)
                CreatePrimitive(parent, "Caution footprint cue", PrimitiveType.Cube, new Vector3(0f, 0.040f, depth * 0.30f), new Vector3(width * 0.46f, 0.012f, 0.055f), MaterialFor(materials, "caution"));
            if (spec.Id.IndexOf("water", StringComparison.OrdinalIgnoreCase) >= 0)
                CreatePrimitive(parent, "Shore highlight", PrimitiveType.Cube, new Vector3(0f, 0.036f, -depth * 0.30f), new Vector3(width * 0.82f, 0.012f, 0.045f), MaterialFor(materials, "grass_dirt"));
            AddStage31TerrainReferenceDetails(parent, spec, width, depth, materials);
        }

        static void AddStage31TerrainReferenceDetails(Transform parent, Stage32PieceSpec spec, float width, float depth, Dictionary<string, Material> materials)
        {
            var id = spec.Id.ToLowerInvariant();

            if (id.IndexOf("road", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                CreatePrimitive(parent, "Stage31 asphalt shoulder left", PrimitiveType.Cube, new Vector3(-width * 0.42f, 0.040f, 0f), new Vector3(width * 0.08f, 0.014f, depth * 0.92f), MaterialFor(materials, "compact_soil"));
                CreatePrimitive(parent, "Stage31 asphalt shoulder right", PrimitiveType.Cube, new Vector3(width * 0.42f, 0.041f, 0f), new Vector3(width * 0.08f, 0.014f, depth * 0.92f), MaterialFor(materials, "compact_soil"));
                CreatePrimitive(parent, "Stage31 lane dash", PrimitiveType.Cube, new Vector3(0f, 0.056f, depth * 0.18f), new Vector3(width * 0.035f, 0.010f, depth * 0.24f), MaterialFor(materials, "concrete"));
                CreatePrimitive(parent, "Stage31 worn lane dash", PrimitiveType.Cube, new Vector3(0f, 0.057f, -depth * 0.24f), new Vector3(width * 0.032f, 0.010f, depth * 0.16f), MaterialFor(materials, "concrete"));
                return;
            }

            if (id.IndexOf("foundation", StringComparison.OrdinalIgnoreCase) >= 0 ||
                id.IndexOf("concrete", StringComparison.OrdinalIgnoreCase) >= 0 ||
                id.IndexOf("footprint", StringComparison.OrdinalIgnoreCase) >= 0 ||
                spec.Category == TerrainPieceCategory.BaseConstruction)
            {
                CreatePrimitive(parent, "Stage31 pad north trim", PrimitiveType.Cube, new Vector3(0f, 0.056f, depth * 0.46f), new Vector3(width * 0.90f, 0.018f, depth * 0.035f), MaterialFor(materials, "foundation"));
                CreatePrimitive(parent, "Stage31 pad east trim", PrimitiveType.Cube, new Vector3(width * 0.46f, 0.057f, 0f), new Vector3(width * 0.035f, 0.018f, depth * 0.82f), MaterialFor(materials, "foundation"));
                CreatePrimitive(parent, "Stage31 worn slab crack", PrimitiveType.Cube, new Vector3(-width * 0.14f, 0.062f, -depth * 0.08f), new Vector3(width * 0.010f, 0.010f, depth * 0.45f), MaterialFor(materials, "scorch"));
                return;
            }

            if (id.IndexOf("water", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                CreatePrimitive(parent, "Stage31 shore bank", PrimitiveType.Cube, new Vector3(width * 0.26f, 0.052f, 0f), new Vector3(width * 0.26f, 0.018f, depth * 0.82f), MaterialFor(materials, "mud"));
                CreatePrimitive(parent, "Stage31 shore stones", PrimitiveType.Sphere, new Vector3(width * 0.08f, 0.085f, depth * 0.20f), new Vector3(0.08f, 0.025f, 0.06f), MaterialFor(materials, "rock"));
                CreatePrimitive(parent, "Stage31 shore foam", PrimitiveType.Cube, new Vector3(-width * 0.18f, 0.064f, -depth * 0.18f), new Vector3(width * 0.35f, 0.010f, depth * 0.035f), MaterialFor(materials, "concrete"));
                return;
            }

            if (id.IndexOf("grass", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                CreatePrimitive(parent, "Stage31 field tuft a", PrimitiveType.Cube, new Vector3(-width * 0.22f, 0.070f, depth * 0.16f), new Vector3(0.035f, 0.095f, 0.035f), MaterialFor(materials, "foliage"));
                CreatePrimitive(parent, "Stage31 field tuft b", PrimitiveType.Cube, new Vector3(width * 0.20f, 0.071f, -depth * 0.20f), new Vector3(0.030f, 0.080f, 0.030f), MaterialFor(materials, "foliage"));
                CreatePrimitive(parent, "Stage31 field stones", PrimitiveType.Sphere, new Vector3(width * 0.08f, 0.068f, depth * 0.26f), new Vector3(0.070f, 0.025f, 0.050f), MaterialFor(materials, "rock"));
                return;
            }

            if (id.IndexOf("mud", StringComparison.OrdinalIgnoreCase) >= 0 || id.IndexOf("compact", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                CreatePrimitive(parent, "Stage31 vehicle rut left", PrimitiveType.Cube, new Vector3(-width * 0.16f, 0.048f, 0f), new Vector3(width * 0.050f, 0.012f, depth * 0.78f), MaterialFor(materials, "scorch"));
                CreatePrimitive(parent, "Stage31 vehicle rut right", PrimitiveType.Cube, new Vector3(width * 0.16f, 0.049f, 0f), new Vector3(width * 0.050f, 0.012f, depth * 0.78f), MaterialFor(materials, "scorch"));
                return;
            }

            if (id.IndexOf("scorch", StringComparison.OrdinalIgnoreCase) >= 0 || id.IndexOf("rocky", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                CreatePrimitive(parent, "Stage31 broken ground dark center", PrimitiveType.Cylinder, new Vector3(0f, 0.060f, 0f), new Vector3(width * 0.18f, 0.018f, depth * 0.18f), MaterialFor(materials, "scorch"));
                CreatePrimitive(parent, "Stage31 broken ground raised rim", PrimitiveType.Sphere, new Vector3(width * 0.22f, 0.086f, -depth * 0.16f), new Vector3(width * 0.08f, 0.030f, depth * 0.06f), MaterialFor(materials, "rock"));
            }
        }

        static Vector2 OffsetFor(int index, int count, float width, float depth)
        {
            var angle = (index + 0.5f) / Mathf.Max(1, count) * Mathf.PI * 2f;
            var radiusX = width * (0.16f + 0.08f * (index % 3));
            var radiusZ = depth * (0.13f + 0.07f * ((index + 1) % 3));
            return new Vector2(Mathf.Cos(angle) * radiusX, Mathf.Sin(angle) * radiusZ);
        }

        static GameObject CreatePrimitive(Transform parent, string objectName, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var obj = GameObject.CreatePrimitive(type);
            obj.name = objectName;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPosition;
            obj.transform.localScale = localScale;
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
            var collider = obj.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);
            return obj;
        }

        static TerrainPieceDefinition CreateDefinition(Stage32PieceSpec spec, GameObject prefab)
        {
            var path = DefinitionPath(spec);
            var definition = AssetDatabase.LoadAssetAtPath<TerrainPieceDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<TerrainPieceDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            definition.pieceId = spec.Id;
            definition.displayName = spec.DisplayName;
            definition.category = spec.Category;
            definition.sizeClass = spec.SizeClass;
            definition.footprintFineWidth = spec.FootprintFineWidth;
            definition.footprintFineHeight = spec.FootprintFineHeight;
            definition.prefab = prefab;
            definition.materialProfileId = spec.MaterialProfileId;
            definition.passabilityVisualHint = spec.PassabilityHint;
            definition.buildableVisualHint = spec.BuildableHint;
            definition.supportsRotation = spec.SupportsRotation;
            definition.supportsTint = spec.SupportsTint;
            definition.isGameplayBlockingVisualOnly = spec.BlockingVisualOnly;
            definition.notes = spec.Notes + " Visual metadata only; Rts.Core terrain remains authoritative.";
            definition.questBudgetTag = spec.QuestBudgetTag;
            EditorUtility.SetDirty(definition);
            return definition;
        }

        static TerrainPieceLibrary CreateTerrainPieceLibrary(List<TerrainPieceDefinition> definitions)
        {
            var library = AssetDatabase.LoadAssetAtPath<TerrainPieceLibrary>(TerrainPieceLibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<TerrainPieceLibrary>();
                AssetDatabase.CreateAsset(library, TerrainPieceLibraryPath);
            }

            library.definitions = definitions;
            library.RebuildLookup();
            EditorUtility.SetDirty(library);
            return library;
        }

        static TerrainSetDressingProfile CreateSetDressingProfile(bool useSourceArt)
        {
            var profile = AssetDatabase.LoadAssetAtPath<TerrainSetDressingProfile>(SetDressingProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<TerrainSetDressingProfile>();
                AssetDatabase.CreateAsset(profile, SetDressingProfilePath);
            }

            profile.profileId = "stage32_player_facing";
            profile.displayName = "Stage 32 Player-Facing Battlefield Set Dressing";
            profile.deterministicSeed = 3201;
            profile.maxRenderedPieces = 180;
            profile.preserveFineGridReadability = true;
            profile.visualOnlyNeverGameplayAuthority = true;
            profile.notes = "Player-facing terrain dressing uses real Unity prefab assemblies and Terrain Sample Asset Pack ground replacements when installed. The Stage16 map is dressed as two vertically stacked 32-cell battlefield sections with classic military RTS-style cliffs, boulders, outpost pads, wreckage, and route details. Reference sheets are art direction only and must not be used as runtime terrain cards.";
            profile.placements = BuildPlayerFacingPlacements();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        static TerrainSetDressingLibrary CreateSetDressingLibrary(TerrainSetDressingProfile profile)
        {
            var library = AssetDatabase.LoadAssetAtPath<TerrainSetDressingLibrary>(SetDressingLibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<TerrainSetDressingLibrary>();
                AssetDatabase.CreateAsset(library, SetDressingLibraryPath);
            }

            library.defaultProfileId = "stage32_player_facing";
            library.profiles = new List<TerrainSetDressingProfile> { profile };
            library.RebuildLookup();
            EditorUtility.SetDirty(library);
            return library;
        }

        static List<TerrainSetDressingPlacement> BuildPlayerFacingPlacements()
        {
            var placements = new List<TerrainSetDressingPlacement>();
            AddPlacement(placements, "base_foundation_pad_01", 4.2f, 6.8f, 0f, 1.18f, "starting base pad");
            AddPlacement(placements, "base_foundation_pad_02", 7.4f, 6.7f, 0f, 1.05f, "starting base pad");
            AddPlacement(placements, "base_foundation_pad_03", 10.7f, 6.9f, 0f, 1.10f, "starting base pad");
            AddPlacement(placements, "base_production_apron_01", 12.6f, 8.2f, 90f, 1.05f, "production exit apron");
            AddPlacement(placements, "base_production_apron_02", 14.8f, 8.2f, 90f, 0.95f, "production exit apron");
            AddPlacement(placements, "base_road_strip_01", 15.4f, 10.4f, 0f, 1.10f, "scouting route guide");
            AddPlacement(placements, "base_road_strip_02", 18.0f, 11.8f, 0f, 1.00f, "scouting route guide");
            AddPlacement(placements, "base_rally_exit_marking_01", 13.5f, 9.6f, 90f, 0.90f, "rally lane");
            AddPlacement(placements, "base_rally_exit_marking_02", 16.4f, 9.6f, 90f, 0.90f, "rally lane");
            AddPlacement(placements, "ground_compact_soil_patch_01", 5.3f, 10.0f, 15f, 1.10f, "base worn ground");
            AddPlacement(placements, "ground_compact_soil_patch_02", 8.8f, 10.6f, -10f, 1.00f, "base worn ground");
            AddPlacement(placements, "ground_compact_soil_patch_03", 11.6f, 10.8f, 8f, 0.92f, "base-to-road worn ground");
            AddPlacement(placements, "ground_grass_dirt_patch_03", 3.8f, 11.5f, 32f, 1.00f, "player-side heather field variation");
            AddPlacement(placements, "ground_grass_dirt_patch_04", 7.1f, 13.0f, -24f, 0.95f, "player-side heather field variation");
            AddPlacement(placements, "ground_scorched_patch_01", 3.1f, 13.6f, 28f, 0.90f, "battle scar edge");
            AddPlacement(placements, "ground_mud_patch_01", 19.4f, 14.4f, -18f, 0.90f, "route texture");
            AddPlacement(placements, "ground_mud_patch_02", 21.2f, 15.5f, 18f, 0.88f, "road shoulder mud");
            AddPlacement(placements, "transition_concrete_ground_edge_01", 6.0f, 8.8f, 0f, 1.0f, "base edge blend");
            AddPlacement(placements, "transition_concrete_ground_edge_02", 9.2f, 8.7f, 0f, 0.95f, "base edge blend");
            AddPlacement(placements, "transition_buildable_edge_01", 11.5f, 5.2f, 0f, 0.95f, "build zone edge");
            AddPlacement(placements, "transition_buildable_edge_02", 3.6f, 5.2f, 0f, 0.88f, "build zone edge");
            AddPlacement(placements, "transition_dirt_road_blend_01", 17.1f, 10.9f, 0f, 1.0f, "road blend");
            AddPlacement(placements, "transition_dirt_road_blend_02", 18.9f, 12.0f, 8f, 0.95f, "road blend");
            AddPlacement(placements, "transition_dirt_road_blend_03", 20.9f, 13.2f, 10f, 0.90f, "road blend");
            AddPlacement(placements, "resource_cluster_01", 24.5f, 16.5f, 20f, 1.0f, "resource decoration");
            AddPlacement(placements, "resource_cluster_02", 25.8f, 17.7f, -12f, 0.92f, "resource decoration");
            AddPlacement(placements, "resource_cluster_03", 26.6f, 15.2f, 32f, 0.82f, "resource decoration");
            AddPlacement(placements, "resource_cluster_04", 28.0f, 17.8f, -28f, 0.76f, "resource decoration");
            AddPlacement(placements, "resource_rich_cluster_01", 27.1f, 16.4f, 45f, 0.88f, "resource decoration");
            AddPlacement(placements, "resource_rich_cluster_02", 25.2f, 15.0f, -20f, 0.78f, "resource decoration");
            AddPlacement(placements, "resource_decal_01", 24.8f, 18.6f, 0f, 1.20f, "resource ground tint");
            AddPlacement(placements, "resource_decal_02", 27.0f, 15.7f, 18f, 1.05f, "resource ground tint");
            AddPlacement(placements, "resource_harvest_marker_01", 22.8f, 15.9f, 90f, 0.80f, "harvest route marker");
            AddPlacement(placements, "resource_harvest_marker_02", 23.4f, 17.2f, 90f, 0.72f, "harvest route marker");
            AddPlacement(placements, "transition_resource_edge_01", 23.4f, 15.2f, 25f, 1.0f, "resource edge");
            AddPlacement(placements, "transition_resource_edge_02", 26.8f, 18.9f, -16f, 0.94f, "resource edge");
            AddPlacement(placements, "obstacle_rock_cluster_01", 1.4f, 2.5f, 14f, 1.0f, "map edge obstacle");
            AddPlacement(placements, "obstacle_rock_cluster_02", 29.2f, 3.6f, -30f, 1.0f, "map edge obstacle");
            AddPlacement(placements, "obstacle_rock_cluster_03", 30.0f, 6.5f, -12f, 0.92f, "map edge obstacle");
            AddPlacement(placements, "obstacle_rock_cluster_04", 1.8f, 6.2f, 28f, 0.85f, "map edge obstacle");
            AddPlacement(placements, "obstacle_ridge_piece_01", 29.5f, 10.8f, 90f, 1.1f, "map edge ridge");
            AddPlacement(placements, "obstacle_ridge_piece_02", 28.8f, 12.7f, 88f, 0.92f, "map edge ridge");
            AddPlacement(placements, "obstacle_ridge_piece_03", 30.2f, 16.9f, 84f, 0.82f, "map edge ridge");
            AddPlacement(placements, "obstacle_cliff_blocker_chunk_01", 2.0f, 19.2f, 32f, 0.95f, "map edge blocker");
            AddPlacement(placements, "obstacle_cliff_blocker_chunk_02", 1.3f, 16.9f, -12f, 0.82f, "map edge blocker");
            AddPlacement(placements, "obstacle_cliff_blocker_chunk_03", 5.8f, 21.0f, 16f, 0.78f, "map edge blocker");
            AddPlacement(placements, "obstacle_tree_bush_cluster_01", 4.4f, 20.6f, -20f, 0.90f, "map edge foliage");
            AddPlacement(placements, "obstacle_tree_bush_cluster_02", 8.0f, 21.2f, 24f, 0.82f, "map edge foliage");
            AddPlacement(placements, "obstacle_wreckage_01", 21.2f, 4.2f, -36f, 0.82f, "edge battlefield wreck");
            AddPlacement(placements, "obstacle_wreckage_02", 24.0f, 5.0f, 28f, 0.74f, "edge battlefield wreck");
            AddPlacement(placements, "obstacle_debris_01", 22.7f, 5.1f, 15f, 0.80f, "edge battlefield debris");
            AddPlacement(placements, "obstacle_debris_02", 24.7f, 6.4f, -15f, 0.72f, "edge battlefield debris");
            AddPlacement(placements, "prop_sandbag_01", 12.0f, 14.1f, 0f, 0.75f, "base perimeter prop");
            AddPlacement(placements, "prop_sandbag_02", 13.2f, 14.2f, 0f, 0.75f, "base perimeter prop");
            AddPlacement(placements, "prop_sandbag_03", 14.5f, 14.0f, 8f, 0.68f, "base perimeter prop");
            AddPlacement(placements, "prop_barrier_01", 18.7f, 7.1f, 90f, 0.86f, "route edge prop");
            AddPlacement(placements, "prop_barrier_02", 19.8f, 8.4f, 90f, 0.74f, "route edge prop");
            AddPlacement(placements, "prop_tank_trap_01", 30.1f, 14.6f, 20f, 0.88f, "edge prop");
            AddPlacement(placements, "prop_tank_trap_02", 28.9f, 15.5f, -18f, 0.72f, "edge prop");
            AddPlacement(placements, "prop_tire_tracks_01", 17.4f, 12.7f, 15f, 1.15f, "vehicle path texture");
            AddPlacement(placements, "prop_tire_tracks_02", 19.6f, 13.5f, 15f, 1.0f, "vehicle path texture");
            AddPlacement(placements, "prop_shell_mark_01", 21.4f, 13.7f, 12f, 0.80f, "battle scar");
            AddPlacement(placements, "prop_shell_mark_02", 22.8f, 12.5f, -18f, 0.68f, "battle scar");
            AddPlacement(placements, "prop_crates_01", 9.6f, 4.3f, 0f, 0.75f, "base logistics prop");
            AddPlacement(placements, "prop_crates_02", 10.7f, 4.5f, 8f, 0.62f, "base logistics prop");
            AddPlacement(placements, "prop_antenna_beacon_01", 6.5f, 4.0f, 0f, 0.82f, "base beacon prop");
            AddPlacement(placements, "prop_destroyed_vehicle_proxy_01", 27.4f, 7.2f, -45f, 0.84f, "edge wreck prop");
            AddPlacement(placements, "prop_destroyed_vehicle_proxy_02", 26.0f, 6.4f, 25f, 0.70f, "edge wreck prop");
            AddPlacement(placements, "transition_rock_edge_01", 28.4f, 5.3f, 35f, 1.0f, "rock edge blend");
            AddPlacement(placements, "transition_rock_edge_02", 30.0f, 8.8f, 80f, 0.86f, "rock edge blend");
            AddPlacement(placements, "ground_road_path_01", 20.7f, 12.9f, 8f, 1.05f, "route texture");
            AddPlacement(placements, "ground_road_path_02", 22.4f, 14.2f, 10f, 0.92f, "route texture");
            AddPlacement(placements, "ground_grass_dirt_patch_01", 2.4f, 9.4f, -18f, 1.1f, "field variation");
            AddPlacement(placements, "ground_grass_dirt_patch_02", 27.6f, 20.5f, 20f, 1.0f, "field variation");
            AddPlacement(placements, "ground_rocky_blocked_01", 30.0f, 19.2f, -15f, 0.95f, "edge blocked visual");
            StackSecondBattlefieldSection(placements);
            AddClassicMilitaryRtsDetailPass(placements);
            return placements;
        }

        static void StackSecondBattlefieldSection(List<TerrainSetDressingPlacement> placements)
        {
            var lowerCount = placements.Count;
            for (var i = 0; i < lowerCount; i++)
            {
                var source = placements[i];
                if (source == null || string.IsNullOrEmpty(source.pieceId))
                    continue;

                AddPlacement(
                    placements,
                    source.pieceId,
                    source.localPosition.x,
                    source.localPosition.z + 32f,
                    source.rotationY,
                    source.uniformScale,
                    "stacked northern section: " + source.placementRole);
            }
        }

        static void AddClassicMilitaryRtsDetailPass(List<TerrainSetDressingPlacement> placements)
        {
            AddPlacement(placements, "obstacle_ridge_piece_01", 3.2f, 33.8f, 4f, 1.05f, "northern cliff line");
            AddPlacement(placements, "obstacle_ridge_piece_02", 6.8f, 34.3f, -5f, 0.95f, "northern cliff line");
            AddPlacement(placements, "obstacle_ridge_piece_03", 10.1f, 35.1f, 8f, 0.92f, "northern cliff line");
            AddPlacement(placements, "obstacle_cliff_blocker_chunk_01", 1.5f, 37.0f, 12f, 0.92f, "northern cliff mass");
            AddPlacement(placements, "obstacle_cliff_blocker_chunk_02", 29.4f, 39.8f, -20f, 0.98f, "eastern boulder gate");
            AddPlacement(placements, "obstacle_cliff_blocker_chunk_03", 28.6f, 42.2f, 18f, 0.88f, "eastern boulder gate");
            AddPlacement(placements, "obstacle_rock_cluster_01", 24.0f, 34.6f, 25f, 0.84f, "scattered boulder detail");
            AddPlacement(placements, "obstacle_rock_cluster_02", 25.8f, 36.2f, -12f, 0.76f, "scattered boulder detail");
            AddPlacement(placements, "obstacle_rock_cluster_03", 6.0f, 59.0f, 18f, 0.92f, "northern edge boulders");
            AddPlacement(placements, "obstacle_rock_cluster_04", 9.2f, 60.0f, -8f, 0.82f, "northern edge boulders");
            AddPlacement(placements, "obstacle_tree_bush_cluster_01", 13.0f, 36.4f, 16f, 0.80f, "northern vegetation pocket");
            AddPlacement(placements, "obstacle_tree_bush_cluster_02", 15.5f, 37.0f, -20f, 0.74f, "northern vegetation pocket");

            AddPlacement(placements, "ground_road_path_01", 17.8f, 32.4f, 88f, 1.18f, "vertical road spine");
            AddPlacement(placements, "ground_road_path_02", 18.0f, 36.6f, 90f, 1.08f, "vertical road spine");
            AddPlacement(placements, "ground_road_path_01", 18.0f, 40.8f, 90f, 1.08f, "vertical road spine");
            AddPlacement(placements, "ground_road_path_02", 18.0f, 44.6f, 90f, 1.02f, "vertical road spine");
            AddPlacement(placements, "transition_dirt_road_blend_01", 15.3f, 45.4f, 0f, 1.0f, "crossroad shoulder");
            AddPlacement(placements, "transition_dirt_road_blend_02", 20.7f, 45.5f, 0f, 0.96f, "crossroad shoulder");
            AddPlacement(placements, "prop_tire_tracks_01", 16.8f, 38.7f, 86f, 1.10f, "northbound vehicle tracks");
            AddPlacement(placements, "prop_tire_tracks_02", 19.4f, 41.4f, 88f, 1.02f, "northbound vehicle tracks");

            AddPlacement(placements, "base_foundation_pad_04", 6.6f, 48.0f, 0f, 0.92f, "abandoned outpost pad");
            AddPlacement(placements, "ground_concrete_pad_01", 9.9f, 49.0f, 0f, 0.88f, "abandoned outpost pad");
            AddPlacement(placements, "ground_concrete_pad_02", 13.0f, 49.1f, 0f, 0.82f, "abandoned outpost pad");
            AddPlacement(placements, "base_production_apron_01", 11.4f, 51.5f, 90f, 0.78f, "abandoned production apron");
            AddPlacement(placements, "base_footprint_decal_01", 7.1f, 51.2f, 0f, 0.78f, "outpost footprint");
            AddPlacement(placements, "base_footprint_decal_02", 14.0f, 51.4f, 0f, 0.72f, "outpost footprint");
            AddPlacement(placements, "prop_crates_01", 5.2f, 50.4f, 12f, 0.70f, "outpost logistics");
            AddPlacement(placements, "prop_crates_02", 6.2f, 51.6f, -8f, 0.62f, "outpost logistics");
            AddPlacement(placements, "prop_antenna_beacon_01", 12.6f, 50.8f, 0f, 0.80f, "outpost beacon");
            AddPlacement(placements, "prop_barrier_01", 4.6f, 48.2f, 90f, 0.82f, "outpost perimeter");
            AddPlacement(placements, "prop_barrier_02", 15.4f, 48.2f, 90f, 0.72f, "outpost perimeter");
            AddPlacement(placements, "prop_sandbag_01", 8.2f, 52.7f, 0f, 0.70f, "outpost defense");
            AddPlacement(placements, "prop_sandbag_02", 9.6f, 52.8f, 0f, 0.70f, "outpost defense");
            AddPlacement(placements, "prop_tank_trap_01", 16.6f, 52.2f, 20f, 0.78f, "outpost blocker");
            AddPlacement(placements, "prop_tank_trap_02", 17.6f, 53.2f, -18f, 0.68f, "outpost blocker");

            AddPlacement(placements, "resource_decal_01", 23.4f, 46.5f, 0f, 1.16f, "northern resource field");
            AddPlacement(placements, "resource_decal_02", 27.2f, 47.0f, 12f, 1.06f, "northern resource field");
            AddPlacement(placements, "resource_cluster_01", 24.7f, 46.3f, 15f, 0.95f, "northern resource cluster");
            AddPlacement(placements, "resource_cluster_02", 25.8f, 47.7f, -20f, 0.92f, "northern resource cluster");
            AddPlacement(placements, "resource_cluster_03", 27.0f, 46.5f, 28f, 0.84f, "northern resource cluster");
            AddPlacement(placements, "resource_rich_cluster_01", 28.0f, 48.1f, -32f, 0.78f, "northern resource cluster");
            AddPlacement(placements, "resource_harvest_marker_01", 22.4f, 47.2f, 90f, 0.70f, "resource route marker");

            AddPlacement(placements, "obstacle_wreckage_01", 21.5f, 56.5f, -32f, 0.84f, "enemy approach wreck");
            AddPlacement(placements, "obstacle_wreckage_02", 24.2f, 57.4f, 20f, 0.76f, "enemy approach wreck");
            AddPlacement(placements, "obstacle_debris_01", 22.8f, 58.4f, 5f, 0.74f, "enemy approach debris");
            AddPlacement(placements, "obstacle_debris_02", 25.6f, 59.0f, -16f, 0.68f, "enemy approach debris");
            AddPlacement(placements, "prop_destroyed_vehicle_proxy_01", 20.2f, 60.3f, -35f, 0.78f, "destroyed vehicle story prop");
            AddPlacement(placements, "prop_destroyed_vehicle_proxy_02", 26.8f, 60.6f, 28f, 0.70f, "destroyed vehicle story prop");
            AddPlacement(placements, "prop_shell_mark_01", 18.6f, 55.3f, 16f, 0.72f, "shell mark detail");
            AddPlacement(placements, "prop_shell_mark_02", 27.2f, 54.8f, -10f, 0.64f, "shell mark detail");
        }

        static List<TerrainSetDressingPlacement> BuildImportedSourceArtPlacements()
        {
            var placements = new List<TerrainSetDressingPlacement>();
            AddSourcePlacement(placements, "base_foundation_pad_01", 4.2f, 6.8f, 0f, 1.18f, "starting base pad");
            AddSourcePlacement(placements, "base_foundation_pad_02", 7.4f, 6.7f, 0f, 1.05f, "starting base pad");
            AddSourcePlacement(placements, "base_foundation_pad_03", 10.7f, 6.9f, 0f, 1.10f, "starting base pad");
            AddSourcePlacement(placements, "base_production_apron_01", 12.6f, 8.2f, 90f, 1.05f, "production exit apron");
            AddSourcePlacement(placements, "base_production_apron_02", 14.8f, 8.2f, 90f, 0.95f, "production exit apron");
            AddSourcePlacement(placements, "base_road_strip_01", 15.4f, 10.4f, 0f, 1.10f, "scouting route guide");
            AddSourcePlacement(placements, "base_road_strip_02", 18.0f, 11.8f, 0f, 1.00f, "scouting route guide");
            AddSourcePlacement(placements, "base_rally_exit_marking_01", 13.5f, 9.6f, 90f, 0.90f, "rally lane");
            AddSourcePlacement(placements, "base_rally_exit_marking_02", 16.4f, 9.6f, 90f, 0.90f, "rally lane");
            AddSourcePlacement(placements, "ground_compact_soil_patch_01", 5.3f, 10.0f, 15f, 1.10f, "base worn ground");
            AddSourcePlacement(placements, "ground_compact_soil_patch_02", 8.8f, 10.6f, -10f, 1.00f, "base worn ground");
            AddSourcePlacement(placements, "ground_scorched_patch_01", 3.1f, 13.6f, 28f, 0.90f, "battle scar edge");
            AddSourcePlacement(placements, "ground_mud_patch_01", 19.4f, 14.4f, -18f, 0.90f, "route texture");
            AddSourcePlacement(placements, "transition_concrete_ground_edge_01", 6.0f, 8.8f, 0f, 1.0f, "base edge blend");
            AddSourcePlacement(placements, "transition_buildable_edge_01", 11.5f, 5.2f, 0f, 0.95f, "build zone edge");
            AddSourcePlacement(placements, "transition_dirt_road_blend_01", 17.1f, 10.9f, 0f, 1.0f, "road blend");
            AddSourcePlacement(placements, "resource_cluster_01", 24.5f, 16.5f, 20f, 1.0f, "resource decoration");
            AddSourcePlacement(placements, "resource_cluster_02", 25.8f, 17.7f, -12f, 0.92f, "resource decoration");
            AddSourcePlacement(placements, "resource_rich_cluster_01", 27.1f, 16.4f, 45f, 0.88f, "resource decoration");
            AddSourcePlacement(placements, "resource_decal_01", 24.8f, 18.6f, 0f, 1.20f, "resource ground tint");
            AddSourcePlacement(placements, "resource_harvest_marker_01", 22.8f, 15.9f, 90f, 0.80f, "harvest route marker");
            AddSourcePlacement(placements, "transition_resource_edge_01", 23.4f, 15.2f, 25f, 1.0f, "resource edge");
            AddSourcePlacement(placements, "obstacle_rock_cluster_01", 1.4f, 2.5f, 14f, 1.0f, "map edge obstacle");
            AddSourcePlacement(placements, "obstacle_rock_cluster_02", 29.2f, 3.6f, -30f, 1.0f, "map edge obstacle");
            AddSourcePlacement(placements, "obstacle_ridge_piece_01", 29.5f, 10.8f, 90f, 1.1f, "map edge ridge");
            AddSourcePlacement(placements, "obstacle_cliff_blocker_chunk_01", 2.0f, 19.2f, 32f, 0.95f, "map edge blocker");
            AddSourcePlacement(placements, "obstacle_tree_bush_cluster_01", 4.4f, 20.6f, -20f, 0.90f, "map edge foliage");
            AddSourcePlacement(placements, "obstacle_wreckage_01", 21.2f, 4.2f, -36f, 0.82f, "edge battlefield wreck");
            AddSourcePlacement(placements, "obstacle_debris_01", 22.7f, 5.1f, 15f, 0.80f, "edge battlefield debris");
            AddSourcePlacement(placements, "prop_sandbag_01", 12.0f, 14.1f, 0f, 0.75f, "base perimeter prop");
            AddSourcePlacement(placements, "prop_sandbag_02", 13.2f, 14.2f, 0f, 0.75f, "base perimeter prop");
            AddSourcePlacement(placements, "prop_barrier_01", 18.7f, 7.1f, 90f, 0.86f, "route edge prop");
            AddSourcePlacement(placements, "prop_tank_trap_01", 30.1f, 14.6f, 20f, 0.88f, "edge prop");
            AddSourcePlacement(placements, "prop_tire_tracks_01", 17.4f, 12.7f, 15f, 1.15f, "vehicle path texture");
            AddSourcePlacement(placements, "prop_tire_tracks_02", 19.6f, 13.5f, 15f, 1.0f, "vehicle path texture");
            AddSourcePlacement(placements, "prop_shell_mark_01", 21.4f, 13.7f, 12f, 0.80f, "battle scar");
            AddSourcePlacement(placements, "prop_crates_01", 9.6f, 4.3f, 0f, 0.75f, "base logistics prop");
            AddSourcePlacement(placements, "prop_antenna_beacon_01", 6.5f, 4.0f, 0f, 0.82f, "base beacon prop");
            AddSourcePlacement(placements, "prop_destroyed_vehicle_proxy_01", 27.4f, 7.2f, -45f, 0.84f, "edge wreck prop");
            AddSourcePlacement(placements, "transition_rock_edge_01", 28.4f, 5.3f, 35f, 1.0f, "rock edge blend");
            AddSourcePlacement(placements, "ground_road_path_01", 20.7f, 12.9f, 8f, 1.05f, "route texture");
            AddSourcePlacement(placements, "ground_grass_dirt_patch_01", 2.4f, 9.4f, -18f, 1.1f, "field variation");
            AddSourcePlacement(placements, "ground_grass_dirt_patch_02", 27.6f, 20.5f, 20f, 1.0f, "field variation");
            AddSourcePlacement(placements, "ground_rocky_blocked_01", 30.0f, 19.2f, -15f, 0.95f, "edge blocked visual");
            return placements;
        }

        static void AddSourcePlacement(List<TerrainSetDressingPlacement> placements, string pieceId, float x, float z, float rotationY, float scale, string role)
        {
            if (!Stage32TerrainArtIngestionGenerator.IsSourceBackedPlayerFacingPiece(pieceId))
                return;

            AddPlacement(placements, pieceId, x, z, rotationY, scale, role);
        }

        static void AddPlacement(List<TerrainSetDressingPlacement> placements, string pieceId, float x, float z, float rotationY, float scale, string role)
        {
            placements.Add(new TerrainSetDressingPlacement
            {
                pieceId = pieceId,
                localPosition = new Vector3(x, 0.018f, z),
                rotationY = rotationY,
                uniformScale = scale,
                tint = Color.white,
                placementRole = role
            });
        }

        static Material AccentMaterialFor(Stage32PieceSpec spec, Dictionary<string, Material> materials)
        {
            if (spec.Category == TerrainPieceCategory.BaseConstruction)
                return MaterialFor(materials, spec.Id.IndexOf("rally", StringComparison.OrdinalIgnoreCase) >= 0 || spec.Id.IndexOf("footprint", StringComparison.OrdinalIgnoreCase) >= 0 ? "caution" : "concrete");
            if (spec.Category == TerrainPieceCategory.Transition)
                return MaterialFor(materials, "compact_soil");
            if (spec.MaterialProfileId == "water")
                return MaterialFor(materials, "grass_dirt");
            return MaterialFor(materials, "compact_soil");
        }

        static Material MaterialFor(Dictionary<string, Material> materials, string id)
        {
            Material material;
            return !string.IsNullOrEmpty(id) && materials.TryGetValue(id, out material) ? material : materials["grass_dirt"];
        }

        static Material CreateMaterial(string path, Color color, float smoothness, float metallic)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                    shader = Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            BattlefieldMaterialLibrary.ApplyMaterialProperties(material, color, smoothness, metallic);
            EditorUtility.SetDirty(material);
            return material;
        }

        static string PrefabPath(Stage32PieceSpec spec)
        {
            return PrefabRoot + "/" + spec.Folder + "/" + spec.Id + ".prefab";
        }

        static string DefinitionPath(Stage32PieceSpec spec)
        {
            return DefinitionRoot + "/" + spec.Folder + "/" + spec.Id + ".asset";
        }

        static void EnsureFolders()
        {
            EnsureFolder("Assets/Rts/Art/Materials", "TerrainPieces");
            EnsureFolder("Assets/Rts/Art/Prefabs", "TerrainPieces");
            EnsureFolder("Assets/Rts/ScriptableObjects/Art", "TerrainPieces");
            EnsureFolder(LibraryFolder, "Definitions");
            EnsureFolder(PrefabRoot, "Ground");
            EnsureFolder(PrefabRoot, "Transitions");
            EnsureFolder(PrefabRoot, "Base");
            EnsureFolder(PrefabRoot, "Obstacles");
            EnsureFolder(PrefabRoot, "Resources");
            EnsureFolder(PrefabRoot, "Props");
            EnsureFolder(DefinitionRoot, "Ground");
            EnsureFolder(DefinitionRoot, "Transitions");
            EnsureFolder(DefinitionRoot, "Base");
            EnsureFolder(DefinitionRoot, "Obstacles");
            EnsureFolder(DefinitionRoot, "Resources");
            EnsureFolder(DefinitionRoot, "Props");
        }

        static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }

        static T GetOrAdd<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        sealed class Stage32PieceSpec
        {
            public string Id;
            public string DisplayName;
            public TerrainPieceCategory Category;
            public string Folder;
            public TerrainPieceSizeClass SizeClass;
            public int FootprintFineWidth;
            public int FootprintFineHeight;
            public string MaterialProfileId;
            public string PassabilityHint;
            public string BuildableHint;
            public bool SupportsRotation;
            public bool SupportsTint;
            public bool BlockingVisualOnly;
            public string Notes;
            public int Variant;
            public string QuestBudgetTag;
        }
    }

    public sealed class Stage32TerrainPieceGenerationSummary
    {
        public int PieceCount;
        public int GroundCount;
        public int TransitionCount;
        public int BaseCount;
        public int ObstacleCount;
        public int ResourceCount;
        public int PropCount;
    }
}
