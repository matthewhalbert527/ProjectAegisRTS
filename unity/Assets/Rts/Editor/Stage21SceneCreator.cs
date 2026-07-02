using System;
using System.Collections.Generic;
using System.IO;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Art.Production;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage21SceneCreator
    {
        public const string ScenePath = "Assets/Rts/Scenes/Stage21_MvpVisualQaReview.unity";

        [MenuItem("ProjectAegisRTS/Stage 21/Create MVP Visual QA Review Scene")]
        public static void CreateStage21SceneMenu()
        {
            CreateOrUpdateStage21Scene();
        }

        public static void CreateStage21SceneBatch()
        {
            try
            {
                CreateOrUpdateStage21Scene();
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

        public static void CreateOrUpdateStage21Scene()
        {
            Stage21MvpVisualQaValidator.ValidateMvpVisualQa();
            var manifest = AssetDatabase.LoadAssetAtPath<ArtistModelImportManifest>(Stage21ArtistModelImportScanner.ManifestAssetPath);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Stage21_MvpVisualQaReview";

            CreateCamera("Main Camera", new Vector3(7.1f, 12.5f, -8.6f), Quaternion.Euler(60f, 0f, 0f), 9.2f, true);
            CreateCamera("Side Inspection Camera", new Vector3(7.1f, 3.1f, -10.8f), Quaternion.Euler(16f, 0f, 0f), 6.4f, false);
            CreateLight();
            EnsureEventSystem();
            CreateCanvas();

            var game = new GameObject("RtsGame");
            var definitions = Stage8SceneCreator.LoadDefinitions();
            var visualLibrary = game.AddComponent<ActorVisualDefinitionLibrary>();
            visualLibrary.definitions = definitions;
            visualLibrary.RebuildLookup();

            var standardLibrary = game.AddComponent<ProductionVisualStandardLibrary>();
            standardLibrary.EnsureDefaults();
            standardLibrary.RebuildLookup();

            var resolver = game.AddComponent<ActorVisualPrefabResolver>();
            resolver.definitionLibrary = visualLibrary;

            var runner = game.AddComponent<MvpVisualQaRunner>();
            runner.definitionLibrary = visualLibrary;
            runner.standardLibrary = standardLibrary;
            runner.artistModelImportManifest = manifest;

            var showcaseObject = new GameObject("Stage21 MVP Visual QA Showcase");
            showcaseObject.transform.SetParent(game.transform, false);
            var showcase = showcaseObject.AddComponent<Stage21MvpVisualQaShowcaseController>();
            showcase.definitionLibrary = visualLibrary;
            showcase.standardLibrary = standardLibrary;
            showcase.artistModelImportManifest = manifest;
            showcase.qaRunner = runner;
            showcase.showLabels = true;
            showcase.showSocketMarkers = true;
            showcase.EnsureShowcase();

            var hud = game.AddComponent<MvpVisualQaHud>();
            hud.showcaseController = showcase;
            hud.visible = true;

            var title = new GameObject("Stage21 QA Title");
            title.transform.SetParent(game.transform, false);
            title.transform.position = new Vector3(6.7f, 0.06f, -2.35f);
            var titleMesh = title.AddComponent<TextMesh>();
            titleMesh.text = "Stage 21 MVP Visual QA / Artist Replacement Readiness";
            titleMesh.fontSize = 38;
            titleMesh.characterSize = 0.072f;
            titleMesh.anchor = TextAnchor.MiddleCenter;
            titleMesh.alignment = TextAlignment.Center;

            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsurePlayerBuildScenes(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("Created Stage 21 scene at " + ScenePath);
        }

        static void CreateCamera(string name, Vector3 position, Quaternion rotation, float orthographicSize, bool main)
        {
            var cameraObject = new GameObject(name);
            if (main)
                cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = orthographicSize;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.09f, 0.11f, 0.12f, 1f);
            cameraObject.transform.position = position;
            cameraObject.transform.rotation = rotation;
            if (main)
                cameraObject.AddComponent<AudioListener>();
            else
                camera.enabled = false;
        }

        static void CreateLight()
        {
            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.8f;
            lightObject.transform.rotation = Quaternion.Euler(54f, -35f, 0f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.72f, 0.74f, 0.72f, 1f);
        }

        static void EnsureEventSystem()
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        static Canvas CreateCanvas()
        {
            var canvasObject = new GameObject("Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        static void EnsurePlayerBuildScenes(string stage21Path)
        {
            var ordered = new List<EditorBuildSettingsScene>();
            AddBuildSceneIfExists(ordered, Stage16_5BuildFlowConfigurator.BootScenePath);
            AddBuildSceneIfExists(ordered, Stage16SceneCreator.ScenePath);

            var existingScenes = EditorBuildSettings.scenes;
            for (var i = 0; i < existingScenes.Length; i++)
                AddBuildSceneIfExists(ordered, existingScenes[i].path);

            AddBuildSceneIfExists(ordered, Stage20SceneCreator.ScenePath);
            AddBuildSceneIfExists(ordered, stage21Path);
            EditorBuildSettings.scenes = ordered.ToArray();
        }

        static void AddBuildSceneIfExists(List<EditorBuildSettingsScene> scenes, string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;
            for (var i = 0; i < scenes.Count; i++)
                if (scenes[i].path == path)
                    return;
            scenes.Add(new EditorBuildSettingsScene(path, true));
        }
    }
}
