using System;
using System.Collections.Generic;
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
    public static class Stage20SceneCreator
    {
        public const string ScenePath = "Assets/Rts/Scenes/Stage20_MvpProductionVisuals.unity";

        [MenuItem("ProjectAegisRTS/Stage 20/Create MVP Production Visual Showcase Scene")]
        public static void CreateStage20SceneMenu()
        {
            CreateOrUpdateStage20Scene();
        }

        public static void CreateStage20SceneBatch()
        {
            try
            {
                CreateOrUpdateStage20Scene();
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

        public static void CreateOrUpdateStage20Scene()
        {
            Stage20MvpProductionProxyGenerator.GenerateMvpProductionProxies();
            Stage20ProductionVisualValidator.ValidateStage20ProductionVisuals();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Stage20_MvpProductionVisuals";

            CreateCamera();
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

            var showcaseObject = new GameObject("Stage20 MVP Production Visual Showcase");
            showcaseObject.transform.SetParent(game.transform, false);
            var showcase = showcaseObject.AddComponent<Stage20MvpVisualShowcaseController>();
            showcase.definitionLibrary = visualLibrary;
            showcase.standardLibrary = standardLibrary;
            showcase.showLabels = true;
            showcase.showSocketLabels = false;
            showcase.EnsureShowcase();

            var hud = game.AddComponent<ProductionVisualDebugHud>();
            hud.definitionLibrary = visualLibrary;
            hud.standardLibrary = standardLibrary;
            hud.showcaseController = showcase;
            hud.visible = false;

            var title = new GameObject("Stage20 Showcase Title");
            title.transform.SetParent(game.transform, false);
            title.transform.position = new Vector3(6.5f, 0.05f, -2.2f);
            var titleMesh = title.AddComponent<TextMesh>();
            titleMesh.text = "Stage 20 MVP Production Visual Proxies";
            titleMesh.fontSize = 40;
            titleMesh.characterSize = 0.08f;
            titleMesh.anchor = TextAnchor.MiddleCenter;
            titleMesh.alignment = TextAlignment.Center;

            EditorSceneManager.SaveScene(scene, ScenePath);
            AppendBuildScene(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("Created Stage 20 scene at " + ScenePath);
        }

        static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8.5f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.10f, 0.12f, 0.12f, 1f);
            cameraObject.transform.position = new Vector3(6.8f, 11f, -8.2f);
            cameraObject.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
            cameraObject.AddComponent<AudioListener>();
        }

        static void CreateLight()
        {
            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.75f;
            lightObject.transform.rotation = Quaternion.Euler(54f, -35f, 0f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.70f, 0.73f, 0.70f, 1f);
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

        static void AppendBuildScene(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            for (var i = 0; i < scenes.Count; i++)
                if (scenes[i].path == path)
                    return;
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
