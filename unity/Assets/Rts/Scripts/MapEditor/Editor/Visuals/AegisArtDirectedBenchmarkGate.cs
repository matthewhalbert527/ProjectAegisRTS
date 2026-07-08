#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using ProjectAegisRTS.UnityClient.MapEditor;
using ProjectAegisRTS.UnityClient.MapEditor.Visuals;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class AegisArtDirectedBenchmarkGate
    {
        const string SamplePath = "Assets/Rts/MapEditor/Samples/sample_art_directed_forest_river_2p.aegismap.json";
        const string VisualMetadataPath = "Assets/Rts/MapEditor/Samples/sample_art_directed_forest_river_2p.visual.json";

        [MenuItem("Project Aegis/Map Editor/Validate Art-Directed Benchmark")]
        public static void ValidateArtDirectedBenchmarkMenu()
        {
            try
            {
                EditorUtility.DisplayDialog("Aegis Art-Directed Benchmark", ValidateArtDirectedBenchmark(), "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Aegis Art-Directed Benchmark", ex.Message, "OK");
                throw;
            }
        }

        [MenuItem("Project Aegis/Map Editor/Capture Art-Directed Preview")]
        public static void CaptureArtDirectedPreviewMenu()
        {
            try
            {
                var path = CaptureArtDirectedPreview();
                EditorUtility.DisplayDialog("Aegis Art-Directed Preview", "Captured benchmark preview:\n" + path, "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Aegis Art-Directed Preview", ex.Message, "OK");
                throw;
            }
        }

        public static void ValidateForBatch()
        {
            Debug.Log(ValidateArtDirectedBenchmark());
        }

        public static void CaptureForBatch()
        {
            Debug.Log("Aegis art-directed benchmark preview: " + CaptureArtDirectedPreview());
        }

        public static string ValidateArtDirectedBenchmark()
        {
            AssetDatabase.Refresh();
            var errors = new List<string>();
            var warnings = new List<string>();

            if (!File.Exists(ProjectPath(SamplePath)))
                errors.Add("Missing art-directed benchmark map: " + SamplePath);
            if (!File.Exists(ProjectPath(VisualMetadataPath)))
                errors.Add("Missing art-directed benchmark visual metadata: " + VisualMetadataPath);
            if (File.Exists(ProjectPath("Assets/Rts/Maps/Generated/sample_small_100.local-export.tiled.json")))
                errors.Add("Temporary Tiled local export exists in Assets/Rts/Maps/Generated.");

            if (errors.Count > 0)
                throw new InvalidOperationException("Aegis art-directed benchmark failed:\n- " + string.Join("\n- ", errors.ToArray()));

            var document = AegisVisualMapDocument.Load(SamplePath);
            var metadata = LoadMetadata(VisualMetadataPath);
            ValidateAuthoringData(document, metadata, errors);

            var settings = AegisMapVisualCompileSettings.ProductionDefault();
            var theme = AegisBiomeVisualTheme.ForestPrototypeVisualTheme();
            var result = AegisMapVisualCompiler.CompileDocument(document, SamplePath, false, theme, document.ReadSeed(), settings);
            try
            {
                ValidateCompiledResult(result, errors, warnings);
            }
            finally
            {
                if (result != null && result.Root != null)
                    Object.DestroyImmediate(result.Root);
            }

            if (errors.Count > 0)
                throw new InvalidOperationException("Aegis art-directed benchmark failed:\n- " + string.Join("\n- ", errors.ToArray()));

            var report = "Aegis art-directed benchmark passed." +
                "\nSample: " + SamplePath +
                "\nVisual metadata: " + VisualMetadataPath +
                "\nMode: ProductionPreview" +
                "\nRoad source: authored benchmark metadata" +
                "\nCrossings: one authored bridge/fording route";
            if (warnings.Count > 0)
                report += "\nWarnings:\n- " + string.Join("\n- ", warnings.ToArray());
            return report;
        }

        public static string CaptureArtDirectedPreview()
        {
            AssetDatabase.Refresh();
            var document = AegisVisualMapDocument.Load(SamplePath);
            var settings = AegisMapVisualCompileSettings.ProductionDefault();
            var theme = AegisBiomeVisualTheme.ForestPrototypeVisualTheme();
            var result = AegisMapVisualCompiler.CompileDocument(document, SamplePath, false, theme, document.ReadSeed(), settings);
            try
            {
                var outputDir = Path.Combine(Path.GetTempPath(), "ProjectAegisRTS", "ArtDirectedPreviews");
                return AegisMapPreviewCaptureTool.CapturePreview(
                    result.Root,
                    document,
                    "sample_art_directed_forest_river_2p.png",
                    1800,
                    1200,
                    0.30f,
                    outputDir);
            }
            finally
            {
                if (result != null && result.Root != null)
                    Object.DestroyImmediate(result.Root);
            }
        }

        static void ValidateAuthoringData(AegisVisualMapDocument document, AegisArtDirectedVisualMetadata metadata, List<string> errors)
        {
            if (document == null)
            {
                errors.Add("Benchmark document could not be loaded.");
                return;
            }

            document.Normalize();
            if (document.width != 100 || document.height != 100)
                errors.Add("Benchmark must be exactly 100x100; found " + document.width + "x" + document.height + ".");
            if (document.playerStarts == null || document.playerStarts.Length != 2)
                errors.Add("Benchmark must contain exactly two player starts.");
            if (!string.Equals(document.mapId, "sample_art_directed_forest_river_2p", StringComparison.OrdinalIgnoreCase))
                errors.Add("Benchmark mapId should be sample_art_directed_forest_river_2p.");

            if (metadata == null)
            {
                errors.Add("Benchmark visual metadata could not be loaded.");
                return;
            }

            metadata.Normalize();
            if (!metadata.HasAuthoredRoads)
                errors.Add("Benchmark metadata must define authored road segments.");
            if (metadata.crossings.Length != 1)
                errors.Add("Benchmark metadata must define exactly one bridge/fording crossing.");
            else if (!metadata.crossings[0].IsBridgeOrFord)
                errors.Add("Benchmark crossing must be marked bridge/fording.");

            var water = CollectWaterCells(document);
            var roadWaterSegments = 0;
            for (var i = 0; i < metadata.roadSegments.Length; i++)
            {
                var segment = metadata.roadSegments[i];
                if (segment == null)
                    continue;

                var touchesWater = SegmentTouchesWater(water, document.width, document.height, segment.A, segment.B);
                if (touchesWater)
                    roadWaterSegments++;
                if (touchesWater && !segment.IsBridgeOrFord)
                    errors.Add("Road segment crosses water without bridge/fording metadata: " + segment.id);
            }

            if (roadWaterSegments != 1)
                errors.Add("Benchmark must have exactly one road segment crossing water; found " + roadWaterSegments + ".");

            if (document.resources != null)
            {
                for (var i = 0; i < document.resources.Length; i++)
                {
                    var resource = document.resources[i];
                    if (resource == null)
                        continue;

                    if (water.Contains(Key(resource.x, resource.y, document.width)))
                        errors.Add("Resource field overlaps water at " + resource.x + "," + resource.y + ".");
                    if (IsInsideBasePad(document, resource.x, resource.y, 8f))
                        errors.Add("Resource field overlaps a base pad at " + resource.x + "," + resource.y + ".");
                    if (IsNearAuthoredRoad(metadata, resource.x, resource.y, 1.7f))
                        errors.Add("Resource field is too close to authored road/crossing at " + resource.x + "," + resource.y + ".");
                }
            }
        }

        static void ValidateCompiledResult(AegisMapVisualCompileResult result, List<string> errors, List<string> warnings)
        {
            if (result == null || result.Root == null)
            {
                errors.Add("Benchmark compiler returned no root object.");
                return;
            }

            if (ContainsName(result.Root, "debug"))
                errors.Add("Production benchmark output contains debug-named objects.");

            var road = FindLayer(result, "Roads And Tire Tracks");
            if (road == null)
            {
                errors.Add("Road layer missing.");
            }
            else
            {
                if (road.AuthoredRoadSegments <= 0)
                    errors.Add("Benchmark did not use authored road metadata.");
                if (road.FallbackRoadSegments > 0 || road.GeneratedRoadFallbacksUsed > 0)
                    errors.Add("Benchmark used generated fallback roads instead of authored composition.");
                if (road.AuthoredCrossings != 1)
                    errors.Add("Benchmark should report one authored crossing; found " + road.AuthoredCrossings + ".");
                if (road.BridgeCrossings + road.FordCrossings != 1)
                    errors.Add("Benchmark should compile exactly one bridge/fording crossing; bridges=" + road.BridgeCrossings + ", fords=" + road.FordCrossings + ".");
                if (road.RoadWaterConflicts > 0)
                    errors.Add("Benchmark has unbridged road-water conflicts.");
            }

            var water = FindLayer(result, "Water And Shoreline");
            if (water == null || water.WaterMeshes != 1)
                errors.Add("Benchmark should compile one continuous river ribbon; water meshes=" + (water == null ? 0 : water.WaterMeshes) + ".");
            if (water == null || water.ShorelineMeshes <= 0 || water.ShorelineDetailDecalCount <= 0)
                errors.Add("Benchmark river needs shoreline mesh and wet/eroded bank detail.");

            var basePads = FindLayer(result, "Modular Base Pads");
            if (basePads == null || basePads.BasePadCount != 2)
                errors.Add("Benchmark should compile two base pads.");
            if (basePads == null || basePads.BasePadDetailDecalCount < 18)
                errors.Add("Benchmark base pads need modular detail decals.");
            if (basePads != null && basePads.Warnings.Count > 0)
                warnings.AddRange(basePads.Warnings);

            var resources = FindLayer(result, "Resource Field Visuals");
            if (resources == null || resources.ResourceFields < 2)
                errors.Add("Benchmark should compile readable resource fields.");
            if (resources != null && resources.ResourceDustDecalCount < resources.ResourceFields)
                errors.Add("Each benchmark resource field should have ore-stained dust/soil.");
            if (resources != null && resources.ResourceGlintCount > resources.ResourceFields * 4)
                errors.Add("Benchmark resource glints exceed the production cap.");

            var scatter = FindLayer(result, "Rule Based Scatter");
            if (scatter == null)
                errors.Add("Scatter layer missing.");
            else if (scatter.ScatterCount > 700)
                warnings.Add("Benchmark scatter is high for a curated tactical preview: " + scatter.ScatterCount + ".");

            if (CountNameContains(result.Root, "bridge_deck_") <= 0)
                errors.Add("Benchmark should contain a bridge deck object.");
            if (CountNameContains(result.Root, "bridge_rail_section_") <= 0)
                errors.Add("Benchmark bridge needs rail/side-beam pieces.");
            if (CountNameContains(result.Root, "bridge_abutment_") <= 0)
                errors.Add("Benchmark bridge needs bank abutment/contact pieces.");
            if (CountNameContains(result.Root, "resource_field_dust_") < 2)
                errors.Add("Benchmark resource fields need dust/soil decals.");
            if (CountNameContains(result.Root, "basepad_panel_marking_center") != 2)
                errors.Add("Benchmark base pads should use detailed center panel markings.");
            if (CountNameContains(result.Root, "water_ribbon_mesh_") != 1)
                errors.Add("Benchmark river should render as one continuous water ribbon mesh.");
        }

        static AegisArtDirectedVisualMetadata LoadMetadata(string assetPath)
        {
            var filePath = ProjectPath(assetPath);
            if (!File.Exists(filePath))
                return null;

            var metadata = JsonUtility.FromJson<AegisArtDirectedVisualMetadata>(File.ReadAllText(filePath));
            if (metadata != null)
                metadata.Normalize();
            return metadata;
        }

        static string ProjectPath(string assetPath)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), assetPath.Replace('/', Path.DirectorySeparatorChar));
        }

        static AegisVisualLayerSummary FindLayer(AegisMapVisualCompileResult result, string name)
        {
            if (result == null)
                return null;

            for (var i = 0; i < result.Layers.Count; i++)
                if (string.Equals(result.Layers[i].LayerName, name, StringComparison.OrdinalIgnoreCase))
                    return result.Layers[i];
            return null;
        }

        static bool ContainsName(GameObject root, string text)
        {
            return CountNameContains(root, text) > 0;
        }

        static int CountNameContains(GameObject root, string text)
        {
            if (root == null || string.IsNullOrEmpty(text))
                return 0;

            var count = 0;
            var transforms = root.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                if (transforms[i] != null && transforms[i].name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                    count++;
            }

            return count;
        }

        static HashSet<int> CollectWaterCells(AegisVisualMapDocument document)
        {
            var water = new HashSet<int>();
            if (document == null || document.terrainBase == null)
                return water;

            for (var i = 0; i < document.terrainBase.Length; i++)
            {
                var cell = document.terrainBase[i];
                if (cell == null || string.IsNullOrEmpty(cell.terrainId))
                    continue;

                var id = cell.terrainId.ToLowerInvariant();
                if (id.Contains("water") || id.Contains("river") || id.Contains("deep"))
                    water.Add(Key(cell.x, cell.y, document.width));
            }

            return water;
        }

        static bool SegmentTouchesWater(HashSet<int> water, int width, int height, Vector2 a, Vector2 b)
        {
            var length = Vector2.Distance(a, b);
            var steps = Math.Max(2, Mathf.CeilToInt(length / 0.35f));
            for (var i = 0; i <= steps; i++)
            {
                var point = Vector2.Lerp(a, b, i / (float)steps);
                var x = Mathf.Clamp(Mathf.FloorToInt(point.x), 0, width - 1);
                var y = Mathf.Clamp(Mathf.FloorToInt(point.y), 0, height - 1);
                if (water.Contains(Key(x, y, width)))
                    return true;
            }

            return false;
        }

        static bool IsInsideBasePad(AegisVisualMapDocument document, int x, int y, float radius)
        {
            if (document == null || document.playerStarts == null)
                return false;

            for (var i = 0; i < document.playerStarts.Length; i++)
            {
                var start = document.playerStarts[i];
                if (start == null)
                    continue;

                var dx = x + 0.5f - (start.x + 0.5f);
                var dy = y + 0.5f - (start.y + 0.5f);
                if (Mathf.Abs(dx) <= radius && Mathf.Abs(dy) <= radius)
                    return true;
            }

            return false;
        }

        static bool IsNearAuthoredRoad(AegisArtDirectedVisualMetadata metadata, int x, int y, float maxDistance)
        {
            if (metadata == null || metadata.roadSegments == null)
                return false;

            var point = new Vector2(x + 0.5f, y + 0.5f);
            for (var i = 0; i < metadata.roadSegments.Length; i++)
            {
                var segment = metadata.roadSegments[i];
                if (segment == null)
                    continue;

                if (DistanceToSegment(point, segment.A, segment.B) <= maxDistance)
                    return true;
            }

            return false;
        }

        static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            var length = ab.sqrMagnitude;
            if (length <= 0.0001f)
                return Vector2.Distance(point, a);

            var t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / length);
            return Vector2.Distance(point, a + ab * t);
        }

        static int Key(int x, int y, int width)
        {
            return y * width + x;
        }
    }
}
#endif
