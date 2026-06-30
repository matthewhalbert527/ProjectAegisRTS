using System;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Economy;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage10SceneValidator
    {
        public static void ValidateStage10SceneBatch()
        {
            try
            {
                ValidateStage10Scene();
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

        public static void ValidateStage10Scene()
        {
            if (!System.IO.File.Exists(Stage10SceneCreator.ScenePath))
                throw new InvalidOperationException("Stage 10 scene missing: " + Stage10SceneCreator.ScenePath);

            var scene = EditorSceneManager.OpenScene(Stage10SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 10 scene did not open.");

            RequireObject("RtsGame");
            RequireObject("BoardRoot");
            if (Camera.main == null && UnityEngine.Object.FindFirstObjectByType<Camera>() == null)
                throw new InvalidOperationException("Stage 10 camera missing.");
            if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                throw new InvalidOperationException("Stage 10 EventSystem missing.");
            if (UnityEngine.Object.FindFirstObjectByType<Canvas>() == null)
                throw new InvalidOperationException("Stage 10 Canvas missing.");

            Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            Require<BoardRenderer>("BoardRenderer");
            Require<ActorRenderSystem>("ActorRenderSystem");
            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            Require<ResourceFieldRenderSystem>("ResourceFieldRenderSystem");
            Require<HarvesterCargoVisualController>("HarvesterCargoVisualController");
            Require<RefineryDockVisualController>("RefineryDockVisualController");
            Require<EconomyEventRenderSystem>("EconomyEventRenderSystem");
            Require<EconomyDebugHud>("EconomyDebugHud");
            if (!driver.UseEconomyDemoWorld)
                throw new InvalidOperationException("Stage 10 scene must use the economy demo world.");

            var previousScenes = new[]
            {
                "Assets/Rts/Scenes/Stage1_DesktopBoard.unity",
                "Assets/Rts/Scenes/Stage2_PCSidebar.unity",
                "Assets/Rts/Scenes/Stage3_XRBoardPlacement.unity",
                "Assets/Rts/Scenes/Stage4_LeftHandBuildSelection.unity",
                "Assets/Rts/Scenes/Stage5_DualHandCommand.unity",
                "Assets/Rts/Scenes/Stage6_MovementVisualization.unity",
                Stage7SceneCreator.ScenePath,
                Stage8SceneCreator.ScenePath,
                Stage9SceneCreator.ScenePath
            };

            for (var i = 0; i < previousScenes.Length; i++)
                if (!System.IO.File.Exists(previousScenes[i]))
                    throw new InvalidOperationException("Previous stage scene missing: " + previousScenes[i]);

            Debug.Log("Stage 10 scene validation passed.");
        }

        static void RequireObject(string name)
        {
            if (GameObject.Find(name) == null)
                throw new InvalidOperationException("Missing GameObject: " + name);
        }

        static T Require<T>(string label) where T : Component
        {
            var component = UnityEngine.Object.FindFirstObjectByType<T>();
            if (component == null)
                throw new InvalidOperationException("Missing component: " + label);
            return component;
        }
    }
}
