using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProjectAegisRTS.UnityClient.Art;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage8PrefabSocketValidator
    {
        [MenuItem("ProjectAegisRTS/Stage 8/Validate Actor Prefabs")]
        public static void ValidatePrefabsMenu()
        {
            ValidatePrefabs();
        }

        public static void ValidatePrefabsBatch()
        {
            try
            {
                ValidatePrefabs();
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

        public static Stage8ValidationSummary ValidatePrefabs()
        {
            Stage8ActorCatalog.EnsureStage8Folders();
            var specs = Stage8ActorCatalog.LoadSpecs();
            var summary = new Stage8ValidationSummary();
            summary.expectedCount = Stage8ActorCatalog.ExpectedActorCount;
            summary.errors = new List<string>();
            summary.warnings = new List<string>();
            summary.ipReviewActorIds = new List<string>();

            for (var i = 0; i < specs.Count; i++)
                ValidateSpec(specs[i], summary);

            WriteReports(summary);
            AssetDatabase.Refresh();

            if (summary.errors.Count > 0)
                throw new InvalidOperationException("Stage 8 prefab validation failed: " + string.Join(" | ", summary.errors.ToArray()));

            Debug.Log("Stage 8 prefab validation passed.");
            return summary;
        }

        static void ValidateSpec(Stage8ActorSpec spec, Stage8ValidationSummary summary)
        {
            summary.definitionCount++;
            if (spec.IpReviewRequired)
                summary.ipReviewActorIds.Add(spec.ActorTypeId);

            if (!File.Exists(Path.GetFullPath(Path.Combine(Application.dataPath, "..", Stage8ActorCatalog.ConceptAssetPath(spec)))))
                summary.warnings.Add(spec.ActorTypeId + ": concept image not copied into Unity art folder");
            else
                summary.conceptCount++;

            var definition = AssetDatabase.LoadAssetAtPath<ActorVisualDefinition>(Stage8ActorCatalog.DefinitionAssetPath(spec));
            if (definition == null)
            {
                summary.errors.Add(spec.ActorTypeId + ": missing ActorVisualDefinition asset");
                return;
            }

            if (definition.conceptReference == null)
                summary.warnings.Add(spec.ActorTypeId + ": missing concept reference assignment");
            if (definition.icon == null)
                summary.errors.Add(spec.ActorTypeId + ": missing generated icon");
            else
                summary.iconCount++;
            if (definition.ipReviewRequired != spec.IpReviewRequired)
                summary.errors.Add(spec.ActorTypeId + ": IP review flag mismatch");

            var prefab = definition.GetBestPrefab();
            if (prefab == null)
            {
                summary.errors.Add(spec.ActorTypeId + ": missing best prefab");
                return;
            }
            summary.blockoutPrefabCount++;

            var descriptor = prefab.GetComponentInChildren<ActorPrefabDescriptor>(true);
            if (descriptor == null)
            {
                summary.errors.Add(spec.ActorTypeId + ": prefab missing ActorPrefabDescriptor");
                return;
            }
            if (descriptor.actorTypeId != spec.ActorTypeId)
                summary.errors.Add(spec.ActorTypeId + ": descriptor actorTypeId mismatch");

            var missing = descriptor.ValidateRequiredSockets(Stage8ActorCatalog.RequiredSocketsFor(spec));
            if (missing.Count > 0)
                summary.errors.Add(spec.ActorTypeId + ": missing sockets " + string.Join(", ", missing.ToArray()));
            else
                summary.socketValidatedCount++;

            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab) > 0)
                summary.errors.Add(spec.ActorTypeId + ": prefab contains missing scripts");
        }

        static void WriteReports(Stage8ValidationSummary summary)
        {
            var jsonPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", Stage8ActorCatalog.ValidationJsonPath));
            Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));
            File.WriteAllText(jsonPath, BuildJson(summary), Encoding.UTF8);

            var markdownPath = Path.Combine(Stage8ActorCatalog.RepoRoot, Stage8ActorCatalog.ValidationMarkdownPath);
            Directory.CreateDirectory(Path.GetDirectoryName(markdownPath));
            File.WriteAllText(markdownPath, BuildMarkdown(summary), Encoding.UTF8);
        }

        static string BuildJson(Stage8ValidationSummary summary)
        {
            var builder = new StringBuilder();
            builder.AppendLine("{");
            builder.AppendLine("  \"expectedCount\": " + summary.expectedCount + ",");
            builder.AppendLine("  \"definitionCount\": " + summary.definitionCount + ",");
            builder.AppendLine("  \"conceptCount\": " + summary.conceptCount + ",");
            builder.AppendLine("  \"blockoutPrefabCount\": " + summary.blockoutPrefabCount + ",");
            builder.AppendLine("  \"iconCount\": " + summary.iconCount + ",");
            builder.AppendLine("  \"socketValidatedCount\": " + summary.socketValidatedCount + ",");
            builder.AppendLine("  \"ipReviewCount\": " + summary.ipReviewActorIds.Count + ",");
            builder.AppendLine("  \"errors\": " + JsonArray(summary.errors) + ",");
            builder.AppendLine("  \"warnings\": " + JsonArray(summary.warnings) + ",");
            builder.AppendLine("  \"ipReviewActorIds\": " + JsonArray(summary.ipReviewActorIds));
            builder.AppendLine("}");
            return builder.ToString();
        }

        static string BuildMarkdown(Stage8ValidationSummary summary)
        {
            var builder = new StringBuilder();
            builder.AppendLine("# Stage 8 Prefab Validation");
            builder.AppendLine();
            builder.AppendLine("- Expected actors: " + summary.expectedCount);
            builder.AppendLine("- Actor visual definitions: " + summary.definitionCount);
            builder.AppendLine("- Concept images copied: " + summary.conceptCount);
            builder.AppendLine("- Generated blockout prefabs: " + summary.blockoutPrefabCount);
            builder.AppendLine("- Icons assigned: " + summary.iconCount);
            builder.AppendLine("- Prefabs with required sockets: " + summary.socketValidatedCount);
            builder.AppendLine("- IP review flags: " + summary.ipReviewActorIds.Count);
            builder.AppendLine("- Errors: " + summary.errors.Count);
            builder.AppendLine("- Warnings: " + summary.warnings.Count);
            builder.AppendLine();
            builder.AppendLine("## IP Review Actors");
            if (summary.ipReviewActorIds.Count == 0)
                builder.AppendLine("- None");
            else
                for (var i = 0; i < summary.ipReviewActorIds.Count; i++)
                    builder.AppendLine("- " + summary.ipReviewActorIds[i]);
            builder.AppendLine();
            builder.AppendLine("## Errors");
            if (summary.errors.Count == 0)
                builder.AppendLine("- None");
            else
                for (var i = 0; i < summary.errors.Count; i++)
                    builder.AppendLine("- " + summary.errors[i]);
            builder.AppendLine();
            builder.AppendLine("## Warnings");
            if (summary.warnings.Count == 0)
                builder.AppendLine("- None");
            else
                for (var i = 0; i < summary.warnings.Count; i++)
                    builder.AppendLine("- " + summary.warnings[i]);
            return builder.ToString();
        }

        static string JsonArray(List<string> values)
        {
            var builder = new StringBuilder();
            builder.Append("[");
            for (var i = 0; i < values.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");
                builder.Append("\"").Append(Escape(values[i])).Append("\"");
            }
            builder.Append("]");
            return builder.ToString();
        }

        static string Escape(string value)
        {
            return (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }

    public sealed class Stage8ValidationSummary
    {
        public int expectedCount;
        public int definitionCount;
        public int conceptCount;
        public int blockoutPrefabCount;
        public int iconCount;
        public int socketValidatedCount;
        public List<string> ipReviewActorIds;
        public List<string> errors;
        public List<string> warnings;
    }
}
