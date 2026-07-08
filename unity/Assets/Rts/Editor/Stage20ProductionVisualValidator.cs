using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Art.Production;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage20ProductionVisualValidator
    {
        public const string ValidationMarkdownPath = "docs/STAGE20_PRODUCTION_VISUAL_VALIDATION.md";

        [MenuItem("ProjectAegisRTS/Stage 20/Validate MVP Production Visuals")]
        public static void ValidateStage20ProductionVisualsMenu()
        {
            ValidateStage20ProductionVisuals();
        }

        public static void ValidateStage20ProductionVisualsBatch()
        {
            try
            {
                ValidateStage20ProductionVisuals();
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

        public static Stage20ProductionVisualValidationSummary ValidateStage20ProductionVisuals()
        {
            Stage20MvpProductionProxyGenerator.EnsureStage20Folders();
            var specs = Stage8ActorCatalog.LoadSpecs();
            var summary = new Stage20ProductionVisualValidationSummary
            {
                expectedMvpCount = Stage20MvpVisualActorSet.ActorTypeIds.Length,
                validatedActorIds = new List<string>(),
                errors = new List<string>(),
                warnings = new List<string>()
            };

            for (var i = 0; i < specs.Count; i++)
            {
                var spec = specs[i];
                if (Stage20MvpVisualActorSet.Contains(spec.ActorTypeId))
                    ValidateSpec(spec, summary);
            }

            WriteReport(summary);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (summary.errors.Count > 0)
                throw new InvalidOperationException("Stage 20 production visual validation failed: " + string.Join(" | ", summary.errors.ToArray()));

            Debug.Log("Stage 20 production visual validation passed. MVP proxies: " + summary.proxyPrefabCount);
            return summary;
        }

        public static bool IsMvpDefinitionUsingProductionProxy(ActorVisualDefinition definition)
        {
            if (definition == null || !Stage20MvpVisualActorSet.Contains(definition.actorTypeId))
                return false;
            if (definition.productionPrefab == null)
                return false;
            if (definition.preferredPrefabMode != ActorVisualPrefabMode.ProductionPrefab)
                return false;
            var tag = definition.productionPrefab.GetComponentInChildren<ProductionVisualValidationTag>(true);
            return tag != null && tag.visualTier == ProductionVisualTier.FirstPassProxy;
        }

        static void ValidateSpec(Stage8ActorSpec spec, Stage20ProductionVisualValidationSummary summary)
        {
            var definition = AssetDatabase.LoadAssetAtPath<ActorVisualDefinition>(Stage8ActorCatalog.DefinitionAssetPath(spec));
            if (definition == null)
            {
                summary.errors.Add(spec.ActorTypeId + ": missing ActorVisualDefinition");
                return;
            }

            summary.definitionCount++;
            if (definition.generatedBlockoutPrefab == null)
                summary.errors.Add(spec.ActorTypeId + ": generated blockout fallback missing");
            if (definition.fallbackPrefab == null && definition.generatedBlockoutPrefab == null)
                summary.errors.Add(spec.ActorTypeId + ": no fallback prefab path remains");
            if (definition.productionPrefab == null)
                summary.errors.Add(spec.ActorTypeId + ": production proxy prefab not assigned");
            if (definition.preferredPrefabMode != ActorVisualPrefabMode.ProductionPrefab)
                summary.errors.Add(spec.ActorTypeId + ": preferred prefab mode must be ProductionPrefab");
            if (definition.productionStatus != ActorArtProductionStatus.ProxyPrefab)
                summary.errors.Add(spec.ActorTypeId + ": production status must be ProxyPrefab");

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Stage20MvpProductionProxyGenerator.ProductionProxyPath(spec));
            if (prefab == null)
            {
                summary.errors.Add(spec.ActorTypeId + ": production proxy prefab missing at " + Stage20MvpProductionProxyGenerator.ProductionProxyPath(spec));
                return;
            }

            if (definition.productionPrefab != prefab)
                summary.errors.Add(spec.ActorTypeId + ": ActorVisualDefinition productionPrefab does not reference the Stage 20 proxy");

            summary.proxyPrefabCount++;
            var descriptor = prefab.GetComponentInChildren<ActorPrefabDescriptor>(true);
            if (descriptor == null)
            {
                summary.errors.Add(spec.ActorTypeId + ": proxy missing ActorPrefabDescriptor");
                return;
            }

            if (descriptor.actorTypeId != spec.ActorTypeId)
                summary.errors.Add(spec.ActorTypeId + ": descriptor actorTypeId mismatch");
            if (descriptor.productionStatus != ActorArtProductionStatus.ProxyPrefab)
                summary.errors.Add(spec.ActorTypeId + ": descriptor production status must be ProxyPrefab");
            if (descriptor.generatedByStage8)
                summary.errors.Add(spec.ActorTypeId + ": Stage 20 proxy must not be marked generatedByStage8");

            var requiredSockets = Stage8ActorCatalog.RequiredSocketsFor(spec);
            if (spec.ActorTypeId == "refinery" && !requiredSockets.Contains(ActorPrefabSocketKind.DockPumpRoot))
                requiredSockets.Add(ActorPrefabSocketKind.DockPumpRoot);
            var missing = descriptor.ValidateRequiredSockets(requiredSockets);
            if (missing.Count > 0)
                summary.errors.Add(spec.ActorTypeId + ": missing sockets " + string.Join(", ", missing.ToArray()));
            else
                summary.socketValidatedCount++;

            var tag = prefab.GetComponentInChildren<ProductionVisualValidationTag>(true);
            if (tag == null)
            {
                summary.errors.Add(spec.ActorTypeId + ": proxy missing ProductionVisualValidationTag");
                return;
            }

            if (tag.actorTypeId != spec.ActorTypeId)
                summary.errors.Add(spec.ActorTypeId + ": validation tag actorTypeId mismatch");
            if (tag.visualTier != ProductionVisualTier.FirstPassProxy)
                summary.errors.Add(spec.ActorTypeId + ": validation tag visual tier must be FirstPassProxy");
            if (!tag.hasGridAccurateBase)
                summary.errors.Add(spec.ActorTypeId + ": grid-accurate base marker missing");
            if ((tag.ViewCoverage & ProductionVisualViewCoverage.AllAround) != ProductionVisualViewCoverage.AllAround)
                summary.errors.Add(spec.ActorTypeId + ": all-around view coverage marker incomplete");
            else
                summary.viewCoverageValidatedCount++;
            if (!tag.hasBeveledOrTieredForm)
                summary.errors.Add(spec.ActorTypeId + ": beveled/tiered form marker missing");

            ValidateCategoryRules(spec, descriptor, tag, summary);

            if (prefab.GetComponentInChildren<LODGroup>(true) == null)
                summary.errors.Add(spec.ActorTypeId + ": missing LODGroup");
            else
                summary.lodGroupCount++;

            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab) > 0)
                summary.errors.Add(spec.ActorTypeId + ": prefab contains missing scripts");

            summary.validatedActorIds.Add(spec.ActorTypeId);
        }

        static void ValidateCategoryRules(Stage8ActorSpec spec, ActorPrefabDescriptor descriptor, ProductionVisualValidationTag tag, Stage20ProductionVisualValidationSummary summary)
        {
            if (spec.Category == ActorArtCategory.Building || spec.Category == ActorArtCategory.Support || spec.Category == ActorArtCategory.Defense)
            {
                if (!tag.hasFrontDetail || !tag.hasBackDetail || !tag.hasLeftDetail || !tag.hasRightDetail || !tag.hasRoofDetail)
                    summary.errors.Add(spec.ActorTypeId + ": building proxy must carry front/back/side/roof detail markers");
            }

            if (spec.ActorTypeId == "light_tank")
            {
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.TurretRoot, summary);
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.TrackLeft, summary);
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.TrackRight, summary);
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.MuzzlePrimary, summary);
            }

            if (spec.ActorTypeId == "gun_tower")
            {
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.TurretRoot, summary);
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.BarrelRoot, summary);
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.MuzzlePrimary, summary);
            }

            if (spec.ActorTypeId == "harvester")
            {
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.TrackLeft, summary);
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.TrackRight, summary);
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.HarvesterDock, summary);
            }

            if (spec.ActorTypeId == "rifle_infantry")
            {
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.Head, summary);
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.WeaponSocket, summary);
                RequireSocket(spec, descriptor, ActorPrefabSocketKind.AimPivot, summary);
            }
        }

        static void RequireSocket(Stage8ActorSpec spec, ActorPrefabDescriptor descriptor, ActorPrefabSocketKind kind, Stage20ProductionVisualValidationSummary summary)
        {
            Transform socket;
            if (!descriptor.TryGetSocket(kind, out socket))
                summary.errors.Add(spec.ActorTypeId + ": missing required Stage 20 socket " + kind);
        }

        static void WriteReport(Stage20ProductionVisualValidationSummary summary)
        {
            var path = Path.Combine(Stage8ActorCatalog.RepoRoot, ValidationMarkdownPath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, BuildMarkdown(summary), Encoding.UTF8);
        }

        static string BuildMarkdown(Stage20ProductionVisualValidationSummary summary)
        {
            var builder = new StringBuilder();
            builder.AppendLine("# Stage 20 Production Visual Validation");
            builder.AppendLine();
            builder.AppendLine("Stage 20 validates the first-pass MVP production proxy layer. These prefabs are still proxy art, but they must be true 3D tabletop miniatures with all-around detail, grid-accurate bases, sockets, LOD groups, and safe fallback paths.");
            builder.AppendLine();
            builder.AppendLine("- Expected MVP proxies: " + summary.expectedMvpCount);
            builder.AppendLine("- ActorVisualDefinitions checked: " + summary.definitionCount);
            builder.AppendLine("- Production proxy prefabs: " + summary.proxyPrefabCount);
            builder.AppendLine("- Required socket sets valid: " + summary.socketValidatedCount);
            builder.AppendLine("- View coverage markers valid: " + summary.viewCoverageValidatedCount);
            builder.AppendLine("- LODGroups present: " + summary.lodGroupCount);
            builder.AppendLine("- Errors: " + summary.errors.Count);
            builder.AppendLine("- Warnings: " + summary.warnings.Count);
            builder.AppendLine();
            builder.AppendLine("## MVP Actors");
            for (var i = 0; i < summary.validatedActorIds.Count; i++)
                builder.AppendLine("- " + summary.validatedActorIds[i]);
            if (summary.validatedActorIds.Count == 0)
                builder.AppendLine("- None");
            builder.AppendLine();
            builder.AppendLine("## Errors");
            AppendList(builder, summary.errors);
            builder.AppendLine();
            builder.AppendLine("## Warnings");
            AppendList(builder, summary.warnings);
            builder.AppendLine();
            builder.AppendLine("## Validation Rules");
            builder.AppendLine("- MVP ActorVisualDefinitions must prefer `ProductionPrefab`.");
            builder.AppendLine("- MVP definitions must keep Stage 8 blockouts or fallback prefabs assigned.");
            builder.AppendLine("- MVP prefabs must include `ActorPrefabDescriptor`, `ProductionVisualValidationTag`, required sockets, and an `LODGroup`.");
            builder.AppendLine("- Buildings must mark top, front, back, left, right, and roof detail.");
            builder.AppendLine("- Light tank and gun tower must keep turret/barrel/muzzle hooks.");
            builder.AppendLine("- Harvester must keep track and dock hooks.");
            builder.AppendLine("- Rifle infantry must keep head, weapon, and aim sockets.");
            return builder.ToString();
        }

        static void AppendList(StringBuilder builder, List<string> values)
        {
            if (values.Count == 0)
            {
                builder.AppendLine("- None");
                return;
            }

            for (var i = 0; i < values.Count; i++)
                builder.AppendLine("- " + values[i]);
        }
    }

    public sealed class Stage20ProductionVisualValidationSummary
    {
        public int expectedMvpCount;
        public int definitionCount;
        public int proxyPrefabCount;
        public int socketValidatedCount;
        public int viewCoverageValidatedCount;
        public int lodGroupCount;
        public List<string> validatedActorIds;
        public List<string> errors;
        public List<string> warnings;
    }
}
