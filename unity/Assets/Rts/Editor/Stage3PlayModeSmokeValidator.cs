using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.UnityClient.Board;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage3PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage3PlayModeSmokeBatch()
        {
            try
            {
                RunStage3PlayModeSmoke();
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

        public static void RunStage3PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage3SceneValidator.ValidateStage3Scene();
                var scene = EditorSceneManager.OpenScene(Stage3SceneCreator.ScenePath);
                if (!scene.IsValid())
                    throw new InvalidOperationException("Stage 3 scene did not open.");

                var boardRoot = GameObject.Find("BoardRoot");
                if (boardRoot == null)
                    throw new InvalidOperationException("BoardRoot missing in Stage 3 scene.");

                var bootstrapper = RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
                var driver = RequireEnabled<RtsSimulationDriver>("RtsSimulationDriver");
                var boardRenderer = RequireEnabled<BoardRenderer>("BoardRenderer");
                var actorRenderer = RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
                var mapper = RequireEnabled<BoardCoordinateMapper>("BoardCoordinateMapper");
                var placement = RequireEnabled<BoardPlacementController>("BoardPlacementController");

                bootstrapper.InitializeScene();
                placement.Initialize();
                StepRuntime(driver, boardRenderer, actorRenderer, 8, 0.1f);

                if (boardRoot.transform.Find("Board Surface") == null)
                    throw new InvalidOperationException("Stage 3 board visuals were not generated.");

                var actorViews = GameObject.Find("Actor Views");
                if (actorViews == null || actorViews.transform.childCount == 0)
                    throw new InvalidOperationException("Stage 3 actor visuals were not generated.");

                if (driver.LatestSnapshot == null || driver.LatestSnapshot.Actors.Count == 0)
                    throw new InvalidOperationException("Stage 3 simulation snapshot missing actors.");

                var tick = driver.LatestSnapshot.Tick;
                StepRuntime(driver, boardRenderer, actorRenderer, 4, 0.1f);
                if (driver.LatestSnapshot.Tick <= tick)
                    throw new InvalidOperationException("Stage 3 simulation tick did not advance.");

                placement.SetPlacementMode(true);
                if (!placement.IsPlacementModeActive)
                    throw new InvalidOperationException("Stage 3 placement mode did not activate.");

                var heightBefore = placement.CurrentHeight;
                placement.AdjustHeight(0.5f);
                if (Mathf.Abs(placement.CurrentHeight - heightBefore) < 0.01f)
                    throw new InvalidOperationException("Stage 3 height adjustment did not change the board.");

                var yawBefore = placement.CurrentYaw;
                placement.AdjustYaw(20f);
                if (Mathf.Abs(Mathf.DeltaAngle(yawBefore, placement.CurrentYaw)) < 0.01f)
                    throw new InvalidOperationException("Stage 3 yaw adjustment did not change the board.");

                var scaleBefore = placement.CurrentScale;
                placement.AdjustScale(0.25f);
                if (Mathf.Abs(placement.CurrentScale - scaleBefore) < 0.01f)
                    throw new InvalidOperationException("Stage 3 scale adjustment did not change the board.");

                placement.ResetPlacement();
                if (Mathf.Abs(placement.CurrentHeight) > 0.01f || Mathf.Abs(placement.CurrentYaw) > 0.01f || Mathf.Abs(placement.CurrentScale - 1f) > 0.01f)
                    throw new InvalidOperationException("Stage 3 reset did not restore defaults.");

                placement.SetHeight(1.2f);
                placement.SetYaw(45f);
                placement.SetScale(1.5f);
                placement.Save();
                placement.ResetPlacement();
                placement.Load();
                if (Mathf.Abs(placement.CurrentHeight - 1.2f) > 0.01f || Mathf.Abs(Mathf.DeltaAngle(placement.CurrentYaw, 45f)) > 0.01f || Mathf.Abs(placement.CurrentScale - 1.5f) > 0.01f)
                    throw new InvalidOperationException("Stage 3 save/load round trip failed.");

                placement.ResetPlacement();
                placement.Save();

                var ray = new Ray(mapper.CellToWorldCenter(Int2.Zero) + mapper.transform.up * 10f, -mapper.transform.up);
                Int2 cell;
                Vector3 hit;
                if (!mapper.TryRayToCell(ray, out cell, out hit) || !mapper.IsInsideBoard(cell))
                    throw new InvalidOperationException("Stage 3 coordinate mapper did not map a valid board cell.");

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 3 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 3 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void StepRuntime(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, int frames, float deltaTime)
        {
            for (var i = 0; i < frames; i++)
            {
                driver.ManualUpdate(deltaTime);
                boardRenderer.UpdateHover(null);
                ProjectAegisRTS.Snapshots.PlacementPreviewSnapshot preview;
                boardRenderer.UpdatePlacementPreview(driver.TryGetPlacementPreview(out preview) ? preview : null);
                actorRenderer.RenderSnapshot(driver.LatestSnapshot, driver.SelectedActorIds, deltaTime);
            }
        }

        static T RequireEnabled<T>(string label) where T : Behaviour
        {
            var component = UnityEngine.Object.FindFirstObjectByType<T>();
            if (component == null)
                throw new InvalidOperationException("Missing component: " + label);
            if (!component.enabled)
                throw new InvalidOperationException("Component is disabled: " + label);
            return component;
        }

        static void CaptureRedError(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                RedErrors.Add(condition);
        }
    }
}
