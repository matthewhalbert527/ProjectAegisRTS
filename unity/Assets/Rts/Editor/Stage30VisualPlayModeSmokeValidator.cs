using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Art.Production;
using ProjectAegisRTS.UnityClient.Rendering.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage30VisualPlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage30VisualPlayModeSmokeBatch()
        {
            try
            {
                RunStage30VisualPlayModeSmoke();
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

        public static void RunStage30VisualPlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                if (!System.IO.File.Exists(System.IO.Path.Combine(Stage8ActorCatalog.RepoRoot, "unity", Stage30SceneCreator.ScenePath)))
                    Stage30SceneCreator.CreateOrUpdateStage30Scene();

                var scene = EditorSceneManager.OpenScene(Stage30SceneCreator.ScenePath);
                if (!scene.IsValid())
                    throw new InvalidOperationException("Stage 30 readability smoke scene did not open.");

                var controller = Require<Stage29BattlefieldVisualReviewController>("Stage29BattlefieldVisualReviewController");
                var hud = Require<Stage30VisualReadabilityHud>("Stage30VisualReadabilityHud");
                var terrainLibrary = Require<TerrainMaterialProfileLibrary>("TerrainMaterialProfileLibrary");
                var lighting = Require<LightingProfileApplier>("LightingProfileApplier");
                var atmosphere = Require<BattlefieldAtmosphereController>("BattlefieldAtmosphereController");

                terrainLibrary.RebuildLookup();
                lighting.ApplyProfile();
                atmosphere.ApplyAtmosphere();
                controller.EnsureReviewScene();
                hud.Refresh();

                if (controller.ActorProxyCount != Stage20MvpVisualActorSet.ActorTypeIds.Length)
                    throw new InvalidOperationException("Stage 30 readability smoke expected all MVP proxies.");
                if (controller.TerrainTileCount < 160 || controller.FineGridLineCount < 40)
                    throw new InvalidOperationException("Stage 30 readability smoke expected terrain and fine-grid guidance.");
                if (!hud.visible)
                    throw new InvalidOperationException("Stage 30 readability HUD should be visible in the review scene.");
                if (Camera.main == null)
                    throw new InvalidOperationException("Stage 30 readability smoke expected a Main Camera.");

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 30 readability smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 30 visual readability play-mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static T Require<T>(string label) where T : Component
        {
            var active = UnityEngine.Object.FindFirstObjectByType<T>();
            if (active != null)
                return active;

            var all = Resources.FindObjectsOfTypeAll<T>();
            for (var i = 0; i < all.Length; i++)
                if (all[i] != null && all[i].gameObject != null && all[i].gameObject.scene.IsValid())
                    return all[i];

            throw new InvalidOperationException("Missing component: " + label);
        }

        static void CaptureRedError(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                RedErrors.Add(condition);
        }
    }
}
