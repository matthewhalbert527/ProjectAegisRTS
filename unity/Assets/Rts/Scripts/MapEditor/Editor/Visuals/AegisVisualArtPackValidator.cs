#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using ProjectAegisRTS.UnityClient.MapEditor;
using ProjectAegisRTS.UnityClient.MapEditor.Visuals;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public sealed class AegisArtPackValidationReport
    {
        public bool ArtPackRootExists;
        public bool ManifestExists;
        public bool SemanticMaterialsExists;
        public int RequiredFilesChecked;
        public int RequiredMeshFilesChecked;
        public int ImportedModelRootsFound;
        public int GeneratedProxyAssetsFoundOrCreated;
        public int ImportedArtPackInstanceCount;
        public int ArtPackDerivedProxyInstanceCount;
        public int GenericFallbackGeometryCount;
        public int ArtPackTexturedMaterialCount;

        public string ToDisplayString()
        {
            return "Root exists: " + ArtPackRootExists +
                "\nManifest exists: " + ManifestExists +
                "\nSemantic materials exists: " + SemanticMaterialsExists +
                "\nRequired files checked: " + RequiredFilesChecked +
                "\nRequired mesh files checked: " + RequiredMeshFilesChecked +
                "\nImported model roots found: " + ImportedModelRootsFound +
                "\nGenerated art-pack proxy assets found/created: " + GeneratedProxyAssetsFoundOrCreated +
                "\nImported art-pack mesh instances in sample compile: " + ImportedArtPackInstanceCount +
                "\nArt-pack-derived proxy instances in sample compile: " + ArtPackDerivedProxyInstanceCount +
                "\nGeneric fallback geometry instances in sample compile: " + GenericFallbackGeometryCount +
                "\nArt-pack textured materials in sample compile: " + ArtPackTexturedMaterialCount;
        }
    }

    public static class AegisVisualArtPackValidator
    {
        static readonly string[] RequiredFiles =
        {
            "manifest.json",
            "Materials/semantic_materials.json",
            "Terrain/forest_grass_albedo.png",
            "Terrain/forest_grass_normal.png",
            "Terrain/forest_grass_roughness_ao.png",
            "Terrain/forest_grass_dark_patch_albedo.png",
            "Terrain/dirt_path_albedo.png",
            "Terrain/road_compacted_albedo.png",
            "Terrain/road_compacted_normal.png",
            "Terrain/road_compacted_roughness_ao.png",
            "Terrain/gravel_path_albedo.png",
            "Terrain/muddy_bank_albedo.png",
            "Terrain/shallow_water_albedo.png",
            "Terrain/deep_water_albedo.png",
            "Terrain/river_muddy_water_albedo.png",
            "Terrain/river_muddy_water_normal.png",
            "Terrain/river_muddy_water_roughness_ao.png",
            "Terrain/cliff_ground_albedo.png",
            "Terrain/rough_ground_albedo.png",
            "Terrain/ore_stained_soil_albedo.png",
            "Terrain/concrete_base_pad_albedo.png",
            "Terrain/concrete_panel_albedo.png",
            "Terrain/concrete_trim_albedo.png",
            "Terrain/bridge_weathered_deck_albedo.png",
            "Terrain/bridge_weathered_deck_normal.png",
            "Terrain/bridge_weathered_deck_roughness_ao.png",
            "Terrain/bridge_weathered_rail_albedo.png",
            "Terrain/bridge_weathered_rail_normal.png",
            "Terrain/bridge_weathered_rail_roughness_ao.png",
            "Decals/Roads/soft_dust_overlay.png",
            "Decals/Roads/tire_rut_left.png",
            "Decals/Roads/tire_rut_right.png",
            "Decals/River/muddy_shoreline_01.png",
            "Decals/River/muddy_shoreline_02.png",
            "Decals/River/shoreline_erosion_cut_01.png",
            "Decals/River/shoreline_wet_pebbles_01.png",
            "Decals/Resources/ore_dust_soft_01.png",
            "Decals/BasePads/concrete_grime_01.png",
            "Decals/BasePads/construction_wear_01.png",
            "Decals/Battlefield/scorch_mark_01.png",
            "Decals/Battlefield/rubble_scatter_01.png",
            "Decals/Battlefield/crater_medium_01.png",
            "Decals/Battlefield/crater_large_01.png",
            "Meshes/BasePads/base_pad_14x14.glb",
            "Meshes/BasePads/base_pad_trim_corner.glb",
            "Meshes/BasePads/base_pad_trim_straight.glb"
        };

        static readonly string[] RequiredThemeRoles =
        {
            "terrain.grass",
            "terrain.dark_grass",
            "terrain.dirt",
            "terrain.gravel",
            "terrain.mud",
            "terrain.shallow_water",
            "terrain.deep_water",
            "terrain.cliff_ground",
            "terrain.ore_stained_soil",
            "terrain.concrete_base_pad",
            "road.dirt",
            "road.gravel",
            "river.water",
            "river.shoreline",
            "river.shoreline_feather",
            "river.deep_pool",
            "river.silt_flow",
            "river.depth_edge",
            "river.shallow_edge",
            "river.ripple",
            "river.bank_erosion",
            "river.bank_pebbles",
            "cliff.edge.straight",
            "cliff.edge.corner_inner",
            "cliff.edge.corner_outer",
            "cliff.edge.endcap",
            "blocker.rock",
            "basepad.panel",
            "basepad.trim",
            "basepad.corner",
            "bridge.deck",
            "bridge.rail",
            "bridge.grime",
            "road.edge_grass",
            "road.pebble_breakup"
        };

        [MenuItem("Project Aegis/Map Editor/Validate Visual Art Pack")]
        public static void ValidateVisualArtPackMenu()
        {
            try
            {
                var report = ValidateVisualArtPack();
                EditorUtility.DisplayDialog("Aegis Visual Art Pack", report, "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Aegis Visual Art Pack", ex.Message, "OK");
                throw;
            }
        }

        public static void ValidateVisualArtPackForBatch()
        {
            Debug.Log(ValidateVisualArtPack());
        }

        public static string ValidateVisualArtPack()
        {
            AssetDatabase.Refresh();
            var errors = new List<string>();
            var warnings = new List<string>();
            var report = new AegisArtPackValidationReport();

            report.ArtPackRootExists = AssetDatabase.IsValidFolder(AegisMapArtPack.Root);
            if (!report.ArtPackRootExists)
                errors.Add("Missing art-pack root: " + AegisMapArtPack.Root);

            for (var i = 0; i < RequiredFiles.Length; i++)
            {
                report.RequiredFilesChecked++;
                var exists = RequireFile(RequiredFiles[i], errors);
                if (RequiredFiles[i] == "manifest.json")
                    report.ManifestExists = exists;
                if (RequiredFiles[i] == "Materials/semantic_materials.json")
                    report.SemanticMaterialsExists = exists;
            }

            RequireMeshes(AegisMapArtPack.CliffMeshes, errors, warnings, report);
            RequireMeshes(AegisMapArtPack.BoulderMeshes, errors, warnings, report);
            RequireMeshes(AegisMapArtPack.PebbleMeshes, errors, warnings, report);
            RequireMeshes(AegisMapArtPack.OreMeshes, errors, warnings, report);
            RequireMeshes(AegisMapArtPack.CrystalMeshes, errors, warnings, report);
            RequireMeshes(AegisMapArtPack.SalvageMeshes, errors, warnings, report);
            RequireMeshes(AegisMapArtPack.EnergyMeshes, errors, warnings, report);
            RequireMeshes(new[] { AegisMapArtPack.BasePadMesh, AegisMapArtPack.BasePadTrimStraightMesh, AegisMapArtPack.BasePadTrimCornerMesh }, errors, warnings, report);
            RequireMeshes(AegisMapArtPack.VegetationMeshes, errors, warnings, report);
            RequireMeshes(AegisMapArtPack.RiverMeshes, errors, warnings, report);
            RequireMeshes(AegisMapArtPack.CraterMeshes, errors, warnings, report);

            ValidateTheme(AegisBiomeVisualTheme.ForestPrototypeVisualTheme(), "forest", errors);
            ValidateTheme(AegisBiomeVisualTheme.DesertPrototypeVisualTheme(), "desert", errors);

            ValidateCompilerSample(errors, warnings, report);

            if (errors.Count > 0)
                throw new InvalidOperationException("Aegis visual art-pack validation failed:\n- " + string.Join("\n- ", errors.ToArray()) + "\n\n" + report.ToDisplayString());

            var message = "Aegis visual art-pack validation passed." +
                "\nRoot: " + AegisMapArtPack.Root +
                "\nTheme roles checked: " + RequiredThemeRoles.Length +
                "\n" + report.ToDisplayString();
            if (warnings.Count > 0)
                message += "\nWarnings:\n- " + string.Join("\n- ", warnings.ToArray());
            return message;
        }

        static void ValidateCompilerSample(List<string> errors, List<string> warnings, AegisArtPackValidationReport report)
        {
            var samplePath = AegisMapEditorPaths.SamplesFolder + "/sample_art_pack_showcase_160_forest_river.aegismap.json";
            var document = AegisVisualMapDocument.Load(samplePath);
            if (document == null)
            {
                errors.Add("Could not load compiler sample: " + samplePath);
                return;
            }

            var result = AegisMapVisualCompiler.CompileDocument(document, samplePath, false);
            int importedPrefabInstances;
            int artPackDerivedProxyInstances;
            AegisMapArtPack.CountInstances(result.Root, out importedPrefabInstances, out artPackDerivedProxyInstances);
            report.ImportedArtPackInstanceCount = importedPrefabInstances;
            report.ArtPackDerivedProxyInstanceCount = artPackDerivedProxyInstances;
            report.GenericFallbackGeometryCount = CountGenericFallbackInstances(result);
            report.ArtPackTexturedMaterialCount = AegisMapArtPack.CountArtPackTexturedMaterials(result.Root);

            if (importedPrefabInstances + artPackDerivedProxyInstances <= 0)
                errors.Add("Visual compiler sample did not instantiate imported art-pack prefabs or art-pack-derived proxy prefabs; it appears to be fallback-only.");

            if (report.ArtPackTexturedMaterialCount <= 0)
                errors.Add("Visual compiler sample did not resolve any art-pack terrain/material textures.");

            if (result.TotalScatterCount <= 0)
                warnings.Add("Visual compiler sample produced no scatter count.");

            UnityEngine.Object.DestroyImmediate(result.Root);
        }

        static int CountGenericFallbackInstances(AegisMapVisualCompileResult result)
        {
            if (result == null)
                return 0;

            var count = 0;
            for (var i = 0; i < result.Layers.Count; i++)
                if (result.Layers[i] != null)
                    count += result.Layers[i].GenericFallbackInstanceCount;
            return count;
        }

        static void ValidateTheme(AegisMapVisualTheme theme, string themeName, List<string> errors)
        {
            for (var i = 0; i < RequiredThemeRoles.Length; i++)
            {
                var role = RequiredThemeRoles[i];
                var rule = theme.RuleFor(role);
                if (rule == null)
                {
                    errors.Add("Theme " + themeName + " missing role: " + role);
                    continue;
                }

                if (string.IsNullOrEmpty(rule.AlbedoPath))
                {
                    errors.Add("Theme " + themeName + " role lacks texture paths: " + role);
                    continue;
                }

                RequireFile(rule.AlbedoPath, errors);

                if (AllowsAlbedoOnly(rule))
                    continue;

                if (string.IsNullOrEmpty(rule.NormalPath) || string.IsNullOrEmpty(rule.MaskPath))
                {
                    errors.Add("Theme " + themeName + " role lacks texture paths: " + role);
                    continue;
                }

                RequireFile(rule.NormalPath, errors);
                RequireFile(rule.MaskPath, errors);
            }
        }

        static bool AllowsAlbedoOnly(AegisVisualSemanticRule rule)
        {
            if (rule == null || string.IsNullOrEmpty(rule.SemanticRole))
                return false;

            var role = rule.SemanticRole;
            return rule.Transparent ||
                role.StartsWith("river.", StringComparison.OrdinalIgnoreCase) ||
                role.StartsWith("road.", StringComparison.OrdinalIgnoreCase) ||
                role.StartsWith("decal.", StringComparison.OrdinalIgnoreCase) ||
                role.StartsWith("terrain.blend_", StringComparison.OrdinalIgnoreCase) ||
                role.EndsWith("_mottle", StringComparison.OrdinalIgnoreCase) ||
                role.EndsWith("_detail", StringComparison.OrdinalIgnoreCase);
        }

        static void RequireMeshes(string[] relativePaths, List<string> errors, List<string> warnings, AegisArtPackValidationReport report)
        {
            for (var i = 0; i < relativePaths.Length; i++)
            {
                report.RequiredMeshFilesChecked++;
                if (!RequireFile(relativePaths[i], errors))
                    continue;

                var assetPath = AegisMapArtPack.Root + "/" + relativePaths[i];
                if (AssetDatabase.LoadAssetAtPath<GameObject>(assetPath) != null)
                {
                    report.ImportedModelRootsFound++;
                    continue;
                }

                var proxy = AegisMapArtPack.EnsureGeneratedProxy(relativePaths[i], null);
                if (proxy != null)
                {
                    report.GeneratedProxyAssetsFoundOrCreated++;
                    warnings.Add("Mesh did not import as a GameObject and will use an art-pack-derived proxy: " + assetPath);
                    continue;
                }

                errors.Add("Mesh did not import as a GameObject and no art-pack proxy could be generated: " + assetPath);
            }
        }

        static bool RequireFile(string relativePath, List<string> errors)
        {
            var assetPath = AegisMapArtPack.Root + "/" + relativePath.Replace('\\', '/');
            var diskPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
            if (File.Exists(diskPath))
                return true;

            if (errors != null)
                errors.Add("Missing file: " + assetPath);
            return false;
        }
    }
}
#endif
