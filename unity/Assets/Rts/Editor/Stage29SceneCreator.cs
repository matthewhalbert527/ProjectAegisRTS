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
    public static class Stage29SceneCreator
    {
        public const string ScenePath = "Assets/Rts/Scenes/Stage29_BattlefieldVisualReview.unity";

        [MenuItem("ProjectAegisRTS/Stage 29/Create Battlefield Visual Review Scene")]
        public static void CreateStage29SceneMenu()
        {
            CreateOrUpdateStage29Scene();
        }

        public static void CreateStage29SceneBatch()
        {
            try
            {
                CreateOrUpdateStage29Scene();
                Debug.Log("Stage 29 battlefield visual review scene created.");
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

        public static void CreateOrUpdateStage29Scene()
        {
            Stage29BattlefieldVisualAssetCreator.EnsureStage29Assets();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Stage29_BattlefieldVisualReview";

            var camera = CreateCamera();
            var directional = CreateDirectionalLight();
            EnsureEventSystem();
            CreateCanvas();

            var game = new GameObject("RtsGame");
            var materialLibrary = game.AddComponent<BattlefieldMaterialLibrary>();
            AssignStage29Materials(materialLibrary);

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

            var hud = game.AddComponent<Stage29VisualQaHud>();
            hud.reviewController = controller;
            hud.terrainMaterialProfileLibrary = terrainLibrary;
            hud.visible = true;

            CreateTitle(game.transform);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsurePlayerBuildScenes(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void AssignStage29Materials(BattlefieldMaterialLibrary library)
        {
            if (library == null)
                return;

            library.grassDirt = LoadMaterial(Stage29BattlefieldVisualAssetCreator.TerrainMaterialFolder + "/stage29_terrain_grass_dirt.mat");
            library.compactedBaseGround = LoadMaterial(Stage29BattlefieldVisualAssetCreator.TerrainMaterialFolder + "/stage29_terrain_compacted_base.mat");
            library.concretePad = LoadMaterial(Stage29BattlefieldVisualAssetCreator.TerrainMaterialFolder + "/stage29_terrain_concrete_pad.mat");
            library.roadPath = LoadMaterial(Stage29BattlefieldVisualAssetCreator.TerrainMaterialFolder + "/stage29_terrain_road_path.mat");
            library.resourceField = LoadMaterial(Stage29BattlefieldVisualAssetCreator.TerrainMaterialFolder + "/stage29_terrain_resource_field.mat");
            library.rockBlocked = LoadMaterial(Stage29BattlefieldVisualAssetCreator.TerrainMaterialFolder + "/stage29_terrain_rock_blocked.mat");
            library.water = LoadMaterial(Stage29BattlefieldVisualAssetCreator.TerrainMaterialFolder + "/stage29_terrain_water.mat");
            library.fogExplored = LoadMaterial(Stage29BattlefieldVisualAssetCreator.TerrainMaterialFolder + "/stage29_terrain_fog_explored.mat");
            library.foundationEdge = LoadMaterial(Stage29BattlefieldVisualAssetCreator.EnvironmentMaterialFolder + "/stage29_foundation_edge.mat");
            library.wornMetal = LoadMaterial(Stage29BattlefieldVisualAssetCreator.EnvironmentMaterialFolder + "/stage29_worn_metal.mat");
            library.warmLight = LoadMaterial(Stage29BattlefieldVisualAssetCreator.EnvironmentMaterialFolder + "/stage29_warm_light.mat");
            library.shadowFog = LoadMaterial(Stage29BattlefieldVisualAssetCreator.EnvironmentMaterialFolder + "/stage29_dark_metal.mat");
            library.fineGridGuide = LoadMaterial(Stage29BattlefieldVisualAssetCreator.EnvironmentMaterialFolder + "/stage29_fine_grid_guide.mat");
        }

        static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8.9f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.06f, 0.08f, 0.08f, 1f);
            cameraObject.transform.position = new Vector3(0f, 14.5f, -10.25f);
            cameraObject.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        static Light CreateDirectionalLight()
        {
            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.65f;
            light.color = new Color(1f, 0.94f, 0.82f, 1f);
            lightObject.transform.rotation = Quaternion.Euler(52f, -38f, 0f);
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
            var title = new GameObject("Stage29 Review Title");
            title.transform.SetParent(parent, false);
            title.transform.position = new Vector3(0f, 0.10f, -6.05f);
            var text = title.AddComponent<TextMesh>();
            text.text = "Stage 29 Realistic Battlefield Visual Review";
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

        static Material LoadMaterial(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Material>(path);
        }

        static void EnsurePlayerBuildScenes(string stage29Path)
        {
            var ordered = new List<EditorBuildSettingsScene>();
            AddBuildSceneIfExists(ordered, Stage16_5BuildFlowConfigurator.BootScenePath);
            AddBuildSceneIfExists(ordered, Stage16SceneCreator.ScenePath);

            var existingScenes = EditorBuildSettings.scenes;
            for (var i = 0; i < existingScenes.Length; i++)
                AddBuildSceneIfExists(ordered, existingScenes[i].path);

            AddBuildSceneIfExists(ordered, Stage20SceneCreator.ScenePath);
            AddBuildSceneIfExists(ordered, Stage21SceneCreator.ScenePath);
            AddBuildSceneIfExists(ordered, stage29Path);
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
