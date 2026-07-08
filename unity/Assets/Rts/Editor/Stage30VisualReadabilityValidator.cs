using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Art.Production;
using ProjectAegisRTS.UnityClient.Rendering.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage30VisualReadabilityValidator
    {
        public const string ReadabilityReportPath = "docs/STAGE30_VISUAL_READABILITY_QA.md";

        [MenuItem("ProjectAegisRTS/Stage 30/Validate Visual Readability QA")]
        public static void ValidateStage30VisualReadabilityMenu()
        {
            ValidateStage30VisualReadability();
        }

        public static void ValidateStage30VisualReadabilityBatch()
        {
            try
            {
                ValidateStage30VisualReadability();
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

        public static Stage30ReadabilitySummary ValidateStage30VisualReadability()
        {
            var summary = new Stage30ReadabilitySummary();
            Stage29VisualQaValidator.ValidateStage29VisualQa();
            Stage30VisualReadabilityAssetCreator.EnsureStage30Assets();
            ValidateDocsAndTools(summary);
            ValidateReadabilityProfile(summary);
            ValidateProxyReadability(summary);
            ValidateReviewScene(summary);
            WriteReport(summary);

            if (summary.Errors.Count > 0)
                throw new InvalidOperationException("Stage 30 visual readability validation failed: " + string.Join(" | ", summary.Errors.ToArray()));

            Debug.Log("Stage 30 visual readability validation passed. Proxy overlays: " + summary.ProxyReadabilityCount);
            return summary;
        }

        static void ValidateDocsAndTools(Stage30ReadabilitySummary summary)
        {
            var repoRoot = Stage8ActorCatalog.RepoRoot;
            RequireFile(summary, repoRoot, "docs", "STAGE30_REPORT.md");
            RequireFile(summary, repoRoot, "docs", "STAGE30_VISUAL_READABILITY_QA.md");
            RequireFile(summary, repoRoot, "docs", "STAGE30_KNOWN_VISUAL_LIMITATIONS.md");
            RequireFile(summary, repoRoot, "tools", "run-unity-stage30-validation.ps1");
            RequireFile(summary, repoRoot, "tools", "run-stage30-fast-checks.ps1");
            RequireFile(summary, repoRoot, "tools", "run-stage30-medium-checks.ps1");
            RequireFile(summary, repoRoot, "tools", "run-stage30-player-facing-checks.ps1");
            RequireFile(summary, repoRoot, "tools", "run-stage30-checks.ps1");
        }

        static void ValidateReadabilityProfile(Stage30ReadabilitySummary summary)
        {
            var profile = Stage30VisualReadabilityAssetCreator.LoadReadabilityProfile();
            if (profile == null)
            {
                summary.Errors.Add("Stage 30 readability profile is missing.");
                return;
            }

            if (profile.minimumActorTerrainLuminanceDelta < 0.18f)
                summary.Errors.Add("Actor/terrain luminance delta threshold is too low.");
            if (profile.maximumFineGridDominance > 0.55f)
                summary.Errors.Add("Fine-grid dominance threshold would overpower terrain.");
            if (profile.minimumResourceAccentLuminance < 0.42f)
                summary.Errors.Add("Resource accent luminance threshold is too low.");
        }

        static void ValidateProxyReadability(Stage30ReadabilitySummary summary)
        {
            var specs = Stage8ActorCatalog.LoadSpecs();
            for (var i = 0; i < Stage20MvpVisualActorSet.ActorTypeIds.Length; i++)
            {
                var actorTypeId = Stage20MvpVisualActorSet.ActorTypeIds[i];
                var spec = FindSpec(specs, actorTypeId);
                if (spec == null)
                {
                    summary.Errors.Add(actorTypeId + ": Stage 8 actor spec missing.");
                    continue;
                }

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Stage20MvpProductionProxyGenerator.ProductionProxyPath(spec));
                if (prefab == null)
                {
                    summary.Errors.Add(actorTypeId + ": production proxy prefab missing.");
                    continue;
                }

                var stage29Tag = prefab.GetComponentInChildren<Stage29VisualDetailTag>(true);
                if (stage29Tag == null || !stage29Tag.IsComplete())
                    summary.Errors.Add(actorTypeId + ": Stage 29 detail tag missing after Stage 30 readability pass.");

                var stage30Tag = prefab.GetComponentInChildren<Stage30VisualReadabilityTag>(true);
                if (stage30Tag == null || !stage30Tag.IsComplete())
                    summary.Errors.Add(actorTypeId + ": Stage 30 readability tag is missing or incomplete.");

                if (prefab.transform.Find(Stage30VisualReadabilityAssetCreator.ReadabilityLayerName) == null)
                    summary.Errors.Add(actorTypeId + ": Stage 30 readability layer missing.");

                var renderers = prefab.GetComponentsInChildren<Renderer>(true);
                if (!HasStage30Material(renderers))
                    summary.Errors.Add(actorTypeId + ": Stage 30 readability materials did not reach the prefab.");

                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab) > 0)
                    summary.Errors.Add(actorTypeId + ": prefab contains missing scripts.");

                summary.ProxyReadabilityCount++;
                summary.ProxyReports.Add(actorTypeId + ": renderers=" + renderers.Length + ", stage30=" + (stage30Tag != null));
            }
        }

        static void ValidateReviewScene(Stage30ReadabilitySummary summary)
        {
            if (!File.Exists(Path.Combine(Stage8ActorCatalog.RepoRoot, "unity", Stage30SceneCreator.ScenePath)))
                Stage30SceneCreator.CreateOrUpdateStage30Scene();

            var scene = EditorSceneManager.OpenScene(Stage30SceneCreator.ScenePath);
            if (!scene.IsValid())
            {
                summary.Errors.Add("Stage 30 readability scene did not open.");
                return;
            }

            var controller = Require<Stage29BattlefieldVisualReviewController>(summary, "Stage29BattlefieldVisualReviewController");
            Require<Stage30VisualReadabilityHud>(summary, "Stage30VisualReadabilityHud");
            Require<BattlefieldMaterialLibrary>(summary, "BattlefieldMaterialLibrary");
            Require<TerrainMaterialProfileLibrary>(summary, "TerrainMaterialProfileLibrary");
            Require<LightingProfileApplier>(summary, "LightingProfileApplier");
            Require<BattlefieldAtmosphereController>(summary, "BattlefieldAtmosphereController");

            if (controller != null)
            {
                controller.EnsureReviewScene();
                if (controller.TerrainTileCount < 160)
                    summary.Errors.Add("Stage 30 review scene terrain board is too small.");
                if (controller.ActorProxyCount != Stage20MvpVisualActorSet.ActorTypeIds.Length)
                    summary.Errors.Add("Stage 30 review scene does not display all MVP proxies.");
                if (controller.FineGridLineCount < 40)
                    summary.Errors.Add("Stage 30 fine-grid readability guidance is missing.");
            }

            var camera = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (camera == null || !camera.orthographic)
                summary.Errors.Add("Stage 30 review scene requires an orthographic review camera.");
            else if (camera.orthographicSize < 7.8f || camera.orthographicSize > 9.4f)
                summary.Errors.Add("Stage 30 review camera orthographic size is outside readable PC review range.");
        }

        static bool HasStage30Material(Renderer[] renderers)
        {
            for (var i = 0; i < renderers.Length; i++)
            {
                var sharedMaterials = renderers[i].sharedMaterials;
                for (var m = 0; m < sharedMaterials.Length; m++)
                {
                    var material = sharedMaterials[m];
                    if (material != null && material.name.IndexOf("stage30", StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
            }

            return false;
        }

        static T Require<T>(Stage30ReadabilitySummary summary, string label) where T : Component
        {
            var active = UnityEngine.Object.FindFirstObjectByType<T>();
            if (active != null)
                return active;

            var all = Resources.FindObjectsOfTypeAll<T>();
            for (var i = 0; i < all.Length; i++)
                if (all[i] != null && all[i].gameObject != null && all[i].gameObject.scene.IsValid())
                    return all[i];

            summary.Errors.Add("Missing Stage 30 scene component: " + label);
            return null;
        }

        static Stage8ActorSpec FindSpec(List<Stage8ActorSpec> specs, string actorTypeId)
        {
            for (var i = 0; i < specs.Count; i++)
                if (specs[i].ActorTypeId == actorTypeId)
                    return specs[i];
            return null;
        }

        static void RequireFile(Stage30ReadabilitySummary summary, string repoRoot, string folder, string fileName)
        {
            var path = Path.Combine(repoRoot, folder, fileName);
            if (!File.Exists(path))
                summary.Errors.Add("Missing required Stage 30 file: " + path);
        }

        static void WriteReport(Stage30ReadabilitySummary summary)
        {
            var path = Path.Combine(Stage8ActorCatalog.RepoRoot, ReadabilityReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, BuildMarkdown(summary), Encoding.UTF8);
        }

        static string BuildMarkdown(Stage30ReadabilitySummary summary)
        {
            var builder = new StringBuilder();
            builder.AppendLine("# Stage 30 Visual Readability QA");
            builder.AppendLine();
            builder.AppendLine("Stage 30 validates top-down readability after the Stage 29 realistic battlefield pass.");
            builder.AppendLine();
            builder.AppendLine("- MVP proxy readability overlays: " + summary.ProxyReadabilityCount + "/" + Stage20MvpVisualActorSet.ActorTypeIds.Length);
            builder.AppendLine("- Errors: " + summary.Errors.Count);
            builder.AppendLine("- Warnings: " + summary.Warnings.Count);
            builder.AppendLine();
            builder.AppendLine("## Proxy Readability");
            AppendList(builder, summary.ProxyReports);
            builder.AppendLine();
            builder.AppendLine("## Errors");
            AppendList(builder, summary.Errors);
            builder.AppendLine();
            builder.AppendLine("## Warnings");
            AppendList(builder, summary.Warnings);
            builder.AppendLine();
            builder.AppendLine("## Coverage");
            builder.AppendLine("- Actor/terrain separation through dark grounding cuts and top identity accents.");
            builder.AppendLine("- Resource readability through refinery/harvester resource pops.");
            builder.AppendLine("- Combat role readability through compact red role accents.");
            builder.AppendLine("- Stage 29 visual details, sockets, pivots, and production proxy metadata are preserved.");
            builder.AppendLine("- Review scene verifies board, material, proxy, camera, lighting, atmosphere, and HUD readability.");
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

    public sealed class Stage30ReadabilitySummary
    {
        public int ProxyReadabilityCount;
        public readonly List<string> ProxyReports = new List<string>();
        public readonly List<string> Errors = new List<string>();
        public readonly List<string> Warnings = new List<string>();
    }
}
