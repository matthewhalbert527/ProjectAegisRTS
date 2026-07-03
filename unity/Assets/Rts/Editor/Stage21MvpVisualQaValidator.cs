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
    public static class Stage21MvpVisualQaValidator
    {
        public const string QaMarkdownPath = "docs/STAGE21_MVP_VISUAL_QA.md";

        [MenuItem("ProjectAegisRTS/Stage 21/Validate MVP Visual QA")]
        public static void ValidateMvpVisualQaMenu()
        {
            ValidateMvpVisualQa();
        }

        public static void ValidateMvpVisualQaBatch()
        {
            try
            {
                ValidateMvpVisualQa();
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

        public static Stage21MvpVisualQaSummary ValidateMvpVisualQa(bool regenerateProxies = true)
        {
            if (regenerateProxies)
                Stage20MvpProductionProxyGenerator.GenerateMvpProductionProxies();
            Stage21ArtistModelImportScanner.ScanMvpArtistModels();

            var manifest = AssetDatabase.LoadAssetAtPath<ArtistModelImportManifest>(Stage21ArtistModelImportScanner.ManifestAssetPath);
            var summary = RunQa(manifest);
            WriteReport(summary);

            if (summary.FailCount > 0)
                throw new InvalidOperationException("Stage 21 MVP visual QA failed. Failing actor count: " + summary.FailCount);

            Debug.Log("Stage 21 MVP visual QA validation passed. Actors: " + summary.reports.Count + ", warnings: " + summary.WarningCount);
            return summary;
        }

        public static Stage21MvpVisualQaSummary RunQa(ArtistModelImportManifest manifest)
        {
            var root = new GameObject("Stage21 MVP Visual QA Validator");
            try
            {
                var visualLibrary = root.AddComponent<ActorVisualDefinitionLibrary>();
                visualLibrary.definitions = Stage8SceneCreator.LoadDefinitions();
                visualLibrary.RebuildLookup();

                var standardLibrary = root.AddComponent<ProductionVisualStandardLibrary>();
                standardLibrary.EnsureDefaults();
                standardLibrary.RebuildLookup();

                var runner = root.AddComponent<MvpVisualQaRunner>();
                runner.definitionLibrary = visualLibrary;
                runner.standardLibrary = standardLibrary;
                runner.artistModelImportManifest = manifest;
                var reports = runner.RunAll();

                var summary = new Stage21MvpVisualQaSummary
                {
                    reports = new List<MvpVisualQaReport>(reports),
                    errors = new List<string>(),
                    warnings = new List<string>()
                };

                for (var i = 0; i < reports.Count; i++)
                {
                    var report = reports[i];
                    if (report == null)
                        continue;
                    if (report.overallStatus == MvpVisualQaStatus.Fail)
                    {
                        summary.FailCount++;
                        summary.errors.Add(report.actorTypeId + ": failed Stage 21 MVP visual QA.");
                    }
                    else if (report.overallStatus == MvpVisualQaStatus.Warning)
                    {
                        summary.WarningCount++;
                        summary.warnings.Add(report.actorTypeId + ": QA passed with warnings.");
                    }
                    else if (report.overallStatus == MvpVisualQaStatus.Pass)
                        summary.PassCount++;
                }

                return summary;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        static void WriteReport(Stage21MvpVisualQaSummary summary)
        {
            var path = Path.Combine(Stage8ActorCatalog.RepoRoot, QaMarkdownPath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, BuildMarkdown(summary), Encoding.UTF8);
        }

        static string BuildMarkdown(Stage21MvpVisualQaSummary summary)
        {
            var builder = new StringBuilder();
            builder.AppendLine("# Stage 21 MVP Visual QA");
            builder.AppendLine();
            builder.AppendLine("Stage 21 validates the MVP production proxy prefabs as player-facing, 360-degree tabletop miniatures and confirms they are ready for one-at-a-time artist model replacement.");
            builder.AppendLine();
            builder.AppendLine("- MVP actors checked: " + summary.reports.Count);
            builder.AppendLine("- Passed: " + summary.PassCount);
            builder.AppendLine("- Passed with warnings: " + summary.WarningCount);
            builder.AppendLine("- Failed: " + summary.FailCount);
            builder.AppendLine();
            builder.AppendLine("## Per Actor QA");
            for (var i = 0; i < summary.reports.Count; i++)
            {
                var report = summary.reports[i];
                builder.AppendLine("### " + report.actorTypeId);
                builder.AppendLine();
                builder.AppendLine("- Status: " + report.overallStatus);
                builder.AppendLine("- Visual tier: " + report.visualTier);
                builder.AppendLine("- Mesh objects: " + report.meshObjectCount);
                builder.AppendLine("- Materials: " + report.materialCount);
                builder.AppendLine("- Sockets: " + report.socketCount);
                builder.AppendLine("- Bounds center: `" + report.localBoundsCenter.ToString("F2") + "`");
                builder.AppendLine("- Bounds size: `" + report.localBoundsSize.ToString("F2") + "`");
                builder.AppendLine("- Artist import: " + report.artistImportStatus);
                builder.AppendLine();
                builder.AppendLine("| Category | Status | Result | Detail |");
                builder.AppendLine("| --- | --- | --- | --- |");
                for (var r = 0; r < report.rules.Count; r++)
                {
                    var rule = report.rules[r];
                    builder.AppendLine("| " + Escape(rule.category) + " | " + rule.status + " | " + Escape(rule.message) + " | " + Escape(rule.detail) + " |");
                }
                builder.AppendLine();
            }

            builder.AppendLine("## QA Rule Coverage");
            builder.AppendLine("- Footprint scale and fine-grid base alignment.");
            builder.AppendLine("- Pivot/origin near footprint center and base.");
            builder.AppendLine("- Top-down, side, rear, roof, and tiered silhouette readability.");
            builder.AppendLine("- Required socket completeness and animation hook readiness.");
            builder.AppendLine("- LOD/performance, material count, and mesh object count budget.");
            builder.AppendLine("- Fallback safety and active production proxy assignment.");
            builder.AppendLine("- Player-facing rendered volume.");
            builder.AppendLine("- Artist replacement metadata and import scan status.");
            builder.AppendLine();
            builder.AppendLine("## Stage 29 Realistic Battlefield Addendum");
            builder.AppendLine();
            builder.AppendLine("Stage 29 keeps the Stage 20/21 socket, pivot, LOD, fallback, and artist replacement metadata contract intact while adding an additive material/detail pass for realistic battlefield readability.");
            builder.AppendLine();
            builder.AppendLine("- MVP proxies keep their existing descriptors, sockets, production visual validation tags, LOD tags, and Stage 6/7/9 gameplay-facing hooks.");
            builder.AppendLine("- Added detail must improve grounding, roof identity, top silhouettes, and front/side/rear cues without changing gameplay scale or deterministic `Rts.Core` data.");
            builder.AppendLine("- The Stage 29 review scene and screenshot capture validate these visual upgrades beside terrain, resource, foundation, lighting, and fine-grid material samples.");
            return builder.ToString();
        }

        static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            return value.Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
        }
    }

    public sealed class Stage21MvpVisualQaSummary
    {
        public List<MvpVisualQaReport> reports;
        public List<string> errors;
        public List<string> warnings;
        public int PassCount;
        public int WarningCount;
        public int FailCount;
    }
}
