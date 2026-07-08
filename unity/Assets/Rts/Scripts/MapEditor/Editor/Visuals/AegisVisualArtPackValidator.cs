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
            "Terrain/gravel_path_albedo.png",
            "Terrain/muddy_bank_albedo.png",
            "Terrain/shallow_water_albedo.png",
            "Terrain/deep_water_albedo.png",
            "Terrain/cliff_ground_albedo.png",
            "Terrain/rough_ground_albedo.png",
            "Terrain/ore_stained_soil_albedo.png",
            "Terrain/concrete_base_pad_albedo.png",
            "Terrain/concrete_panel_albedo.png",
            "Terrain/concrete_trim_albedo.png",
            "Decals/Roads/soft_dust_overlay.png",
            "Decals/Roads/tire_rut_left.png",
            "Decals/Roads/tire_rut_right.png",
            "Decals/River/muddy_shoreline_01.png",
            "Decals/River/muddy_shoreline_02.png",
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
            "bridge.grime"
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

            if (!AssetDatabase.IsValidFolder(AegisMapArtPack.Root))
                errors.Add("Missing art-pack root: " + AegisMapArtPack.Root);

            for (var i = 0; i < RequiredFiles.Length; i++)
                RequireFile(RequiredFiles[i], errors);

            RequireMeshes(AegisMapArtPack.CliffMeshes, errors);
            RequireMeshes(AegisMapArtPack.BoulderMeshes, errors);
            RequireMeshes(AegisMapArtPack.PebbleMeshes, errors);
            RequireMeshes(AegisMapArtPack.OreMeshes, errors);
            RequireMeshes(AegisMapArtPack.CrystalMeshes, errors);
            RequireMeshes(AegisMapArtPack.SalvageMeshes, errors);
            RequireMeshes(AegisMapArtPack.EnergyMeshes, errors);
            RequireMeshes(AegisMapArtPack.VegetationMeshes, errors);
            RequireMeshes(AegisMapArtPack.RiverMeshes, errors);
            RequireMeshes(AegisMapArtPack.CraterMeshes, errors);

            ValidateTheme(AegisBiomeVisualTheme.ForestPrototypeVisualTheme(), "forest", errors);
            ValidateTheme(AegisBiomeVisualTheme.DesertPrototypeVisualTheme(), "desert", errors);

            var importedPrefabInstances = ValidateCompilerSample(errors, warnings);

            if (errors.Count > 0)
                throw new InvalidOperationException("Aegis visual art-pack validation failed:\n- " + string.Join("\n- ", errors.ToArray()));

            var report = "Aegis visual art-pack validation passed." +
                "\nRoot: " + AegisMapArtPack.Root +
                "\nRequired files checked: " + RequiredFiles.Length +
                "\nTheme roles checked: " + RequiredThemeRoles.Length +
                "\nImported art-pack prefab instances in sample compile: " + importedPrefabInstances;
            if (warnings.Count > 0)
                report += "\nWarnings:\n- " + string.Join("\n- ", warnings.ToArray());
            return report;
        }

        static int ValidateCompilerSample(List<string> errors, List<string> warnings)
        {
            var samplePath = AegisMapEditorPaths.SamplesFolder + "/sample_art_pack_showcase_160_forest_river.aegismap.json";
            var document = AegisVisualMapDocument.Load(samplePath);
            if (document == null)
            {
                errors.Add("Could not load compiler sample: " + samplePath);
                return 0;
            }

            var result = AegisMapVisualCompiler.CompileDocument(document, samplePath, false);
            var importedPrefabInstances = CountArtPackPrefabInstances(result.Root);
            if (importedPrefabInstances <= 0)
                errors.Add("Visual compiler sample did not instantiate any imported art-pack prefabs; it appears to be fallback-only.");

            if (result.TotalScatterCount <= 0)
                warnings.Add("Visual compiler sample produced no scatter count.");

            UnityEngine.Object.DestroyImmediate(result.Root);
            return importedPrefabInstances;
        }

        static int CountArtPackPrefabInstances(GameObject root)
        {
            if (root == null)
                return 0;

            var count = 0;
            var transforms = root.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(transforms[i].gameObject);
                if (!string.IsNullOrEmpty(path) && path.StartsWith(AegisMapArtPack.Root, StringComparison.OrdinalIgnoreCase))
                    count++;
            }

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

        static void RequireMeshes(string[] relativePaths, List<string> errors)
        {
            for (var i = 0; i < relativePaths.Length; i++)
            {
                RequireFile(relativePaths[i], errors);
                var assetPath = AegisMapArtPack.Root + "/" + relativePaths[i];
                if (AssetDatabase.LoadAssetAtPath<GameObject>(assetPath) == null)
                    errors.Add("Mesh did not import as a GameObject: " + assetPath);
            }
        }

        static void RequireFile(string relativePath, List<string> errors)
        {
            var assetPath = AegisMapArtPack.Root + "/" + relativePath.Replace('\\', '/');
            var diskPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
            if (!File.Exists(diskPath))
                errors.Add("Missing file: " + assetPath);
        }
    }
}
#endif
