#if UNITY_EDITOR
using System;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.MapEditor;
using ProjectAegisRTS.UnityClient.MapEditor.Visuals;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Scenario;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class AegisArtDirectedPlayableSceneCreator
    {
        public const string ScenePath = "Assets/Rts/Scenes/Stage16_ArtDirectedPlayableMap.unity";
        const string MapPath = "Assets/Rts/MapEditor/Samples/sample_art_directed_forest_river_2p.aegismap.json";

        [MenuItem("Project Aegis/Map Editor/Open Playable Art-Directed Map")]
        public static void OpenPlayableArtDirectedMapMenu()
        {
            CreateOrUpdatePlayableScene(true);
            EnterPlayModeWhenReady();
        }

        public static void OpenForEditorLaunch()
        {
            CreateOrUpdatePlayableScene(true);
            EnterPlayModeWhenReady();
        }

        public static void CreateForBatch()
        {
            try
            {
                CreateOrUpdatePlayableScene(false);
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

        public static void CreateOrUpdatePlayableScene(bool selectSceneObjects)
        {
            Stage16SceneCreator.CreateOrUpdateStage16Scene();

            var scene = EditorSceneManager.GetActiveScene();
            var game = GameObject.Find("RtsGame");
            if (game == null)
                throw new InvalidOperationException("RtsGame object is missing after creating Stage 16.");

            var driver = game.GetComponent<RtsSimulationDriver>();
            if (driver == null)
                driver = game.AddComponent<RtsSimulationDriver>();

            var mapAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(MapPath);
            if (mapAsset == null)
                throw new InvalidOperationException("Playable art-directed map asset is missing: " + MapPath);

            driver.ConfigureAegisMapDocumentWorld(mapAsset, MapPath, true);
            driver.UsePlayerPerspectiveSnapshot = true;

            var bootstrapper = game.GetComponent<RtsGameBootstrapper>();
            if (bootstrapper != null)
            {
                bootstrapper.boardWidth = 100;
                bootstrapper.boardHeight = 100;
                bootstrapper.boardCellSizeMeters = 1f;
                bootstrapper.startPaused = false;
            }

            var boardRenderer = UnityEngine.Object.FindFirstObjectByType<BoardRenderer>();
            if (boardRenderer != null)
            {
                boardRenderer.showStaticGridLines = false;
                EditorUtility.SetDirty(boardRenderer);
            }

            var scenario = game.GetComponent<VerticalSliceScenarioController>();
            if (scenario != null)
            {
                scenario.resetWorldOnInitialize = false;
                scenario.startOnInitialize = false;
                EditorUtility.SetDirty(scenario);
            }

            var playerInitializer = game.GetComponent<PlayerBuildSceneInitializer>();
            if (playerInitializer != null)
            {
                playerInitializer.startScenarioOnLoad = false;
                playerInitializer.frameCameraOnStart = true;
                playerInitializer.cameraPosition = new Vector3(50f, 92f, 7f);
                playerInitializer.cameraRotationEuler = new Vector3(60f, 0f, 0f);
                playerInitializer.cameraOrthographicSize = 55f;
                EditorUtility.SetDirty(playerInitializer);
            }

            var playInitializer = game.GetComponent<AegisMapDocumentPlayModeInitializer>();
            if (playInitializer == null)
                playInitializer = game.AddComponent<AegisMapDocumentPlayModeInitializer>();
            playInitializer.driver = driver;
            playInitializer.startMatchOnLoad = true;
            playInitializer.selectFirstCombatGroupOnLoad = true;
            playInitializer.revealScenarioMapOnLoad = true;

            BuildVisualScene();
            ConfigureCamera();

            EditorUtility.SetDirty(game);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (selectSceneObjects)
            {
                UnityEditor.Selection.activeObject = game;
                SceneView.RepaintAll();
            }

            Debug.Log("Created playable art-directed map scene at " + ScenePath + " using " + MapPath);
        }

        static void EnterPlayModeWhenReady()
        {
            EditorApplication.delayCall += () =>
            {
                if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode)
                    return;

                EditorApplication.isPlaying = true;
            };
        }

        static void BuildVisualScene()
        {
            var existing = GameObject.Find("Art Directed Playable Visual Root");
            if (existing != null)
                UnityEngine.Object.DestroyImmediate(existing);

            var document = AegisVisualMapDocument.Load(MapPath);
            var settings = AegisMapVisualCompileSettings.ProductionDefault();
            var theme = AegisBiomeVisualTheme.ForestPrototypeVisualTheme();
            var result = AegisMapVisualCompiler.CompileDocument(document, MapPath, false, theme, document.ReadSeed(), settings);
            if (result == null || result.Root == null)
                throw new InvalidOperationException("Visual compiler did not create a playable art-directed map root.");

            result.Root.name = "Art Directed Playable Visual Root";
            result.Root.transform.position = Vector3.zero;
        }

        static void ConfigureCamera()
        {
            var camera = Camera.main;
            if (camera == null)
                camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (camera == null)
                return;

            camera.orthographic = true;
            camera.orthographicSize = 55f;
            camera.transform.position = new Vector3(50f, 92f, 7f);
            camera.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;

            var controller = camera.GetComponent<ProjectAegisRTS.UnityClient.CameraControls.RtsCameraController>();
            if (controller != null)
            {
                controller.preserveConfiguredTransform = true;
                controller.orthographicSize = 55f;
                controller.maxHeight = 92f;
                EditorUtility.SetDirty(controller);
            }

            EditorUtility.SetDirty(camera);
        }
    }
}
#endif
