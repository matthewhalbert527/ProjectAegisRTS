using System;
using System.Collections.Generic;
using System.IO;
using ProjectAegisRTS.UnityClient.Rendering.TerrainPieces;
using ProjectAegisRTS.UnityClient.Rendering.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage32SceneCreator
    {
        public const string ScenePath = "Assets/Rts/Scenes/Stage32_TerrainSetDressingReview.unity";

        [MenuItem("ProjectAegisRTS/Stage 32/Create Terrain Set Dressing Review Scene")]
        public static void CreateStage32SceneMenu()
        {
            CreateOrUpdateStage32Scene();
        }

        public static void CreateStage32SceneBatch()
        {
            try
            {
                CreateOrUpdateStage32Scene();
                Debug.Log("Stage 32 terrain set dressing review scene created.");
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

        public static void CreateOrUpdateStage32Scene()
        {
            Stage32TerrainPieceGenerator.EnsureStage32TerrainPieces();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Stage32_TerrainSetDressingReview";

            var camera = CreateCamera();
            var light = CreateDirectionalLight();
            EnsureEventSystem();
            CreateCanvas();

            var game = new GameObject("RtsGame");
            var materialLibrary = game.AddComponent<BattlefieldMaterialLibrary>();
            Stage29SceneCreator.AssignStage29Materials(materialLibrary);

            var lightingProfile = Stage29BattlefieldVisualAssetCreator.LoadLightingProfile();
            var lightingApplier = game.AddComponent<LightingProfileApplier>();
            lightingApplier.profile = lightingProfile;
            lightingApplier.directionalLight = light;
            lightingApplier.targetCamera = camera;
            lightingApplier.ApplyProfile();

            var atmosphere = game.AddComponent<BattlefieldAtmosphereController>();
            atmosphere.lightingProfile = lightingProfile;
            atmosphere.lightingProfileApplier = lightingApplier;
            atmosphere.materialLibrary = materialLibrary;
            atmosphere.ApplyAtmosphere();

            var controller = game.AddComponent<Stage32TerrainSetDressingReviewController>();
            controller.pieceLibrary = Stage32TerrainPieceGenerator.LoadTerrainPieceLibrary();
            controller.materialLibrary = Stage32TerrainPieceGenerator.LoadMaterialLibrary();
            controller.setDressingProfile = Stage32TerrainPieceGenerator.LoadPlayerFacingSetDressingProfile();
            controller.showLabels = true;
            controller.EnsureReviewScene();

            var hud = game.AddComponent<Stage32TerrainPieceQaHud>();
            hud.reviewController = controller;
            hud.pieceLibrary = controller.pieceLibrary;
            hud.visible = true;

            CreateTitle(game.transform);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsurePlayerBuildScenes(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 12.9f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.045f, 0.060f, 0.058f, 1f);
            cameraObject.transform.position = new Vector3(0f, 19.8f, -15.6f);
            cameraObject.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        static Light CreateDirectionalLight()
        {
            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.60f;
            light.color = new Color(1f, 0.94f, 0.84f, 1f);
            lightObject.transform.rotation = Quaternion.Euler(54f, -36f, 0f);
            return light;
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

        static void CreateTitle(Transform parent)
        {
            var title = new GameObject("Stage32 Review Title");
            title.transform.SetParent(parent, false);
            title.transform.position = new Vector3(0f, 0.10f, 8.05f);
            var text = title.AddComponent<TextMesh>();
            text.text = "Stage 32 Terrain Piece Library / Battlefield Set Dressing";
            text.fontSize = 42;
            text.characterSize = 0.072f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
        }

        static void EnsurePlayerBuildScenes(string stage32Path)
        {
            var ordered = new List<EditorBuildSettingsScene>();
            AddBuildSceneIfExists(ordered, Stage16_5BuildFlowConfigurator.BootScenePath);
            AddBuildSceneIfExists(ordered, Stage16SceneCreator.ScenePath);

            var existingScenes = EditorBuildSettings.scenes;
            for (var i = 0; i < existingScenes.Length; i++)
                AddBuildSceneIfExists(ordered, existingScenes[i].path);

            AddBuildSceneIfExists(ordered, Stage20SceneCreator.ScenePath);
            AddBuildSceneIfExists(ordered, Stage21SceneCreator.ScenePath);
            AddBuildSceneIfExists(ordered, Stage29SceneCreator.ScenePath);
            AddBuildSceneIfExists(ordered, Stage30SceneCreator.ScenePath);
            AddBuildSceneIfExists(ordered, stage32Path);
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
