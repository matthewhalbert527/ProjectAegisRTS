using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Art.Production;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage21MvpSocketPivotScaleValidator
    {
        [MenuItem("ProjectAegisRTS/Stage 21/Validate MVP Sockets Pivot Scale")]
        public static void ValidateMvpSocketPivotScaleMenu()
        {
            ValidateMvpSocketPivotScale();
        }

        public static void ValidateMvpSocketPivotScaleBatch()
        {
            try
            {
                ValidateMvpSocketPivotScale();
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

        public static Stage21SocketPivotScaleSummary ValidateMvpSocketPivotScale()
        {
            Stage20MvpProductionProxyGenerator.GenerateMvpProductionProxies();

            var summary = new Stage21SocketPivotScaleSummary
            {
                errors = new List<string>(),
                warnings = new List<string>(),
                validatedActorIds = new List<string>()
            };

            var specs = Stage8ActorCatalog.LoadSpecs();
            for (var i = 0; i < specs.Count; i++)
            {
                var spec = specs[i];
                if (!Stage20MvpVisualActorSet.Contains(spec.ActorTypeId))
                    continue;
                ValidateSpec(spec, summary);
            }

            if (summary.errors.Count > 0)
                throw new InvalidOperationException("Stage 21 socket/pivot/scale validation failed: " + string.Join(" | ", summary.errors.ToArray()));

            Debug.Log("Stage 21 socket/pivot/scale validation passed. Actors: " + summary.validatedActorIds.Count + ", warnings: " + summary.warnings.Count);
            return summary;
        }

        static void ValidateSpec(Stage8ActorSpec spec, Stage21SocketPivotScaleSummary summary)
        {
            var definition = AssetDatabase.LoadAssetAtPath<ActorVisualDefinition>(Stage8ActorCatalog.DefinitionAssetPath(spec));
            if (definition == null)
            {
                summary.errors.Add(spec.ActorTypeId + ": missing ActorVisualDefinition");
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Stage20MvpProductionProxyGenerator.ProductionProxyPath(spec));
            if (prefab == null)
            {
                summary.errors.Add(spec.ActorTypeId + ": missing Stage20/21 production proxy prefab");
                return;
            }

            var descriptor = prefab.GetComponentInChildren<ActorPrefabDescriptor>(true);
            var tag = prefab.GetComponentInChildren<ProductionVisualValidationTag>(true);
            if (descriptor == null)
            {
                summary.errors.Add(spec.ActorTypeId + ": missing ActorPrefabDescriptor");
                return;
            }
            if (tag == null)
            {
                summary.errors.Add(spec.ActorTypeId + ": missing ProductionVisualValidationTag");
                return;
            }

            int rendererCount;
            int materialCount;
            var bounds = MvpVisualQaRunner.CalculateLocalBounds(prefab, out rendererCount, out materialCount);
            if (bounds.min.y < -0.12f || bounds.min.y > 0.22f)
                summary.errors.Add(spec.ActorTypeId + ": root pivot/base is not near Y=0; bounds min Y " + bounds.min.y.ToString("F2"));
            if (Mathf.Abs(bounds.center.x) > 0.45f || Mathf.Abs(bounds.center.z) > 0.65f)
                summary.warnings.Add(spec.ActorTypeId + ": visual mass is off-center from root pivot: " + bounds.center.ToString("F2"));

            var maxWidth = Mathf.Max(1f, spec.FootprintWidth) + 1.25f;
            var maxDepth = Mathf.Max(1f, spec.FootprintHeight) + 1.75f;
            if (bounds.size.x > maxWidth || bounds.size.z > maxDepth)
                summary.errors.Add(spec.ActorTypeId + ": visual bounds exceed allowed footprint overhang: " + bounds.size.ToString("F2"));

            if (!tag.hasGridAccurateBase)
                summary.errors.Add(spec.ActorTypeId + ": grid-accurate base metadata missing");
            if (!tag.hasStage21ReadabilityPass || !tag.hasArtistReplacementMetadata || !tag.hasSocketScaffold)
                summary.errors.Add(spec.ActorTypeId + ": Stage 21 replacement metadata incomplete");

            ValidateRequiredSockets(spec, descriptor, summary);
            ValidateActorSpecificSockets(spec, descriptor, summary);

            if (definition.fallbackPrefab == null && definition.generatedBlockoutPrefab == null)
                summary.errors.Add(spec.ActorTypeId + ": fallback blockout is not assigned");
            if (definition.productionPrefab != prefab)
                summary.errors.Add(spec.ActorTypeId + ": ActorVisualDefinition does not reference the active proxy prefab");
            if (prefab.GetComponentInChildren<LODGroup>(true) == null)
                summary.errors.Add(spec.ActorTypeId + ": missing LODGroup");
            if (rendererCount <= 3)
                summary.errors.Add(spec.ActorTypeId + ": proxy has too few renderer parts to read as a Stage 21 miniature");
            if (materialCount <= 2)
                summary.errors.Add(spec.ActorTypeId + ": proxy has too few materials to distinguish identity/readability zones");

            summary.validatedActorIds.Add(spec.ActorTypeId);
        }

        static void ValidateRequiredSockets(Stage8ActorSpec spec, ActorPrefabDescriptor descriptor, Stage21SocketPivotScaleSummary summary)
        {
            var required = Stage8ActorCatalog.RequiredSocketsFor(spec);
            if (spec.ActorTypeId == "refinery" && !required.Contains(ActorPrefabSocketKind.DockPumpRoot))
                required.Add(ActorPrefabSocketKind.DockPumpRoot);

            var missing = descriptor.ValidateRequiredSockets(required);
            if (missing.Count > 0)
                summary.errors.Add(spec.ActorTypeId + ": missing required sockets " + string.Join(", ", missing.ToArray()));
        }

        static void ValidateActorSpecificSockets(Stage8ActorSpec spec, ActorPrefabDescriptor descriptor, Stage21SocketPivotScaleSummary summary)
        {
            RequireSocket(spec, descriptor, ActorPrefabSocketKind.SelectionAnchor, summary);
            RequireSocket(spec, descriptor, ActorPrefabSocketKind.HealthBarAnchor, summary);

            if (spec.ActorTypeId == "fabrication_hub" || spec.ActorTypeId == "barracks" || spec.ActorTypeId == "war_factory")
            {
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.ProductionExit, summary);
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.RallyExit, summary);
            }
            if (spec.ActorTypeId == "power_plant")
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.TurbineRoot, summary);
            if (spec.ActorTypeId == "refinery")
            {
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.HarvesterDock, summary);
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.DockPumpRoot, summary);
            }
            if (spec.ActorTypeId == "gun_tower" || spec.ActorTypeId == "light_tank")
            {
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.TurretRoot, summary);
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.BarrelRoot, summary);
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.MuzzlePrimary, summary);
            }
            if (spec.ActorTypeId == "rifle_infantry")
            {
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.Head, summary);
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.WeaponSocket, summary);
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.AimPivot, summary);
            }
        }

        static void RequireSocket(Stage8ActorSpec spec, ActorPrefabDescriptor descriptor, ActorPrefabSocketKind kind, Stage21SocketPivotScaleSummary summary)
        {
            Transform socket;
            if (!descriptor.TryGetSocket(kind, out socket))
                summary.errors.Add(spec.ActorTypeId + ": missing Stage 21 socket " + kind);
        }
    }

    public sealed class Stage21SocketPivotScaleSummary
    {
        public List<string> validatedActorIds;
        public List<string> errors;
        public List<string> warnings;
    }
}
