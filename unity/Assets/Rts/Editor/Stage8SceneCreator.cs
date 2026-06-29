using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Buildings;
using ProjectAegisRTS.UnityClient.Rendering.Motion;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage8SceneCreator
    {
        public const string ScenePath = "Assets/Rts/Scenes/Stage8_ArtPipelineShowcase.unity";

        [MenuItem("ProjectAegisRTS/Stage 8/Create Art Pipeline Showcase Scene")]
        public static void CreateStage8SceneMenu()
        {
            CreateOrUpdateStage8Scene();
        }

        public static void CreateStage8SceneBatch()
        {
            try
            {
                CreateOrUpdateStage8Scene();
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

        public static void CreateOrUpdateStage8Scene()
        {
            Stage8ActorCatalog.EnsureStage8Folders();
            Stage8ConceptArtImporter.ImportConceptArt();
            Stage8BlockoutPrefabGenerator.GenerateBlockouts();
            var definitions = Stage8ActorVisualDefinitionGenerator.CreateOrUpdateDefinitions();
            Stage8IconGenerator.GenerateIcons();
            definitions = Stage8ActorVisualDefinitionGenerator.CreateOrUpdateDefinitions();

            if (System.IO.File.Exists(Stage7SceneCreator.ScenePath))
                EditorSceneManager.OpenScene(Stage7SceneCreator.ScenePath);
            else
                Stage7SceneCreator.CreateOrUpdateStage7Scene();

            var scene = EditorSceneManager.GetActiveScene();
            var game = RequireObject("RtsGame");
            RequireObject("BoardRoot");
            var camera = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            ConfigureCamera(camera);

            var actorRenderer = UnityEngine.Object.FindFirstObjectByType<ActorRenderSystem>();
            var bootstrapper = UnityEngine.Object.FindFirstObjectByType<RtsGameBootstrapper>();
            var motionLibrary = UnityEngine.Object.FindFirstObjectByType<VisualMotionProfileLibrary>();
            var buildingLibrary = UnityEngine.Object.FindFirstObjectByType<BuildingVisualProfileLibrary>();
            var statusLog = UnityEngine.Object.FindFirstObjectByType<RtsStatusLog>();

            var visualLibrary = GetOrAdd<ActorVisualDefinitionLibrary>(game);
            visualLibrary.definitions = definitions;
            visualLibrary.RebuildLookup();

            var conceptLibrary = GetOrAdd<ConceptArtReferenceLibrary>(game);
            conceptLibrary.references = LoadConceptReferences();
            conceptLibrary.RebuildLookup();

            var resolver = GetOrAdd<ActorVisualPrefabResolver>(game);
            resolver.definitionLibrary = visualLibrary;

            if (actorRenderer != null)
            {
                actorRenderer.motionProfileLibrary = motionLibrary;
                actorRenderer.buildingProfileLibrary = buildingLibrary;
                actorRenderer.actorVisualDefinitionLibrary = visualLibrary;
                actorRenderer.actorVisualPrefabResolver = resolver;
            }

            var hud = GetOrAdd<ArtPipelineDebugHud>(game);
            hud.actorRenderSystem = actorRenderer;
            hud.definitionLibrary = visualLibrary;
            hud.conceptLibrary = conceptLibrary;
            hud.prefabResolver = resolver;
            hud.visible = true;

            var showcaseObject = GameObject.Find("Stage8 Art Pipeline Showcase");
            if (showcaseObject == null)
                showcaseObject = new GameObject("Stage8 Art Pipeline Showcase");
            showcaseObject.transform.SetParent(game.transform, false);
            var showcase = GetOrAdd<ArtPipelineShowcaseController>(showcaseObject);
            showcase.definitionLibrary = visualLibrary;
            showcase.conceptLibrary = conceptLibrary;
            showcase.showConceptCards = true;
            showcase.showSocketLabels = false;
            showcase.EnsureShowcase();
            hud.showcaseController = showcase;

            if (bootstrapper != null)
                bootstrapper.actorRenderSystem = actorRenderer;
            if (statusLog == null)
                game.AddComponent<RtsStatusLog>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            UpdateBuildScenes();
            Stage8PrefabSocketValidator.ValidatePrefabs();
            AssetDatabase.SaveAssets();
            Debug.Log("Created Stage 8 scene at " + ScenePath);
        }

        public static List<ActorVisualDefinition> LoadDefinitions()
        {
            var specs = Stage8ActorCatalog.LoadSpecs();
            var definitions = new List<ActorVisualDefinition>();
            for (var i = 0; i < specs.Count; i++)
            {
                var definition = AssetDatabase.LoadAssetAtPath<ActorVisualDefinition>(Stage8ActorCatalog.DefinitionAssetPath(specs[i]));
                if (definition != null)
                    definitions.Add(definition);
            }
            return definitions;
        }

        public static List<ConceptArtReference> LoadConceptReferences()
        {
            var specs = Stage8ActorCatalog.LoadSpecs();
            var references = new List<ConceptArtReference>();
            for (var i = 0; i < specs.Count; i++)
            {
                var reference = AssetDatabase.LoadAssetAtPath<ConceptArtReference>(Stage8ActorCatalog.ConceptReferenceAssetPath(specs[i]));
                if (reference != null)
                    references.Add(reference);
            }
            return references;
        }

        static void ConfigureCamera(Camera camera)
        {
            if (camera == null)
                return;

            camera.orthographic = true;
            camera.orthographicSize = 28f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.transform.position = new Vector3(16f, 38f, -26f);
            camera.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
        }

        static void UpdateBuildScenes()
        {
            var paths = new[]
            {
                "Assets/Rts/Scenes/Stage1_DesktopBoard.unity",
                "Assets/Rts/Scenes/Stage2_PCSidebar.unity",
                "Assets/Rts/Scenes/Stage3_XRBoardPlacement.unity",
                "Assets/Rts/Scenes/Stage4_LeftHandBuildSelection.unity",
                "Assets/Rts/Scenes/Stage5_DualHandCommand.unity",
                "Assets/Rts/Scenes/Stage6_MovementVisualization.unity",
                Stage7SceneCreator.ScenePath,
                ScenePath
            };

            var scenes = new List<EditorBuildSettingsScene>();
            for (var i = 0; i < paths.Length; i++)
                if (System.IO.File.Exists(paths[i]))
                    scenes.Add(new EditorBuildSettingsScene(paths[i], true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static GameObject RequireObject(string name)
        {
            var obj = GameObject.Find(name);
            if (obj == null)
                throw new InvalidOperationException("Missing GameObject: " + name);
            return obj;
        }

        static T GetOrAdd<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }
    }
}
