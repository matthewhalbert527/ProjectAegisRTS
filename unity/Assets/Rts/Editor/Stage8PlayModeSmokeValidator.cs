using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage8PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage8PlayModeSmokeBatch()
        {
            try
            {
                RunStage8PlayModeSmoke();
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

        public static void RunStage8PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage8SceneValidator.ValidateStage8Scene();
                var scene = EditorSceneManager.OpenScene(Stage8SceneCreator.ScenePath);
                if (!scene.IsValid())
                    throw new InvalidOperationException("Stage 8 scene did not open.");

                var bootstrapper = RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
                var driver = RequireEnabled<RtsSimulationDriver>("RtsSimulationDriver");
                var boardRenderer = RequireEnabled<BoardRenderer>("BoardRenderer");
                var actorRenderer = RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
                var visualLibrary = RequireEnabled<ActorVisualDefinitionLibrary>("ActorVisualDefinitionLibrary");
                var conceptLibrary = RequireEnabled<ConceptArtReferenceLibrary>("ConceptArtReferenceLibrary");
                var resolver = RequireEnabled<ActorVisualPrefabResolver>("ActorVisualPrefabResolver");
                var showcase = RequireEnabled<ArtPipelineShowcaseController>("ArtPipelineShowcaseController");
                RequireEnabled<ArtPipelineDebugHud>("ArtPipelineDebugHud");

                bootstrapper.InitializeScene();
                visualLibrary.EnsureInitialized();
                conceptLibrary.EnsureInitialized();
                showcase.EnsureShowcase();
                StepRuntime(driver, boardRenderer, actorRenderer, 8, 0.1f);

                if (driver.LatestSnapshot == null || driver.LatestSnapshot.Tick <= 0)
                    throw new InvalidOperationException("Stage 8 tick did not advance.");
                if (actorRenderer.ActorVisualCount < 2)
                    throw new InvalidOperationException("Stage 8 actor visuals were not generated.");
                if (visualLibrary.DefinitionCount != Stage8ActorCatalog.ExpectedActorCount)
                    throw new InvalidOperationException("Stage 8 definition library count mismatch.");
                if (conceptLibrary.ReferenceCount != Stage8ActorCatalog.ExpectedActorCount)
                    throw new InvalidOperationException("Stage 8 concept reference library count mismatch.");

                RequireResolution(resolver, "fabrication_hub");
                RequireResolution(resolver, "power_plant");
                RequireResolution(resolver, "barracks");
                RequireResolution(resolver, "war_factory");
                RequireResolution(resolver, "refinery");
                RequireResolution(resolver, "rifle_infantry");
                RequireResolution(resolver, "light_tank");
                RequireResolution(resolver, "harvester");

                ValidatePrefabInstantiation();
                RequireRepresentativeSocket(visualLibrary, ActorArtCategory.Building, ActorPrefabSocketKind.DoorRoot);
                RequireRepresentativeSocket(visualLibrary, ActorArtCategory.Defense, ActorPrefabSocketKind.MuzzlePrimary);
                RequireRepresentativeSocket(visualLibrary, ActorArtCategory.Vehicle, ActorPrefabSocketKind.BodyRoot);
                RequireRepresentativeSocket(visualLibrary, ActorArtCategory.Infantry, ActorPrefabSocketKind.WeaponSocket);
                RequireRepresentativeSocket(visualLibrary, ActorArtCategory.Aircraft, ActorPrefabSocketKind.AircraftRotor);

                if (actorRenderer.ResolvedPrefabCount <= 0 && actorRenderer.FallbackPrimitiveCount <= 0)
                    throw new InvalidOperationException("Stage 8 renderer did not report prefab resolution or fallback behavior.");
                if (showcase.DefinitionCount != Stage8ActorCatalog.ExpectedActorCount || showcase.BlockoutPrefabCount != Stage8ActorCatalog.ExpectedActorCount)
                    throw new InvalidOperationException("Stage 8 showcase counts are not complete.");

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 8 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 8 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void RequireResolution(ActorVisualPrefabResolver resolver, string actorTypeId)
        {
            ActorVisualDefinition definition;
            GameObject prefab;
            if (!resolver.ResolvePrefab(actorTypeId, out definition, out prefab) || definition == null || prefab == null)
                throw new InvalidOperationException("Stage 8 resolver failed for " + actorTypeId);
        }

        static void ValidatePrefabInstantiation()
        {
            var specs = Stage8ActorCatalog.LoadSpecs();
            for (var i = 0; i < specs.Count; i++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Stage8ActorCatalog.BlockoutPrefabPath(specs[i]));
                if (prefab == null)
                    throw new InvalidOperationException("Stage 8 blockout prefab missing for smoke: " + specs[i].ActorTypeId);
                var instance = UnityEngine.Object.Instantiate(prefab);
                try
                {
                    var descriptor = instance.GetComponentInChildren<ActorPrefabDescriptor>(true);
                    if (descriptor == null)
                        throw new InvalidOperationException("Stage 8 instantiated prefab missing descriptor: " + specs[i].ActorTypeId);
                    var missing = descriptor.ValidateRequiredSockets(Stage8ActorCatalog.RequiredSocketsFor(specs[i]));
                    if (missing.Count > 0)
                        throw new InvalidOperationException("Stage 8 instantiated prefab missing sockets for " + specs[i].ActorTypeId + ": " + string.Join(", ", missing.ToArray()));
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }
        }

        static void RequireRepresentativeSocket(ActorVisualDefinitionLibrary library, ActorArtCategory category, ActorPrefabSocketKind socketKind)
        {
            var definitions = library.GetDefinitionsByCategory(category);
            for (var i = 0; i < definitions.Count; i++)
            {
                var prefab = definitions[i] == null ? null : definitions[i].GetBestPrefab();
                var descriptor = prefab == null ? null : prefab.GetComponentInChildren<ActorPrefabDescriptor>(true);
                Transform socket;
                if (descriptor != null && descriptor.TryGetSocket(socketKind, out socket))
                    return;
            }

            throw new InvalidOperationException("Stage 8 missing representative socket " + socketKind + " for " + category);
        }

        static void StepRuntime(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, int frames, float deltaTime)
        {
            for (var i = 0; i < frames; i++)
            {
                driver.ManualUpdate(deltaTime);
                boardRenderer.UpdateHover(driver.HasHoveredCell ? (Int2?)driver.HoveredCell : null);
                PlacementPreviewSnapshot preview;
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
