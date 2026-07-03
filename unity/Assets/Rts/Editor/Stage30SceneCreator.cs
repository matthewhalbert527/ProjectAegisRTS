using System;
using System.Collections.Generic;
using System.IO;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Art.Production;
using ProjectAegisRTS.UnityClient.Rendering.Visuals;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage30SceneCreator
    {
        public const string ScenePath = "Assets/Rts/Scenes/Stage30_VisualReadabilityQa.unity";

        [MenuItem("ProjectAegisRTS/Stage 30/Create Visual Readability QA Scene")]
        public static void CreateStage30SceneMenu()
        {
            CreateOrUpdateStage30Scene();
        }

        public static void CreateStage30SceneBatch()
        {
            try
            {
                CreateOrUpdateStage30Scene();
                Debug.Log("Stage 30 visual readability QA scene created.");
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

        public static void CreateOrUpdateStage30Scene()
        {
            Stage30VisualReadabilityAssetCreator.EnsureStage30Assets();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Stage30_VisualReadabilityQa";

            var camera = CreateCamera();
            var directional = CreateDirectionalLight();
            EnsureEventSystem();
            CreateCanvas();

            var game = new GameObject("RtsGame");
            var materialLibrary = game.AddComponent<BattlefieldMaterialLibrary>();
            Stage29SceneCreator.AssignStage29Materials(materialLibrary);

            var terrainLibrary = game.AddComponent<TerrainMaterialProfileLibrary>();
            terrainLibrary.materialLibrary = materialLibrary;
            terrainLibrary.profiles = LoadTerrainProfiles();
            terrainLibrary.RebuildLookup();

            var lightingProfile = Stage29BattlefieldVisualAssetCreator.LoadLightingProfile();
            var lightingApplier = game.AddComponent<LightingProfileApplier>();
            lightingApplier.profile = lightingProfile;
            lightingApplier.directionalLight = directional;
            lightingApplier.targetCamera = camera;
            lightingApplier.ApplyProfile();

            var atmosphere = game.AddComponent<BattlefieldAtmosphereController>();
            atmosphere.lightingProfile = lightingProfile;
            atmosphere.lightingProfileApplier = lightingApplier;
            atmosphere.materialLibrary = materialLibrary;
            atmosphere.ApplyAtmosphere();

            var visualLibrary = game.AddComponent<ActorVisualDefinitionLibrary>();
            visualLibrary.definitions = Stage8SceneCreator.LoadDefinitions();
            visualLibrary.RebuildLookup();

            var standardLibrary = game.AddComponent<ProductionVisualStandardLibrary>();
            standardLibrary.EnsureDefaults();
            standardLibrary.RebuildLookup();

            var resolver = game.AddComponent<ActorVisualPrefabResolver>();
            resolver.definitionLibrary = visualLibrary;

            var controller = game.AddComponent<Stage29BattlefieldVisualReviewController>();
            controller.materialLibrary = materialLibrary;
            controller.terrainMaterialProfileLibrary = terrainLibrary;
            controller.actorVisualDefinitionLibrary = visualLibrary;
            controller.productionVisualStandardLibrary = standardLibrary;
            controller.showLabels = true;
            controller.EnsureReviewScene();

            var stage29Hud = game.AddComponent<Stage29VisualQaHud>();
            stage29Hud.reviewController = controller;
            stage29Hud.terrainMaterialProfileLibrary = terrainLibrary;
            stage29Hud.visible = false;

            var stage30Hud = game.AddComponent<Stage30VisualReadabilityHud>();
            stage30Hud.reviewController = controller;
            stage30Hud.readabilityProfile = Stage30VisualReadabilityAssetCreator.LoadReadabilityProfile();
            stage30Hud.visible = true;

            CreateTitle(game.transform);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8.6f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.045f, 0.060f, 0.060f, 1f);
            cameraObject.transform.position = new Vector3(0f, 14.2f, -9.85f);
            cameraObject.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        static Light CreateDirectionalLight()
        {
            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.50f;
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
            var title = new GameObject("Stage30 Review Title");
            title.transform.SetParent(parent, false);
            title.transform.position = new Vector3(0f, 0.10f, -6.05f);
            var text = title.AddComponent<TextMesh>();
            text.text = "Stage 30 Visual Readability QA";
            text.fontSize = 42;
            text.characterSize = 0.07f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
        }

        static List<TerrainMaterialProfile> LoadTerrainProfiles()
        {
            var profiles = new List<TerrainMaterialProfile>();
            for (var i = 0; i < Stage29BattlefieldVisualAssetCreator.RequiredTerrainKinds.Length; i++)
            {
                var profile = AssetDatabase.LoadAssetAtPath<TerrainMaterialProfile>(Stage29BattlefieldVisualAssetCreator.TerrainProfilePath(Stage29BattlefieldVisualAssetCreator.RequiredTerrainKinds[i]));
                if (profile != null)
                    profiles.Add(profile);
            }
            return profiles;
        }
    }
}
