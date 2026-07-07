#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using ProjectAegisRTS.UnityClient.MapEditor;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class AegisMapVisualBuilder
    {
        const float CellSize = 1f;
        const int PixelsPerCell = 4;
        const int MaxCliffRocks = 1400;
        const int MaxNearCliffBoulders = 650;
        const int MaxVegetation = 850;
        const int MaxCraters = 120;
        const int MaxRoadPebbles = 450;
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
            RequireChild(root.transform, "Blended Terrain Surface");
            RequireChild(root.transform, "Base Pads");
            RequireChild(root.transform, "Cliff Rock Chains");
            RequireChild(root.transform, "Ore Clusters");
            RequireChild(root.transform, "Deterministic Detail Scatter");
            UnityEngine.Object.DestroyImmediate(root);
            Debug.Log("Aegis map visual builder sample validation passed.");
        }

        public static void RenderSamplePreviewForBatch()
        {
            var samplePath = AegisMapEditorPaths.SamplesFolder + "/sample_ai_medium_rocky_4p_chokepoint.aegismap.json";
            var document = AegisVisualMapDocument.Load(samplePath);
            if (document == null)
                throw new InvalidOperationException("Visual builder render could not load " + samplePath + ".");

            var root = BuildScene(document, samplePath, false);
            var light = new GameObject("Aegis Visual Preview Sun");
            var lightComponent = light.AddComponent<Light>();
            lightComponent.type = LightType.Directional;
            lightComponent.intensity = 1.25f;
            light.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            var cameraObject = new GameObject("Aegis Visual Preview Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.06f, 0.08f, 0.07f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = Mathf.Max(document.width, document.height) * 0.42f;
            camera.transform.position = new Vector3(document.width * 0.5f, 105f, -document.height * 0.12f);
            camera.transform.rotation = Quaternion.Euler(64f, 0f, 0f);

            var renderTexture = new RenderTexture(1400, 1000, 24, RenderTextureFormat.ARGB32);
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
            var outputPath = Path.Combine(outputDir, "aegis_visual_builder_sample.png");
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
            document.Normalize();
            var safeMapId = Sanitize(document.mapId);
            var seed = document.ReadSeed();
            var profile = AegisVisualBiomeProfile.ForDocument(document);

            var root = new GameObject("Aegis Visual Map - " + safeMapId);
            root.transform.position = Vector3.zero;
            var marker = root.AddComponent<AegisMapVisualScene>();
            marker.MapId = document.mapId;
            marker.SourceAssetPath = sourcePath;
            marker.Width = document.width;
            marker.Height = document.height;
            marker.Seed = seed;
            marker.Biome = profile.Name;

            var lookup = new AegisVisualMapLookup(document);
            var paths = BuildGameplayPathSegments(document);
            var materials = AegisVisualMaterialSet.LoadOrCreate(profile, persistAssets);
            BuildTerrainPlane(root.transform, document, lookup, paths, profile, materials, safeMapId, seed, persistAssets);
            BuildBasePads(root.transform, document, materials);
            BuildCliffRidges(root.transform, document, lookup, materials, seed);
            BuildOreClusters(root.transform, document, materials, seed);
            BuildScatter(root.transform, document, lookup, paths, materials, seed);
            return root;
        }

        static void BuildTerrainPlane(Transform root, AegisVisualMapDocument document, AegisVisualMapLookup lookup, List<AegisPathSegment> paths, AegisVisualBiomeProfile profile, AegisVisualMaterialSet materials, string safeMapId, int seed, bool persistAssets)
        {
            var texture = CreateTerrainTexture(document, lookup, paths, profile, seed);
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

        static Texture2D CreateTerrainTexture(AegisVisualMapDocument document, AegisVisualMapLookup lookup, List<AegisPathSegment> paths, AegisVisualBiomeProfile profile, int seed)
        {
            var width = document.width * PixelsPerCell;
            var height = document.height * PixelsPerCell;
            var pixels = new Color32[width * height];
            var waterDistanceByCell = new int[document.width * document.height];
            var bankDistanceByCell = new int[document.width * document.height];
            for (var cy = 0; cy < document.height; cy++)
                for (var cx = 0; cx < document.width; cx++)
                {
                    var key = cy * document.width + cx;
                    waterDistanceByCell[key] = lookup.DistanceToTerrain(cx, cy, "water", 5);
                    bankDistanceByCell[key] = lookup.DistanceToNotTerrain(cx, cy, "water", 4);
                }

            for (var py = 0; py < height; py++)
            {
                for (var px = 0; px < width; px++)
                {
                    var cellX = px / PixelsPerCell;
                    var cellY = py / PixelsPerCell;
                    var localX = (px + 0.5f) / PixelsPerCell;
                    var localY = (py + 0.5f) / PixelsPerCell;
                    var terrain = lookup.TerrainAt(cellX, cellY);
                    var color = profile.ColorForTerrain(terrain);
                    color = AddNoise(color, Hash01(seed, px, py), 0.13f);

                    var cellKey = cellY * document.width + cellX;
                    var waterDistance = waterDistanceByCell[cellKey];
                    if (waterDistance >= 0 && waterDistance <= 4 && terrain != "water")
                    {
                        var t = 1f - waterDistance / 4f;
                        color = Color.Lerp(color, profile.MuddyBank, t * 0.7f);
                    }

                    var pathDistance = DistanceToPaths(paths, localX, localY);
                    if (pathDistance < 3.1f && terrain != "water")
                    {
                        var edge = Mathf.Clamp01(1f - pathDistance / 3.1f);
                        var worn = Color.Lerp(profile.DirtPath, profile.GravelPath, Mathf.Clamp01(1f - pathDistance / 1.1f));
                        color = Color.Lerp(color, worn, edge * 0.82f);
                    }

                    if (terrain == "water")
                    {
                        var bank = bankDistanceByCell[cellKey];
                        if (bank >= 0 && bank < 3)
                            color = Color.Lerp(profile.MuddyBank, profile.Water, bank / 3f);
                        else
                            color = Color.Lerp(profile.Water, profile.DeepWater, Hash01(seed ^ 0x515EA, cellX, cellY) * 0.35f);
                    }

                    if (lookup.HasResource(cellX, cellY))
                        color = Color.Lerp(color, profile.OreGround, 0.7f);

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

        static void BuildBasePads(Transform root, AegisVisualMapDocument document, AegisVisualMaterialSet materials)
        {
            var parent = new GameObject("Base Pads").transform;
            parent.SetParent(root, false);
            for (var i = 0; i < document.playerStarts.Length; i++)
            {
                var start = document.playerStarts[i];
                var pad = CreateQuad("Base Pad P" + start.playerId, 14f, 14f, materials.Concrete);
                pad.transform.SetParent(parent, false);
                pad.transform.position = CellCenter(start.x, start.y, 0.035f);

                var stripe = CreateQuad("Base Pad Trim P" + start.playerId, 10f, 1.2f, materials.ConcreteTrim);
                stripe.transform.SetParent(parent, false);
                stripe.transform.position = CellCenter(start.x, start.y + 4, 0.04f);
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
                if (Hash01(seed ^ 0xC11FF, cell.x, cell.y) > 0.34f)
                    continue;

                CreateRockCluster(parent, "Cliff Ridge", cell.x, cell.y, materials.Cliff, seed, 1.8f, 3.7f, 2 + HashRange(seed, cell.x, cell.y, 3));
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
                for (var c = 0; c < chunkCount; c++)
                {
                    var ore = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    ore.name = "Ore Chunk";
                    ore.transform.SetParent(parent, false);
                    var ox = (Hash01(seed, resource.x + c * 11, resource.y) - 0.5f) * 0.72f;
                    var oz = (Hash01(seed, resource.x, resource.y + c * 17) - 0.5f) * 0.72f;
                    ore.transform.position = CellCenter(resource.x, resource.y, 0.15f) + new Vector3(ox, 0f, oz);
                    ore.transform.rotation = Quaternion.Euler(0f, Hash01(seed, resource.x + c, resource.y - c) * 360f, 0f);
                    ore.transform.localScale = new Vector3(0.28f, 0.18f + Hash01(seed, resource.x, c) * 0.22f, 0.28f);
                    AssignMaterialAndStripCollider(ore, materials.Ore);
                }
            }
        }

        static void BuildScatter(Transform root, AegisVisualMapDocument document, AegisVisualMapLookup lookup, List<AegisPathSegment> paths, AegisVisualMaterialSet materials, int seed)
        {
            var parent = new GameObject("Deterministic Detail Scatter").transform;
            parent.SetParent(root, false);
            var boulders = 0;
            var vegetation = 0;
            var craters = 0;
            var pebbles = 0;

            for (var y = 2; y < document.height - 2; y += 2)
            {
                for (var x = 2; x < document.width - 2; x += 2)
                {
                    var terrain = lookup.TerrainAt(x, y);
                    if (terrain == "water" || lookup.HasResource(x, y) || lookup.IsNearStart(x, y, 9))
                        continue;

                    var cliffDistance = lookup.DistanceToCliff(x, y, 6);
                    var pathDistance = DistanceToPaths(paths, x + 0.5f, y + 0.5f);
                    var h = Hash01(seed ^ 0x5CA77E, x, y);
                    if (cliffDistance >= 0 && cliffDistance <= 3 && boulders < MaxNearCliffBoulders && h < 0.22f)
                    {
                        CreateRockCluster(parent, "Boulder Scatter", x, y, materials.Boulder, seed ^ 0xB011, 0.35f, 1.1f, 1 + HashRange(seed, x, y, 2));
                        boulders++;
                    }
                    else if ((terrain == "forest" || h < 0.055f) && vegetation < MaxVegetation)
                    {
                        CreateVegetation(parent, x, y, materials.Vegetation, seed);
                        vegetation++;
                    }
                    else if (pathDistance < 3.7f && pathDistance > 1.0f && pebbles < MaxRoadPebbles && h < 0.19f)
                    {
                        CreatePebble(parent, x, y, materials.Pebble, seed);
                        pebbles++;
                    }
                    else if (pathDistance > 7f && craters < MaxCraters && h > 0.994f)
                    {
                        CreateCrater(parent, x, y, materials.Crater);
                        craters++;
                    }
                }
            }
        }

        static void CreateRockCluster(Transform parent, string name, int cellX, int cellY, Material material, int seed, float minScale, float maxScale, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var rock = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                rock.name = name;
                rock.transform.SetParent(parent, false);
                var offset = new Vector3((Hash01(seed, cellX + i * 7, cellY) - 0.5f) * 1.2f, 0f, (Hash01(seed, cellX, cellY + i * 13) - 0.5f) * 1.2f);
                var height = Mathf.Lerp(minScale, maxScale, Hash01(seed ^ 0xA7A, cellX + i, cellY - i));
                rock.transform.position = CellCenter(cellX, cellY, height * 0.45f) + offset;
                rock.transform.localScale = new Vector3(height * 0.28f, height * 0.52f, height * 0.36f);
                rock.transform.rotation = Quaternion.Euler(Hash01(seed, cellX, i) * 12f - 6f, Hash01(seed, i, cellY) * 360f, Hash01(seed, cellY, i) * 12f - 6f);
                AssignMaterialAndStripCollider(rock, material);
            }
        }

        static void CreateVegetation(Transform parent, int cellX, int cellY, Material material, int seed)
        {
            var bush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bush.name = "Grass Bush";
            bush.transform.SetParent(parent, false);
            bush.transform.position = CellCenter(cellX, cellY, 0.22f);
            var scale = 0.45f + Hash01(seed, cellX, cellY) * 0.55f;
            bush.transform.localScale = new Vector3(scale, 0.34f + scale * 0.22f, scale);
            AssignMaterialAndStripCollider(bush, material);
        }

        static void CreatePebble(Transform parent, int cellX, int cellY, Material material, int seed)
        {
            var pebble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pebble.name = "Road Pebble";
            pebble.transform.SetParent(parent, false);
            pebble.transform.position = CellCenter(cellX, cellY, 0.06f);
            var scale = 0.12f + Hash01(seed, cellX, cellY) * 0.18f;
            pebble.transform.localScale = new Vector3(scale, scale * 0.55f, scale);
            AssignMaterialAndStripCollider(pebble, material);
        }

        static void CreateCrater(Transform parent, int cellX, int cellY, Material material)
        {
            var crater = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            crater.name = "Crater Decal";
            crater.transform.SetParent(parent, false);
            crater.transform.position = CellCenter(cellX, cellY, 0.045f);
            crater.transform.localScale = new Vector3(1.35f, 0.015f, 1.35f);
            AssignMaterialAndStripCollider(crater, material);
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

        static List<AegisPathSegment> BuildGameplayPathSegments(AegisVisualMapDocument document)
        {
            var segments = new List<AegisPathSegment>();
            var center = new Vector2(document.width * 0.5f, document.height * 0.5f);
            for (var i = 0; i < document.playerStarts.Length; i++)
            {
                var start = document.playerStarts[i];
                var startPoint = new Vector2(start.x + 0.5f, start.y + 0.5f);
                segments.Add(new AegisPathSegment(startPoint, center));
            }
            for (var i = 0; i < document.playerStarts.Length; i++)
            {
                var a = document.playerStarts[i];
                var b = document.playerStarts[(i + 1) % document.playerStarts.Length];
                segments.Add(new AegisPathSegment(new Vector2(a.x + 0.5f, a.y + 0.5f), new Vector2(b.x + 0.5f, b.y + 0.5f)));
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
        public readonly Color MuddyBank;
        public readonly Color Cliff;
        public readonly Color OreGround;

        AegisVisualBiomeProfile(string name, Color grass, Color forest, Color rough, Color dirtPath, Color gravelPath, Color water, Color deepWater, Color muddyBank, Color cliff, Color oreGround)
        {
            Name = name;
            Grass = grass;
            Forest = forest;
            Rough = rough;
            DirtPath = dirtPath;
            GravelPath = gravelPath;
            Water = water;
            DeepWater = deepWater;
            MuddyBank = muddyBank;
            Cliff = cliff;
            OreGround = oreGround;
        }

        public static AegisVisualBiomeProfile ForDocument(AegisVisualMapDocument document)
        {
            var id = (document.displayName + " " + document.mapId).ToLowerInvariant();
            if (id.Contains("desert"))
                return new AegisVisualBiomeProfile("desert", C(0.56f, 0.48f, 0.31f), C(0.32f, 0.35f, 0.19f), C(0.49f, 0.43f, 0.34f), C(0.52f, 0.38f, 0.23f), C(0.42f, 0.40f, 0.35f), C(0.18f, 0.36f, 0.43f), C(0.08f, 0.19f, 0.26f), C(0.35f, 0.25f, 0.16f), C(0.48f, 0.46f, 0.40f), C(0.72f, 0.58f, 0.22f));
            if (id.Contains("tundra"))
                return new AegisVisualBiomeProfile("tundra", C(0.48f, 0.54f, 0.50f), C(0.24f, 0.36f, 0.32f), C(0.55f, 0.56f, 0.52f), C(0.40f, 0.37f, 0.32f), C(0.46f, 0.48f, 0.48f), C(0.19f, 0.38f, 0.50f), C(0.08f, 0.17f, 0.28f), C(0.42f, 0.38f, 0.32f), C(0.58f, 0.60f, 0.58f), C(0.78f, 0.64f, 0.24f));
            if (id.Contains("volcanic"))
                return new AegisVisualBiomeProfile("volcanic", C(0.21f, 0.22f, 0.19f), C(0.12f, 0.17f, 0.12f), C(0.26f, 0.24f, 0.23f), C(0.31f, 0.23f, 0.18f), C(0.35f, 0.34f, 0.31f), C(0.18f, 0.22f, 0.26f), C(0.04f, 0.06f, 0.08f), C(0.27f, 0.18f, 0.12f), C(0.34f, 0.33f, 0.32f), C(0.84f, 0.48f, 0.15f));
            if (id.Contains("rocky") || id.Contains("wasteland"))
                return new AegisVisualBiomeProfile("rocky", C(0.35f, 0.39f, 0.30f), C(0.18f, 0.27f, 0.16f), C(0.45f, 0.43f, 0.38f), C(0.42f, 0.34f, 0.25f), C(0.44f, 0.43f, 0.39f), C(0.15f, 0.31f, 0.40f), C(0.05f, 0.13f, 0.19f), C(0.28f, 0.23f, 0.16f), C(0.50f, 0.49f, 0.45f), C(0.74f, 0.59f, 0.23f));
            return new AegisVisualBiomeProfile("forest", C(0.27f, 0.42f, 0.25f), C(0.10f, 0.28f, 0.15f), C(0.38f, 0.36f, 0.28f), C(0.39f, 0.30f, 0.20f), C(0.38f, 0.39f, 0.34f), C(0.16f, 0.35f, 0.44f), C(0.05f, 0.15f, 0.21f), C(0.28f, 0.21f, 0.13f), C(0.47f, 0.47f, 0.43f), C(0.74f, 0.58f, 0.19f));
        }

        public Color ColorForTerrain(string terrain)
        {
            if (terrain == "forest")
                return Forest;
            if (terrain == "rough")
                return Rough;
            if (terrain == "road")
                return GravelPath;
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
        public Material ConcreteTrim;
        public Material Crater;

        public static AegisVisualMaterialSet LoadOrCreate(AegisVisualBiomeProfile profile, bool persistAssets)
        {
            return new AegisVisualMaterialSet
            {
                Ground = Material("aegis_visual_ground.mat", Color.white, true, persistAssets),
                Cliff = Material("aegis_visual_cliff.mat", profile.Cliff, false, persistAssets),
                Boulder = Material("aegis_visual_boulder.mat", Color.Lerp(profile.Cliff, Color.white, 0.12f), false, persistAssets),
                Pebble = Material("aegis_visual_pebble.mat", Color.Lerp(profile.Rough, Color.white, 0.1f), false, persistAssets),
                Ore = Material("aegis_visual_ore.mat", profile.OreGround, false, persistAssets),
                Vegetation = Material("aegis_visual_vegetation.mat", profile.Forest, false, persistAssets),
                Concrete = Material("aegis_visual_concrete_pad.mat", new Color(0.45f, 0.47f, 0.44f, 1f), false, persistAssets),
                ConcreteTrim = Material("aegis_visual_concrete_trim.mat", new Color(0.65f, 0.58f, 0.42f, 1f), false, persistAssets),
                Crater = Material("aegis_visual_crater.mat", new Color(0.08f, 0.07f, 0.06f, 1f), false, persistAssets)
            };
        }

        static Material Material(string fileName, Color color, bool textured, bool persistAssets)
        {
            if (!persistAssets)
            {
                var transient = new Material(FindShader(textured));
                transient.name = Path.GetFileNameWithoutExtension(fileName) + "_transient";
                transient.color = color;
                return transient;
            }

            var path = AegisMapEditorPaths.VisualAssetsFolder + "/" + fileName;
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(FindShader(textured));
                AssetDatabase.CreateAsset(material, path);
            }
            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        static Shader FindShader(bool textured)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            if (shader == null && textured)
                shader = Shader.Find("Unlit/Texture");
            if (shader == null)
                shader = Shader.Find("Unlit/Color");
            return shader;
        }
    }
}
#endif
