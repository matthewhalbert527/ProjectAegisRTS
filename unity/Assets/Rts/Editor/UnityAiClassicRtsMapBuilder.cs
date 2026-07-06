using System;
using System.Collections.Generic;
using System.IO;
using ProjectAegisRTS.UnityClient.Rendering.Buildings;
using ProjectAegisRTS.UnityClient.Rendering.TerrainPieces;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class UnityAiClassicRtsMapBuilder
    {
        public const string ScenePath = "Assets/Rts/Scenes/UnityAI_ClassicRtsBattlefield.unity";
        public const string ScreenshotRelativePath = "build/screenshots/unity_ai_classic_rts_battlefield.png";
        const string AssetFolder = "Assets/Rts/Art/UnityAIClassicRtsMap";
        const string TextureFolder = AssetFolder + "/Textures";
        const string TerrainDataPath = AssetFolder + "/unity_ai_classic_rts_terrain.asset";
        const string GroundMeshPath = AssetFolder + "/unity_ai_classic_rts_ground_mesh.asset";
        const string GroundTexturePath = AssetFolder + "/unity_ai_classic_rts_ground_texture.png";
        const string AiBattlefieldMapTexturePath = AssetFolder + "/AITextures/ClassicRtsBattlefieldMapAlbedo.png";
        const string AiHeatherTerrainTexturePath = AssetFolder + "/AITextures/ClassicRtsHeatherGrassDirtTerrain.png";
        const string BuildPathArgument = "-classicRtsWindowsPlayerPath";
        const string DefaultWindowsPlayerPath = "../../build/windows-player-unity-ai-classic-map/ProjectAegisRTS_ClassicMap.exe";
        const float MapWidth = 64f;
        const float MapDepth = 128f;
        const float TerrainHeight = 5.8f;
        const int HeightResolution = 257;
        const int AlphaResolution = 256;
        const int GroundMeshXSegments = 128;
        const int GroundMeshZSegments = 256;
        const int GroundTextureWidth = 768;
        const int GroundTextureHeight = 1536;
        const int SurfaceTextureSize = 256;

        static readonly Vector3[] MainRoad =
        {
            new Vector3(-27f, 0f, -50f),
            new Vector3(-21f, 0f, -35f),
            new Vector3(-8f, 0f, -18f),
            new Vector3(2f, 0f, -2f),
            new Vector3(10f, 0f, 17f),
            new Vector3(22f, 0f, 42f)
        };

        static readonly Vector3[] CrossRoad =
        {
            new Vector3(-30f, 0f, 19f),
            new Vector3(-17f, 0f, 15f),
            new Vector3(-2f, 0f, 11f),
            new Vector3(12f, 0f, 5f),
            new Vector3(29f, 0f, 4f)
        };

        static readonly Vector3[] SouthernStream =
        {
            new Vector3(-32f, 0f, 4f),
            new Vector3(-22f, 0f, 0f),
            new Vector3(-12f, 0f, 4f),
            new Vector3(-4f, 0f, -4f),
            new Vector3(7f, 0f, -13f),
            new Vector3(18f, 0f, -19f),
            new Vector3(32f, 0f, -17f)
        };

        static readonly Vector3[] NorthernStream =
        {
            new Vector3(5f, 0f, 59f),
            new Vector3(14f, 0f, 53f),
            new Vector3(24f, 0f, 54f),
            new Vector3(32f, 0f, 49f)
        };

        static readonly Ridge[] Ridges =
        {
            new Ridge(new[]
            {
                new Vector3(-31f, 0f, 27f),
                new Vector3(-22f, 0f, 32f),
                new Vector3(-14f, 0f, 39f),
                new Vector3(-7f, 0f, 48f)
            }, 2.8f, 0.78f),
            new Ridge(new[]
            {
                new Vector3(-31f, 0f, -12f),
                new Vector3(-23f, 0f, -5f),
                new Vector3(-17f, 0f, 8f),
                new Vector3(-13f, 0f, 22f)
            }, 2.4f, 0.68f),
            new Ridge(new[]
            {
                new Vector3(-3f, 0f, 34f),
                new Vector3(8f, 0f, 30f),
                new Vector3(20f, 0f, 35f),
                new Vector3(29f, 0f, 45f)
            }, 3.0f, 0.82f),
            new Ridge(new[]
            {
                new Vector3(6f, 0f, -50f),
                new Vector3(15f, 0f, -43f),
                new Vector3(24f, 0f, -36f),
                new Vector3(31f, 0f, -28f)
            }, 2.6f, 0.72f),
            new Ridge(new[]
            {
                new Vector3(-30f, 0f, -60f),
                new Vector3(-18f, 0f, -57f),
                new Vector3(-8f, 0f, -60f)
            }, 2.2f, 0.58f)
        };

        static readonly OreField[] OreFields =
        {
            new OreField(-24f, 32f, 7f, "northwest ore field"),
            new OreField(-7f, 14f, 5.5f, "central ore pocket"),
            new OreField(24f, -21f, 6.5f, "southeast ore field"),
            new OreField(23f, 51f, 5.5f, "enemy-side ore pocket"),
            new OreField(-22f, -50f, 4.8f, "southwest expansion ore")
        };

        [MenuItem("ProjectAegisRTS/Unity AI/Build Classic RTS Battlefield Map")]
        public static void BuildSceneMenu()
        {
            BuildScene();
        }

        [MenuItem("ProjectAegisRTS/Unity AI/Build And Capture Classic RTS Battlefield Map")]
        public static void BuildAndCaptureMenu()
        {
            BuildScene();
            CaptureScreenshot();
        }

        public static void BuildSceneBatch()
        {
            try
            {
                BuildScene();
                Debug.Log("Unity AI classic RTS battlefield scene built.");
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

        public static void BuildAndCaptureBatch()
        {
            try
            {
                BuildScene();
                var path = CaptureScreenshot();
                Debug.Log("Unity AI classic RTS battlefield screenshot captured: " + path);
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

        public static void BuildWindowsPlayerBatch()
        {
            try
            {
                BuildScene();
                var outputPath = GetCommandLineArgument(BuildPathArgument);
                if (string.IsNullOrWhiteSpace(outputPath))
                    outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, DefaultWindowsPlayerPath));
                outputPath = Path.GetFullPath(outputPath);

                var outputDirectory = Path.GetDirectoryName(outputPath);
                if (string.IsNullOrWhiteSpace(outputDirectory))
                    throw new InvalidOperationException("Classic RTS map Windows player output path must include a directory.");
                Directory.CreateDirectory(outputDirectory);

                var options = new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath },
                    locationPathName = outputPath,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.None
                };

                var report = BuildPipeline.BuildPlayer(options);
                if (report.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException("Unity AI classic RTS map Windows player build failed with result " + report.summary.result + ".");

                Debug.Log("Unity AI classic RTS map Windows player build succeeded: " + outputPath + " (" + report.summary.totalSize + " bytes).");
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

        public static string BuildScene()
        {
            Stage32TerrainPieceGenerator.EnsureStage32TerrainPieces();
            if (AssetDatabase.IsValidFolder(Stage32TerrainSampleGroundTileIntegrator.SampleRoot))
                Stage32TerrainSampleGroundTileIntegrator.IntegrateGroundTiles();

            EnsureFolderRecursive(AssetFolder);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "UnityAI_ClassicRtsBattlefield";

            var root = new GameObject("Unity AI Classic RTS Battlefield");
            var materials = CreateMaterials();
            CreateTerrain(root.transform, materials);
            if (!HasAiBattlefieldMapTexture())
                CreateRoadsAndStreams(root.transform, materials);
            CreateHighReadabilityClassicDetails(root.transform, materials);
            CreateBuildingSlateVisualTestBase(root.transform);
            CreateLighting();
            CreateCamera();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return ScenePath;
        }

        public static string CaptureScreenshot()
        {
            if (!File.Exists(Path.Combine(Stage8ActorCatalog.RepoRoot, "unity", ScenePath)))
                BuildScene();

            var scene = EditorSceneManager.OpenScene(ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Could not open Unity AI classic RTS battlefield scene.");

            var camera = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
            if (camera == null)
                throw new InvalidOperationException("Unity AI classic RTS battlefield scene has no camera.");

            var outputPath = Path.Combine(Stage8ActorCatalog.RepoRoot, ScreenshotRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            RenderCameraToPng(camera, outputPath, 1024, 2048);
            CaptureDetailScreenshots(camera, outputPath);
            return outputPath;
        }

        static void CreateTerrain(Transform parent, MaterialBundle materials)
        {
            var heather = LoadTerrainLayer("Heather_TerrainLayer");
            var grass = LoadTerrainLayer("Grass_Soil_TerrainLayer");
            var soil = LoadTerrainLayer("Soil_Rocks_TerrainLayer");
            var pebbles = LoadTerrainLayer("Pebbles_B_TerrainLayer");
            var muddy = LoadTerrainLayer("Muddy_TerrainLayer");
            var rock = LoadTerrainLayer("Rock_TerrainLayer");
            var blackSand = LoadTerrainLayer("Black_Sand_TerrainLayer");
            var tidal = LoadTerrainLayer("Tidal_Pools_TerrainLayer");
            var layers = new[] { heather, grass, soil, pebbles, muddy, rock, blackSand, tidal };

            var data = AssetDatabase.LoadAssetAtPath<TerrainData>(TerrainDataPath);
            if (data == null)
            {
                data = new TerrainData();
                AssetDatabase.CreateAsset(data, TerrainDataPath);
            }

            data.heightmapResolution = HeightResolution;
            data.alphamapResolution = AlphaResolution;
            data.size = new Vector3(MapWidth, TerrainHeight, MapDepth);
            data.terrainLayers = layers;
            data.SetHeights(0, 0, BuildHeights());
            data.SetAlphamaps(0, 0, BuildAlphamaps(layers.Length));
            EditorUtility.SetDirty(data);

            var terrainObject = UnityEngine.Terrain.CreateTerrainGameObject(data);
            terrainObject.name = "Smooth Heather Terrain - no visible square grid";
            terrainObject.transform.SetParent(parent, false);
            terrainObject.transform.position = new Vector3(-MapWidth * 0.5f, 0f, -MapDepth * 0.5f);
            var terrain = terrainObject.GetComponent<UnityEngine.Terrain>();
            terrain.drawInstanced = true;
            terrain.heightmapPixelError = 4f;
            terrain.basemapDistance = 900f;
            terrain.enabled = false;
            terrain.Flush();

            var collider = terrainObject.GetComponent<TerrainCollider>();
            if (collider != null)
                Object.DestroyImmediate(collider);

            CreateVisibleGroundMesh(parent, materials);
        }

        static TerrainLayer LoadTerrainLayer(string name)
        {
            var path = Stage32TerrainSampleGroundTileIntegrator.TerrainLayerFolder + "/" + name + ".terrainlayer";
            var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
            if (layer == null)
                throw new InvalidOperationException("Missing Terrain Sample terrain layer: " + path);
            return layer;
        }

        static float[,] BuildHeights()
        {
            var heights = new float[HeightResolution, HeightResolution];
            for (var z = 0; z < HeightResolution; z++)
            {
                for (var x = 0; x < HeightResolution; x++)
                {
                    var worldX = Mathf.Lerp(-MapWidth * 0.5f, MapWidth * 0.5f, x / (HeightResolution - 1f));
                    var worldZ = Mathf.Lerp(-MapDepth * 0.5f, MapDepth * 0.5f, z / (HeightResolution - 1f));
                    heights[z, x] = Mathf.Clamp01(HeightNormalized(worldX, worldZ));
                }
            }

            return heights;
        }

        static float[,,] BuildAlphamaps(int layerCount)
        {
            var maps = new float[AlphaResolution, AlphaResolution, layerCount];
            for (var z = 0; z < AlphaResolution; z++)
            {
                for (var x = 0; x < AlphaResolution; x++)
                {
                    var worldX = Mathf.Lerp(-MapWidth * 0.5f, MapWidth * 0.5f, x / (AlphaResolution - 1f));
                    var worldZ = Mathf.Lerp(-MapDepth * 0.5f, MapDepth * 0.5f, z / (AlphaResolution - 1f));
                    var weights = TerrainWeights(worldX, worldZ, layerCount);
                    for (var i = 0; i < layerCount; i++)
                        maps[z, x, i] = weights[i];
                }
            }

            return maps;
        }

        static float[] TerrainWeights(float x, float z, int count)
        {
            var weights = new float[count];
            weights[0] = 0.62f;
            weights[1] = 0.28f;
            weights[2] = 0.08f;
            weights[3] = 0.02f;

            var road = Mathf.Min(DistanceToPath(x, z, MainRoad), DistanceToPath(x, z, CrossRoad));
            if (road < 4.8f)
            {
                Add(weights, 2, Smooth01(4.8f, 0f, road) * 0.80f);
                Add(weights, 3, Smooth01(3.0f, 0f, road) * 0.70f);
                Add(weights, 0, -0.55f);
            }

            var water = Mathf.Min(DistanceToPath(x, z, SouthernStream), DistanceToPath(x, z, NorthernStream));
            if (water < 3.6f)
            {
                Add(weights, 7, Smooth01(3.6f, 0f, water) * 0.90f);
                Add(weights, 4, Smooth01(5.2f, 0f, water) * 0.45f);
                Add(weights, 0, -0.45f);
            }

            for (var i = 0; i < Ridges.Length; i++)
            {
                var d = DistanceToPath(x, z, Ridges[i].Points);
                if (d < Ridges[i].Width + 3f)
                {
                    Add(weights, 5, Smooth01(Ridges[i].Width + 3f, 0f, d) * 1.05f);
                    Add(weights, 0, -0.48f);
                }
            }

            for (var i = 0; i < OreFields.Length; i++)
            {
                var d = Vector2.Distance(new Vector2(x, z), new Vector2(OreFields[i].X, OreFields[i].Z));
                if (d < OreFields[i].Radius + 3f)
                {
                    Add(weights, 2, Smooth01(OreFields[i].Radius + 3f, 0f, d) * 0.85f);
                    Add(weights, 5, Smooth01(OreFields[i].Radius, 0f, d) * 0.30f);
                    Add(weights, 0, -0.38f);
                }
            }

            if (InsideRect(x, z, -28f, -51f, -8f, -29f) || InsideRect(x, z, 8f, 30f, 29f, 51f) || InsideRect(x, z, -24f, 44f, -6f, 57f))
            {
                Add(weights, 2, 0.60f);
                Add(weights, 3, 0.42f);
                Add(weights, 0, -0.35f);
            }

            var scorch = Mathf.PerlinNoise((x + 120f) * 0.075f, (z - 42f) * 0.075f);
            if (scorch > 0.68f && Mathf.Abs(x) < 26f && z > -25f && z < 32f)
                Add(weights, 6, (scorch - 0.68f) * 1.4f);

            Normalize(weights);
            return weights;
        }

        static void CreateRoadsAndStreams(Transform parent, MaterialBundle materials)
        {
            var roads = new GameObject("Layered roads with painted markings");
            roads.transform.SetParent(parent, false);
            CreateRibbon("Main road worn shoulder", MainRoad, 5.7f, materials.RoadShoulder, 0.22f, roads.transform);
            CreateRibbon("Main asphalt road", MainRoad, 4.2f, materials.Road, 0.27f, roads.transform);
            CreateRibbon("Cross road worn shoulder", CrossRoad, 5.1f, materials.RoadShoulder, 0.22f, roads.transform);
            CreateRibbon("Cross asphalt road", CrossRoad, 3.7f, materials.Road, 0.27f, roads.transform);
            CreateRoadMarkings(MainRoad, 5.1f, 0.72f, roads.transform, materials);
            CreateRoadMarkings(CrossRoad, 4.6f, 0.62f, roads.transform, materials);

            var streams = new GameObject("Layered streams with banks and bridges");
            streams.transform.SetParent(parent, false);
            CreateRibbon("Southern muddy stream bank", SouthernStream, 4.8f, materials.MudBank, 0.20f, streams.transform);
            CreateRibbon("Southern blue water channel", SouthernStream, 2.45f, materials.Water, 0.26f, streams.transform);
            CreateRibbon("Northern muddy stream bank", NorthernStream, 4.1f, materials.MudBank, 0.20f, streams.transform);
            CreateRibbon("Northern blue water channel", NorthernStream, 2.15f, materials.Water, 0.26f, streams.transform);
            CreateBridge("Main road bridge", new Vector3(-3.6f, 0f, -4.4f), Quaternion.Euler(0f, 38f, 0f), 8.0f, streams.transform, materials);
            CreateBridge("Cross road culvert bridge", new Vector3(-25.4f, 0f, 1.5f), Quaternion.Euler(0f, 76f, 0f), 6.8f, streams.transform, materials);
        }

        static void CreateRoadMarkings(Vector3[] points, float spacing, float dashLength, Transform parent, MaterialBundle materials)
        {
            for (var i = 0; i < points.Length - 1; i++)
            {
                var a = points[i];
                var b = points[i + 1];
                var length = Vector3.Distance(a, b);
                var direction = (b - a).normalized;
                var normal = new Vector3(-direction.z, 0f, direction.x);
                var rotation = Quaternion.LookRotation(direction, Vector3.up);
                var count = Mathf.Max(1, Mathf.FloorToInt(length / spacing));
                for (var step = 0; step < count; step++)
                {
                    var t = (step + 0.5f) / count;
                    var p = Vector3.Lerp(a, b, t);
                    var y = HeightWorld(p.x, p.z) + 0.145f;
                    CreateBox("worn road center dash", new Vector3(p.x, y, p.z), new Vector3(0.18f, 0.025f, dashLength), rotation, materials.RoadLine, parent);

                    if (step % 3 == 1)
                    {
                        var left = p + normal * 1.88f;
                        var right = p - normal * 1.88f;
                        CreateBox("road edge marker L", new Vector3(left.x, HeightWorld(left.x, left.z) + 0.135f, left.z), new Vector3(0.10f, 0.018f, dashLength * 0.72f), rotation, materials.RoadLine, parent);
                        CreateBox("road edge marker R", new Vector3(right.x, HeightWorld(right.x, right.z) + 0.135f, right.z), new Vector3(0.10f, 0.018f, dashLength * 0.72f), rotation, materials.RoadLine, parent);
                    }
                }
            }
        }

        static void CreateBridge(string name, Vector3 center, Quaternion rotation, float length, Transform parent, MaterialBundle materials)
        {
            var y = HeightWorld(center.x, center.z);
            CreateBox(name + " concrete deck", new Vector3(center.x, y + 0.34f, center.z), new Vector3(5.2f, 0.34f, length), rotation, materials.Concrete, parent);
            CreateBox(name + " asphalt cap", new Vector3(center.x, y + 0.54f, center.z), new Vector3(4.1f, 0.05f, length * 0.94f), rotation, materials.Road, parent);
            CreateBox(name + " left rail", new Vector3(center.x, y + 0.82f, center.z), new Vector3(0.22f, 0.46f, length), rotation * Quaternion.Euler(0f, 0f, 0f), materials.MetalDark, parent).transform.Translate(Vector3.left * 2.62f, Space.Self);
            CreateBox(name + " right rail", new Vector3(center.x, y + 0.82f, center.z), new Vector3(0.22f, 0.46f, length), rotation * Quaternion.Euler(0f, 0f, 0f), materials.MetalDark, parent).transform.Translate(Vector3.right * 2.62f, Space.Self);
            CreateBox(name + " center marking", new Vector3(center.x, y + 0.59f, center.z), new Vector3(0.15f, 0.024f, length * 0.58f), rotation, materials.RoadLine, parent);
        }

        static void CreateSetDressing(Transform parent)
        {
            var root = new GameObject("Map set dressing - cliffs ore trees props");
            root.transform.SetParent(parent, false);

            for (var i = 0; i < Ridges.Length; i++)
            {
                PlaceAlongPath("cliff_ridge_straight_01", Ridges[i].Points, 4.1f, 1.15f, 0f, root.transform);
                PlaceAlongPath("cliff_wall_straight_01", Ridges[i].Points, 7.2f, 0.92f, 1.55f, root.transform);
                PlaceAlongPath("rock_blocker_01", Ridges[i].Points, 8.5f, 0.78f, -1.15f, root.transform);
            }

            for (var i = 0; i < OreFields.Length; i++)
                CreateOreField(OreFields[i], root.transform);

            PlaceForestPatch(-28f, -28f, 7, root.transform);
            PlaceForestPatch(-28f, 53f, 9, root.transform);
            PlaceForestPatch(29f, 25f, 6, root.transform);
            PlaceForestPatch(17f, -54f, 7, root.transform);
            PlaceForestPatch(2f, 51f, 5, root.transform);

            PlaceScatter("boulder_cluster_small_01", -4f, 24f, 8, 10f, root.transform);
            PlaceScatter("boulder_cluster_small_01", 26f, 10f, 6, 8f, root.transform);
            PlaceScatter("crater_01", 6f, 18f, 7, 9f, root.transform);
            PlaceScatter("debris_small_01", 8f, -10f, 8, 12f, root.transform);
            PlaceScatter("wreckage_small_01", -2f, -26f, 4, 8f, root.transform);
            PlaceScatter("tire_tracks_01", -10f, -18f, 5, 14f, root.transform);
            PlaceScatter("sandbags_straight_01", 4f, 28f, 5, 8f, root.transform);
            PlaceScatter("fence_straight_01", 17f, 46f, 4, 6f, root.transform);
        }

        static void CreateHighReadabilityClassicDetails(Transform parent, MaterialBundle materials)
        {
            var root = new GameObject("High readability classic RTS terrain details");
            root.transform.SetParent(parent, false);
            var useAiBattlefieldTexture = HasAiBattlefieldMapTexture();

            if (!useAiBattlefieldTexture)
            {
                for (var i = 0; i < Ridges.Length; i++)
                    CreateVisibleCliffLine(Ridges[i].Points, root.transform, materials);

                for (var i = 0; i < OreFields.Length; i++)
                    CreateVisibleOreField(OreFields[i], root.transform, materials, i);
            }

            if (!useAiBattlefieldTexture)
            {
                CreateVisibleForest(-28f, -28f, 13, root.transform, materials);
                CreateVisibleForest(-27f, 54f, 16, root.transform, materials);
                CreateVisibleForest(27f, 26f, 14, root.transform, materials);
                CreateVisibleForest(17f, -55f, 13, root.transform, materials);
                CreateVisibleForest(1f, 50f, 11, root.transform, materials);
            }

            if (useAiBattlefieldTexture)
            {
                return;
            }
            else
            {
                CreateReadableBaseSilhouette(-20f, -39f, true, root.transform, materials);
                CreateReadableBaseSilhouette(18f, 39f, false, root.transform, materials);
                CreateBox("Neutral depot pad", new Vector3(-14f, HeightWorld(-14f, 51f) + 0.08f, 51f), new Vector3(8f, 0.16f, 6f), Quaternion.identity, materials.Concrete, root.transform);
                CreateBox("Neutral depot shell", new Vector3(-14f, HeightWorld(-14f, 51f) + 0.65f, 51f), new Vector3(3.7f, 1.1f, 2.7f), Quaternion.identity, materials.NeutralBuilding, root.transform);
            }
        }

        static void CreateVisibleCliffLine(Vector3[] points, Transform parent, MaterialBundle materials)
        {
            CreateRibbon("cliff shadow and talus strip", points, 5.4f, materials.CliffShadow, 0.045f, parent);

            for (var i = 0; i < points.Length - 1; i++)
            {
                var a = points[i];
                var b = points[i + 1];
                var length = Vector3.Distance(a, b);
                var count = Mathf.Max(2, Mathf.RoundToInt(length / 2.7f));
                var direction = (b - a).normalized;
                var normal = new Vector3(-direction.z, 0f, direction.x);
                var rotation = Quaternion.LookRotation(direction, Vector3.up);
                for (var step = 0; step < count; step++)
                {
                    var t = (step + 0.5f) / count;
                    var center = Vector3.Lerp(a, b, t);
                    var ridgeOffset = normal * (Mathf.Sin((i + 1) * (step + 2) * 0.83f) * 0.36f);
                    var cx = center.x + ridgeOffset.x;
                    var cz = center.z + ridgeOffset.z;
                    var ridgeHeight = 0.84f + (step % 4) * 0.18f;
                    var ridgeScale = new Vector3(1.65f + (step % 3) * 0.22f, ridgeHeight, 1.04f + ((step + 1) % 4) * 0.16f);
                    var ridgePos = new Vector3(cx, HeightWorld(cx, cz) + ridgeHeight * 0.5f + 0.10f, cz);
                    CreateBoulder("faceted cliff ridge stone", ridgePos, ridgeScale, rotation * Quaternion.Euler(0f, (step % 5 - 2) * 9f, 0f), materials.Rock, parent);

                    for (var side = -1; side <= 1; side += 2)
                    {
                        var offset = normal * (side * (2.15f + (step % 3) * 0.36f) + Mathf.Sin((i + 1) * (step + 2) * 0.83f) * 0.42f);
                        var px = center.x + offset.x;
                        var pz = center.z + offset.z;
                        var height = 0.25f + ((step + side + 5) % 4) * 0.10f;
                        var scale = new Vector3(0.54f + ((step + 2) % 3) * 0.10f, height, 0.42f + ((step + 1) % 4) * 0.08f);
                        var pos = new Vector3(px, HeightWorld(px, pz) + height * 0.5f + 0.08f, pz);
                        CreateBoulder("loose cliff talus stone", pos, scale, rotation * Quaternion.Euler(0f, (step % 3 - 1) * 19f, 0f), materials.Rock, parent);
                    }
                }
            }
        }

        static void CreateCliffRidgeMesh(string name, Vector3[] points, float width, float height, Material material, Transform parent)
        {
            for (var segment = 0; segment < points.Length - 1; segment++)
            {
                var a = points[segment];
                var b = points[segment + 1];
                var length = Vector3.Distance(a, b);
                var samples = Mathf.Max(3, Mathf.CeilToInt(length / 1.6f));
                var direction = (b - a).normalized;
                var normal = new Vector3(-direction.z, 0f, direction.x);
                var vertices = new List<Vector3>();
                var uvs = new List<Vector2>();

                for (var i = 0; i <= samples; i++)
                {
                    var t = i / (float)samples;
                    var center = Vector3.Lerp(a, b, t);
                    var wobble = Mathf.PerlinNoise(center.x * 0.31f + segment, center.z * 0.27f - segment);
                    center += normal * ((wobble - 0.5f) * 0.82f);
                    var baseY = HeightWorld(center.x, center.z) + 0.08f;
                    var crestY = baseY + height * (0.72f + wobble * 0.44f);
                    var shelfY = baseY + height * (0.36f + wobble * 0.16f);
                    vertices.Add(center - normal * width * 0.58f + Vector3.up * baseY);
                    vertices.Add(center - normal * width * 0.28f + Vector3.up * shelfY);
                    vertices.Add(center + Vector3.up * crestY);
                    vertices.Add(center + normal * width * 0.32f + Vector3.up * shelfY);
                    vertices.Add(center + normal * width * 0.60f + Vector3.up * baseY);
                    for (var band = 0; band < 5; band++)
                        uvs.Add(new Vector2(band / 4f, t * length * 0.18f));
                }

                var triangles = new List<int>();
                for (var i = 0; i < samples; i++)
                {
                    var row = i * 5;
                    var next = row + 5;
                    for (var band = 0; band < 4; band++)
                    {
                        triangles.Add(row + band);
                        triangles.Add(next + band);
                        triangles.Add(row + band + 1);
                        triangles.Add(row + band + 1);
                        triangles.Add(next + band);
                        triangles.Add(next + band + 1);
                        triangles.Add(row + band + 1);
                        triangles.Add(next + band);
                        triangles.Add(row + band);
                        triangles.Add(next + band + 1);
                        triangles.Add(next + band);
                        triangles.Add(row + band + 1);
                    }
                }

                var mesh = new Mesh();
                mesh.name = name + " " + segment;
                mesh.SetVertices(vertices);
                mesh.SetUVs(0, uvs);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();

                var go = new GameObject(name + " " + segment);
                go.transform.SetParent(parent, false);
                var filter = go.AddComponent<MeshFilter>();
                filter.sharedMesh = mesh;
                var renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                renderer.receiveShadows = true;
            }
        }

        static void CreateVisibleOreField(OreField field, Transform parent, MaterialBundle materials, int fieldIndex)
        {
            var baseMaterial = fieldIndex % 3 == 1 ? materials.OreGold : materials.OreBlue;
            var accentMaterial = fieldIndex % 3 == 1 ? materials.OreBlue : materials.OreGold;
            CreateIrregularPatch("irregular ore-bearing soil bed", field.X, field.Z, field.Radius * 1.15f, field.Radius * 0.96f, fieldIndex * 27f, materials.OreSoil, parent, 0.052f);
            PlacePiece("resource_cluster_blue_01", field.X - 0.6f, field.Z + 0.4f, fieldIndex * 33f, Mathf.Clamp(field.Radius / 5.7f, 0.82f, 1.25f), parent);

            for (var i = 0; i < 34; i++)
            {
                var angle = (i * 137.5f + fieldIndex * 19f) * Mathf.Deg2Rad;
                var radius = 0.75f + (i % 9) * field.Radius / 9.4f;
                var px = field.X + Mathf.Cos(angle) * radius + Mathf.Sin(i * 0.71f) * 0.45f;
                var pz = field.Z + Mathf.Sin(angle) * radius + Mathf.Cos(i * 0.53f) * 0.45f;
                var h = 0.58f + (i % 6) * 0.18f;
                CreateCrystal("Ore crystal", new Vector3(px, HeightWorld(px, pz) + h * 0.5f + 0.10f, pz), h, Quaternion.Euler(0f, i * 29f, 0f), i % 4 == 0 ? accentMaterial : baseMaterial, parent);
            }
        }

        static void CreateIrregularPatch(string name, float x, float z, float radiusX, float radiusZ, float rotationY, Material material, Transform parent, float yOffset)
        {
            const int sides = 34;
            var rotation = Quaternion.Euler(0f, rotationY, 0f);
            var vertices = new List<Vector3> { new Vector3(x, HeightWorld(x, z) + yOffset, z) };
            var uvs = new List<Vector2> { new Vector2(0.5f, 0.5f) };
            for (var i = 0; i < sides; i++)
            {
                var angle = i * Mathf.PI * 2f / sides;
                var wobble = 0.78f + Mathf.PerlinNoise(x * 0.17f + i * 0.41f, z * 0.19f - i * 0.37f) * 0.34f;
                var local = new Vector3(Mathf.Cos(angle) * radiusX * wobble, 0f, Mathf.Sin(angle) * radiusZ * wobble);
                var world = new Vector3(x, 0f, z) + rotation * local;
                vertices.Add(new Vector3(world.x, HeightWorld(world.x, world.z) + yOffset, world.z));
                uvs.Add(new Vector2(0.5f + Mathf.Cos(angle) * 0.5f, 0.5f + Mathf.Sin(angle) * 0.5f));
            }

            var triangles = new List<int>();
            for (var i = 1; i <= sides; i++)
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i == sides ? 1 : i + 1);
            }

            var mesh = new Mesh();
            mesh.name = name + " mesh";
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            var patch = new GameObject(name);
            patch.transform.SetParent(parent, false);
            var filter = patch.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = patch.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.receiveShadows = true;
        }

        static void CreateVisibleForest(float x, float z, int count, Transform parent, MaterialBundle materials)
        {
            for (var i = 0; i < count; i++)
            {
                var angle = i * 73f * Mathf.Deg2Rad;
                var radius = 1.2f + (i % 6) * 1.05f;
                var px = x + Mathf.Cos(angle) * radius + Mathf.Sin(i * 1.9f) * 0.55f;
                var pz = z + Mathf.Sin(angle) * radius + Mathf.Cos(i * 1.3f) * 0.55f;
                CreateTree("Conifer", px, pz, 1.45f + (i % 4) * 0.22f, parent, materials);
            }
        }

        static void CreateTerrainSampleDetailScatter(Transform parent)
        {
            var ids = new[] { "Heather_A", "Heather_B", "Grass_A", "Grass_B", "Grass_D", "Bush_A", "Bush_B", "Fern_A" };
            for (var i = 0; i < 150; i++)
            {
                var x = -29f + ((i * 37) % 580) / 580f * 58f;
                var z = -59f + ((i * 71) % 1160) / 1160f * 116f;
                var road = Mathf.Min(DistanceToPath(x, z, MainRoad), DistanceToPath(x, z, CrossRoad));
                var water = Mathf.Min(DistanceToPath(x, z, SouthernStream), DistanceToPath(x, z, NorthernStream));
                if (road < 5.8f || water < 4.8f || InsideRect(x, z, -32f, -53f, -7f, -27f) || InsideRect(x, z, 6f, 27f, 31f, 53f))
                    continue;

                PlaceSamplePrefab(ids[i % ids.Length], x, z, (StableHash(ids[i % ids.Length] + i) % 360), 0.62f + (i % 5) * 0.08f, parent);
            }
        }

        static void CreateGroundArtPatches(Transform parent)
        {
            var patches = new[]
            {
                new Vector3(-18f, 0f, 20f),
                new Vector3(12f, 0f, 19f),
                new Vector3(3f, 0f, -31f),
                new Vector3(-27f, 0f, -45f),
                new Vector3(25f, 0f, -48f),
                new Vector3(27f, 0f, 38f)
            };

            for (var i = 0; i < patches.Length; i++)
                PlacePiece("ground_grass_dirt_01", patches[i].x, patches[i].z, i * 41f, 1.0f + (i % 3) * 0.12f, parent);
        }

        static void CreateReadableBaseSilhouette(float x, float z, bool playerBase, Transform parent, MaterialBundle materials)
        {
            var building = playerBase ? materials.PlayerBuilding : materials.EnemyBuilding;
            var wall = playerBase ? materials.PlayerWall : materials.EnemyWall;
            var rotation = playerBase ? Quaternion.Euler(0f, 7f, 0f) : Quaternion.Euler(0f, -172f, 0f);
            var accent = playerBase ? materials.PlayerWall : materials.EnemyWall;

            CreateCompoundPad(x, z, 24f, 21f, materials.Concrete, parent);
            CreateCompoundWall(x, z, 24.8f, 21.8f, wall, parent);
            CreateIndustrialBuilding("command center", x - 3.6f, z + 1.0f, 5.4f, 4.6f, 2.45f, rotation, building, accent, parent);
            CreatePowerPlant("power plant", x - 8.0f, z + 6.4f, rotation, building, wall, parent);
            CreateFactory("vehicle factory", x + 6.3f, z - 5.2f, rotation, building, wall, parent);
            CreateRefinery("ore refinery", x + 5.4f, z + 5.7f, rotation, building, wall, parent);
            CreateTurret("north guard turret", x - 10.4f, z - 7.2f, wall, parent);
            CreateTurret("east guard turret", x + 10.9f, z - 6.6f, wall, parent);
            CreateVehicleProxy(playerBase ? "blue scout tank" : "red patrol tank", x + 11.0f, z + 8.6f, playerBase ? 22f : -156f, building, wall, parent);
            CreateVehicleProxy(playerBase ? "blue harvester" : "red heavy tank", x + 14.0f, z + 5.6f, playerBase ? -14f : 168f, building, wall, parent);
            CreateCrateStack("supply crates", x - 9.6f, z - 2.0f, parent, materials);
            CreateBox("base gate dust apron", new Vector3(x + 12.6f, HeightWorld(x + 12.6f, z + 1.2f) + 0.07f, z + 1.2f), new Vector3(5.8f, 0.06f, 4.2f), Quaternion.identity, materials.RoadShoulder, parent);
        }

        static void CreateBuildingSlateVisualTestBase(Transform parent)
        {
            var root = new GameObject("Unity AI building slate visual test base");
            root.transform.SetParent(parent, false);

            PlaceBuildingSlate("fabrication_hub", -24.0f, -42.5f, 8f, 1.25f, true, root.transform);
            PlaceBuildingSlate("war_factory", -14.2f, -45.2f, 7f, 1.18f, true, root.transform);
            PlaceBuildingSlate("barracks", -28.4f, -34.8f, 8f, 1.16f, true, root.transform);
            PlaceBuildingSlate("field_hospital", -30.2f, -40.0f, 8f, 1.08f, true, root.transform);
            PlaceBuildingSlate("repair_bay", -17.8f, -34.5f, 7f, 1.10f, true, root.transform);
            PlaceBuildingSlate("refinery", -12.7f, -36.7f, 7f, 1.15f, false, root.transform);
            PlaceBuildingSlate("comm_center", -23.1f, -28.2f, 8f, 1.08f, false, root.transform);
            PlaceBuildingSlate("tech_center", -11.3f, -29.0f, 7f, 1.08f, false, root.transform);
            PlaceBuildingSlate("power_plant", -30.0f, -47.6f, 8f, 1.08f, false, root.transform);
            PlaceBuildingSlate("advanced_power_plant", -20.9f, -52.0f, 7f, 1.08f, false, root.transform);
            PlaceBuildingSlate("dual_helipad", -7.2f, -51.0f, 7f, 1.02f, true, root.transform);

            PlaceBuildingSlate("cannon_turret", -31.4f, -27.8f, 24f, 1.18f, false, root.transform);
            PlaceBuildingSlate("gun_tower", -7.1f, -30.6f, -32f, 1.18f, false, root.transform);
            PlaceBuildingSlate("advanced_gun_tower", -7.0f, -53.8f, -18f, 1.18f, false, root.transform);
        }

        static GameObject PlaceBuildingSlate(string id, float x, float z, float rotationY, float scale, bool cycleProduction, Transform parent)
        {
            var prefab = LoadBuildingSlatePrefab(id);
            if (prefab == null)
                return null;

            var instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            if (instance == null)
                return null;

            instance.name = "VisualTestBuilding " + id;
            instance.transform.position = new Vector3(x, HeightWorld(x, z) + 0.20f, z);
            instance.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
            instance.transform.localScale = Vector3.one * Mathf.Clamp(scale, 0.65f, 1.60f);
            RemoveColliders(instance);

            var controller = instance.GetComponent<BuildingArtShowcaseController>();
            if (controller == null)
                controller = instance.AddComponent<BuildingArtShowcaseController>();
            controller.actorTypeId = id;
            controller.profile = LoadBuildingProfile(id);
            controller.cycleProductionState = cycleProduction;
            controller.productionCycleSeconds = id == "war_factory" ? 6.8f : 5.2f;

            return instance;
        }

        static GameObject LoadBuildingSlatePrefab(string id)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Rts/Art/UnityAIBuildingSlate/Prefabs/" + id + "_unity_ai_building.prefab");
        }

        static BuildingVisualProfile LoadBuildingProfile(string id)
        {
            return AssetDatabase.LoadAssetAtPath<BuildingVisualProfile>("Assets/Rts/ScriptableObjects/BuildingProfiles/" + id + "_building_visual.asset");
        }

        static void CreateCompoundPad(float x, float z, float width, float depth, Material material, Transform parent)
        {
            CreateBox("base concrete slab", new Vector3(x, HeightWorld(x, z) + 0.055f, z), new Vector3(width, 0.11f, depth), Quaternion.identity, material, parent);
            for (var ix = -2; ix <= 2; ix++)
            {
                CreateBox("base expansion seam x", new Vector3(x + ix * width / 5f, HeightWorld(x, z) + 0.125f, z), new Vector3(0.055f, 0.022f, depth * 0.94f), Quaternion.identity, material, parent);
            }

            for (var iz = -2; iz <= 2; iz++)
            {
                CreateBox("base expansion seam z", new Vector3(x, HeightWorld(x, z) + 0.126f, z + iz * depth / 5f), new Vector3(width * 0.94f, 0.022f, 0.055f), Quaternion.identity, material, parent);
            }
        }

        static void CreateCompoundWall(float x, float z, float width, float depth, Material material, Transform parent)
        {
            CreateBox("compound north wall", new Vector3(x, HeightWorld(x, z + depth * 0.5f) + 0.38f, z + depth * 0.5f), new Vector3(width, 0.76f, 0.46f), Quaternion.identity, material, parent);
            CreateBox("compound south wall", new Vector3(x - 2.5f, HeightWorld(x, z - depth * 0.5f) + 0.38f, z - depth * 0.5f), new Vector3(width - 5.0f, 0.76f, 0.46f), Quaternion.identity, material, parent);
            CreateBox("compound west wall", new Vector3(x - width * 0.5f, HeightWorld(x - width * 0.5f, z) + 0.38f, z), new Vector3(0.46f, 0.76f, depth), Quaternion.identity, material, parent);
            CreateBox("compound east wall upper", new Vector3(x + width * 0.5f, HeightWorld(x + width * 0.5f, z + 4.4f) + 0.38f, z + 4.4f), new Vector3(0.46f, 0.76f, depth * 0.46f), Quaternion.identity, material, parent);
            CreateBox("compound east wall lower", new Vector3(x + width * 0.5f, HeightWorld(x + width * 0.5f, z - 6.2f) + 0.38f, z - 6.2f), new Vector3(0.46f, 0.76f, depth * 0.32f), Quaternion.identity, material, parent);
            CreateBox("compound gate header", new Vector3(x + width * 0.5f, HeightWorld(x + width * 0.5f, z - 0.8f) + 0.95f, z - 0.8f), new Vector3(0.55f, 0.42f, 3.0f), Quaternion.identity, material, parent);
        }

        static void CreateIndustrialBuilding(string name, float x, float z, float width, float depth, float height, Quaternion rotation, Material body, Material trim, Transform parent)
        {
            var y = HeightWorld(x, z);
            CreateBox(name + " body", new Vector3(x, y + height * 0.5f, z), new Vector3(width, height, depth), rotation, body, parent);
            CreateBox(name + " heavy roof cap", new Vector3(x, y + height + 0.20f, z), new Vector3(width + 0.45f, 0.32f, depth + 0.45f), rotation, trim, parent);
            CreateBox(name + " front blast door", new Vector3(x, y + 0.54f, z - depth * 0.51f), new Vector3(width * 0.42f, 0.72f, 0.12f), rotation, trim, parent);
            CreateBox(name + " roof service panel A", new Vector3(x - width * 0.22f, y + height + 0.42f, z + depth * 0.18f), new Vector3(width * 0.24f, 0.10f, depth * 0.22f), rotation, body, parent);
            CreateBox(name + " roof service panel B", new Vector3(x + width * 0.24f, y + height + 0.43f, z - depth * 0.14f), new Vector3(width * 0.20f, 0.10f, depth * 0.24f), rotation, body, parent);
            CreateCylinder(name + " antenna mast", new Vector3(x + width * 0.33f, y + height + 0.86f, z + depth * 0.31f), new Vector3(0.10f, 0.56f, 0.10f), trim, parent);
        }

        static void CreatePowerPlant(string name, float x, float z, Quaternion rotation, Material body, Material trim, Transform parent)
        {
            CreateIndustrialBuilding(name, x, z, 3.8f, 3.4f, 1.9f, rotation, body, trim, parent);
            CreateCylinder(name + " cooling tower A", new Vector3(x - 2.45f, HeightWorld(x - 2.45f, z + 0.4f) + 1.08f, z + 0.4f), new Vector3(0.82f, 1.05f, 0.82f), trim, parent);
            CreateCylinder(name + " cooling tower B", new Vector3(x - 3.55f, HeightWorld(x - 3.55f, z - 0.8f) + 0.95f, z - 0.8f), new Vector3(0.70f, 0.92f, 0.70f), trim, parent);
            CreateBox(name + " warm generator glow", new Vector3(x + 1.35f, HeightWorld(x + 1.35f, z - 1.65f) + 0.72f, z - 1.65f), new Vector3(0.95f, 0.14f, 0.10f), rotation, trim, parent);
        }

        static void CreateFactory(string name, float x, float z, Quaternion rotation, Material body, Material trim, Transform parent)
        {
            CreateIndustrialBuilding(name, x, z, 6.6f, 4.8f, 1.85f, rotation, body, trim, parent);
            CreateBox(name + " vehicle exit ramp", new Vector3(x + 0.3f, HeightWorld(x + 0.3f, z - 3.7f) + 0.08f, z - 3.7f), new Vector3(4.8f, 0.10f, 2.4f), rotation, trim, parent);
            CreateBox(name + " crane rail", new Vector3(x + 2.9f, HeightWorld(x + 2.9f, z + 0.3f) + 2.55f, z + 0.3f), new Vector3(0.18f, 0.18f, 4.4f), rotation, trim, parent);
            CreateBox(name + " crane arm", new Vector3(x + 2.05f, HeightWorld(x + 2.05f, z + 0.3f) + 2.82f, z + 0.3f), new Vector3(1.8f, 0.14f, 0.16f), rotation, trim, parent);
        }

        static void CreateRefinery(string name, float x, float z, Quaternion rotation, Material body, Material trim, Transform parent)
        {
            CreateIndustrialBuilding(name, x, z, 4.7f, 3.9f, 1.75f, rotation, body, trim, parent);
            CreateCylinder(name + " storage tank A", new Vector3(x + 3.5f, HeightWorld(x + 3.5f, z + 0.7f) + 0.92f, z + 0.7f), new Vector3(0.88f, 0.92f, 0.88f), trim, parent);
            CreateCylinder(name + " storage tank B", new Vector3(x + 4.9f, HeightWorld(x + 4.9f, z - 0.8f) + 0.82f, z - 0.8f), new Vector3(0.76f, 0.80f, 0.76f), trim, parent);
            CreateBox(name + " ore unload pad", new Vector3(x - 2.5f, HeightWorld(x - 2.5f, z - 2.5f) + 0.10f, z - 2.5f), new Vector3(3.0f, 0.12f, 1.9f), rotation, trim, parent);
        }

        static void CreateTurret(string name, float x, float z, Material material, Transform parent)
        {
            var y = HeightWorld(x, z);
            CreateCylinder(name + " base", new Vector3(x, y + 0.34f, z), new Vector3(0.88f, 0.34f, 0.88f), material, parent);
            CreateBox(name + " head", new Vector3(x, y + 0.86f, z), new Vector3(1.0f, 0.48f, 0.78f), Quaternion.Euler(0f, 22f, 0f), material, parent);
            CreateBox(name + " barrel", new Vector3(x + 0.82f, y + 0.88f, z + 0.34f), new Vector3(1.25f, 0.12f, 0.12f), Quaternion.Euler(0f, 22f, 0f), material, parent);
        }

        static void CreateVehicleProxy(string name, float x, float z, float rotationY, Material body, Material trim, Transform parent)
        {
            var rotation = Quaternion.Euler(0f, rotationY, 0f);
            var y = HeightWorld(x, z);
            CreateBox(name + " hull", new Vector3(x, y + 0.36f, z), new Vector3(1.6f, 0.48f, 2.25f), rotation, body, parent);
            CreateBox(name + " left track", new Vector3(x - 0.95f, y + 0.25f, z), new Vector3(0.32f, 0.28f, 2.35f), rotation, trim, parent);
            CreateBox(name + " right track", new Vector3(x + 0.95f, y + 0.25f, z), new Vector3(0.32f, 0.28f, 2.35f), rotation, trim, parent);
            CreateBox(name + " turret", new Vector3(x, y + 0.78f, z + 0.12f), new Vector3(0.92f, 0.34f, 0.78f), rotation, trim, parent);
            CreateBox(name + " cannon", new Vector3(x, y + 0.80f, z + 1.08f), new Vector3(0.14f, 0.12f, 1.45f), rotation, trim, parent);
        }

        static void CreateCrateStack(string name, float x, float z, Transform parent, MaterialBundle materials)
        {
            CreateBox(name + " crate A", new Vector3(x, HeightWorld(x, z) + 0.34f, z), new Vector3(0.82f, 0.68f, 0.82f), Quaternion.Euler(0f, 12f, 0f), materials.NeutralBuilding, parent);
            CreateBox(name + " crate B", new Vector3(x + 0.86f, HeightWorld(x + 0.86f, z + 0.18f) + 0.32f, z + 0.18f), new Vector3(0.72f, 0.64f, 0.72f), Quaternion.Euler(0f, -7f, 0f), materials.NeutralBuilding, parent);
            CreateBox(name + " barrel", new Vector3(x - 0.74f, HeightWorld(x - 0.74f, z - 0.42f) + 0.36f, z - 0.42f), new Vector3(0.36f, 0.46f, 0.36f), Quaternion.identity, materials.MetalDark, parent);
        }

        static void CreateBase(float x, float z, float rotation, bool playerBase, Transform parent)
        {
            var root = new GameObject(playerBase ? "Southwest player base layout" : "Northeast enemy base layout");
            root.transform.SetParent(parent, false);
            var tint = playerBase ? new Color(0.55f, 0.86f, 1f, 1f) : new Color(1f, 0.42f, 0.32f, 1f);

            PlacePiece("foundation_pad_large", x, z, rotation, 1.25f, root.transform);
            PlacePiece("base_industrial_pad_01", x + 5f, z + 2.5f, rotation, 1.0f, root.transform);
            PlacePiece("base_foundation_pad_01", x - 5f, z + 3f, rotation, 1.0f, root.transform);
            PlacePiece("base_road_strip_01", x + 8f, z + 6f, rotation + 7f, 1.12f, root.transform);
            PlacePiece("base_road_strip_02", x + 12f, z + 8f, rotation + 7f, 1.05f, root.transform);
            PlacePiece("base_rally_exit_marking_01", x + 9f, z + 5f, rotation + 90f, 0.92f, root.transform);
            PlacePiece("barrier_concrete_01", x - 8f, z - 5f, rotation, 0.95f, root.transform);
            PlacePiece("barrier_corner_01", x + 10f, z - 5f, rotation + 90f, 0.90f, root.transform);
            PlacePiece("fence_gate_01", x + 13f, z + 2f, rotation + 90f, 0.82f, root.transform);

            PlaceActor("fabrication_hub_blockout", x - 2.6f, z + 0.2f, rotation, 1.15f, tint, root.transform);
            PlaceActor("power_plant_blockout", x - 8.2f, z + 4.4f, rotation, 0.90f, tint, root.transform);
            PlaceActor("refinery_blockout", x + 4.0f, z + 5.0f, rotation, 1.0f, tint, root.transform);
            PlaceActor("barracks_blockout", x - 6.0f, z - 3.8f, rotation, 0.88f, tint, root.transform);
            PlaceActor("war_factory_blockout", x + 5.8f, z - 3.8f, rotation, 1.08f, tint, root.transform);
            PlaceActor("gun_tower_blockout", x - 11f, z - 7.3f, rotation, 0.72f, tint, root.transform);
            PlaceActor("gun_tower_blockout", x + 12f, z - 6.5f, rotation, 0.72f, tint, root.transform);
            PlaceActor(playerBase ? "light_tank_blockout" : "medium_tank_blockout", x + 11f, z + 10f, rotation + 20f, 0.72f, tint, root.transform);
            PlaceActor(playerBase ? "harvester_blockout" : "heavy_tank_blockout", x + 15f, z + 7f, rotation - 12f, 0.70f, tint, root.transform);
        }

        static void CreateNeutralOutpost(float x, float z, Transform parent)
        {
            var root = new GameObject("Neutral abandoned outpost");
            root.transform.SetParent(parent, false);
            PlacePiece("base_octagon_pad_01", x, z, 0f, 1.05f, root.transform);
            PlacePiece("base_curb_straight", x - 5f, z - 4f, 0f, 1.0f, root.transform);
            PlacePiece("base_curb_straight", x + 5f, z - 4f, 0f, 1.0f, root.transform);
            PlacePiece("crate_stack_01", x - 3f, z + 2f, -8f, 0.70f, root.transform);
            PlacePiece("barrel_group_01", x + 2.8f, z + 1.2f, 12f, 0.78f, root.transform);
            PlacePiece("anti_tank_obstacle_01", x + 5.8f, z + 4.0f, 45f, 0.70f, root.transform);
            PlaceActor("comm_center_blockout", x, z, 0f, 0.82f, new Color(0.72f, 0.76f, 0.70f, 1f), root.transform);
        }

        static void CreateOreField(OreField field, Transform parent)
        {
            PlacePiece("resource_field_ground_01", field.X, field.Z, 0f, Mathf.Clamp(field.Radius / 3.8f, 1.0f, 1.55f), parent);
            var ids = new[] { "resource_cluster_blue_01", "resource_cluster_gold_01", "resource_cluster_green_01", "resource_cluster_01", "resource_rich_cluster_01" };
            for (var i = 0; i < 18; i++)
            {
                var angle = i * 137.5f;
                var radius = 0.8f + (i % 6) * field.Radius / 6.5f;
                var px = field.X + Mathf.Cos(angle * Mathf.Deg2Rad) * radius + Mathf.Sin(i * 0.73f) * 0.6f;
                var pz = field.Z + Mathf.Sin(angle * Mathf.Deg2Rad) * radius + Mathf.Cos(i * 0.49f) * 0.5f;
                PlacePiece(ids[i % ids.Length], px, pz, angle, 0.62f + (i % 4) * 0.08f, parent);
            }
        }

        static void PlaceForestPatch(float x, float z, int count, Transform parent)
        {
            var ids = new[] { "tree_cluster_01", "conifer_line_01", "shrub_cluster_01", "bush_line_01", "obstacle_tree_bush_cluster_01" };
            for (var i = 0; i < count; i++)
            {
                var angle = i * 61f;
                var radius = 1.2f + (i % 5) * 1.4f;
                var px = x + Mathf.Cos(angle * Mathf.Deg2Rad) * radius + Mathf.Sin(i * 1.7f);
                var pz = z + Mathf.Sin(angle * Mathf.Deg2Rad) * radius + Mathf.Cos(i * 1.1f);
                PlacePiece(ids[i % ids.Length], px, pz, angle, 0.70f + (i % 3) * 0.09f, parent);
            }
        }

        static void PlaceScatter(string id, float x, float z, int count, float radius, Transform parent)
        {
            for (var i = 0; i < count; i++)
            {
                var angle = i * 89f + StableHash(id) % 47;
                var r = radius * (0.20f + ((i * 37) % 100) / 130f);
                var px = x + Mathf.Cos(angle * Mathf.Deg2Rad) * r;
                var pz = z + Mathf.Sin(angle * Mathf.Deg2Rad) * r;
                PlacePiece(id, px, pz, angle, 0.65f + (i % 4) * 0.08f, parent);
            }
        }

        static void PlaceAlongPath(string id, Vector3[] points, float spacing, float scale, float sideOffset, Transform parent)
        {
            for (var i = 0; i < points.Length - 1; i++)
            {
                var a = points[i];
                var b = points[i + 1];
                var length = Vector3.Distance(a, b);
                var count = Mathf.Max(1, Mathf.RoundToInt(length / spacing));
                var direction = (b - a).normalized;
                var normal = new Vector3(-direction.z, 0f, direction.x);
                var rotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                for (var step = 0; step < count; step++)
                {
                    var t = (step + 0.5f) / count;
                    var p = Vector3.Lerp(a, b, t) + normal * sideOffset;
                    PlacePiece(id, p.x, p.z, rotation + ((step % 2 == 0) ? 0f : 180f), scale * (0.92f + (step % 3) * 0.04f), parent);
                }
            }
        }

        static GameObject PlacePiece(string id, float x, float z, float rotationY, float scale, Transform parent)
        {
            var prefab = LoadTerrainPrefab(id);
            if (prefab == null)
                return null;

            var instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            if (instance == null)
                return null;

            instance.name = "MapPiece " + id;
            instance.transform.position = new Vector3(x, HeightWorld(x, z) + 0.08f, z);
            instance.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
            instance.transform.localScale = Vector3.one * Mathf.Clamp(scale, 0.45f, 1.80f);
            RemoveColliders(instance);
            return instance;
        }

        static GameObject PlaceSamplePrefab(string id, float x, float z, float rotationY, float scale, Transform parent)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TerrainSampleAssets/Prefabs/" + id + ".prefab");
            if (prefab == null)
                return null;

            var instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            if (instance == null)
                return null;

            instance.name = "TerrainSample " + id;
            instance.transform.position = new Vector3(x, HeightWorld(x, z) + 0.06f, z);
            instance.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
            instance.transform.localScale = Vector3.one * Mathf.Clamp(scale, 0.35f, 1.25f);
            RemoveColliders(instance);
            return instance;
        }

        static GameObject PlaceActor(string id, float x, float z, float rotationY, float scale, Color tint, Transform parent)
        {
            var prefab = LoadActorPrefab(id);
            if (prefab == null)
                return null;

            var instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            if (instance == null)
                return null;

            instance.name = "MapActor " + id;
            instance.transform.position = new Vector3(x, HeightWorld(x, z) + 0.16f, z);
            instance.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
            instance.transform.localScale = Vector3.one * Mathf.Clamp(scale, 0.45f, 1.55f);
            ApplyTint(instance, tint);
            RemoveColliders(instance);
            return instance;
        }

        static GameObject LoadTerrainPrefab(string id)
        {
            var candidatePaths = new[]
            {
                "Assets/Rts/Art/Prefabs/Terrain/FinalMeshBatch01/" + id + ".prefab",
                "Assets/Rts/Art/Prefabs/Terrain/Stage32Generated/" + id + ".prefab",
                "Assets/Rts/Art/Prefabs/Terrain/Stage32_6Runtime/" + id + ".prefab",
                "Assets/Rts/Art/Prefabs/Terrain/TerrainSampleGroundTiles/" + id + ".prefab",
                "Assets/Rts/Art/Prefabs/TerrainPieces/Ground/" + id + ".prefab",
                "Assets/Rts/Art/Prefabs/TerrainPieces/Transitions/" + id + ".prefab",
                "Assets/Rts/Art/Prefabs/TerrainPieces/Base/" + id + ".prefab",
                "Assets/Rts/Art/Prefabs/TerrainPieces/Obstacles/" + id + ".prefab",
                "Assets/Rts/Art/Prefabs/TerrainPieces/Resources/" + id + ".prefab",
                "Assets/Rts/Art/Prefabs/TerrainPieces/Props/" + id + ".prefab"
            };

            for (var i = 0; i < candidatePaths.Length; i++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(candidatePaths[i]);
                if (prefab != null)
                    return prefab;
            }

            var library = Stage32TerrainPieceGenerator.LoadTerrainPieceLibrary();
            var definition = library != null ? library.GetDefinition(id) : null;
            return definition != null ? definition.prefab : null;
        }

        static GameObject LoadActorPrefab(string id)
        {
            var paths = new[]
            {
                "Assets/Rts/Art/Prefabs/Actors/GeneratedBlockouts/" + id + ".prefab",
                "Assets/Rts/Art/Prefabs/Actors/ProductionProxies/" + id.Replace("_blockout", "_production_proxy") + ".prefab"
            };

            for (var i = 0; i < paths.Length; i++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
                if (prefab != null)
                    return prefab;
            }

            return null;
        }

        static string GetCommandLineArgument(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            return null;
        }

        static void CreateRibbon(string name, Vector3[] points, float width, Material material, float yOffset, Transform parent)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            for (var i = 0; i < points.Length; i++)
            {
                var previous = i == 0 ? points[i] : points[i - 1];
                var next = i == points.Length - 1 ? points[i] : points[i + 1];
                var direction = (next - previous).normalized;
                var normal = new Vector3(-direction.z, 0f, direction.x) * (width * 0.5f);
                var p = points[i];
                var y = HeightWorld(p.x, p.z) + yOffset;
                vertices.Add(new Vector3(p.x - normal.x, y, p.z - normal.z));
                vertices.Add(new Vector3(p.x + normal.x, y, p.z + normal.z));
                uvs.Add(new Vector2(0f, i));
                uvs.Add(new Vector2(1f, i));
            }

            for (var i = 0; i < points.Length - 1; i++)
            {
                var a = i * 2;
                triangles.Add(a);
                triangles.Add(a + 2);
                triangles.Add(a + 1);
                triangles.Add(a + 1);
                triangles.Add(a + 2);
                triangles.Add(a + 3);
            }

            var mesh = new Mesh();
            mesh.name = name + " mesh";
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            var filter = root.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = root.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.receiveShadows = true;
        }

        static void CreateVisibleGroundMesh(Transform parent, MaterialBundle materials)
        {
            var root = new GameObject("Continuous AI-painted heather terrain surface");
            root.transform.SetParent(parent, false);
            root.transform.position = Vector3.zero;

            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(GroundMeshPath);
            if (mesh == null)
            {
                mesh = new Mesh();
                mesh.name = "unity_ai_classic_rts_ground_mesh";
                AssetDatabase.CreateAsset(mesh, GroundMeshPath);
            }
            else
            {
                mesh.Clear();
            }

            var vertices = new Vector3[(GroundMeshXSegments + 1) * (GroundMeshZSegments + 1)];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[GroundMeshXSegments * GroundMeshZSegments * 6];
            var vertex = 0;
            for (var z = 0; z <= GroundMeshZSegments; z++)
            {
                var vz = z / (float)GroundMeshZSegments;
                var worldZ = Mathf.Lerp(-MapDepth * 0.5f, MapDepth * 0.5f, vz);
                for (var x = 0; x <= GroundMeshXSegments; x++)
                {
                    var vx = x / (float)GroundMeshXSegments;
                    var worldX = Mathf.Lerp(-MapWidth * 0.5f, MapWidth * 0.5f, vx);
                    vertices[vertex] = new Vector3(worldX, HeightWorld(worldX, worldZ) + 0.015f, worldZ);
                    uvs[vertex] = new Vector2(vx, vz);
                    vertex++;
                }
            }

            var index = 0;
            var stride = GroundMeshXSegments + 1;
            for (var z = 0; z < GroundMeshZSegments; z++)
            {
                for (var x = 0; x < GroundMeshXSegments; x++)
                {
                    var a = z * stride + x;
                    var b = a + 1;
                    var c = a + stride;
                    var d = c + 1;
                    triangles[index++] = a;
                    triangles[index++] = c;
                    triangles[index++] = b;
                    triangles[index++] = b;
                    triangles[index++] = c;
                    triangles[index++] = d;
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            EditorUtility.SetDirty(mesh);

            var filter = root.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = root.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = materials.Terrain;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = true;
        }

        static void CreateLighting()
        {
            var lightObject = new GameObject("Warm low sun key light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.35f;
            light.color = new Color(1f, 0.92f, 0.80f, 1f);
            lightObject.transform.rotation = Quaternion.Euler(52f, -38f, 0f);

            var fillObject = new GameObject("Cool ambient fill");
            var fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.34f;
            fill.color = new Color(0.62f, 0.74f, 0.92f, 1f);
            fillObject.transform.rotation = Quaternion.Euler(66f, 132f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.48f, 0.54f, 0.50f, 1f);
            RenderSettings.fog = false;
        }

        static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 64f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 500f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.045f, 0.057f, 0.054f, 1f);
            cameraObject.transform.position = new Vector3(0f, 120f, 0f);
            cameraObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        static void CreateSceneLabels(Transform parent)
        {
            CreateLabel("Unity AI terrain pass: smooth heather map, cliffs, ore, bases, roads, streams", new Vector3(0f, 0.45f, -61f), parent, 1.15f);
            CreateLabel("player base", new Vector3(-21f, HeightWorld(-21f, -26f) + 0.3f, -26f), parent, 0.62f);
            CreateLabel("enemy base", new Vector3(20f, HeightWorld(20f, 27f) + 0.3f, 27f), parent, 0.62f);
        }

        static void CreateLabel(string text, Vector3 position, Transform parent, float scale)
        {
            var label = new GameObject("Map label - " + text);
            label.transform.SetParent(parent, false);
            label.transform.position = position;
            label.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
            label.transform.localScale = Vector3.one * scale;
            var mesh = label.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.fontSize = 44;
            mesh.characterSize = 0.085f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = new Color(0.84f, 0.90f, 0.86f, 1f);
        }

        static MaterialBundle CreateMaterials()
        {
            EnsureFolderRecursive(AssetFolder);
            EnsureFolderRecursive(TextureFolder);
            return new MaterialBundle
            {
                Terrain = CreateTerrainMaterial(AssetFolder + "/unity_ai_terrain_template.mat"),
                Road = CreateMaterial(AssetFolder + "/unity_ai_road_asphalt.mat", "road_asphalt", new Color(0.18f, 0.18f, 0.16f, 1f), new Color(0.36f, 0.34f, 0.30f, 1f), SurfacePattern.Concrete, 0.18f, 0f),
                RoadShoulder = CreateMaterial(AssetFolder + "/unity_ai_road_shoulder.mat", "road_shoulder", new Color(0.34f, 0.30f, 0.22f, 1f), new Color(0.53f, 0.48f, 0.38f, 1f), SurfacePattern.Dirt, 0.10f, 0f),
                RoadLine = CreateMaterial(AssetFolder + "/unity_ai_road_line.mat", "road_line", new Color(0.78f, 0.76f, 0.62f, 1f), new Color(0.94f, 0.88f, 0.58f, 1f), SurfacePattern.Concrete, 0.16f, 0f),
                Water = CreateMaterial(AssetFolder + "/unity_ai_stream_water.mat", "stream_water", new Color(0.10f, 0.46f, 0.72f, 1f), new Color(0.45f, 0.82f, 0.94f, 1f), SurfacePattern.Water, 0.62f, 0f),
                MudBank = CreateMaterial(AssetFolder + "/unity_ai_stream_mud_bank.mat", "stream_mud_bank", new Color(0.24f, 0.20f, 0.14f, 1f), new Color(0.42f, 0.33f, 0.22f, 1f), SurfacePattern.Dirt, 0.12f, 0f),
                Rock = CreateMaterial(AssetFolder + "/unity_ai_cliff_rock.mat", "cliff_rock", new Color(0.28f, 0.26f, 0.22f, 1f), new Color(0.62f, 0.58f, 0.49f, 1f), SurfacePattern.Rock, 0.18f, 0f),
                CliffShadow = CreateMaterial(AssetFolder + "/unity_ai_cliff_shadow.mat", "cliff_shadow", new Color(0.08f, 0.075f, 0.065f, 0.78f), new Color(0.19f, 0.17f, 0.13f, 0.78f), SurfacePattern.Rock, 0.10f, 0f),
                OreBlue = CreateMaterial(AssetFolder + "/unity_ai_ore_blue.mat", "ore_blue", new Color(0.08f, 0.72f, 1f, 1f), new Color(0.72f, 0.96f, 1f, 1f), SurfacePattern.Crystal, 0.45f, 0f),
                OreGold = CreateMaterial(AssetFolder + "/unity_ai_ore_gold.mat", "ore_gold", new Color(0.95f, 0.78f, 0.22f, 1f), new Color(1f, 0.95f, 0.50f, 1f), SurfacePattern.Crystal, 0.36f, 0f),
                OreSoil = CreateMaterial(AssetFolder + "/unity_ai_ore_soil.mat", "ore_soil", new Color(0.18f, 0.14f, 0.10f, 1f), new Color(0.39f, 0.28f, 0.17f, 1f), SurfacePattern.Dirt, 0.10f, 0f),
                TreeTrunk = CreateMaterial(AssetFolder + "/unity_ai_tree_trunk.mat", "tree_trunk", new Color(0.22f, 0.15f, 0.09f, 1f), new Color(0.42f, 0.28f, 0.16f, 1f), SurfacePattern.Wood, 0.08f, 0f),
                TreeFoliage = CreateMaterial(AssetFolder + "/unity_ai_tree_foliage.mat", "tree_foliage", new Color(0.08f, 0.20f, 0.09f, 1f), new Color(0.19f, 0.42f, 0.16f, 1f), SurfacePattern.Foliage, 0.15f, 0f),
                Concrete = CreateMaterial(AssetFolder + "/unity_ai_base_concrete.mat", "base_concrete", new Color(0.34f, 0.36f, 0.33f, 1f), new Color(0.58f, 0.59f, 0.54f, 1f), SurfacePattern.Concrete, 0.24f, 0f),
                PlayerBuilding = CreateMaterial(AssetFolder + "/unity_ai_player_buildings.mat", "player_buildings", new Color(0.28f, 0.58f, 0.72f, 1f), new Color(0.62f, 0.92f, 1f, 1f), SurfacePattern.MetalPanel, 0.28f, 0f),
                EnemyBuilding = CreateMaterial(AssetFolder + "/unity_ai_enemy_buildings.mat", "enemy_buildings", new Color(0.62f, 0.08f, 0.06f, 1f), new Color(1.0f, 0.32f, 0.24f, 1f), SurfacePattern.MetalPanel, 0.30f, 0f),
                NeutralBuilding = CreateMaterial(AssetFolder + "/unity_ai_neutral_buildings.mat", "neutral_buildings", new Color(0.48f, 0.50f, 0.43f, 1f), new Color(0.72f, 0.74f, 0.62f, 1f), SurfacePattern.MetalPanel, 0.22f, 0f),
                PlayerWall = CreateMaterial(AssetFolder + "/unity_ai_player_walls.mat", "player_walls", new Color(0.10f, 0.34f, 0.46f, 1f), new Color(0.35f, 0.78f, 0.95f, 1f), SurfacePattern.Concrete, 0.20f, 0f),
                EnemyWall = CreateMaterial(AssetFolder + "/unity_ai_enemy_walls.mat", "enemy_walls", new Color(0.40f, 0.04f, 0.03f, 1f), new Color(0.80f, 0.18f, 0.14f, 1f), SurfacePattern.Concrete, 0.20f, 0f),
                MetalDark = CreateMaterial(AssetFolder + "/unity_ai_dark_metal.mat", "dark_metal", new Color(0.12f, 0.13f, 0.13f, 1f), new Color(0.32f, 0.34f, 0.32f, 1f), SurfacePattern.MetalPanel, 0.30f, 0.05f)
            };
        }

        static Material CreateTerrainMaterial(string path)
        {
            var texture = CreateGroundTexture();
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
                shader = Shader.Find("Unlit/Texture");
            if (shader == null)
                shader = Shader.Find("Standard");

            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            SetColor(material, "_BaseColor", Color.white);
            SetColor(material, "_Color", Color.white);
            SetTexture(material, "_BaseMap", texture);
            SetTexture(material, "_MainTex", texture);
            SetFloat(material, "_Smoothness", 0.22f);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Texture2D CreateGroundTexture()
        {
            var texturePath = Path.Combine(Application.dataPath, GroundTexturePath.Substring("Assets/".Length)).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(texturePath));

            var aiBattlefieldTexture = LoadOptionalTexture(AiBattlefieldMapTexturePath);
            var aiHeatherTexture = LoadOptionalTexture(AiHeatherTerrainTexturePath);
            var texture = new Texture2D(GroundTextureWidth, GroundTextureHeight, TextureFormat.RGBA32, false, true);
            for (var y = 0; y < GroundTextureHeight; y++)
            {
                var v = y / (GroundTextureHeight - 1f);
                var worldZ = Mathf.Lerp(-MapDepth * 0.5f, MapDepth * 0.5f, v);
                for (var x = 0; x < GroundTextureWidth; x++)
                {
                    var u = x / (GroundTextureWidth - 1f);
                    var worldX = Mathf.Lerp(-MapWidth * 0.5f, MapWidth * 0.5f, u);
                    texture.SetPixel(x, y, GroundColor(worldX, worldZ, aiHeatherTexture, aiBattlefieldTexture));
                }
            }

            texture.Apply();
            File.WriteAllBytes(texturePath, texture.EncodeToPNG());
            if (aiBattlefieldTexture != null)
                Object.DestroyImmediate(aiBattlefieldTexture);
            if (aiHeatherTexture != null)
                Object.DestroyImmediate(aiHeatherTexture);
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(GroundTexturePath, ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(GroundTexturePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.mipmapEnabled = true;
                importer.filterMode = FilterMode.Trilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
            }

            var imported = AssetDatabase.LoadAssetAtPath<Texture2D>(GroundTexturePath);
            if (imported == null)
                throw new InvalidOperationException("Could not create Unity AI ground texture at " + GroundTexturePath);
            return imported;
        }

        static Texture2D LoadOptionalTexture(string assetPath)
        {
            var filePath = Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length)).Replace('\\', '/');
            if (!File.Exists(filePath))
                return null;

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            if (!texture.LoadImage(File.ReadAllBytes(filePath)))
            {
                Object.DestroyImmediate(texture);
                return null;
            }

            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Trilinear;
            return texture;
        }

        static Color GroundColor(float x, float z, Texture2D aiHeatherTexture, Texture2D aiBattlefieldTexture)
        {
            var palette = new[]
            {
                new Color(0.20f, 0.30f, 0.20f, 1f),
                new Color(0.18f, 0.36f, 0.18f, 1f),
                new Color(0.33f, 0.27f, 0.20f, 1f),
                new Color(0.40f, 0.38f, 0.32f, 1f),
                new Color(0.23f, 0.20f, 0.16f, 1f),
                new Color(0.42f, 0.40f, 0.35f, 1f),
                new Color(0.08f, 0.075f, 0.065f, 1f),
                new Color(0.10f, 0.27f, 0.25f, 1f)
            };

            var weights = TerrainWeights(x, z, palette.Length);
            var color = Color.black;
            for (var i = 0; i < weights.Length; i++)
                color += palette[i] * weights[i];

            if (aiHeatherTexture != null)
            {
                var heatherWeight = Mathf.Clamp01(weights[0] + weights[1] * 0.85f);
                var roadOrBase = Mathf.Max(weights[2], weights[3]);
                var rockOrWater = Mathf.Max(weights[5], weights[7]);
                var blend = Mathf.Clamp01(heatherWeight - roadOrBase * 0.9f - rockOrWater * 0.75f) * 0.24f;
                if (blend > 0.01f)
                {
                    var sample = aiHeatherTexture.GetPixelBilinear((x + MapWidth * 0.5f) / 9.0f, (z + MapDepth * 0.5f) / 9.0f);
                    sample = Color.Lerp(sample, new Color(sample.grayscale, sample.grayscale, sample.grayscale, 1f), 0.18f);
                    color = Color.Lerp(color, sample * 0.72f, blend);
                }
            }

            var detail = Mathf.PerlinNoise((x + 300f) * 0.55f, (z - 91f) * 0.55f);
            var broad = Mathf.PerlinNoise((x - 21f) * 0.12f, (z + 48f) * 0.12f);
            var speckle = Mathf.PerlinNoise((x + 11f) * 2.7f, (z + 17f) * 2.7f);
            color *= 0.82f + detail * 0.19f + broad * 0.08f;

            if (speckle > 0.72f)
                color = Color.Lerp(color, new Color(0.62f, 0.60f, 0.50f, 1f), (speckle - 0.72f) * 0.55f);

            for (var i = 0; i < OreFields.Length; i++)
            {
                var d = Vector2.Distance(new Vector2(x, z), new Vector2(OreFields[i].X, OreFields[i].Z));
                if (d < OreFields[i].Radius + 1.2f)
                    color = Color.Lerp(color, new Color(0.28f, 0.24f, 0.18f, 1f), Smooth01(OreFields[i].Radius + 1.2f, 0f, d) * 0.38f);
            }

            var road = Mathf.Min(DistanceToPath(x, z, MainRoad), DistanceToPath(x, z, CrossRoad));
            if (road < 3.7f)
                color = Color.Lerp(color, new Color(0.14f, 0.14f, 0.12f, 1f), Smooth01(3.7f, 0f, road) * 0.72f);
            else if (road < 5.6f)
                color = Color.Lerp(color, new Color(0.28f, 0.25f, 0.18f, 1f), Smooth01(5.6f, 3.7f, road) * 0.36f);

            if (aiBattlefieldTexture != null)
            {
                var u = Mathf.Clamp01((x + MapWidth * 0.5f) / MapWidth);
                var v = Mathf.Clamp01((z + MapDepth * 0.5f) / MapDepth);
                var sample = aiBattlefieldTexture.GetPixelBilinear(u, v);
                sample = Color.Lerp(sample, new Color(sample.grayscale, sample.grayscale, sample.grayscale, 1f), 0.06f);
                color = Color.Lerp(color, sample, 0.92f);
            }

            color.a = 1f;
            return color;
        }

        static bool HasAiBattlefieldMapTexture()
        {
            var filePath = Path.Combine(Application.dataPath, AiBattlefieldMapTexturePath.Substring("Assets/".Length)).Replace('\\', '/');
            return File.Exists(filePath);
        }

        static Material CreateMaterial(string path, string textureName, Color color, Color accent, SurfacePattern pattern, float smoothness, float metallic)
        {
            var texture = CreateSurfaceTexture(textureName, color, accent, pattern);
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            if (shader == null)
                shader = Shader.Find("Unlit/Color");

            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            SetColor(material, "_BaseColor", color);
            SetColor(material, "_Color", color);
            SetTexture(material, "_BaseMap", texture);
            SetTexture(material, "_MainTex", texture);
            SetFloat(material, "_Smoothness", smoothness);
            SetFloat(material, "_Metallic", metallic);
            SetFloat(material, "_Cull", 0f);
            if (color.a < 0.99f)
            {
                SetFloat(material, "_Surface", 1f);
                SetFloat(material, "_AlphaClip", 0f);
                material.renderQueue = 3000;
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        static Texture2D CreateSurfaceTexture(string name, Color baseColor, Color accent, SurfacePattern pattern)
        {
            var assetPath = TextureFolder + "/unity_ai_" + name + "_texture.png";
            var filePath = Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length)).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            var texture = new Texture2D(SurfaceTextureSize, SurfaceTextureSize, TextureFormat.RGBA32, false, true);
            var seed = StableHash(name) * 0.0017f;
            for (var y = 0; y < SurfaceTextureSize; y++)
            {
                var v = y / (SurfaceTextureSize - 1f);
                for (var x = 0; x < SurfaceTextureSize; x++)
                {
                    var u = x / (SurfaceTextureSize - 1f);
                    texture.SetPixel(x, y, SurfaceColor(baseColor, accent, pattern, u, v, seed));
                }
            }

            texture.Apply();
            File.WriteAllBytes(filePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.mipmapEnabled = true;
                importer.filterMode = FilterMode.Trilinear;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.SaveAndReimport();
            }

            var imported = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (imported == null)
                throw new InvalidOperationException("Could not create generated surface texture: " + assetPath);
            return imported;
        }

        static Color SurfaceColor(Color baseColor, Color accent, SurfacePattern pattern, float u, float v, float seed)
        {
            var n1 = Mathf.PerlinNoise(u * 8.0f + seed, v * 8.0f - seed);
            var n2 = Mathf.PerlinNoise(u * 26.0f - seed * 0.7f, v * 26.0f + seed * 0.4f);
            var n3 = Mathf.PerlinNoise(u * 72.0f + seed * 1.3f, v * 72.0f - seed * 1.1f);
            var color = Color.Lerp(baseColor * 0.74f, accent, n1 * 0.45f + n2 * 0.20f);

            switch (pattern)
            {
                case SurfacePattern.Rock:
                    {
                        var crack = Mathf.Abs(Mathf.Sin((u * 15f + n1 * 2.5f) * Mathf.PI) + Mathf.Sin((v * 17f + n2 * 1.8f) * Mathf.PI));
                        if (crack < 0.34f)
                            color = Color.Lerp(color, Color.black, 0.28f);
                        color *= 0.78f + n3 * 0.38f;
                        break;
                    }
                case SurfacePattern.Concrete:
                    {
                        var line = Mathf.Min(Mathf.Abs(Mathf.Repeat(u * 5f, 1f) - 0.5f), Mathf.Abs(Mathf.Repeat(v * 5f, 1f) - 0.5f));
                        if (line < 0.025f)
                            color = Color.Lerp(color, Color.black, 0.22f);
                        color *= 0.86f + n3 * 0.20f;
                        break;
                    }
                case SurfacePattern.MetalPanel:
                    {
                        var panel = Mathf.Min(Mathf.Abs(Mathf.Repeat(u * 4f, 1f) - 0.5f), Mathf.Abs(Mathf.Repeat(v * 3f, 1f) - 0.5f));
                        if (panel < 0.035f)
                            color = Color.Lerp(color, Color.black, 0.24f);
                        if (n3 > 0.74f)
                            color = Color.Lerp(color, new Color(0.95f, 0.90f, 0.75f, 1f), 0.18f);
                        break;
                    }
                case SurfacePattern.Foliage:
                    {
                        var needle = Mathf.Abs(Mathf.Sin((u * 36f + v * 9f) * Mathf.PI));
                        color = Color.Lerp(color, accent * 0.62f, needle * 0.28f);
                        color *= 0.72f + n2 * 0.42f;
                        break;
                    }
                case SurfacePattern.Wood:
                    {
                        var grain = Mathf.Abs(Mathf.Sin((u * 6f + n1 * 2f) * Mathf.PI));
                        color = Color.Lerp(color, accent * 0.55f, grain * 0.42f);
                        break;
                    }
                case SurfacePattern.Crystal:
                    {
                        var facets = Mathf.Abs(Mathf.Sin((u * 7f + v * 11f + seed) * Mathf.PI));
                        color = Color.Lerp(color, Color.white, facets * 0.28f);
                        break;
                    }
                case SurfacePattern.Water:
                    {
                        var ripple = Mathf.Abs(Mathf.Sin((u * 12f + n1 * 2f) * Mathf.PI) * Mathf.Cos((v * 9f + n2 * 2f) * Mathf.PI));
                        color = Color.Lerp(color, accent, ripple * 0.36f);
                        break;
                    }
                case SurfacePattern.Dirt:
                    color *= 0.76f + n2 * 0.32f + n3 * 0.12f;
                    break;
            }

            color.a = baseColor.a;
            return color;
        }

        static void SetColor(Material material, string propertyName, Color color)
        {
            if (material.HasProperty(propertyName))
                material.SetColor(propertyName, color);
        }

        static void SetFloat(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
                material.SetFloat(propertyName, value);
        }

        static void SetTexture(Material material, string propertyName, Texture texture)
        {
            if (material.HasProperty(propertyName))
                material.SetTexture(propertyName, texture);
        }

        static GameObject CreateBox(string name, Vector3 position, Vector3 scale, Quaternion rotation, Material material, Transform parent)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.position = position;
            box.transform.rotation = rotation;
            box.transform.localScale = scale;
            var renderer = box.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.receiveShadows = true;
            }

            var collider = box.GetComponent<Collider>();
            if (collider != null)
                Object.DestroyImmediate(collider);
            return box;
        }

        static GameObject CreateBoulder(string name, Vector3 position, Vector3 scale, Quaternion rotation, Material material, Transform parent)
        {
            var boulder = new GameObject(name);
            boulder.name = name;
            boulder.transform.SetParent(parent, false);
            boulder.transform.position = position;
            boulder.transform.rotation = rotation;
            boulder.transform.localScale = scale;
            var filter = boulder.AddComponent<MeshFilter>();
            filter.sharedMesh = CreateJaggedRockMesh(name + " mesh", position);
            var renderer = boulder.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.receiveShadows = true;
            return boulder;
        }

        static Mesh CreateJaggedRockMesh(string name, Vector3 seedPosition)
        {
            var mesh = new Mesh();
            mesh.name = name;
            const int sides = 9;
            var vertices = new List<Vector3>();
            vertices.Add(new Vector3(0f, 0.62f, 0f));
            vertices.Add(new Vector3(0f, -0.48f, 0f));
            for (var ring = 0; ring < 2; ring++)
            {
                var y = ring == 0 ? 0.18f : -0.28f;
                var radius = ring == 0 ? 0.74f : 0.92f;
                for (var i = 0; i < sides; i++)
                {
                    var angle = i * Mathf.PI * 2f / sides;
                    var wobble = Mathf.PerlinNoise(seedPosition.x * 0.37f + i * 0.51f, seedPosition.z * 0.29f + ring * 2.7f);
                    var r = radius * (0.72f + wobble * 0.46f);
                    vertices.Add(new Vector3(Mathf.Cos(angle) * r, y + (wobble - 0.5f) * 0.22f, Mathf.Sin(angle) * r));
                }
            }

            var triangles = new List<int>();
            for (var i = 0; i < sides; i++)
            {
                var topCurrent = 2 + i;
                var topNext = 2 + ((i + 1) % sides);
                var bottomCurrent = 2 + sides + i;
                var bottomNext = 2 + sides + ((i + 1) % sides);

                triangles.Add(0);
                triangles.Add(topCurrent);
                triangles.Add(topNext);

                triangles.Add(topCurrent);
                triangles.Add(bottomCurrent);
                triangles.Add(topNext);
                triangles.Add(topNext);
                triangles.Add(bottomCurrent);
                triangles.Add(bottomNext);

                triangles.Add(1);
                triangles.Add(bottomNext);
                triangles.Add(bottomCurrent);
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        static GameObject CreateCylinder(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
        {
            var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = name;
            cylinder.transform.SetParent(parent, false);
            cylinder.transform.position = position;
            cylinder.transform.localScale = scale;
            var renderer = cylinder.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.receiveShadows = true;
            }

            var collider = cylinder.GetComponent<Collider>();
            if (collider != null)
                Object.DestroyImmediate(collider);
            return cylinder;
        }

        static GameObject CreateCrystal(string name, Vector3 position, float height, Quaternion rotation, Material material, Transform parent)
        {
            var mesh = new Mesh();
            mesh.name = name + " mesh";
            var radius = height * 0.24f;
            var vertices = new[]
            {
                new Vector3(0f, height * 0.55f, 0f),
                new Vector3(radius, 0f, 0f),
                new Vector3(0f, 0f, radius),
                new Vector3(-radius, 0f, 0f),
                new Vector3(0f, 0f, -radius),
                new Vector3(0f, -height * 0.45f, 0f)
            };
            var triangles = new[]
            {
                0, 1, 2,
                0, 2, 3,
                0, 3, 4,
                0, 4, 1,
                5, 2, 1,
                5, 3, 2,
                5, 4, 3,
                5, 1, 4
            };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            var crystal = new GameObject(name);
            crystal.transform.SetParent(parent, false);
            crystal.transform.position = position;
            crystal.transform.rotation = rotation * Quaternion.Euler((StableHash(name) % 9) - 4f, 0f, 0f);
            var filter = crystal.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = crystal.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.receiveShadows = true;
            return crystal;
        }

        static void CreateTree(string name, float x, float z, float scale, Transform parent, MaterialBundle materials)
        {
            var y = HeightWorld(x, z);
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = name + " trunk";
            trunk.transform.SetParent(parent, false);
            trunk.transform.position = new Vector3(x, y + 0.36f * scale, z);
            trunk.transform.localScale = new Vector3(0.22f * scale, 0.36f * scale, 0.22f * scale);
            trunk.GetComponent<MeshRenderer>().sharedMaterial = materials.TreeTrunk;
            var trunkCollider = trunk.GetComponent<Collider>();
            if (trunkCollider != null)
                Object.DestroyImmediate(trunkCollider);

            for (var tier = 0; tier < 3; tier++)
            {
                var foliage = new GameObject(name + " foliage tier " + tier);
                foliage.transform.SetParent(parent, false);
                foliage.transform.position = new Vector3(x, y + (0.84f + tier * 0.32f) * scale, z);
                foliage.transform.rotation = Quaternion.Euler(0f, StableHash(name + x + z + tier) % 360, 0f);
                foliage.transform.localScale = Vector3.one * scale * (1.12f - tier * 0.22f);
                var filter = foliage.AddComponent<MeshFilter>();
                filter.sharedMesh = CreateConeMesh();
                var renderer = foliage.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = materials.TreeFoliage;
                renderer.receiveShadows = true;
            }
        }

        static Mesh CreateConeMesh()
        {
            var mesh = new Mesh();
            mesh.name = "procedural conifer cone";
            var vertices = new List<Vector3> { new Vector3(0f, 0.78f, 0f), new Vector3(0f, -0.58f, 0f) };
            const int sides = 8;
            for (var i = 0; i < sides; i++)
            {
                var angle = i * Mathf.PI * 2f / sides;
                vertices.Add(new Vector3(Mathf.Cos(angle) * 0.72f, -0.48f, Mathf.Sin(angle) * 0.72f));
            }

            var triangles = new List<int>();
            for (var i = 0; i < sides; i++)
            {
                var current = 2 + i;
                var next = 2 + ((i + 1) % sides);
                triangles.Add(0);
                triangles.Add(current);
                triangles.Add(next);
                triangles.Add(1);
                triangles.Add(next);
                triangles.Add(current);
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        static float HeightWorld(float x, float z)
        {
            return HeightNormalized(x, z) * TerrainHeight;
        }

        static float HeightNormalized(float x, float z)
        {
            var n = Mathf.PerlinNoise((x + 91f) * 0.042f, (z - 37f) * 0.042f) * 0.036f;
            n += Mathf.PerlinNoise((x - 14f) * 0.13f, (z + 75f) * 0.13f) * 0.010f;

            for (var i = 0; i < Ridges.Length; i++)
            {
                var d = DistanceToPath(x, z, Ridges[i].Points);
                if (d < Ridges[i].Width + 2.4f)
                    n += Smooth01(Ridges[i].Width + 2.4f, 0f, d) * Ridges[i].HeightNormalized;
            }

            var water = Mathf.Min(DistanceToPath(x, z, SouthernStream), DistanceToPath(x, z, NorthernStream));
            if (water < 3.2f)
                n -= Smooth01(3.2f, 0f, water) * 0.028f;

            return Mathf.Clamp(n, 0f, 0.94f);
        }

        static bool InsideRect(float x, float z, float minX, float minZ, float maxX, float maxZ)
        {
            return x >= minX && x <= maxX && z >= minZ && z <= maxZ;
        }

        static float DistanceToPath(float x, float z, Vector3[] points)
        {
            var best = float.MaxValue;
            var p = new Vector2(x, z);
            for (var i = 0; i < points.Length - 1; i++)
            {
                var a = new Vector2(points[i].x, points[i].z);
                var b = new Vector2(points[i + 1].x, points[i + 1].z);
                best = Mathf.Min(best, DistanceToSegment(p, a, b));
            }

            return best;
        }

        static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            var denominator = Mathf.Max(0.0001f, Vector2.Dot(ab, ab));
            var t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / denominator);
            return Vector2.Distance(p, a + ab * t);
        }

        static float Smooth01(float edge0, float edge1, float value)
        {
            var t = Mathf.Clamp01((value - edge0) / Mathf.Max(0.0001f, edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        static void Add(float[] weights, int index, float amount)
        {
            if (index >= 0 && index < weights.Length)
                weights[index] = Mathf.Max(0f, weights[index] + amount);
        }

        static void Normalize(float[] weights)
        {
            var sum = 0f;
            for (var i = 0; i < weights.Length; i++)
                sum += Mathf.Max(0f, weights[i]);
            if (sum <= 0.0001f)
            {
                weights[0] = 1f;
                return;
            }

            for (var i = 0; i < weights.Length; i++)
                weights[i] = Mathf.Max(0f, weights[i]) / sum;
        }

        static void ApplyTint(GameObject root, Color tint)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            var block = new MaterialPropertyBlock();
            for (var i = 0; i < renderers.Length; i++)
            {
                renderers[i].GetPropertyBlock(block);
                block.SetColor("_BaseColor", tint);
                block.SetColor("_Color", tint);
                renderers[i].SetPropertyBlock(block);
            }
        }

        static void RemoveColliders(GameObject root)
        {
            var colliders = root.GetComponentsInChildren<Collider>(true);
            for (var i = colliders.Length - 1; i >= 0; i--)
                Object.DestroyImmediate(colliders[i]);
        }

        static int StableHash(string value)
        {
            unchecked
            {
                var hash = 17;
                for (var i = 0; i < value.Length; i++)
                    hash = hash * 31 + value[i];
                return Mathf.Abs(hash);
            }
        }

        static void RenderCameraToPng(Camera camera, string outputPath, int width, int height)
        {
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(outputPath, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                renderTexture.Release();
                Object.DestroyImmediate(renderTexture);
            }
        }

        static void CaptureDetailScreenshots(Camera camera, string fullMapPath)
        {
            var originalPosition = camera.transform.position;
            var originalRotation = camera.transform.rotation;
            var originalSize = camera.orthographicSize;

            try
            {
                CaptureCameraAt(camera, "south_player_base", new Vector3(-12f, 0f, -38f), 20f, fullMapPath);
                CaptureCameraAt(camera, "central_cliffs_ore", new Vector3(-2f, 0f, 15f), 22f, fullMapPath);
                CaptureCameraAt(camera, "north_enemy_base", new Vector3(12f, 0f, 39f), 20f, fullMapPath);
            }
            finally
            {
                camera.transform.position = originalPosition;
                camera.transform.rotation = originalRotation;
                camera.orthographicSize = originalSize;
            }
        }

        static void CaptureCameraAt(Camera camera, string suffix, Vector3 target, float orthographicSize, string fullMapPath)
        {
            camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            camera.transform.position = new Vector3(target.x, target.y + 120f, target.z);
            camera.orthographicSize = orthographicSize;
            var directory = Path.GetDirectoryName(fullMapPath);
            var fileName = Path.GetFileNameWithoutExtension(fullMapPath) + "_" + suffix + ".png";
            RenderCameraToPng(camera, Path.Combine(directory, fileName), 1600, 1600);
        }

        static void EnsureFolderRecursive(string assetFolder)
        {
            var parts = assetFolder.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        enum SurfacePattern
        {
            Dirt,
            Rock,
            Concrete,
            MetalPanel,
            Foliage,
            Wood,
            Crystal,
            Water
        }

        sealed class MaterialBundle
        {
            public Material Terrain;
            public Material Road;
            public Material RoadShoulder;
            public Material RoadLine;
            public Material Water;
            public Material MudBank;
            public Material Rock;
            public Material CliffShadow;
            public Material OreBlue;
            public Material OreGold;
            public Material OreSoil;
            public Material TreeTrunk;
            public Material TreeFoliage;
            public Material Concrete;
            public Material PlayerBuilding;
            public Material EnemyBuilding;
            public Material NeutralBuilding;
            public Material PlayerWall;
            public Material EnemyWall;
            public Material MetalDark;
        }

        sealed class Ridge
        {
            public readonly Vector3[] Points;
            public readonly float Width;
            public readonly float HeightNormalized;

            public Ridge(Vector3[] points, float width, float heightNormalized)
            {
                Points = points;
                Width = width;
                HeightNormalized = heightNormalized;
            }
        }

        sealed class OreField
        {
            public readonly float X;
            public readonly float Z;
            public readonly float Radius;
            public readonly string Role;

            public OreField(float x, float z, float radius, string role)
            {
                X = x;
                Z = z;
                Radius = radius;
                Role = role;
            }
        }
    }
}
