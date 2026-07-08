using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Combat;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.XR.RightHand;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage9PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage9PlayModeSmokeBatch()
        {
            try
            {
                RunStage9PlayModeSmoke();
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

        public static void RunStage9PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage9SceneValidator.ValidateStage9Scene();
                var scene = EditorSceneManager.OpenScene(Stage9SceneCreator.ScenePath);
                if (!scene.IsValid())
                    throw new InvalidOperationException("Stage 9 scene did not open.");

                var bootstrapper = RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
                var driver = RequireEnabled<RtsSimulationDriver>("RtsSimulationDriver");
                var boardRenderer = RequireEnabled<BoardRenderer>("BoardRenderer");
                var actorRenderer = RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
                var profileLibrary = RequireEnabled<CombatVisualProfileLibrary>("CombatVisualProfileLibrary");
                var projectileRenderer = RequireEnabled<ProjectileRenderSystem>("ProjectileRenderSystem");
                var eventRenderer = RequireEnabled<CombatEventRenderSystem>("CombatEventRenderSystem");
                RequireEnabled<CombatDebugHud>("CombatDebugHud");

                bootstrapper.InitializeScene();
                driver.TryCreateCombatDemoWorld();
                StepRuntime(driver, boardRenderer, actorRenderer, projectileRenderer, eventRenderer, 4, 0.1f);

                if (driver.LatestSnapshot == null || driver.LatestSnapshot.Actors.Count < 4)
                    throw new InvalidOperationException("Stage 9 combat demo actors were not created.");

                var attackerId = FindActor(driver.LatestSnapshot, "light_tank", 1);
                var towerId = FindActor(driver.LatestSnapshot, "gun_tower", 1);
                var targetId = FindActor(driver.LatestSnapshot, "medium_tank", 2);
                var targetStartHealth = GetActor(driver.LatestSnapshot, targetId).Health;
                driver.SetSelectedActorIds(new[] { attackerId, towerId });
                var attack = driver.TryIssueAttackSelectedToActor(targetId);
                if (!attack.Success)
                    throw new InvalidOperationException("Stage 9 attack command failed: " + attack);

                var sawProjectile = false;
                var sawProjectileVisual = false;
                var sawDamage = false;
                var sawDeath = false;
                for (var i = 0; i < 150; i++)
                {
                    StepRuntime(driver, boardRenderer, actorRenderer, projectileRenderer, eventRenderer, 1, 0.1f);
                    var snapshot = driver.LatestSnapshot;
                    if (snapshot.Projectiles.Count > 0)
                        sawProjectile = true;
                    if (projectileRenderer.ProjectileVisualCount > 0)
                        sawProjectileVisual = true;
                    var target = GetActor(snapshot, targetId);
                    if (target.Health < targetStartHealth)
                        sawDamage = true;
                    if (target.IsDestroyed)
                    {
                        sawDeath = true;
                        break;
                    }
                }

                if (driver.LatestSnapshot.Tick <= 0)
                    throw new InvalidOperationException("Stage 9 tick did not advance.");
                if (actorRenderer.ActorVisualCount < 4)
                    throw new InvalidOperationException("Stage 9 actor visuals were not generated.");
                if (!sawProjectile)
                    throw new InvalidOperationException("Stage 9 projectile snapshot did not appear.");
                if (!sawProjectileVisual)
                    throw new InvalidOperationException("Stage 9 projectile visual did not appear.");
                if (!sawDamage)
                    throw new InvalidOperationException("Stage 9 damage was not applied.");
                if (!sawDeath)
                    throw new InvalidOperationException("Stage 9 target was not destroyed.");
                if (eventRenderer.PlayedEventCount <= 0)
                    throw new InvalidOperationException("Stage 9 combat event renderer did not play events.");
                if (profileLibrary.ProfileCount < 6)
                    throw new InvalidOperationException("Stage 9 combat profile library incomplete.");

                var router = UnityEngine.Object.FindFirstObjectByType<RightHandCommandRouter>();
                if (router != null)
                {
                    router.EnterAttackMode();
                    var route = router.IssueAttackPlaceholderAtCell(new Int2(13, 9), false);
                    if (route == null)
                        throw new InvalidOperationException("Stage 9 right-hand attack route returned null.");
                }

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 9 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 9 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void StepRuntime(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, ProjectileRenderSystem projectileRenderer, CombatEventRenderSystem eventRenderer, int frames, float deltaTime)
        {
            for (var i = 0; i < frames; i++)
            {
                driver.ManualUpdate(deltaTime);
                boardRenderer.UpdateHover(driver.HasHoveredCell ? (Int2?)driver.HoveredCell : null);
                PlacementPreviewSnapshot preview;
                boardRenderer.UpdatePlacementPreview(driver.TryGetPlacementPreview(out preview) ? preview : null);
                actorRenderer.RenderSnapshot(driver.LatestSnapshot, driver.SelectedActorIds, deltaTime);
                projectileRenderer.RenderSnapshot(driver.LatestSnapshot);
                eventRenderer.RenderSnapshot(driver.LatestSnapshot);
            }
        }

        static int FindActor(WorldSnapshot snapshot, string typeId, int ownerId)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
                if (snapshot.Actors[i].TypeId == typeId && snapshot.Actors[i].OwnerId == ownerId && !snapshot.Actors[i].IsDestroyed)
                    return snapshot.Actors[i].ActorId;
            throw new InvalidOperationException("Missing actor " + typeId + " for owner " + ownerId);
        }

        static ActorSnapshot GetActor(WorldSnapshot snapshot, int actorId)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
                if (snapshot.Actors[i].ActorId == actorId)
                    return snapshot.Actors[i];
            throw new InvalidOperationException("Missing actor snapshot " + actorId);
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
