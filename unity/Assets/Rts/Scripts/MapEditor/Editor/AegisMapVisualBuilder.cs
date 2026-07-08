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
    public static class AegisMapVisualBuilder
    {
        const float CellSize = 1f;
        const int PixelsPerCell = 10;
        const int MaxCliffRocks = 1400;
        const int MaxNearCliffBoulders = 650;
        const int MaxVegetation = 850;
        const int MaxCraters = 36;
        const int MaxRoadPebbles = 320;
        const int MaxShorePebbles = 170;
        const int MaxOreProps = 900;

        public static void BuildFromSelectedMap()
        {
            var selectedPath = AssetDatabase.GetAssetPath(UnityEditor.Selection.activeObject);
            if (string.IsNullOrEmpty(selectedPath) || !selectedPath.EndsWith(".aegismap.json", StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog("Build Visual Terrain", "Select a .aegismap.json asset first.", "OK");
                return;
            }

            var document = AegisVisualMapDocument.Load(selectedPath);
            if (document == null || document.width <= 0 || document.height <= 0)
            {
                EditorUtility.DisplayDialog("Build Visual Terrain", "The selected map could not be parsed.", "OK");
                return;
            }

            var root = BuildScene(document, selectedPath);
            UnityEditor.Selection.activeObject = root;
            EditorGUIUtility.PingObject(root);
            Debug.Log("Built Project Aegis visual terrain for " + document.mapId + " from " + selectedPath + ".");
        }

        public static GameObject BuildScene(AegisVisualMapDocument document, string sourcePath)
        {
            return BuildScene(document, sourcePath, true);
        }

        public static void ValidateSampleBuildForBatch()
        {
            var samplePath = AegisMapEditorPaths.SamplesFolder + "/sample_ai_medium_forest_4p_balanced.aegismap.json";
            var document = AegisVisualMapDocument.Load(samplePath);
            if (document == null)
                throw new InvalidOperationException("Visual builder validation could not load " + samplePath + ".");

            var root = BuildScene(document, samplePath, false);
            if (root.GetComponent<AegisMapVisualScene>() == null)
                throw new InvalidOperationException("Visual builder validation did not add an AegisMapVisualScene marker.");
            RequireChild(root.transform, "Base Terrain Surface");
            RequireChild(root.transform, "Terrain Transition Masks");
            RequireChild(root.transform, "Water Surface");
            RequireChild(root.transform, "Roads And Tire Tracks");
            RequireChild(root.transform, "Topology Driven Cliffs");
            RequireChild(root.transform, "Resource Field Visuals");
            RequireChild(root.transform, "Modular Base Pads");
            RequireChild(root.transform, "Rule Based Scatter");
            UnityEngine.Object.DestroyImmediate(root);
            Debug.Log("Aegis map visual compiler sample validation passed.");
        }

        public static void RenderSamplePreviewForBatch()
        {
            var samplePath = AegisMapEditorPaths.SamplesFolder + "/sample_ai_medium_forest_2p_river_chokepoint.aegismap.json";
            RenderPreviewForBatch(samplePath, "aegis_visual_builder_sample.png", 0.17f, 0.38f, 0.48f, 1400, 1000, AegisMapVisualCompileSettings.ProductionDefault());
        }

        public static void RenderArtPackShowcaseForBatch()
        {
            var samplePath = AegisMapEditorPaths.SamplesFolder + "/sample_art_pack_showcase_160_forest_river.aegismap.json";
            RenderPreviewForBatch(samplePath, "aegis_art_pack_showcase.png", 0.24f, 0.50f, 0.50f, 1600, 1100, AegisMapVisualCompileSettings.ProductionDefault());
        }

        public static void RenderProductionAndDebugPreviewsForBatch()
        {
            var samplePath = AegisMapEditorPaths.SamplesFolder + "/sample_art_pack_showcase_160_forest_river.aegismap.json";
            RenderPreviewForBatch(samplePath, "aegis_visual_quality_production.png", 0.24f, 0.50f, 0.50f, 1600, 1100, AegisMapVisualCompileSettings.ProductionDefault());
            RenderPreviewForBatch(samplePath, "aegis_visual_quality_debug.png", 0.24f, 0.50f, 0.50f, 1600, 1100, AegisMapVisualCompileSettings.DebugDefault());
        }

        public static void RenderProductionDetailPreviewForBatch()
        {
            var samplePath = AegisMapEditorPaths.SamplesFolder + "/sample_art_pack_showcase_160_forest_river.aegismap.json";
            RenderPreviewForBatch(samplePath, "aegis_visual_quality_detail.png", 0.105f, 0.50f, 0.49f, 1600, 1100, AegisMapVisualCompileSettings.ProductionDefault());
        }

        static void RenderPreviewForBatch(string samplePath, string outputFileName, float orthographicScale, float centerXScale, float centerZScale, int outputWidth, int outputHeight, AegisMapVisualCompileSettings settings)
        {
            var document = AegisVisualMapDocument.Load(samplePath);
            if (document == null)
                throw new InvalidOperationException("Visual builder render could not load " + samplePath + ".");

            var root = BuildScene(document, samplePath, false, settings);
            RenderSettings.ambientLight = new Color(0.34f, 0.36f, 0.34f, 1f);

            var light = new GameObject("Aegis Visual Preview Sun");
            var lightComponent = light.AddComponent<Light>();
            lightComponent.type = LightType.Directional;
            lightComponent.intensity = 1.1f;
            lightComponent.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            var cameraObject = new GameObject("Aegis Visual Preview Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.06f, 0.08f, 0.07f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = Mathf.Max(document.width, document.height) * orthographicScale;
            var pitch = 64f;
            var cameraHeight = 105f;
            var centerX = document.width * centerXScale;
            var centerZ = document.height * centerZScale;
            camera.transform.position = new Vector3(centerX, cameraHeight, centerZ - cameraHeight / Mathf.Tan(pitch * Mathf.Deg2Rad));
            camera.transform.rotation = Quaternion.Euler(pitch, 0f, 0f);

            var renderTexture = new RenderTexture(outputWidth, outputHeight, 24, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            camera.targetTexture = renderTexture;
            camera.Render();
            RenderTexture.active = renderTexture;

            var image = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            image.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            image.Apply(false, false);
            EnsurePreviewIsNonBlank(image);

            var outputDir = Path.Combine(Path.GetTempPath(), "ProjectAegisRTS");
            Directory.CreateDirectory(outputDir);
            var outputPath = Path.Combine(outputDir, outputFileName);
            File.WriteAllBytes(outputPath, image.EncodeToPNG());
            Debug.Log("Aegis map visual builder rendered preview: " + outputPath);

            RenderTexture.active = previous;
            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(renderTexture);
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(light);
            UnityEngine.Object.DestroyImmediate(root);
        }

        static void EnsurePreviewIsNonBlank(Texture2D image)
        {
            var pixels = image.GetPixels32();
            if (pixels.Length == 0)
                throw new InvalidOperationException("Visual builder render produced an empty image.");

            var first = pixels[0];
            var varied = 0;
            var step = Math.Max(1, pixels.Length / 4096);
            for (var i = 0; i < pixels.Length; i += step)
            {
                var p = pixels[i];
                if (Math.Abs(p.r - first.r) + Math.Abs(p.g - first.g) + Math.Abs(p.b - first.b) > 12)
                    varied++;
                if (varied > 48)
                    return;
            }

            throw new InvalidOperationException("Visual builder render looked blank or single-color.");
        }

        static void RequireChild(Transform root, string childName)
        {
            if (root.Find(childName) == null)
                throw new InvalidOperationException("Visual builder validation missing child: " + childName + ".");
        }

        static GameObject BuildScene(AegisVisualMapDocument document, string sourcePath, bool persistAssets)
        {
            return BuildScene(document, sourcePath, persistAssets, AegisMapVisualCompileSettings.ProductionDefault());
        }

        static GameObject BuildScene(AegisVisualMapDocument document, string sourcePath, bool persistAssets, AegisMapVisualCompileSettings settings)
        {
            var result = AegisMapVisualCompiler.CompileDocument(document, sourcePath, persistAssets, settings);
            return result.Root;
        }

        static void BuildTerrainPlane(Transform root, AegisVisualMapDocument document, AegisVisualMapLookup lookup, List<AegisPathSegment> paths, List<AegisRiverSegment> rivers, AegisVisualBiomeProfile profile, AegisVisualMaterialSet materials, string safeMapId, int seed, bool persistAssets)
        {
            var texture = CreateTerrainTexture(document, lookup, paths, rivers, profile, seed);
            var material = new Material(materials.Ground);
            material.name = safeMapId + "_terrain_material";
            material.mainTexture = texture;
            if (persistAssets)
            {
                var texturePath = AssetDatabase.GenerateUniqueAssetPath(AegisMapEditorPaths.VisualBuildsFolder + "/" + safeMapId + "_terrain_albedo.asset");
                AssetDatabase.CreateAsset(texture, texturePath);
                var materialPath = AssetDatabase.GenerateUniqueAssetPath(AegisMapEditorPaths.VisualBuildsFolder + "/" + safeMapId + "_terrain.mat");
                AssetDatabase.CreateAsset(material, materialPath);
            }

            var terrain = CreateQuad("Blended Terrain Surface", document.width * CellSize, document.height * CellSize, material);
            terrain.transform.SetParent(root, false);
            terrain.transform.position = new Vector3((document.width - 1) * CellSize * 0.5f, 0f, (document.height - 1) * CellSize * 0.5f);
        }

        static Texture2D CreateTerrainTexture(AegisVisualMapDocument document, AegisVisualMapLookup lookup, List<AegisPathSegment> paths, List<AegisRiverSegment> rivers, AegisVisualBiomeProfile profile, int seed)
        {
            var width = document.width * PixelsPerCell;
            var height = document.height * PixelsPerCell;
            var pixels = new Color32[width * height];
            var resourceFields = BuildVisualResourceFields(document);
            var waterDistanceByCell = new int[document.width * document.height];
            for (var cy = 0; cy < document.height; cy++)
                for (var cx = 0; cx < document.width; cx++)
                {
                    var key = cy * document.width + cx;
                    waterDistanceByCell[key] = lookup.DistanceToTerrain(cx, cy, "water", 5);
                }

            for (var py = 0; py < height; py++)
            {
                for (var px = 0; px < width; px++)
                {
                    var cellX = px / PixelsPerCell;
                    var cellY = py / PixelsPerCell;
                    var localX = (px + 0.5f) / PixelsPerCell;
                    var localY = (py + 0.5f) / PixelsPerCell;
                    var fracX = localX - cellX;
                    var fracY = localY - cellY;
                    var terrain = lookup.TerrainAt(cellX, cellY);
                    var waterInfluence = WaterInfluence(lookup, rivers, localX, localY);
                    var color = profile.ColorForTerrain(terrain == "water" ? document.defaultTerrainId : terrain);
                    color = ApplyClusteredTerrainStrength(document, lookup, profile, terrain, cellX, cellY, color);
                    color = FeatherTerrainEdge(document, lookup, profile, terrain, cellX, cellY, fracX, fracY, color);
                    var fineNoise = Hash01(seed, px, py) * 0.45f + Hash01(seed ^ 0x2715, px / 4, py / 4) * 0.35f + Hash01(seed ^ 0x6EED, px / 17, py / 19) * 0.2f;
                    color = AddNoise(color, fineNoise, 0.105f);

                    var cellKey = cellY * document.width + cellX;
                    var waterDistance = waterDistanceByCell[cellKey];
                    if (rivers.Count == 0 && waterDistance >= 0 && waterDistance <= 4 && terrain != "water")
                    {
                        var t = 1f - waterDistance / 4f;
                        color = Color.Lerp(color, profile.MuddyBank, t * 0.7f);
                    }

                    var pathDistance = DistanceToPaths(paths, localX, localY);
                    if (pathDistance < 2.6f && waterInfluence < 0.36f)
                    {
                        var edge = Mathf.Clamp01(1f - pathDistance / 2.6f);
                        var worn = Color.Lerp(profile.DirtPath, profile.GravelPath, Mathf.Clamp01(1f - pathDistance / 1.1f));
                        var rut = Mathf.Clamp01(1f - Mathf.Abs(pathDistance - 0.82f) / 0.28f);
                        var dusty = Color.Lerp(worn, profile.DirtPath, Hash01(seed ^ 0xDA7A, px / 5, py / 5) * 0.22f);
                        color = Color.Lerp(color, dusty, edge * 0.76f);
                        color = Color.Lerp(color, profile.GravelPath, rut * 0.24f);
                    }

                    if (waterInfluence > 0.02f)
                    {
                        if (waterInfluence < 0.42f)
                            color = Color.Lerp(color, profile.MuddyBank, waterInfluence * 1.65f);
                        else
                        {
                            var deep = Mathf.Clamp01((waterInfluence - 0.42f) / 0.58f);
                            var flowNoise = Hash01(seed ^ 0x515EA, px / 3, py / 5) * 0.25f;
                            color = Color.Lerp(profile.Water, profile.DeepWater, Mathf.Clamp01(deep * 0.76f + flowNoise));
                            var glint = Hash01(seed ^ 0x7A71, px / 13, py / 3);
                            if (glint > 0.82f && deep < 0.72f)
                                color = Color.Lerp(color, profile.WaterHighlight, (glint - 0.82f) * 0.45f);
                        }
                    }

                    var resourceInfluence = ResourceGroundInfluence(resourceFields, localX, localY, seed);
                    if (resourceInfluence > 0f && waterInfluence < 0.2f)
                    {
                        var oreSoil = Color.Lerp(profile.Rough, profile.OreGround, 0.32f);
                        color = Color.Lerp(color, oreSoil, resourceInfluence * 0.62f);
                        if (resourceInfluence > 0.52f)
                            color = Color.Lerp(color, profile.OreGround, (resourceInfluence - 0.52f) * 0.18f);
                    }

                    pixels[(height - 1 - py) * width + px] = ToColor32(color);
                }
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, true);
            texture.name = Sanitize(document.mapId) + "_terrain_albedo";
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels32(pixels);
            texture.Apply(true, false);
            return texture;
        }

        static void BuildBasePads(Transform root, AegisVisualMapDocument document, AegisVisualMaterialSet materials, int seed)
        {
            var parent = new GameObject("Base Pads").transform;
            parent.SetParent(root, false);
            var mapCenter = new Vector2(document.width * 0.5f, document.height * 0.5f);
            for (var i = 0; i < document.playerStarts.Length; i++)
            {
                var start = document.playerStarts[i];
                var startPoint = new Vector2(start.x + 0.5f, start.y + 0.5f);
                var approach = mapCenter - startPoint;
                if (approach.sqrMagnitude < 0.01f)
                    approach = Vector2.up;
                approach.Normalize();
                var approachAngle = DirectionAngle(approach);

                var blend = CreateQuad("Base Pad Terrain Blend P" + start.playerId, 18f, 18f, materials.Dirt);
                blend.transform.SetParent(parent, false);
                blend.transform.position = CellCenter(start.x, start.y, 0.028f);

                var apronCenter = startPoint + approach * 8.6f;
                CreateOrientedQuad(parent, "Base Pad Approach Dust P" + start.playerId, apronCenter, 7.4f, 5.6f, 0.033f, materials.RoadDust, approachAngle);

                var loadedPadMesh = AegisMapArtPack.TryInstantiatePrefab(
                    parent,
                    "Base Pad Mesh P" + start.playerId,
                    "Meshes/BasePads/base_pad_14x14.glb",
                    CellCenter(start.x, start.y, 0.038f),
                    Quaternion.identity,
                    Vector3.one,
                    materials.Concrete);
                if (!loadedPadMesh)
                {
                    var pad = CreateQuad("Base Pad P" + start.playerId, 14.5f, 14.5f, materials.Concrete);
                    pad.transform.SetParent(parent, false);
                    pad.transform.position = CellCenter(start.x, start.y, 0.035f);
                }

                var panel = CreateQuad("Base Pad Inner Panel P" + start.playerId, 8.5f, 8.5f, materials.ConcretePanel);
                panel.transform.SetParent(parent, false);
                panel.transform.position = CellCenter(start.x, start.y, 0.041f);

                CreatePadLine(parent, "Base Pad Center Seam X P" + start.playerId, start.x, start.y, 0f, 0f, 12.8f, 0.12f, materials.ConcreteLine, 0.049f);
                CreatePadLine(parent, "Base Pad Center Seam Z P" + start.playerId, start.x, start.y, 0f, 0f, 0.12f, 12.8f, materials.ConcreteLine, 0.05f);
                CreatePadLine(parent, "Base Pad Inner Seam North P" + start.playerId, start.x, start.y, 0f, 2.9f, 7.2f, 0.08f, materials.ConcreteLine, 0.051f);
                CreatePadLine(parent, "Base Pad Inner Seam South P" + start.playerId, start.x, start.y, 0f, -2.9f, 7.2f, 0.08f, materials.ConcreteLine, 0.051f);
                CreatePadLine(parent, "Base Pad Inner Seam East P" + start.playerId, start.x, start.y, 2.9f, 0f, 0.08f, 7.2f, materials.ConcreteLine, 0.052f);
                CreatePadLine(parent, "Base Pad Inner Seam West P" + start.playerId, start.x, start.y, -2.9f, 0f, 0.08f, 7.2f, materials.ConcreteLine, 0.052f);

                CreatePadTrim(parent, "North Trim P" + start.playerId, start.x, start.y + 4, 10f, 1.1f, materials.ConcreteTrim);
                CreatePadTrim(parent, "South Trim P" + start.playerId, start.x, start.y - 4, 10f, 1.1f, materials.ConcreteTrim);
                CreatePadTrim(parent, "West Trim P" + start.playerId, start.x - 4, start.y, 1.1f, 10f, materials.ConcreteTrim);
                CreatePadTrim(parent, "East Trim P" + start.playerId, start.x + 4, start.y, 1.1f, 10f, materials.ConcreteTrim);

                CreateBaseGrime(parent, start.x, start.y, start.playerId, seed, materials.ConcreteGrime);
            }
        }

        static void BuildRiverDressing(Transform root, List<AegisRiverSegment> rivers, AegisVisualMaterialSet materials, int seed)
        {
            if (rivers.Count == 0)
                return;

            var parent = new GameObject("River Edge Art Pack Dressing").transform;
            parent.SetParent(root, false);
            for (var i = 0; i < rivers.Count; i++)
            {
                var segment = rivers[i];
                var delta = segment.B - segment.A;
                var length = delta.magnitude;
                if (length < 1.2f)
                    continue;

                var direction = delta / length;
                var perpendicular = new Vector2(-direction.y, direction.x);
                var angle = DirectionAngle(direction);
                var center = (segment.A + segment.B) * 0.5f;
                CreateOrientedQuad(parent, "Muddy Shoreline Left", center + perpendicular * (segment.Radius + 0.35f), 1.15f, length + 0.4f, 0.033f, materials.MuddyShoreline, angle);
                CreateOrientedQuad(parent, "Muddy Shoreline Right", center - perpendicular * (segment.Radius + 0.35f), 1.15f, length + 0.4f, 0.033f, materials.MuddyShoreline, angle + 180f);

                if (i % 3 == 0)
                    CreateOrientedQuad(parent, "Water Highlight Streak", center, Mathf.Max(0.55f, segment.Radius * 0.65f), Mathf.Min(length * 0.56f, 8f), 0.036f, materials.WaterHighlightDecal, angle + Hash01(seed ^ 0xA7E5, i, Mathf.RoundToInt(length)) * 12f - 6f);

                if (i % 5 == 0)
                {
                    var meshPath = AegisMapArtPack.Pick(AegisMapArtPack.RiverMeshes, seed ^ 0x7711, i, Mathf.RoundToInt(length * 10f));
                    var pos = new Vector3(center.x + perpendicular.x * (segment.Radius + 0.8f), 0.05f, center.y + perpendicular.y * (segment.Radius + 0.8f));
                    AegisMapArtPack.TryInstantiatePrefab(parent, "River Edge Mesh", meshPath, pos, Quaternion.Euler(0f, angle, 0f), Vector3.one, materials.Vegetation);
                }
            }
        }

        static void BuildRoadWear(Transform root, List<AegisPathSegment> paths, AegisVisualMaterialSet materials, int seed)
        {
            var parent = new GameObject("Road And Path Decals").transform;
            parent.SetParent(root, false);
            for (var i = 0; i < paths.Count; i++)
            {
                var segment = paths[i];
                var delta = segment.B - segment.A;
                var length = delta.magnitude;
                if (length < 1.2f)
                    continue;

                var direction = delta / length;
                var perpendicular = new Vector2(-direction.y, direction.x);
                var angle = DirectionAngle(direction);
                var center = (segment.A + segment.B) * 0.5f;
                var width = Mathf.Lerp(2.8f, 4.4f, Hash01(seed ^ 0xA71D, i, Mathf.RoundToInt(length * 10f)));
                CreateOrientedQuad(parent, "Soft Road Dust", center, width, length + 1.1f, 0.031f, materials.RoadDust, angle);

                var rutOffset = Mathf.Clamp(width * 0.22f, 0.55f, 0.92f);
                CreateOrientedQuad(parent, "Left Road Rut", center + perpendicular * rutOffset, 0.18f, length * 0.86f, 0.034f, materials.RoadRut, angle);
                CreateOrientedQuad(parent, "Right Road Rut", center - perpendicular * rutOffset, 0.18f, length * 0.86f, 0.034f, materials.RoadRut, angle);

                if (length > 8f)
                {
                    var scuffT = Mathf.Lerp(0.24f, 0.76f, Hash01(seed ^ 0x5C0F, i, 11));
                    var scuffCenter = Vector2.Lerp(segment.A, segment.B, scuffT) + perpendicular * (Hash01(seed ^ 0x5C10, i, 3) - 0.5f) * width * 0.5f;
                    CreateOrientedQuad(parent, "Road Gravel Scuff", scuffCenter, width * 0.65f, 2.8f, 0.036f, materials.RoadScuff, angle + Hash01(seed ^ 0x5C11, i, 5) * 22f - 11f);
                }
            }
        }

        static void CreatePadTrim(Transform parent, string name, int x, int y, float width, float height, Material material)
        {
            var trim = CreateQuad(name, width, height, material);
            trim.transform.SetParent(parent, false);
            trim.transform.position = CellCenter(x, y, 0.046f);
        }

        static void CreatePadLine(Transform parent, string name, int x, int y, float offsetX, float offsetY, float width, float height, Material material, float elevation)
        {
            var line = CreateQuad(name, width, height, material);
            line.transform.SetParent(parent, false);
            line.transform.position = new Vector3((x + 0.5f) * CellSize + offsetX, elevation, (y + 0.5f) * CellSize + offsetY);
        }

        static void CreateBaseGrime(Transform parent, int cellX, int cellY, int playerId, int seed, Material material)
        {
            for (var i = 0; i < 4; i++)
            {
                var grime = CreateGroundDiscObject("Concrete Grime P" + playerId, Mathf.Lerp(0.55f, 1.45f, Hash01(seed ^ 0x6A1, cellX + i, cellY)), Mathf.Lerp(0.38f, 1.18f, Hash01(seed ^ 0x6A2, cellX, cellY + i)), material);
                grime.transform.SetParent(parent, false);
                var ox = (Hash01(seed ^ 0x6A3, cellX + i * 7, cellY) - 0.5f) * 9.8f;
                var oz = (Hash01(seed ^ 0x6A4, cellX, cellY + i * 11) - 0.5f) * 9.8f;
                grime.transform.position = new Vector3((cellX + 0.5f) * CellSize + ox, 0.054f + i * 0.001f, (cellY + 0.5f) * CellSize + oz);
                grime.transform.rotation = Quaternion.Euler(0f, Hash01(seed ^ 0x6A5, cellX + i, cellY - i) * 360f, 0f);
            }
        }

        static void BuildCliffRidges(Transform root, AegisVisualMapDocument document, AegisVisualMapLookup lookup, AegisVisualMaterialSet materials, int seed)
        {
            var parent = new GameObject("Cliff Rock Chains").transform;
            parent.SetParent(root, false);
            var built = 0;
            for (var i = 0; i < document.blockers.Length && built < MaxCliffRocks; i++)
            {
                var cell = document.blockers[i];
                if (!lookup.IsCliffLike(cell.x, cell.y) || !lookup.IsBoundary(cell.x, cell.y))
                    continue;
                if (Hash01(seed ^ 0xC11FF, cell.x, cell.y) > 0.58f)
                    continue;

                CreateGroundDisc(parent, "Cliff Contact Shadow", cell.x, cell.y, 1.05f, 0.72f, materials.Shadow, seed);
                CreateRockCluster(parent, "Cliff Ridge", cell.x, cell.y, materials.Cliff, seed, 1.25f, 2.65f, 2 + HashRange(seed, cell.x, cell.y, 3));
                built++;
            }
        }

        static void BuildOreClusters(Transform root, AegisVisualMapDocument document, AegisVisualMaterialSet materials, int seed)
        {
            var parent = new GameObject("Ore Clusters").transform;
            parent.SetParent(root, false);
            var count = Math.Min(document.resources.Length, MaxOreProps);
            for (var i = 0; i < count; i++)
            {
                var resource = document.resources[i];
                var chunkCount = 2 + HashRange(seed ^ 0x0A0E, resource.x, resource.y, 4);
                if (Hash01(seed ^ 0x0D057, resource.x, resource.y) < 0.34f)
                    CreateGroundDisc(parent, "Ore Dust Decal", resource.x, resource.y, 0.68f, 0.52f, materials.OreDust, seed ^ 0x0D057);
                for (var c = 0; c < chunkCount; c++)
                {
                    var ox = (Hash01(seed, resource.x + c * 11, resource.y) - 0.5f) * 0.72f;
                    var oz = (Hash01(seed, resource.x, resource.y + c * 17) - 0.5f) * 0.72f;
                    var radius = 0.13f + Hash01(seed ^ 0x0E11, resource.x + c, resource.y - c) * 0.17f;
                    var height = 0.18f + Hash01(seed ^ 0x0E12, resource.x - c, resource.y + c) * 0.28f;
                    var position = CellCenter(resource.x, resource.y, height * 0.34f) + new Vector3(ox, 0f, oz);
                    var rotation = Quaternion.Euler(0f, Hash01(seed, resource.x + c, resource.y - c) * 360f, 0f);
                    var meshPath = AegisMapArtPack.Pick(AegisMapArtPack.OreMeshes, seed ^ 0x0E13, resource.x + c, resource.y - c);
                    var imported = AegisMapArtPack.TryInstantiatePrefab(parent, "Ore Chunk", meshPath, position, rotation, Vector3.one * Mathf.Lerp(0.58f, 1.05f, Hash01(seed ^ 0x0E14, resource.x, resource.y + c)), materials.Ore);
                    if (!imported)
                    {
                        var ore = new GameObject("Ore Nugget");
                        ore.name = "Ore Chunk";
                        ore.transform.SetParent(parent, false);
                        ore.AddComponent<MeshFilter>().sharedMesh = CreateFacetedOreMesh(seed, resource.x, resource.y, c, radius, height);
                        ore.AddComponent<MeshRenderer>().sharedMaterial = materials.Ore;
                        ore.transform.position = position;
                        ore.transform.rotation = rotation;
                    }

                    if (c == 0 && Hash01(seed ^ 0x6117, resource.x, resource.y) > 0.55f)
                        CreateGroundDisc(parent, "Resource Glint Decal", resource.x, resource.y, 0.32f, 0.22f, materials.ResourceGlint, seed ^ 0x6117);
                }
            }
        }

        static List<AegisOreVisualField> BuildVisualResourceFields(AegisVisualMapDocument document)
        {
            var groups = new Dictionary<string, AegisOreVisualFieldBuilder>();
            for (var i = 0; i < document.resources.Length; i++)
            {
                var resource = document.resources[i];
                var id = string.IsNullOrEmpty(resource.fieldId) ? "resource_" + i.ToString() : resource.fieldId;
                AegisOreVisualFieldBuilder builder;
                if (!groups.TryGetValue(id, out builder))
                {
                    builder = new AegisOreVisualFieldBuilder();
                    groups[id] = builder;
                }
                builder.Add(resource.x + 0.5f, resource.y + 0.5f);
            }

            var fields = new List<AegisOreVisualField>();
            foreach (var pair in groups)
            {
                var field = pair.Value.Build();
                if (field != null)
                    fields.Add(field);
            }
            return fields;
        }

        static void BuildScatter(Transform root, AegisVisualMapDocument document, AegisVisualMapLookup lookup, List<AegisPathSegment> paths, AegisVisualMaterialSet materials, int seed)
        {
            var parent = new GameObject("Deterministic Detail Scatter").transform;
            parent.SetParent(root, false);
            var boulders = 0;
            var vegetation = 0;
            var craters = 0;
            var pebbles = 0;
            var shorePebbles = 0;

            for (var y = 2; y < document.height - 2; y += 2)
            {
                for (var x = 2; x < document.width - 2; x += 2)
                {
                    var terrain = lookup.TerrainAt(x, y);
                    if (terrain == "water" || lookup.HasResource(x, y) || lookup.IsNearStart(x, y, 9))
                        continue;

                    var cliffDistance = lookup.DistanceToCliff(x, y, 6);
                    var waterDistance = lookup.DistanceToTerrain(x, y, "water", 3);
                    var pathDistance = DistanceToPaths(paths, x + 0.5f, y + 0.5f);
                    var h = Hash01(seed ^ 0x5CA77E, x, y);
                    if (cliffDistance >= 0 && cliffDistance <= 3 && boulders < MaxNearCliffBoulders && h < 0.22f)
                    {
                        CreateGroundDisc(parent, "Boulder Contact Shadow", x, y, 0.46f, 0.32f, materials.Shadow, seed ^ 0x51A);
                        CreateRockCluster(parent, "Boulder Scatter", x, y, materials.Boulder, seed ^ 0xB011, 0.35f, 1.1f, 1 + HashRange(seed, x, y, 2));
                        boulders++;
                    }
                    else if (waterDistance >= 0 && waterDistance <= 2 && shorePebbles < MaxShorePebbles && h < 0.14f)
                    {
                        if (Hash01(seed ^ 0xBA11, x, y) < 0.72f)
                            CreatePebble(parent, x, y, materials.Pebble, seed, "Shore Pebble");
                        else
                            CreateVegetation(parent, x, y, materials.Vegetation, seed, "Bank Grass");
                        shorePebbles++;
                    }
                    else if ((terrain == "forest" || h < 0.055f) && vegetation < MaxVegetation)
                    {
                        CreateVegetation(parent, x, y, materials.Vegetation, seed, "Grass Bush");
                        vegetation++;
                    }
                    else if (pathDistance < 3.7f && pathDistance > 1.0f && pebbles < MaxRoadPebbles && h < 0.19f)
                    {
                        CreatePebble(parent, x, y, materials.Pebble, seed, "Road Pebble");
                        pebbles++;
                    }
                    else if (pathDistance > 7f && craters < MaxCraters && h > 0.9986f)
                    {
                        CreateCrater(parent, x, y, materials.CraterRim, materials.Crater, seed);
                        craters++;
                    }
                }
            }
        }

        static void CreateRockCluster(Transform parent, string name, int cellX, int cellY, Material material, int seed, float minScale, float maxScale, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var offset = new Vector3((Hash01(seed, cellX + i * 7, cellY) - 0.5f) * 1.2f, 0f, (Hash01(seed, cellX, cellY + i * 13) - 0.5f) * 1.2f);
                var height = Mathf.Lerp(minScale, maxScale, Hash01(seed ^ 0xA7A, cellX + i, cellY - i));
                var radius = Mathf.Lerp(0.32f, 0.78f, Hash01(seed ^ 0x77A, cellX - i, cellY + i));
                var position = CellCenter(cellX, cellY, 0.05f) + offset;
                var rotation = Quaternion.Euler(Hash01(seed, cellX, i) * 12f - 6f, Hash01(seed, i, cellY) * 360f, Hash01(seed, cellY, i) * 12f - 6f);
                var isBoulder = name.IndexOf("Boulder", StringComparison.OrdinalIgnoreCase) >= 0;
                var candidates = isBoulder ? AegisMapArtPack.BoulderMeshes : AegisMapArtPack.CliffMeshes;
                var meshPath = AegisMapArtPack.Pick(candidates, seed ^ 0xA771, cellX + i, cellY - i);
                var artMinScale = isBoulder ? 0.48f : 0.34f;
                var artMaxScale = isBoulder ? 0.92f : 0.62f;
                var imported = AegisMapArtPack.TryInstantiatePrefab(parent, name, meshPath, position, rotation, Vector3.one * Mathf.Lerp(artMinScale, artMaxScale, Hash01(seed ^ 0xC0DE, cellX, cellY + i)), material);
                if (!imported)
                {
                    var rock = new GameObject(name);
                    rock.name = name;
                    rock.transform.SetParent(parent, false);
                    rock.AddComponent<MeshFilter>().sharedMesh = CreateFacetedRockMesh(seed, cellX, cellY, i, radius, height);
                    rock.AddComponent<MeshRenderer>().sharedMaterial = material;
                    rock.transform.position = position;
                    rock.transform.rotation = rotation;
                }
            }
        }

        static void CreateVegetation(Transform parent, int cellX, int cellY, Material material, int seed, string name)
        {
            var meshPath = AegisMapArtPack.Pick(AegisMapArtPack.VegetationMeshes, seed ^ 0xF01A6E, cellX, cellY);
            if (AegisMapArtPack.TryInstantiatePrefab(parent, name, meshPath, CellCenter(cellX, cellY, 0.04f), Quaternion.Euler(0f, Hash01(seed ^ 0x6E71, cellX, cellY) * 360f, 0f), Vector3.one * Mathf.Lerp(0.72f, 1.22f, Hash01(seed ^ 0x6E72, cellX, cellY)), material))
                return;

            var bush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bush.name = name;
            bush.transform.SetParent(parent, false);
            bush.transform.position = CellCenter(cellX, cellY, 0.22f);
            var scale = 0.26f + Hash01(seed, cellX, cellY) * 0.42f;
            bush.transform.localScale = new Vector3(scale, 0.18f + scale * 0.16f, scale);
            AssignMaterialAndStripCollider(bush, material);
        }

        static void CreatePebble(Transform parent, int cellX, int cellY, Material material, int seed, string name)
        {
            var meshPath = AegisMapArtPack.Pick(AegisMapArtPack.PebbleMeshes, seed ^ 0x0BB1E, cellX, cellY);
            if (AegisMapArtPack.TryInstantiatePrefab(parent, name, meshPath, CellCenter(cellX, cellY, 0.055f), Quaternion.Euler(0f, Hash01(seed ^ 0xDEB, cellX, cellY) * 360f, 0f), Vector3.one * Mathf.Lerp(0.45f, 0.85f, Hash01(seed ^ 0x0BB2E, cellX, cellY)), material))
                return;

            var pebble = new GameObject(name);
            pebble.name = name;
            pebble.transform.SetParent(parent, false);
            pebble.transform.position = CellCenter(cellX, cellY, 0.06f);
            var scale = 0.12f + Hash01(seed, cellX, cellY) * 0.18f;
            pebble.AddComponent<MeshFilter>().sharedMesh = CreateFacetedPebbleMesh(seed, cellX, cellY, scale);
            pebble.AddComponent<MeshRenderer>().sharedMaterial = material;
            pebble.transform.rotation = Quaternion.Euler(0f, Hash01(seed ^ 0xDEB, cellX, cellY) * 360f, 0f);
        }

        static void CreateCrater(Transform parent, int cellX, int cellY, Material rimMaterial, Material centerMaterial, int seed)
        {
            var craterMesh = AegisMapArtPack.Pick(AegisMapArtPack.CraterMeshes, seed ^ 0xC2A7E, cellX, cellY);
            AegisMapArtPack.TryInstantiatePrefab(parent, "Crater Mesh", craterMesh, CellCenter(cellX, cellY, 0.036f), Quaternion.Euler(0f, Hash01(seed, cellX, cellY) * 360f, 0f), Vector3.one, rimMaterial);

            var rim = CreateGroundDiscObject("Crater Mud Rim", 1.08f, 0.86f, rimMaterial);
            rim.transform.SetParent(parent, false);
            rim.transform.position = CellCenter(cellX, cellY, 0.043f);
            rim.transform.rotation = Quaternion.Euler(0f, Hash01(seed, cellX, cellY) * 360f, 0f);

            var center = CreateGroundDiscObject("Crater Shadow", 0.56f, 0.42f, centerMaterial);
            center.transform.SetParent(parent, false);
            center.transform.position = CellCenter(cellX, cellY, 0.047f);
            center.transform.rotation = rim.transform.rotation;
        }

        static Mesh CreateFacetedRockMesh(int seed, int cellX, int cellY, int variant, float radius, float height)
        {
            const int sides = 8;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            for (var i = 0; i < sides; i++)
            {
                var angle = i / (float)sides * Mathf.PI * 2f;
                var jitter = Mathf.Lerp(0.72f, 1.18f, Hash01(seed ^ 0x51A7E, cellX + i + variant * 13, cellY - i));
                vertices.Add(new Vector3(Mathf.Cos(angle) * radius * jitter, 0f, Mathf.Sin(angle) * radius * 0.72f * jitter));
            }
            for (var i = 0; i < sides; i++)
            {
                var angle = (i + 0.18f) / sides * Mathf.PI * 2f;
                var jitter = Mathf.Lerp(0.34f, 0.58f, Hash01(seed ^ 0x923D, cellX - i, cellY + variant * 17));
                vertices.Add(new Vector3(Mathf.Cos(angle) * radius * jitter, height, Mathf.Sin(angle) * radius * 0.7f * jitter));
            }

            var top = vertices.Count;
            vertices.Add(new Vector3((Hash01(seed, cellX, variant) - 0.5f) * radius * 0.28f, height * 1.16f, (Hash01(seed, variant, cellY) - 0.5f) * radius * 0.22f));

            for (var i = 0; i < sides; i++)
            {
                var next = (i + 1) % sides;
                triangles.Add(i);
                triangles.Add(next);
                triangles.Add(sides + i);
                triangles.Add(next);
                triangles.Add(sides + next);
                triangles.Add(sides + i);

                triangles.Add(sides + i);
                triangles.Add(sides + next);
                triangles.Add(top);
            }

            var mesh = new Mesh();
            mesh.name = "Aegis Faceted Rock Mesh";
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        static Mesh CreateFacetedPebbleMesh(int seed, int cellX, int cellY, float radius)
        {
            const int sides = 6;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            for (var i = 0; i < sides; i++)
            {
                var angle = i / (float)sides * Mathf.PI * 2f;
                var jitter = Mathf.Lerp(0.72f, 1.12f, Hash01(seed ^ 0x0B1E, cellX + i, cellY - i));
                vertices.Add(new Vector3(Mathf.Cos(angle) * radius * jitter, 0f, Mathf.Sin(angle) * radius * 0.72f * jitter));
            }
            for (var i = 0; i < sides; i++)
            {
                var angle = (i + 0.2f) / sides * Mathf.PI * 2f;
                vertices.Add(new Vector3(Mathf.Cos(angle) * radius * 0.6f, radius * 0.32f, Mathf.Sin(angle) * radius * 0.42f));
            }

            var top = vertices.Count;
            vertices.Add(new Vector3(0f, radius * 0.42f, 0f));
            for (var i = 0; i < sides; i++)
            {
                var next = (i + 1) % sides;
                triangles.Add(i);
                triangles.Add(next);
                triangles.Add(sides + i);
                triangles.Add(next);
                triangles.Add(sides + next);
                triangles.Add(sides + i);
                triangles.Add(sides + i);
                triangles.Add(sides + next);
                triangles.Add(top);
            }

            var mesh = new Mesh();
            mesh.name = "Aegis Faceted Pebble Mesh";
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        static Mesh CreateFacetedOreMesh(int seed, int cellX, int cellY, int variant, float radius, float height)
        {
            const int sides = 7;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            for (var i = 0; i < sides; i++)
            {
                var angle = i / (float)sides * Mathf.PI * 2f;
                var jitter = Mathf.Lerp(0.62f, 1.18f, Hash01(seed ^ 0x0E31, cellX + i + variant, cellY - i));
                vertices.Add(new Vector3(Mathf.Cos(angle) * radius * jitter, 0f, Mathf.Sin(angle) * radius * 0.78f * jitter));
            }

            var crown = vertices.Count;
            vertices.Add(new Vector3((Hash01(seed ^ 0x0E32, cellX, variant) - 0.5f) * radius * 0.35f, height, (Hash01(seed ^ 0x0E33, variant, cellY) - 0.5f) * radius * 0.3f));
            var shoulder = vertices.Count;
            vertices.Add(new Vector3((Hash01(seed ^ 0x0E34, cellY, variant) - 0.5f) * radius * 0.5f, height * 0.56f, (Hash01(seed ^ 0x0E35, variant, cellX) - 0.5f) * radius * 0.5f));

            for (var i = 0; i < sides; i++)
            {
                var next = (i + 1) % sides;
                triangles.Add(i);
                triangles.Add(next);
                triangles.Add(crown);
                if (i % 2 == 0)
                {
                    triangles.Add(i);
                    triangles.Add(shoulder);
                    triangles.Add(next);
                }
            }

            var mesh = new Mesh();
            mesh.name = "Aegis Faceted Ore Mesh";
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        static GameObject CreateQuad(string name, float width, float height, Material material)
        {
            var go = new GameObject(name);
            var mesh = new Mesh();
            mesh.name = name + " Mesh";
            var hw = width * 0.5f;
            var hh = height * 0.5f;
            mesh.vertices = new[]
            {
                new Vector3(-hw, 0f, -hh),
                new Vector3(hw, 0f, -hh),
                new Vector3(-hw, 0f, hh),
                new Vector3(hw, 0f, hh)
            };
            mesh.uv = new[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) };
            mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }

        static void CreateGroundDisc(Transform parent, string name, int cellX, int cellY, float radiusX, float radiusZ, Material material, int seed)
        {
            var disc = CreateGroundDiscObject(name, radiusX, radiusZ, material);
            disc.transform.SetParent(parent, false);
            disc.transform.position = CellCenter(cellX, cellY, 0.032f);
            disc.transform.rotation = Quaternion.Euler(0f, Hash01(seed, cellX, cellY) * 360f, 0f);
        }

        static void CreateOrientedQuad(Transform parent, string name, Vector2 center, float width, float height, float elevation, Material material, float angleDegrees)
        {
            var quad = CreateQuad(name, width, height, material);
            quad.transform.SetParent(parent, false);
            quad.transform.position = new Vector3(center.x, elevation, center.y);
            quad.transform.rotation = Quaternion.Euler(0f, angleDegrees, 0f);
        }

        static GameObject CreateGroundDiscObject(string name, float radiusX, float radiusZ, Material material)
        {
            var go = new GameObject(name);
            var mesh = new Mesh();
            mesh.name = name + " Mesh";
            const int segments = 20;
            var vertices = new Vector3[segments + 1];
            var triangles = new int[segments * 3];
            vertices[0] = Vector3.zero;
            for (var i = 0; i < segments; i++)
            {
                var angle = i / (float)segments * Mathf.PI * 2f;
                vertices[i + 1] = new Vector3(Mathf.Cos(angle) * radiusX, 0f, Mathf.Sin(angle) * radiusZ);
            }
            for (var i = 0; i < segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i == segments - 1 ? 1 : i + 2;
                triangles[i * 3 + 2] = i + 1;
            }
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }

        static List<AegisPathSegment> BuildGameplayPathSegments(AegisVisualMapDocument document, int seed)
        {
            var segments = new List<AegisPathSegment>();
            var center = new Vector2(document.width * 0.5f, document.height * 0.5f);
            for (var i = 0; i < document.playerStarts.Length; i++)
            {
                var start = document.playerStarts[i];
                var startPoint = new Vector2(start.x + 0.5f, start.y + 0.5f);
                AddMeanderingPath(segments, startPoint, center, seed, i + 1);
            }
            return segments;
        }

        static void AddMeanderingPath(List<AegisPathSegment> segments, Vector2 start, Vector2 end, int seed, int salt)
        {
            var delta = end - start;
            var distance = delta.magnitude;
            if (distance <= 0.01f)
                return;

            var direction = delta / distance;
            var perpendicular = new Vector2(-direction.y, direction.x);
            var amplitude = Mathf.Min(10f, distance * 0.11f);
            var previous = start;
            const int steps = 8;
            for (var i = 1; i <= steps; i++)
            {
                var t = i / (float)steps;
                var point = Vector2.Lerp(start, end, t);
                if (i < steps)
                {
                    var fade = Mathf.Sin(t * Mathf.PI);
                    var offset = (Hash01(seed ^ (salt * 0x45D9F3B), i * 37, i * 91) - 0.5f) * 2f * amplitude * fade;
                    point += perpendicular * offset;
                }

                segments.Add(new AegisPathSegment(previous, point));
                previous = point;
            }
        }

        static List<AegisRiverSegment> BuildVisualRiverSegments(AegisVisualMapDocument document, AegisVisualMapLookup lookup)
        {
            var boundsSet = false;
            var minX = document.width;
            var maxX = -1;
            var minY = document.height;
            var maxY = -1;
            for (var y = 0; y < document.height; y++)
                for (var x = 0; x < document.width; x++)
                    if (lookup.TerrainAt(x, y) == "water")
                    {
                        boundsSet = true;
                        if (x < minX)
                            minX = x;
                        if (x > maxX)
                            maxX = x;
                        if (y < minY)
                            minY = y;
                        if (y > maxY)
                            maxY = y;
                    }

            if (!boundsSet)
                return new List<AegisRiverSegment>();

            var verticalSpan = maxY - minY;
            var horizontalSpan = maxX - minX;
            if (verticalSpan >= horizontalSpan)
                return BuildRowRiverSegments(document, lookup);
            return BuildColumnRiverSegments(document, lookup);
        }

        static List<AegisRiverSegment> BuildRowRiverSegments(AegisVisualMapDocument document, AegisVisualMapLookup lookup)
        {
            var samples = new AegisRiverSample[document.height];
            for (var y = 0; y < document.height; y++)
            {
                var count = 0;
                var sum = 0f;
                var min = document.width;
                var max = -1;
                for (var x = 0; x < document.width; x++)
                    if (lookup.TerrainAt(x, y) == "water")
                    {
                        count++;
                        sum += x + 0.5f;
                        if (x < min)
                            min = x;
                        if (x > max)
                            max = x;
                    }

                if (count > 0)
                    samples[y] = new AegisRiverSample(sum / count, y + 0.5f, Mathf.Clamp((max - min + 1) * 0.5f + 0.72f, 1.25f, 4.4f));
            }

            return ConnectRiverSamples(SmoothRiverSamples(samples));
        }

        static List<AegisRiverSegment> BuildColumnRiverSegments(AegisVisualMapDocument document, AegisVisualMapLookup lookup)
        {
            var samples = new AegisRiverSample[document.width];
            for (var x = 0; x < document.width; x++)
            {
                var count = 0;
                var sum = 0f;
                var min = document.height;
                var max = -1;
                for (var y = 0; y < document.height; y++)
                    if (lookup.TerrainAt(x, y) == "water")
                    {
                        count++;
                        sum += y + 0.5f;
                        if (y < min)
                            min = y;
                        if (y > max)
                            max = y;
                    }

                if (count > 0)
                    samples[x] = new AegisRiverSample(x + 0.5f, sum / count, Mathf.Clamp((max - min + 1) * 0.5f + 0.72f, 1.25f, 4.4f));
            }

            return ConnectRiverSamples(SmoothRiverSamples(samples));
        }

        static AegisRiverSample[] SmoothRiverSamples(AegisRiverSample[] samples)
        {
            var smoothed = new AegisRiverSample[samples.Length];
            for (var i = 0; i < samples.Length; i++)
            {
                if (samples[i] == null)
                    continue;

                var weight = 0f;
                var x = 0f;
                var y = 0f;
                var radius = 0f;
                for (var offset = -2; offset <= 2; offset++)
                {
                    var index = i + offset;
                    if (index < 0 || index >= samples.Length || samples[index] == null)
                        continue;
                    var w = offset == 0 ? 3f : offset == -1 || offset == 1 ? 2f : 1f;
                    weight += w;
                    x += samples[index].X * w;
                    y += samples[index].Y * w;
                    radius += samples[index].Radius * w;
                }

                smoothed[i] = new AegisRiverSample(x / weight, y / weight, radius / weight);
            }

            return smoothed;
        }

        static List<AegisRiverSegment> ConnectRiverSamples(AegisRiverSample[] samples)
        {
            var segments = new List<AegisRiverSegment>();
            AegisRiverSample previous = null;
            for (var i = 0; i < samples.Length; i++)
            {
                var sample = samples[i];
                if (sample == null)
                    continue;
                if (previous != null)
                {
                    var gap = Vector2.Distance(previous.Point, sample.Point);
                    if (gap <= 2.4f)
                        segments.Add(new AegisRiverSegment(previous.Point, sample.Point, (previous.Radius + sample.Radius) * 0.5f, 1f));
                    else if (gap <= 9f)
                        segments.Add(new AegisRiverSegment(previous.Point, sample.Point, (previous.Radius + sample.Radius) * 0.34f, 0.34f));
                }
                previous = sample;
            }

            return segments;
        }

        static float DistanceToPaths(List<AegisPathSegment> paths, float x, float y)
        {
            if (paths == null || paths.Count == 0)
                return 9999f;
            var point = new Vector2(x, y);
            var min = 9999f;
            for (var i = 0; i < paths.Count; i++)
                min = Mathf.Min(min, DistanceToSegment(point, paths[i].A, paths[i].B));
            return min;
        }

        static float WaterInfluence(AegisVisualMapLookup lookup, List<AegisRiverSegment> rivers, float x, float y)
        {
            var cellInfluence = CellWaterInfluence(lookup, x, y);
            if (rivers == null || rivers.Count == 0)
                return cellInfluence;

            var riverInfluence = RiverInfluence(rivers, x, y);
            return Mathf.Max(riverInfluence, cellInfluence * 0.38f);
        }

        static float RiverInfluence(List<AegisRiverSegment> rivers, float x, float y)
        {
            var point = new Vector2(x, y);
            var influence = 0f;
            for (var i = 0; i < rivers.Count; i++)
            {
                var signedDistance = DistanceToSegment(point, rivers[i].A, rivers[i].B) - rivers[i].Radius;
                var segmentInfluence = RiverSegmentInfluence(signedDistance);
                if (segmentInfluence > rivers[i].MaxInfluence)
                    segmentInfluence = rivers[i].MaxInfluence;
                if (segmentInfluence > influence)
                    influence = segmentInfluence;
            }

            return influence;
        }

        static float RiverSegmentInfluence(float nearestSignedDistance)
        {
            const float bankWidth = 1.65f;
            if (nearestSignedDistance > bankWidth)
                return 0f;
            if (nearestSignedDistance > 0f)
                return Mathf.Lerp(0.03f, 0.41f, Mathf.SmoothStep(0f, 1f, 1f - nearestSignedDistance / bankWidth));

            var deepest = Mathf.Clamp01((-nearestSignedDistance + 0.18f) / 2.1f);
            return Mathf.Lerp(0.44f, 1f, Mathf.SmoothStep(0f, 1f, deepest));
        }

        static float CellWaterInfluence(AegisVisualMapLookup lookup, float x, float y)
        {
            var cellX = Mathf.FloorToInt(x);
            var cellY = Mathf.FloorToInt(y);
            var nearest = 9999f;
            for (var yy = cellY - 3; yy <= cellY + 3; yy++)
                for (var xx = cellX - 3; xx <= cellX + 3; xx++)
                    if (lookup.TerrainAt(xx, yy) == "water")
                    {
                        var dx = x - (xx + 0.5f);
                        var dy = y - (yy + 0.5f);
                        var distance = Mathf.Sqrt(dx * dx + dy * dy);
                        if (distance < nearest)
                            nearest = distance;
                    }

            if (nearest > 2.35f)
                return 0f;
            return Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((2.35f - nearest) / 1.55f));
        }

        static Color FeatherTerrainEdge(AegisVisualMapDocument document, AegisVisualMapLookup lookup, AegisVisualBiomeProfile profile, string terrain, int cellX, int cellY, float fracX, float fracY, Color color)
        {
            if (terrain == "water" || terrain == document.defaultTerrainId)
                return color;

            var edge = 0f;
            if (lookup.TerrainAt(cellX - 1, cellY) != terrain)
                edge = Mathf.Max(edge, 1f - Mathf.SmoothStep(0f, 0.42f, fracX));
            if (lookup.TerrainAt(cellX + 1, cellY) != terrain)
                edge = Mathf.Max(edge, 1f - Mathf.SmoothStep(0f, 0.42f, 1f - fracX));
            if (lookup.TerrainAt(cellX, cellY - 1) != terrain)
                edge = Mathf.Max(edge, 1f - Mathf.SmoothStep(0f, 0.42f, fracY));
            if (lookup.TerrainAt(cellX, cellY + 1) != terrain)
                edge = Mathf.Max(edge, 1f - Mathf.SmoothStep(0f, 0.42f, 1f - fracY));

            if (edge <= 0f)
                return color;

            var baseColor = profile.ColorForTerrain(document.defaultTerrainId);
            return Color.Lerp(color, baseColor, edge * 0.72f);
        }

        static Color ApplyClusteredTerrainStrength(AegisVisualMapDocument document, AegisVisualMapLookup lookup, AegisVisualBiomeProfile profile, string terrain, int cellX, int cellY, Color color)
        {
            if (terrain == "water" || terrain == "cliff" || terrain == document.defaultTerrainId)
                return color;

            var same = 0;
            for (var y = -1; y <= 1; y++)
                for (var x = -1; x <= 1; x++)
                    if (lookup.TerrainAt(cellX + x, cellY + y) == terrain)
                        same++;

            var strength = Mathf.Clamp01((same - 1f) / 7f);
            strength = Mathf.Lerp(0.18f, 0.72f, strength);
            if (terrain == "rough")
                strength *= 0.78f;

            var baseColor = profile.ColorForTerrain(document.defaultTerrainId);
            return Color.Lerp(baseColor, color, strength);
        }

        static float ResourceGroundInfluence(List<AegisOreVisualField> fields, float x, float y, int seed)
        {
            if (fields == null || fields.Count == 0)
                return 0f;

            var influence = 0f;
            for (var i = 0; i < fields.Count; i++)
            {
                var dx = (x - fields[i].Center.x) / fields[i].RadiusX;
                var dy = (y - fields[i].Center.y) / fields[i].RadiusY;
                var distance = Mathf.Sqrt(dx * dx + dy * dy);
                if (distance > 1.35f)
                    continue;

                var fieldInfluence = Mathf.SmoothStep(0f, 1f, 1f - distance / 1.35f);
                if (fieldInfluence > influence)
                    influence = fieldInfluence;
            }

            if (influence <= 0f)
                return 0f;

            var brokenEdge = Mathf.Lerp(0.68f, 1.04f, Hash01(seed ^ 0x0E99, Mathf.FloorToInt(x * 3f), Mathf.FloorToInt(y * 3f)));
            return Mathf.Clamp01(influence * brokenEdge);
        }

        static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            var length = Vector2.Dot(ab, ab);
            if (length <= 0.0001f)
                return Vector2.Distance(point, a);
            var t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / length);
            return Vector2.Distance(point, a + ab * t);
        }

        static Vector3 CellCenter(int x, int y, float elevation)
        {
            return new Vector3((x + 0.5f) * CellSize, elevation, (y + 0.5f) * CellSize);
        }

        static float DirectionAngle(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f)
                return 0f;
            return Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        }

        static Color AddNoise(Color color, float noise, float amount)
        {
            var delta = (noise - 0.5f) * amount;
            return new Color(Mathf.Clamp01(color.r + delta), Mathf.Clamp01(color.g + delta), Mathf.Clamp01(color.b + delta), 1f);
        }

        static Color32 ToColor32(Color color)
        {
            return new Color32((byte)(Mathf.Clamp01(color.r) * 255f), (byte)(Mathf.Clamp01(color.g) * 255f), (byte)(Mathf.Clamp01(color.b) * 255f), 255);
        }

        static void AssignMaterialAndStripCollider(GameObject go, Material material)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
            var collider = go.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);
        }

        static string Sanitize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "aegis_map";
            var chars = text.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
                if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '_' && chars[i] != '-')
                    chars[i] = '_';
            return new string(chars);
        }

        static int HashRange(int seed, int x, int y, int maxExclusive)
        {
            if (maxExclusive <= 0)
                return 0;
            return Mathf.Abs(Hash(seed, x, y)) % maxExclusive;
        }

        static float Hash01(int seed, int x, int y)
        {
            var value = Hash(seed, x, y) & 0x7FFFFFFF;
            return value / 2147483647f;
        }

        static int Hash(int seed, int x, int y)
        {
            unchecked
            {
                var h = seed == 0 ? 2166136261u : (uint)seed;
                h ^= (uint)(x * 374761393);
                h = (h << 13) | (h >> 19);
                h ^= (uint)(y * 668265263);
                h *= 1274126177u;
                h ^= h >> 16;
                return (int)h;
            }
        }

        sealed class AegisPathSegment
        {
            public readonly Vector2 A;
            public readonly Vector2 B;

            public AegisPathSegment(Vector2 a, Vector2 b)
            {
                A = a;
                B = b;
            }
        }

        sealed class AegisRiverSegment
        {
            public readonly Vector2 A;
            public readonly Vector2 B;
            public readonly float Radius;
            public readonly float MaxInfluence;

            public AegisRiverSegment(Vector2 a, Vector2 b, float radius, float maxInfluence)
            {
                A = a;
                B = b;
                Radius = radius;
                MaxInfluence = maxInfluence;
            }
        }

        sealed class AegisRiverSample
        {
            public readonly Vector2 Point;
            public readonly float Radius;

            public float X
            {
                get { return Point.x; }
            }

            public float Y
            {
                get { return Point.y; }
            }

            public AegisRiverSample(float x, float y, float radius)
            {
                Point = new Vector2(x, y);
                Radius = radius;
            }
        }

        sealed class AegisOreVisualField
        {
            public readonly Vector2 Center;
            public readonly float RadiusX;
            public readonly float RadiusY;

            public AegisOreVisualField(Vector2 center, float radiusX, float radiusY)
            {
                Center = center;
                RadiusX = radiusX;
                RadiusY = radiusY;
            }
        }

        sealed class AegisOreVisualFieldBuilder
        {
            float sumX;
            float sumY;
            int count;
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            public void Add(float x, float y)
            {
                sumX += x;
                sumY += y;
                count++;
                if (x < minX)
                    minX = x;
                if (x > maxX)
                    maxX = x;
                if (y < minY)
                    minY = y;
                if (y > maxY)
                    maxY = y;
            }

            public AegisOreVisualField Build()
            {
                if (count <= 0)
                    return null;

                var center = new Vector2(sumX / count, sumY / count);
                var radiusX = Mathf.Clamp((maxX - minX) * 0.5f + 2.6f, 2.8f, 7.5f);
                var radiusY = Mathf.Clamp((maxY - minY) * 0.5f + 2.6f, 2.8f, 7.5f);
                return new AegisOreVisualField(center, radiusX, radiusY);
            }
        }

        sealed class AegisVisualMapLookup
        {
            readonly AegisVisualMapDocument document;
            readonly Dictionary<int, string> terrain = new Dictionary<int, string>();
            readonly HashSet<int> resources = new HashSet<int>();
            readonly HashSet<int> blockers = new HashSet<int>();

            public AegisVisualMapLookup(AegisVisualMapDocument document)
            {
                this.document = document;
                for (var i = 0; i < document.terrainBase.Length; i++)
                    terrain[Key(document.terrainBase[i].x, document.terrainBase[i].y)] = document.terrainBase[i].terrainId ?? string.Empty;
                for (var i = 0; i < document.resources.Length; i++)
                    resources.Add(Key(document.resources[i].x, document.resources[i].y));
                for (var i = 0; i < document.blockers.Length; i++)
                    blockers.Add(Key(document.blockers[i].x, document.blockers[i].y));
            }

            public string TerrainAt(int x, int y)
            {
                string value;
                return terrain.TryGetValue(Key(x, y), out value) ? value : document.defaultTerrainId;
            }

            public bool HasResource(int x, int y)
            {
                return resources.Contains(Key(x, y));
            }

            public bool IsCliffLike(int x, int y)
            {
                return TerrainAt(x, y) == "cliff" || blockers.Contains(Key(x, y));
            }

            public bool IsBoundary(int x, int y)
            {
                if (!IsCliffLike(x, y))
                    return false;
                return !IsCliffLike(x + 1, y) || !IsCliffLike(x - 1, y) || !IsCliffLike(x, y + 1) || !IsCliffLike(x, y - 1);
            }

            public bool IsNearStart(int x, int y, int radius)
            {
                for (var i = 0; i < document.playerStarts.Length; i++)
                {
                    var dx = document.playerStarts[i].x - x;
                    var dy = document.playerStarts[i].y - y;
                    if (dx * dx + dy * dy <= radius * radius)
                        return true;
                }
                return false;
            }

            public int DistanceToTerrain(int x, int y, string terrainId, int radius)
            {
                for (var r = 0; r <= radius; r++)
                    for (var yy = y - r; yy <= y + r; yy++)
                        for (var xx = x - r; xx <= x + r; xx++)
                            if (Mathf.Abs(xx - x) + Mathf.Abs(yy - y) <= r && InBounds(xx, yy) && TerrainAt(xx, yy) == terrainId)
                                return r;
                return -1;
            }

            public int DistanceToNotTerrain(int x, int y, string terrainId, int radius)
            {
                for (var r = 0; r <= radius; r++)
                    for (var yy = y - r; yy <= y + r; yy++)
                        for (var xx = x - r; xx <= x + r; xx++)
                            if (Mathf.Abs(xx - x) + Mathf.Abs(yy - y) <= r && InBounds(xx, yy) && TerrainAt(xx, yy) != terrainId)
                                return r;
                return -1;
            }

            public int DistanceToCliff(int x, int y, int radius)
            {
                for (var r = 0; r <= radius; r++)
                    for (var yy = y - r; yy <= y + r; yy++)
                        for (var xx = x - r; xx <= x + r; xx++)
                            if (Mathf.Abs(xx - x) + Mathf.Abs(yy - y) <= r && InBounds(xx, yy) && IsCliffLike(xx, yy))
                                return r;
                return -1;
            }

            bool InBounds(int x, int y)
            {
                return x >= 0 && y >= 0 && x < document.width && y < document.height;
            }

            int Key(int x, int y)
            {
                return y * document.width + x;
            }
        }
    }

    [Serializable]
    public sealed class AegisVisualMapDocument
    {
        public string formatVersion;
        public string mapId;
        public string displayName;
        public int width;
        public int height;
        public string defaultTerrainId;
        public AegisVisualTerrainCell[] terrainBase;
        public AegisVisualBlockerCell[] blockers;
        public AegisVisualResourceCell[] resources;
        public AegisVisualPlayerStart[] playerStarts;

        public static AegisVisualMapDocument Load(string assetPath)
        {
            return JsonUtility.FromJson<AegisVisualMapDocument>(File.ReadAllText(assetPath));
        }

        public void Normalize()
        {
            if (string.IsNullOrEmpty(mapId))
                mapId = "aegis_visual_map";
            if (string.IsNullOrEmpty(defaultTerrainId))
                defaultTerrainId = "clear";
            terrainBase = terrainBase ?? new AegisVisualTerrainCell[0];
            blockers = blockers ?? new AegisVisualBlockerCell[0];
            resources = resources ?? new AegisVisualResourceCell[0];
            playerStarts = playerStarts ?? new AegisVisualPlayerStart[0];
        }

        public int ReadSeed()
        {
            unchecked
            {
                var hash = 2166136261u;
                hash = Mix(hash, mapId);
                hash = Mix(hash, width);
                hash = Mix(hash, height);
                return (int)(hash & 0x7FFFFFFF);
            }
        }

        static uint Mix(uint hash, string value)
        {
            value = value ?? string.Empty;
            for (var i = 0; i < value.Length; i++)
            {
                hash ^= value[i];
                hash *= 16777619u;
            }
            return hash;
        }

        static uint Mix(uint hash, int value)
        {
            hash ^= (uint)value;
            hash *= 16777619u;
            return hash;
        }
    }

    [Serializable]
    public sealed class AegisVisualTerrainCell
    {
        public int x;
        public int y;
        public string terrainId;
    }

    [Serializable]
    public sealed class AegisVisualBlockerCell
    {
        public int x;
        public int y;
        public bool blocksGround;
        public string reason;
    }

    [Serializable]
    public sealed class AegisVisualResourceCell
    {
        public string fieldId;
        public int x;
        public int y;
        public string resourceKind;
        public int amount;
    }

    [Serializable]
    public sealed class AegisVisualPlayerStart
    {
        public int playerId;
        public int x;
        public int y;
        public string name;
    }

    sealed class AegisVisualBiomeProfile
    {
        public readonly string Name;
        public readonly Color Grass;
        public readonly Color Forest;
        public readonly Color Rough;
        public readonly Color DirtPath;
        public readonly Color GravelPath;
        public readonly Color Water;
        public readonly Color DeepWater;
        public readonly Color WaterHighlight;
        public readonly Color MuddyBank;
        public readonly Color Cliff;
        public readonly Color OreGround;

        AegisVisualBiomeProfile(string name, Color grass, Color forest, Color rough, Color dirtPath, Color gravelPath, Color water, Color deepWater, Color waterHighlight, Color muddyBank, Color cliff, Color oreGround)
        {
            Name = name;
            Grass = grass;
            Forest = forest;
            Rough = rough;
            DirtPath = dirtPath;
            GravelPath = gravelPath;
            Water = water;
            DeepWater = deepWater;
            WaterHighlight = waterHighlight;
            MuddyBank = muddyBank;
            Cliff = cliff;
            OreGround = oreGround;
        }

        public static AegisVisualBiomeProfile ForDocument(AegisVisualMapDocument document)
        {
            var id = (document.displayName + " " + document.mapId).ToLowerInvariant();
            if (id.Contains("desert"))
                return new AegisVisualBiomeProfile("desert", C(0.56f, 0.48f, 0.31f), C(0.32f, 0.35f, 0.19f), C(0.49f, 0.43f, 0.34f), C(0.52f, 0.38f, 0.23f), C(0.42f, 0.40f, 0.35f), C(0.18f, 0.36f, 0.43f), C(0.08f, 0.19f, 0.26f), C(0.30f, 0.48f, 0.52f), C(0.35f, 0.25f, 0.16f), C(0.48f, 0.46f, 0.40f), C(0.58f, 0.46f, 0.18f));
            if (id.Contains("tundra"))
                return new AegisVisualBiomeProfile("tundra", C(0.48f, 0.54f, 0.50f), C(0.24f, 0.36f, 0.32f), C(0.55f, 0.56f, 0.52f), C(0.40f, 0.37f, 0.32f), C(0.46f, 0.48f, 0.48f), C(0.19f, 0.38f, 0.50f), C(0.08f, 0.17f, 0.28f), C(0.36f, 0.54f, 0.62f), C(0.42f, 0.38f, 0.32f), C(0.58f, 0.60f, 0.58f), C(0.60f, 0.50f, 0.20f));
            if (id.Contains("volcanic"))
                return new AegisVisualBiomeProfile("volcanic", C(0.21f, 0.22f, 0.19f), C(0.12f, 0.17f, 0.12f), C(0.26f, 0.24f, 0.23f), C(0.31f, 0.23f, 0.18f), C(0.35f, 0.34f, 0.31f), C(0.18f, 0.22f, 0.26f), C(0.04f, 0.06f, 0.08f), C(0.30f, 0.34f, 0.38f), C(0.27f, 0.18f, 0.12f), C(0.34f, 0.33f, 0.32f), C(0.62f, 0.36f, 0.15f));
            if (id.Contains("rocky") || id.Contains("wasteland"))
                return new AegisVisualBiomeProfile("rocky", C(0.35f, 0.39f, 0.30f), C(0.18f, 0.27f, 0.16f), C(0.45f, 0.43f, 0.38f), C(0.42f, 0.34f, 0.25f), C(0.44f, 0.43f, 0.39f), C(0.15f, 0.31f, 0.40f), C(0.05f, 0.13f, 0.19f), C(0.27f, 0.43f, 0.48f), C(0.28f, 0.23f, 0.16f), C(0.50f, 0.49f, 0.45f), C(0.58f, 0.47f, 0.19f));
            return new AegisVisualBiomeProfile("forest", C(0.16f, 0.29f, 0.14f), C(0.06f, 0.19f, 0.10f), C(0.30f, 0.29f, 0.22f), C(0.34f, 0.25f, 0.16f), C(0.31f, 0.30f, 0.25f), C(0.09f, 0.24f, 0.29f), C(0.03f, 0.10f, 0.14f), C(0.20f, 0.39f, 0.42f), C(0.23f, 0.17f, 0.10f), C(0.43f, 0.43f, 0.39f), C(0.55f, 0.42f, 0.15f));
        }

        public Color ColorForTerrain(string terrain)
        {
            if (terrain == "forest")
                return Forest;
            if (terrain == "rough")
                return Rough;
            if (terrain == "road")
                return Grass;
            if (terrain == "water")
                return Water;
            if (terrain == "cliff")
                return Cliff;
            if (terrain == "ore")
                return OreGround;
            return Grass;
        }

        static Color C(float r, float g, float b)
        {
            return new Color(r, g, b, 1f);
        }
    }

    sealed class AegisVisualMaterialSet
    {
        public Material Ground;
        public Material Cliff;
        public Material Boulder;
        public Material Pebble;
        public Material Ore;
        public Material Vegetation;
        public Material Concrete;
        public Material ConcretePanel;
        public Material ConcreteTrim;
        public Material ConcreteLine;
        public Material ConcreteGrime;
        public Material Crater;
        public Material CraterRim;
        public Material Dirt;
        public Material RoadDust;
        public Material RoadRut;
        public Material RoadScuff;
        public Material MuddyShoreline;
        public Material WaterHighlightDecal;
        public Material OreDust;
        public Material ResourceGlint;
        public Material Shadow;

        public static AegisVisualMaterialSet LoadOrCreate(AegisVisualBiomeProfile profile, bool persistAssets)
        {
            return new AegisVisualMaterialSet
            {
                Ground = AegisMapArtPack.Material("aegis_visual_ground.mat", Color.white, true, persistAssets),
                Cliff = AegisMapArtPack.Material("aegis_visual_cliff.mat", Color.Lerp(profile.Cliff, new Color(0.22f, 0.22f, 0.20f, 1f), 0.28f), true, persistAssets, false, "Textures/aegis_cliff_rock_albedo.png", "Textures/aegis_cliff_rock_normal.png", "Textures/aegis_cliff_rock_roughness_ao.png"),
                Boulder = AegisMapArtPack.Material("aegis_visual_boulder.mat", Color.Lerp(profile.Cliff, new Color(0.28f, 0.29f, 0.27f, 1f), 0.45f), true, persistAssets, false, "Textures/aegis_boulder_stone_albedo.png", "Textures/aegis_boulder_stone_normal.png", "Textures/aegis_boulder_stone_roughness_ao.png"),
                Pebble = AegisMapArtPack.Material("aegis_visual_pebble.mat", Color.Lerp(profile.Cliff, profile.Rough, 0.62f), true, persistAssets, false, "Textures/aegis_boulder_stone_albedo.png", "Textures/aegis_boulder_stone_normal.png", "Textures/aegis_boulder_stone_roughness_ao.png"),
                Ore = AegisMapArtPack.Material("aegis_visual_ore.mat", profile.OreGround, true, persistAssets, false, "Textures/aegis_gold_ore_albedo.png", "Textures/aegis_gold_ore_normal.png", "Textures/aegis_gold_ore_roughness_ao.png"),
                Vegetation = AegisMapArtPack.Material("aegis_visual_vegetation.mat", Color.Lerp(profile.Forest, profile.Grass, 0.22f), true, persistAssets, false, "Textures/aegis_vegetation_albedo.png", "Textures/aegis_vegetation_normal.png", "Textures/aegis_vegetation_roughness_ao.png"),
                Concrete = AegisMapArtPack.Material("aegis_visual_concrete_pad.mat", new Color(0.82f, 0.84f, 0.80f, 1f), true, persistAssets, false, "Terrain/concrete_base_pad_albedo.png", "Terrain/concrete_base_pad_normal.png", "Terrain/concrete_base_pad_roughness_ao.png"),
                ConcretePanel = AegisMapArtPack.Material("aegis_visual_concrete_panel.mat", new Color(0.88f, 0.90f, 0.86f, 1f), true, persistAssets, false, "Terrain/concrete_panel_albedo.png", "Terrain/concrete_panel_normal.png", "Terrain/concrete_panel_roughness_ao.png"),
                ConcreteTrim = AegisMapArtPack.Material("aegis_visual_concrete_trim.mat", new Color(0.92f, 0.82f, 0.58f, 1f), true, persistAssets, false, "Terrain/concrete_trim_albedo.png", "Terrain/concrete_trim_normal.png", "Terrain/concrete_trim_roughness_ao.png"),
                ConcreteLine = AegisMapArtPack.Material("aegis_visual_concrete_line.mat", new Color(0.16f, 0.17f, 0.16f, 0.72f), true, persistAssets, true, "Decals/concrete_seam_line.png"),
                ConcreteGrime = AegisMapArtPack.Material("aegis_visual_concrete_grime.mat", new Color(0.08f, 0.075f, 0.06f, 0.48f), true, persistAssets, true, "Decals/concrete_grime_01.png"),
                Crater = AegisMapArtPack.Material("aegis_visual_crater.mat", new Color(0.045f, 0.038f, 0.030f, 0.88f), true, persistAssets, true, "Decals/crater_large_01.png"),
                CraterRim = AegisMapArtPack.Material("aegis_visual_crater_rim.mat", new Color(0.16f, 0.115f, 0.072f, 0.88f), true, persistAssets, true, "Decals/crater_medium_01.png"),
                Dirt = AegisMapArtPack.Material("aegis_visual_dirt_blend.mat", Color.Lerp(profile.DirtPath, profile.Grass, 0.28f), true, persistAssets, false, "Terrain/dirt_path_albedo.png", "Terrain/dirt_path_normal.png", "Terrain/dirt_path_roughness_ao.png"),
                RoadDust = AegisMapArtPack.Material("aegis_visual_road_dust_decal.mat", new Color(profile.DirtPath.r, profile.DirtPath.g, profile.DirtPath.b, 0.42f), true, persistAssets, true, "Decals/soft_dust_overlay.png"),
                RoadRut = AegisMapArtPack.Material("aegis_visual_road_rut_decal.mat", new Color(0.12f, 0.105f, 0.075f, 0.38f), true, persistAssets, true, "Decals/tire_rut_left.png"),
                RoadScuff = AegisMapArtPack.Material("aegis_visual_road_scuff_decal.mat", new Color(profile.GravelPath.r, profile.GravelPath.g, profile.GravelPath.b, 0.46f), true, persistAssets, true, "Decals/gravel_scuff_01.png"),
                MuddyShoreline = AegisMapArtPack.Material("aegis_visual_muddy_shoreline_decal.mat", new Color(profile.MuddyBank.r, profile.MuddyBank.g, profile.MuddyBank.b, 0.46f), true, persistAssets, true, "Decals/muddy_shoreline_01.png"),
                WaterHighlightDecal = AegisMapArtPack.Material("aegis_visual_water_highlight_decal.mat", new Color(profile.WaterHighlight.r, profile.WaterHighlight.g, profile.WaterHighlight.b, 0.38f), true, persistAssets, true, "Decals/water_highlight_streaks.png"),
                OreDust = AegisMapArtPack.Material("aegis_visual_ore_dust_decal.mat", new Color(profile.OreGround.r, profile.OreGround.g, profile.OreGround.b, 0.42f), true, persistAssets, true, "Decals/ore_dust_soft_01.png"),
                ResourceGlint = AegisMapArtPack.Material("aegis_visual_resource_glint_decal.mat", new Color(1f, 0.85f, 0.38f, 0.46f), true, persistAssets, true, "Decals/resource_glint_01.png"),
                Shadow = AegisMapArtPack.Material("aegis_visual_contact_shadow.mat", new Color(0.02f, 0.025f, 0.018f, 0.42f), false, persistAssets, true)
            };
        }

        static Material Material(string fileName, Color color, bool textured, bool persistAssets, bool transparent = false)
        {
            if (!persistAssets)
            {
                var transient = new Material(FindShader(textured));
                transient.name = Path.GetFileNameWithoutExtension(fileName) + "_transient";
                ConfigureMaterial(transient, color, textured, transparent);
                return transient;
            }

            var path = AegisMapEditorPaths.VisualAssetsFolder + "/" + fileName;
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(FindShader(textured));
                AssetDatabase.CreateAsset(material, path);
            }
            ConfigureMaterial(material, color, textured, transparent);
            EditorUtility.SetDirty(material);
            return material;
        }

        static void ConfigureMaterial(Material material, Color color, bool textured, bool transparent)
        {
            material.color = color;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
            if (material.HasProperty("_Metallic"))
                material.SetFloat("_Metallic", 0f);
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", textured ? 0f : 0.12f);
            if (material.HasProperty("_Glossiness"))
                material.SetFloat("_Glossiness", textured ? 0f : 0.08f);
            if (material.HasProperty("_SpecColor"))
                material.SetColor("_SpecColor", new Color(0.02f, 0.02f, 0.018f, 1f));

            if (transparent)
            {
                if (material.HasProperty("_Surface"))
                    material.SetFloat("_Surface", 1f);
                if (material.HasProperty("_Mode"))
                    material.SetFloat("_Mode", 3f);
                if (material.HasProperty("_SrcBlend"))
                    material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                if (material.HasProperty("_DstBlend"))
                    material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                if (material.HasProperty("_ZWrite"))
                    material.SetFloat("_ZWrite", 0f);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.renderQueue = 3000;
            }
            else
            {
                if (material.HasProperty("_ZWrite"))
                    material.SetFloat("_ZWrite", 1f);
                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.renderQueue = -1;
            }
        }

        static Shader FindShader(bool textured)
        {
            var shader = textured ? Shader.Find("Universal Render Pipeline/Unlit") : null;
            if (shader == null && textured)
                shader = Shader.Find("Unlit/Texture");
            if (shader == null)
                shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            if (shader == null)
                shader = Shader.Find("Unlit/Color");
            return shader;
        }
    }
}
#endif
