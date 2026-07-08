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
    public static class Stage29VisualQaValidator
    {
        public const string VisualQaReportPath = "docs/STAGE29_VISUAL_QA_REPORT.md";

        [MenuItem("ProjectAegisRTS/Stage 29/Validate Battlefield Visual QA")]
        public static void ValidateStage29VisualQaMenu()
        {
            ValidateStage29VisualQa();
        }

        public static void ValidateStage29VisualQaBatch()
        {
            try
            {
                ValidateStage29VisualQa();
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

        public static Stage29VisualQaSummary ValidateStage29VisualQa()
        {
            var summary = new Stage29VisualQaSummary();
            Stage29BattlefieldVisualAssetCreator.EnsureStage29Assets();
            ValidateDocsAndTools(summary);
            ValidateTerrainProfiles(summary);
            ValidateMvpProxyDetails(summary);
            ValidateReviewScene(summary);
            WriteReport(summary);

            if (summary.Errors.Count > 0)
                throw new InvalidOperationException("Stage 29 visual QA validation failed: " + string.Join(" | ", summary.Errors.ToArray()));

            Debug.Log("Stage 29 visual QA validation passed. Terrain profiles: " + summary.TerrainProfileCount + ", MVP detail prefabs: " + summary.ProxyDetailCount);
            return summary;
        }

        static void ValidateDocsAndTools(Stage29VisualQaSummary summary)
        {
            var repoRoot = Stage8ActorCatalog.RepoRoot;
            RequireFile(summary, repoRoot, "docs", "STAGE29_REPORT.md");
            RequireFile(summary, repoRoot, "docs", "STAGE29_REALISTIC_BATTLEFIELD_VISUAL_DIRECTION.md");
            RequireFile(summary, repoRoot, "docs", "STAGE29_VISUAL_QA_REPORT.md");
            RequireFile(summary, repoRoot, "docs", "STAGE29_TERRAIN_MATERIALS.md");
            RequireFile(summary, repoRoot, "docs", "STAGE29_KNOWN_VISUAL_LIMITATIONS.md");
            RequireFile(summary, repoRoot, "tools", "run-unity-stage29-validation.ps1");
            RequireFile(summary, repoRoot, "tools", "run-stage29-fast-checks.ps1");
            RequireFile(summary, repoRoot, "tools", "run-stage29-medium-checks.ps1");
            RequireFile(summary, repoRoot, "tools", "run-stage29-player-facing-checks.ps1");
            RequireFile(summary, repoRoot, "tools", "run-stage29-checks.ps1");

            var audit = File.ReadAllText(Path.Combine(repoRoot, "tools", "audit-medium-validation-recursion.ps1"));
            if (!audit.Contains("run-stage29-medium-checks.ps1") ||
                !audit.Contains("run-unity-stage29-validation.ps1") ||
                !audit.Contains("run-stage29-player-facing-checks.ps1"))
                summary.Errors.Add("Medium recursion audit does not include Stage 29 direct dependency coverage.");
        }

        static void ValidateTerrainProfiles(Stage29VisualQaSummary summary)
        {
            for (var i = 0; i < Stage29BattlefieldVisualAssetCreator.RequiredTerrainKinds.Length; i++)
            {
                var terrainKind = Stage29BattlefieldVisualAssetCreator.RequiredTerrainKinds[i];
                var profile = AssetDatabase.LoadAssetAtPath<TerrainMaterialProfile>(Stage29BattlefieldVisualAssetCreator.TerrainProfilePath(terrainKind));
                if (profile == null)
                {
                    summary.Errors.Add("Missing Stage 29 terrain profile: " + terrainKind);
                    continue;
                }

                if (profile.material == null)
                    summary.Errors.Add(terrainKind + ": terrain profile has no material assigned.");
                if (string.IsNullOrEmpty(profile.readabilityNotes))
                    summary.Warnings.Add(terrainKind + ": terrain profile is missing readability notes.");
                if (terrainKind == "ResourceField" && !profile.supportsResources)
                    summary.Errors.Add("ResourceField profile must mark supportsResources.");
                if (terrainKind == "RockBlocked" && !profile.blocksGroundMovement)
                    summary.Errors.Add("RockBlocked profile must mark blocksGroundMovement.");
                if (terrainKind == "Water" && !profile.blocksGroundMovement)
                    summary.Errors.Add("Water profile must mark ground-blocking for visual QA.");
                summary.TerrainProfileCount++;
            }
        }

        static void ValidateMvpProxyDetails(Stage29VisualQaSummary summary)
        {
            Stage21MvpVisualQaValidator.ValidateMvpVisualQa(false);

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
                    summary.Errors.Add(actorTypeId + ": Stage 20 production proxy prefab missing.");
                    continue;
                }

                ValidatePrefab(actorTypeId, spec, prefab, summary);
            }
        }

        static void ValidatePrefab(string actorTypeId, Stage8ActorSpec spec, GameObject prefab, Stage29VisualQaSummary summary)
        {
            var detailTag = prefab.GetComponentInChildren<Stage29VisualDetailTag>(true);
            if (detailTag == null || !detailTag.IsComplete())
                summary.Errors.Add(actorTypeId + ": Stage29VisualDetailTag is missing or incomplete.");

            var descriptor = prefab.GetComponentInChildren<ActorPrefabDescriptor>(true);
            if (descriptor == null)
                summary.Errors.Add(actorTypeId + ": ActorPrefabDescriptor missing after Stage 29 detail pass.");
            else
            {
                var required = Stage8ActorCatalog.RequiredSocketsFor(spec);
                if (actorTypeId == "refinery" && !required.Contains(ActorPrefabSocketKind.DockPumpRoot))
                    required.Add(ActorPrefabSocketKind.DockPumpRoot);
                var missing = descriptor.ValidateRequiredSockets(required);
                if (missing.Count > 0)
                    summary.Errors.Add(actorTypeId + ": Stage 29 detail pass lost sockets " + string.Join(", ", missing.ToArray()));
            }

            var productionTag = prefab.GetComponentInChildren<ProductionVisualValidationTag>(true);
            if (productionTag == null)
                summary.Errors.Add(actorTypeId + ": ProductionVisualValidationTag missing after Stage 29 detail pass.");
            else if ((productionTag.ViewCoverage & ProductionVisualViewCoverage.AllAround) != ProductionVisualViewCoverage.AllAround)
                summary.Errors.Add(actorTypeId + ": all-around view coverage regressed.");

            if (prefab.GetComponentInChildren<LODGroup>(true) == null)
                summary.Errors.Add(actorTypeId + ": LODGroup missing after Stage 29 detail pass.");

            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length < 8)
                summary.Errors.Add(actorTypeId + ": visual still reads too sparse for Stage 29. Renderer count=" + renderers.Length);
            if (!HasStage29Material(renderers))
                summary.Errors.Add(actorTypeId + ": Stage 29 material pass did not reach the prefab.");
            if (prefab.transform.Find(Stage29BattlefieldVisualAssetCreator.DetailRootName) == null)
                summary.Errors.Add(actorTypeId + ": additive Stage 29 detail root missing.");
            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab) > 0)
                summary.Errors.Add(actorTypeId + ": prefab contains missing scripts.");

            int rendererCount;
            int materialCount;
            var bounds = MvpVisualQaRunner.CalculateLocalBounds(prefab, out rendererCount, out materialCount);
            if (bounds.size.y < 0.20f)
                summary.Errors.Add(actorTypeId + ": Stage 29 visual bounds are too flat: " + bounds.size.ToString("F2"));

            summary.ProxyDetailCount++;
            summary.ProxyReports.Add(actorTypeId + ": renderers=" + rendererCount + ", materials=" + materialCount + ", bounds=" + bounds.size.ToString("F2"));
        }

        static bool HasStage29Material(Renderer[] renderers)
        {
            for (var i = 0; i < renderers.Length; i++)
            {
                var sharedMaterials = renderers[i].sharedMaterials;
                for (var m = 0; m < sharedMaterials.Length; m++)
                {
                    var material = sharedMaterials[m];
                    if (material != null && material.name.IndexOf("stage29", StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
            }

            return false;
        }

        static void ValidateReviewScene(Stage29VisualQaSummary summary)
        {
            if (!File.Exists(Path.Combine(Stage8ActorCatalog.RepoRoot, "unity", Stage29SceneCreator.ScenePath)))
                Stage29SceneCreator.CreateOrUpdateStage29Scene();

            var scene = EditorSceneManager.OpenScene(Stage29SceneCreator.ScenePath);
            if (!scene.IsValid())
            {
                summary.Errors.Add("Stage 29 review scene did not open.");
                return;
            }

            var controller = Require<Stage29BattlefieldVisualReviewController>(summary, "Stage29BattlefieldVisualReviewController");
            Require<BattlefieldMaterialLibrary>(summary, "BattlefieldMaterialLibrary");
            Require<TerrainMaterialProfileLibrary>(summary, "TerrainMaterialProfileLibrary");
            Require<LightingProfileApplier>(summary, "LightingProfileApplier");
            Require<BattlefieldAtmosphereController>(summary, "BattlefieldAtmosphereController");
            Require<Stage29VisualQaHud>(summary, "Stage29VisualQaHud");

            if (controller != null)
            {
                controller.EnsureReviewScene();
                if (controller.TerrainTileCount < 160)
                    summary.Errors.Add("Stage 29 review scene terrain board is too small.");
                if (controller.ActorProxyCount != Stage20MvpVisualActorSet.ActorTypeIds.Length)
                    summary.Errors.Add("Stage 29 review scene does not display all MVP proxies.");
                if (controller.MaterialSwatchCount < Stage29BattlefieldVisualAssetCreator.RequiredTerrainKinds.Length)
                    summary.Errors.Add("Stage 29 review scene material swatches are incomplete.");
                if (controller.FineGridLineCount < 40)
                    summary.Errors.Add("Stage 29 fine placement-grid guidance is missing or too sparse.");
            }
        }

        static T Require<T>(Stage29VisualQaSummary summary, string label) where T : Component
        {
            var active = UnityEngine.Object.FindFirstObjectByType<T>();
            if (active != null)
                return active;

            var all = Resources.FindObjectsOfTypeAll<T>();
            for (var i = 0; i < all.Length; i++)
                if (all[i] != null && all[i].gameObject != null && all[i].gameObject.scene.IsValid())
                    return all[i];

            summary.Errors.Add("Missing Stage 29 scene component: " + label);
            return null;
        }

        static Stage8ActorSpec FindSpec(List<Stage8ActorSpec> specs, string actorTypeId)
        {
            for (var i = 0; i < specs.Count; i++)
                if (specs[i].ActorTypeId == actorTypeId)
                    return specs[i];
            return null;
        }

        static void RequireFile(Stage29VisualQaSummary summary, string repoRoot, string folder, string fileName)
        {
            var path = Path.Combine(repoRoot, folder, fileName);
            if (!File.Exists(path))
                summary.Errors.Add("Missing required Stage 29 file: " + path);
        }

        static void WriteReport(Stage29VisualQaSummary summary)
        {
            var path = Path.Combine(Stage8ActorCatalog.RepoRoot, VisualQaReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, BuildMarkdown(summary), Encoding.UTF8);
        }

        static string BuildMarkdown(Stage29VisualQaSummary summary)
        {
            var builder = new StringBuilder();
            builder.AppendLine("# Stage 29 Visual QA Report");
            builder.AppendLine();
            builder.AppendLine("Stage 29 validates the realistic battlefield material pass, terrain/material profile coverage, MVP proxy detail pass, and visual review scene.");
            builder.AppendLine();
            builder.AppendLine("- Terrain profiles: " + summary.TerrainProfileCount + "/" + Stage29BattlefieldVisualAssetCreator.RequiredTerrainKinds.Length);
            builder.AppendLine("- MVP proxy detail prefabs: " + summary.ProxyDetailCount + "/" + Stage20MvpVisualActorSet.ActorTypeIds.Length);
            builder.AppendLine("- Errors: " + summary.Errors.Count);
            builder.AppendLine("- Warnings: " + summary.Warnings.Count);
            builder.AppendLine();
            builder.AppendLine("## MVP Proxy Detail");
            AppendList(builder, summary.ProxyReports);
            builder.AppendLine();
            builder.AppendLine("## Errors");
            AppendList(builder, summary.Errors);
            builder.AppendLine();
            builder.AppendLine("## Warnings");
            AppendList(builder, summary.Warnings);
            builder.AppendLine();
            builder.AppendLine("## Validation Coverage");
            builder.AppendLine("- Terrain profiles for grass/dirt, compacted base, concrete pad, road/path, resource field, rock/blocked, water, and fog/explored.");
            builder.AppendLine("- Stage 29 material assignment on every MVP production proxy.");
            builder.AppendLine("- Additive silhouette, top-profile, grounding, and front/side/rear details without removing Stage 20 sockets.");
            builder.AppendLine("- Stage 21 MVP visual QA still passes after the detail pass.");
            builder.AppendLine("- Review scene contains terrain board, fine-grid guidance, material swatches, lighting/atmosphere, and all MVP proxies.");
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

    public sealed class Stage29VisualQaSummary
    {
        public int TerrainProfileCount;
        public int ProxyDetailCount;
        public readonly List<string> ProxyReports = new List<string>();
        public readonly List<string> Errors = new List<string>();
        public readonly List<string> Warnings = new List<string>();
    }
}
