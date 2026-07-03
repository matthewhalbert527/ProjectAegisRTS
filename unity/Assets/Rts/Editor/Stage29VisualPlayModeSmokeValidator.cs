using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Art.Production;
using ProjectAegisRTS.UnityClient.Rendering.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage29VisualPlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage29VisualPlayModeSmokeBatch()
        {
            try
            {
                RunStage29VisualPlayModeSmoke();
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

        public static void RunStage29VisualPlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                if (!System.IO.File.Exists(System.IO.Path.Combine(Stage8ActorCatalog.RepoRoot, "unity", Stage29SceneCreator.ScenePath)))
                    Stage29SceneCreator.CreateOrUpdateStage29Scene();

                var scene = EditorSceneManager.OpenScene(Stage29SceneCreator.ScenePath);
                if (!scene.IsValid())
                    throw new InvalidOperationException("Stage 29 visual smoke scene did not open.");

                var controller = Require<Stage29BattlefieldVisualReviewController>("Stage29BattlefieldVisualReviewController");
                var materialLibrary = Require<BattlefieldMaterialLibrary>("BattlefieldMaterialLibrary");
                var terrainLibrary = Require<TerrainMaterialProfileLibrary>("TerrainMaterialProfileLibrary");
                var lighting = Require<LightingProfileApplier>("LightingProfileApplier");
                var atmosphere = Require<BattlefieldAtmosphereController>("BattlefieldAtmosphereController");
                var hud = Require<Stage29VisualQaHud>("Stage29VisualQaHud");

                materialLibrary.EnsureRuntimeDefaults();
                terrainLibrary.RebuildLookup();
                lighting.ApplyProfile();
                atmosphere.ApplyAtmosphere();
                controller.EnsureReviewScene();
                hud.Refresh();

                if (terrainLibrary.ProfileCount < Stage29BattlefieldVisualAssetCreator.RequiredTerrainKinds.Length)
                    throw new InvalidOperationException("Stage 29 visual smoke expected all terrain profiles.");
                if (controller.TerrainTileCount < 160 || controller.FineGridLineCount < 40)
                    throw new InvalidOperationException("Stage 29 visual smoke expected terrain and fine-grid guidance.");
                if (controller.ActorProxyCount != Stage20MvpVisualActorSet.ActorTypeIds.Length)
                    throw new InvalidOperationException("Stage 29 visual smoke expected all MVP proxies.");
                if (controller.MaterialSwatchCount < Stage29BattlefieldVisualAssetCreator.RequiredTerrainKinds.Length)
                    throw new InvalidOperationException("Stage 29 visual smoke expected all material swatches.");
                if (!hud.visible)
                    throw new InvalidOperationException("Stage 29 visual review HUD should be visible in the review scene.");
                if (Camera.main == null)
                    throw new InvalidOperationException("Stage 29 visual smoke expected a Main Camera.");

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 29 visual smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 29 visual play-mode smoke validation passed.");
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
