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
    public static class AegisMapVisualQualityGate
    {
        [MenuItem("Project Aegis/Map Editor/Validate Visual Quality Gate")]
        public static void ValidateVisualQualityGateMenu()
        {
            try
            {
                EditorUtility.DisplayDialog("Aegis Visual Quality Gate", ValidateVisualQualityGate(), "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Aegis Visual Quality Gate", ex.Message, "OK");
                throw;
            }
        }

        public static void ValidateSampleForBatch()
        {
            Debug.Log(ValidateVisualQualityGate());
        }

        public static string ValidateVisualQualityGate()
        {
            AssetDatabase.Refresh();
            var errors = new List<string>();
            var warnings = new List<string>();

            var settings = AegisMapVisualCompileSettings.ProductionDefault();
            if (settings.RenderMode != AegisMapVisualRenderMode.ProductionPreview)
                errors.Add("Default visual mode is not ProductionPreview.");
            if (settings.Overlays == null || settings.Overlays.Terrain || settings.Overlays.Blockers || settings.Overlays.Resources || settings.Overlays.BuildPads || settings.Overlays.Cliffs || settings.Overlays.Pathability)
                errors.Add("Production default enables debug overlays.");

            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Assets/Rts/Maps/Generated/sample_small_100.local-export.tiled.json")))
                errors.Add("Temporary Tiled local export exists in Assets/Rts/Maps/Generated.");

            RequireAsset(AegisMapArtPack.Root + "/manifest.json", errors);
            RequireAsset(AegisMapArtPack.Root + "/Materials/semantic_materials.json", errors);

            var theme = AegisBiomeVisualTheme.ForestPrototypeVisualTheme();
            RequireTexturedRole(theme, "terrain.grass", errors);
            RequireTexturedRole(theme, "terrain.dirt", errors);
            RequireTexturedRole(theme, "terrain.shallow_water", errors);
            RequireTexturedRole(theme, "river.water", errors);
            RequireTexturedRole(theme, "river.shoreline", errors);
            RequireTexturedRole(theme, "river.shoreline_feather", errors);
            RequireTexturedRole(theme, "basepad.panel", errors);
            RequireTexturedRole(theme, "terrain.grass_mottle", errors);
            RequireTexturedRole(theme, "road.soft_dust", errors);
            RequireTexturedRole(theme, "basepad.panel_decal", errors);
            RequireTexturedRole(theme, "resource.ore_dust", errors);
            RequireTexturedRole(theme, "decal.scorch", errors);

            if (AegisTerrainLayerCompiler.ProductionChunkSize >= 16)
                errors.Add("Production terrain chunk size still uses old 16x16 behavior.");

            var samplePath = AegisMapEditorPaths.SamplesFolder + "/sample_art_pack_showcase_160_forest_river.aegismap.json";
            var document = AegisVisualMapDocument.Load(samplePath);
            if (document == null)
            {
                errors.Add("Could not load visual quality sample: " + samplePath);
            }
            else
            {
                var result = AegisMapVisualCompiler.CompileDocument(document, samplePath, false, theme, document.ReadSeed(), settings);
                ValidateResult(result, errors, warnings);
                UnityEngine.Object.DestroyImmediate(result.Root);
            }

            ValidateBridgeCrossingRule(theme, settings, errors);

            if (errors.Count > 0)
                throw new InvalidOperationException("Aegis visual quality gate failed:\n- " + string.Join("\n- ", errors.ToArray()));

            var report = "Aegis visual quality gate passed." +
                "\nDefault mode: ProductionPreview" +
                "\nProduction terrain chunk size: " + AegisTerrainLayerCompiler.ProductionChunkSize +
                "\nSample: " + samplePath;
            if (warnings.Count > 0)
                report += "\nWarnings:\n- " + string.Join("\n- ", warnings.ToArray());
            return report;
        }

        static void ValidateResult(AegisMapVisualCompileResult result, List<string> errors, List<string> warnings)
        {
            if (result == null || result.Root == null)
            {
                errors.Add("Visual compiler returned no production preview root.");
                return;
            }

            if (ContainsDebugLayer(result.Root.transform))
                errors.Add("Production preview contains a debug overlay layer.");

            var importedPrefabs = CountArtPackPrefabInstances(result.Root);
            if (importedPrefabs <= 0)
                errors.Add("Sample compiled with fallback-only visuals.");

            var terrain = FindLayer(result, "Production Terrain Surface");
            if (terrain == null)
                errors.Add("Production terrain layer missing.");
            else if (terrain.TerrainChunks <= 0)
                errors.Add("Production terrain layer produced no chunks.");

            var terrainDetails = FindLayer(result, "Production Terrain Detail Decals");
            if (terrainDetails == null)
                errors.Add("Production terrain detail decal layer missing.");
            else if (terrainDetails.TerrainDetailDecalCount < 180)
                errors.Add("Production terrain detail decal layer produced too few detail decals.");

            var transitions = FindLayer(result, "Terrain Transition Masks");
            if (transitions == null)
                errors.Add("Terrain transition layer missing.");
            else if (transitions.TransitionEdges <= 0)
                errors.Add("Terrain transition layer produced no blend edges.");
            else if (transitions.OrganicTransitionMeshCount <= 0)
                errors.Add("Terrain transition layer did not produce organic feather meshes.");

            var water = FindLayer(result, "Water And Shoreline");
            if (water != null && water.WaterCells > 0 && water.WaterStrips <= 0)
                errors.Add("Water cells were present but no merged water strips were produced.");
            if (water != null && water.WaterCells > 0 && water.WaterMeshes <= 0)
                errors.Add("Water cells were present but no production water ribbon mesh was produced.");
            if (water != null && water.WaterCells > 0 && water.ShorelineMeshes <= 0)
                errors.Add("Water cells were present but no production shoreline bank mesh was produced.");

            var road = FindLayer(result, "Roads And Tire Tracks");
            if (road != null && road.RoadWaterConflicts > 0)
                errors.Add("Road-water conflicts were reported in production preview.");
            if (road != null && road.RoadSegments > 0 && road.RoadDetailDecalCount <= 0)
                errors.Add("Roads were present but no road detail decals were produced.");
            if (road != null && road.BridgeCrossings == 0 && water != null && water.WaterCells > 0)
                warnings.Add("Sample has water but no bridge crossing was needed; bridge rule remains covered by compiler logic, not this sample layout.");

            var resources = FindLayer(result, "Resource Field Visuals");
            if (resources != null && resources.ResourceGlintCount > resources.ResourceFields * 4)
                errors.Add("Resource glint count exceeds the production cap.");

            var basePads = FindLayer(result, "Modular Base Pads");
            if (basePads != null && basePads.Warnings.Count > 0)
                warnings.AddRange(basePads.Warnings);
            if (basePads != null && basePads.BasePadCount > 0 && basePads.BasePadDetailDecalCount <= basePads.BasePadCount * 4)
                errors.Add("Base pads produced too few detail decals for production preview.");
        }

        static void ValidateBridgeCrossingRule(AegisMapVisualTheme theme, AegisMapVisualCompileSettings settings, List<string> errors)
        {
            var terrain = new List<AegisVisualTerrainCell>();
            for (var y = 0; y < 40; y++)
            {
                for (var x = 18; x <= 21; x++)
                {
                    terrain.Add(new AegisVisualTerrainCell
                    {
                        x = x,
                        y = y,
                        terrainId = "water"
                    });
                }
            }

            var bridgeDocument = new AegisVisualMapDocument
            {
                formatVersion = "visual-quality-gate",
                mapId = "visual_quality_bridge_crossing",
                displayName = "Visual Quality Bridge Crossing",
                width = 40,
                height = 40,
                defaultTerrainId = "clear",
                terrainBase = terrain.ToArray(),
                playerStarts = new[]
                {
                    new AegisVisualPlayerStart { playerId = 1, x = 7, y = 20, name = "West" },
                    new AegisVisualPlayerStart { playerId = 2, x = 33, y = 20, name = "East" }
                }
            };

            var result = AegisMapVisualCompiler.CompileDocument(bridgeDocument, "synthetic_visual_quality_bridge.aegismap.json", false, theme, 7001, settings);
            try
            {
                var water = FindLayer(result, "Water And Shoreline");
                if (water == null || water.WaterStrips <= 0)
                    errors.Add("Synthetic bridge sample did not produce merged water strips.");

                var road = FindLayer(result, "Roads And Tire Tracks");
                if (road == null)
                {
                    errors.Add("Synthetic bridge sample did not produce a road layer.");
                    return;
                }

                if (road.RoadWaterConflicts > 0)
                    errors.Add("Synthetic bridge sample reported road-water conflicts.");
                if (road.BridgeCrossings <= 0)
                    errors.Add("Synthetic bridge sample did not produce a bridge crossing.");
            }
            finally
            {
                if (result != null && result.Root != null)
                    UnityEngine.Object.DestroyImmediate(result.Root);
            }
        }

        static AegisVisualLayerSummary FindLayer(AegisMapVisualCompileResult result, string name)
        {
            for (var i = 0; i < result.Layers.Count; i++)
                if (string.Equals(result.Layers[i].LayerName, name, StringComparison.OrdinalIgnoreCase))
                    return result.Layers[i];
            return null;
        }

        static bool ContainsDebugLayer(Transform root)
        {
            var transforms = root.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
                if (transforms[i] != null && transforms[i].name.IndexOf("debug", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            return false;
        }

        static int CountArtPackPrefabInstances(GameObject root)
        {
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

        static void RequireTexturedRole(AegisMapVisualTheme theme, string role, List<string> issues)
        {
            var rule = theme.RuleFor(role);
            if (rule == null)
            {
                issues.Add("Theme missing role: " + role);
                return;
            }

            if (string.IsNullOrEmpty(rule.AlbedoPath))
            {
                issues.Add("Theme role lacks albedo path: " + role);
                return;
            }

            RequireAsset(AegisMapArtPack.Root + "/" + rule.AlbedoPath, issues);
        }

        static void RequireAsset(string assetPath, List<string> issues)
        {
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), assetPath)))
                issues.Add("Missing asset: " + assetPath);
        }
    }
}
#endif
