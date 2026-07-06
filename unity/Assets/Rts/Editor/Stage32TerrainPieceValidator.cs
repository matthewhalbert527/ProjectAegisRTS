using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProjectAegisRTS.UnityClient.Rendering.TerrainPieces;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage32TerrainPieceValidator
    {
        public const string VisualQaReportPath = "docs/STAGE32_VISUAL_QA_REPORT.md";

        [MenuItem("ProjectAegisRTS/Stage 32/Validate Terrain Piece Library")]
        public static void ValidateStage32TerrainPiecesMenu()
        {
            ValidateStage32TerrainPieces();
        }

        public static void ValidateStage32TerrainPiecesBatch()
        {
            try
            {
                ValidateStage32TerrainPieces();
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

        public static Stage32ValidationSummary ValidateStage32TerrainPieces()
        {
            Stage32TerrainPieceGenerator.EnsureStage32TerrainPieces();
            var summary = new Stage32ValidationSummary();
            ValidateDocsAndTools(summary);
            ValidateMaterialLibrary(summary);
            ValidatePieceLibrary(summary);
            ValidateSetDressing(summary);
            WriteReport(summary);

            if (summary.Errors.Count > 0)
                throw new InvalidOperationException("Stage 32 terrain piece validation failed: " + string.Join(" | ", summary.Errors.ToArray()));

            Debug.Log("Stage 32 terrain piece validation passed. Pieces: " + summary.PieceCount);
            return summary;
        }

        static void ValidateDocsAndTools(Stage32ValidationSummary summary)
        {
            var repoRoot = Stage8ActorCatalog.RepoRoot;
            RequireFile(summary, repoRoot, "docs", "STAGE32_REPORT.md");
            RequireFile(summary, repoRoot, "docs", "STAGE32_TERRAIN_PIECE_LIBRARY.md");
            RequireFile(summary, repoRoot, "docs", "STAGE32_SET_DRESSING_GUIDE.md");
            RequireFile(summary, repoRoot, "docs", "STAGE32_VISUAL_QA_REPORT.md");
            RequireFile(summary, repoRoot, "tools", "run-unity-stage32-validation.ps1");
            RequireFile(summary, repoRoot, "tools", "run-stage32-fast-checks.ps1");
            RequireFile(summary, repoRoot, "tools", "run-stage32-medium-checks.ps1");
            RequireFile(summary, repoRoot, "tools", "run-stage32-player-facing-checks.ps1");
            RequireFile(summary, repoRoot, "tools", "run-stage32-checks.ps1");
        }

        static void ValidateMaterialLibrary(Stage32ValidationSummary summary)
        {
            var library = Stage32TerrainPieceGenerator.LoadMaterialLibrary();
            if (library == null)
            {
                summary.Errors.Add("Stage 32 material library asset is missing.");
                return;
            }

            library.RebuildLookup();
            if (library.profiles == null || library.profiles.Count < 18)
                summary.Errors.Add("Stage 32 material library needs at least 18 material profiles.");

            for (var i = 0; i < library.profiles.Count; i++)
            {
                var profile = library.profiles[i];
                if (profile == null || string.IsNullOrEmpty(profile.profileId) || profile.material == null)
                    summary.Errors.Add("Stage 32 material profile " + i + " is incomplete.");
            }

            summary.MaterialProfileCount = library.profiles == null ? 0 : library.profiles.Count;
        }

        static void ValidatePieceLibrary(Stage32ValidationSummary summary)
        {
            var library = Stage32TerrainPieceGenerator.LoadTerrainPieceLibrary();
            if (library == null)
            {
                summary.Errors.Add("Stage 32 terrain piece library asset is missing.");
                return;
            }

            library.RebuildLookup();
            summary.PieceCount = library.Count;
            summary.GroundCount = library.CountByCategory(TerrainPieceCategory.Ground);
            summary.TransitionCount = library.CountByCategory(TerrainPieceCategory.Transition);
            summary.BaseCount = library.CountByCategory(TerrainPieceCategory.BaseConstruction);
            summary.ObstacleCount = library.CountByCategory(TerrainPieceCategory.Obstacle);
            summary.ResourceCount = library.CountByCategory(TerrainPieceCategory.Resource);
            summary.PropCount = library.CountByCategory(TerrainPieceCategory.Prop);

            if (summary.PieceCount < 64 || summary.PieceCount > 100)
                summary.Errors.Add("Stage 32 expected 64-100 terrain pieces; found " + summary.PieceCount + ".");
            if (summary.GroundCount < 12)
                summary.Errors.Add("Stage 32 ground/base terrain category is underfilled.");
            if (summary.TransitionCount < 12)
                summary.Errors.Add("Stage 32 transition category is underfilled.");
            if (summary.BaseCount < 12)
                summary.Errors.Add("Stage 32 base construction category is underfilled.");
            if (summary.ObstacleCount < 12)
                summary.Errors.Add("Stage 32 obstacle category is underfilled.");
            if (summary.ResourceCount < 8)
                summary.Errors.Add("Stage 32 resource category is underfilled.");
            if (summary.PropCount < 10)
                summary.Errors.Add("Stage 32 prop category is underfilled.");

            var definitions = library.GetDefinitions();
            var ids = new HashSet<string>();
            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                if (definition == null)
                {
                    summary.Errors.Add("Stage 32 library contains a null definition.");
                    continue;
                }

                if (!definition.IsComplete())
                    summary.Errors.Add(definition.name + ": definition is incomplete.");
                if (!ids.Add(definition.pieceId))
                    summary.Errors.Add(definition.pieceId + ": duplicate terrain piece id.");
                if (definition.prefab == null)
                {
                    summary.Errors.Add(definition.pieceId + ": prefab reference is missing.");
                    continue;
                }

                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(definition.prefab) > 0)
                    summary.Errors.Add(definition.pieceId + ": prefab contains missing scripts.");

                var tag = definition.prefab.GetComponent<TerrainPieceValidationTag>();
                if (tag == null || !tag.IsComplete())
                    summary.Errors.Add(definition.pieceId + ": TerrainPieceValidationTag missing or incomplete.");
                else if (tag.pieceId != definition.pieceId || tag.category != definition.category)
                    summary.Errors.Add(definition.pieceId + ": prefab validation tag does not match definition.");

                var colliders = definition.prefab.GetComponentsInChildren<Collider>(true);
                if (colliders.Length > 0)
                    summary.Errors.Add(definition.pieceId + ": visual-only terrain piece should not include colliders.");

                var renderers = definition.prefab.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0)
                    summary.Errors.Add(definition.pieceId + ": terrain piece has no renderers.");
                if (renderers.Length > 14)
                    summary.Errors.Add(definition.pieceId + ": renderer count exceeds Quest-safe Stage32 budget.");
            }
        }

        static void ValidateSetDressing(Stage32ValidationSummary summary)
        {
            var profile = Stage32TerrainPieceGenerator.LoadPlayerFacingSetDressingProfile();
            var library = Stage32TerrainPieceGenerator.LoadSetDressingLibrary();
            if (profile == null || !profile.IsComplete())
            {
                summary.Errors.Add("Stage 32 player-facing set dressing profile is missing or incomplete.");
                return;
            }

            if (library == null || library.GetDefaultProfile() != profile)
                summary.Errors.Add("Stage 32 set dressing library default profile is not configured.");
            if (profile.placements.Count < 32)
                summary.Errors.Add("Stage 32 player-facing set dressing should include at least 32 placements.");
            if (profile.maxRenderedPieces > 180)
                summary.Errors.Add("Stage 32 player-facing set dressing max pieces should remain bounded for the 32x64 player-facing map.");

            summary.PlayerFacingPlacementCount = profile.placements.Count;
            ValidatePlayerFacingSourceArt(summary, profile);
        }

        static void ValidatePlayerFacingSourceArt(Stage32ValidationSummary summary, TerrainSetDressingProfile profile)
        {
            var manifest = Stage32TerrainArtIngestionGenerator.LoadManifest();
            summary.SourceArtReplacementCount = manifest == null ? 0 : manifest.CountPlayerFacingReplacements();
            if (summary.SourceArtReplacementCount == 0)
                return;

            if (summary.SourceArtReplacementCount < Stage32TerrainArtIngestionGenerator.MinimumPlayerFacingSourceReplacements)
                summary.Errors.Add("Stage 32 Batch01 source art exists, but fewer than " + Stage32TerrainArtIngestionGenerator.MinimumPlayerFacingSourceReplacements + " player-facing source replacements were generated.");

            var library = Stage32TerrainPieceGenerator.LoadTerrainPieceLibrary();
            if (library == null)
            {
                summary.Errors.Add("Stage 32 cannot validate source-art replacement because the terrain piece library is missing.");
                return;
            }

            var sourcePlacements = 0;
            var proxyPlacements = new List<string>();
            for (var i = 0; i < profile.placements.Count; i++)
            {
                var placement = profile.placements[i];
                if (placement == null || string.IsNullOrEmpty(placement.pieceId))
                    continue;

                var definition = library.GetDefinition(placement.pieceId);
                var prefab = definition != null ? definition.prefab : null;
                var tag = prefab != null ? prefab.GetComponent<TerrainArtSourceTag>() : null;
                if (tag != null && tag.IsPlayerFacingSourceArt())
                    sourcePlacements++;
                else
                    proxyPlacements.Add(placement.pieceId);
            }

            summary.PlayerFacingSourceArtPlacementCount = sourcePlacements;
            if (sourcePlacements < Stage32TerrainArtIngestionGenerator.MinimumPlayerFacingSourceReplacements)
                summary.Errors.Add("Stage 32 player-facing set dressing is not using enough imported Batch01 source-art pieces. Source placements: " + sourcePlacements + ".");
            if (proxyPlacements.Count > 0)
                summary.Errors.Add("Stage 32 player-facing set dressing still references proxy-only terrain while Batch01 source art is available: " + string.Join(", ", proxyPlacements.ToArray()));
        }

        static void RequireFile(Stage32ValidationSummary summary, string repoRoot, string folder, string fileName)
        {
            var path = Path.Combine(repoRoot, folder, fileName);
            if (!File.Exists(path))
                summary.Errors.Add("Missing required Stage 32 file: " + path);
        }

        static void WriteReport(Stage32ValidationSummary summary)
        {
            var path = Path.Combine(Stage8ActorCatalog.RepoRoot, VisualQaReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, BuildMarkdown(summary), Encoding.UTF8);
        }

        static string BuildMarkdown(Stage32ValidationSummary summary)
        {
            var builder = new StringBuilder();
            builder.AppendLine("# Stage 32 Visual QA Report");
            builder.AppendLine();
            builder.AppendLine("Stage 32 validates the modular terrain-piece library and visual-only player-facing set dressing.");
            builder.AppendLine();
            builder.AppendLine("- Terrain pieces: " + summary.PieceCount);
            builder.AppendLine("- Material profiles: " + summary.MaterialProfileCount);
            builder.AppendLine("- Player-facing placements: " + summary.PlayerFacingPlacementCount);
            builder.AppendLine("- Batch01 source-art replacements: " + summary.SourceArtReplacementCount);
            builder.AppendLine("- Player-facing source-art placements: " + summary.PlayerFacingSourceArtPlacementCount);
            builder.AppendLine("- Ground: " + summary.GroundCount);
            builder.AppendLine("- Transitions: " + summary.TransitionCount);
            builder.AppendLine("- Base construction: " + summary.BaseCount);
            builder.AppendLine("- Obstacles: " + summary.ObstacleCount);
            builder.AppendLine("- Resources: " + summary.ResourceCount);
            builder.AppendLine("- Props: " + summary.PropCount);
            builder.AppendLine("- Errors: " + summary.Errors.Count);
            builder.AppendLine();
            builder.AppendLine("## Boundary");
            builder.AppendLine("- Terrain pieces are Unity-side visual metadata and prefabs.");
            builder.AppendLine("- `Rts.Core` terrain, passability, resources, and building placement remain authoritative.");
            builder.AppendLine("- Prefabs have no colliders and are rendered under the Stage32 set-dressing root.");
            builder.AppendLine();
            builder.AppendLine("## Errors");
            if (summary.Errors.Count == 0)
                builder.AppendLine("- None");
            else
                for (var i = 0; i < summary.Errors.Count; i++)
                    builder.AppendLine("- " + summary.Errors[i]);
            return builder.ToString();
        }
    }

    public sealed class Stage32ValidationSummary
    {
        public int PieceCount;
        public int MaterialProfileCount;
        public int PlayerFacingPlacementCount;
        public int SourceArtReplacementCount;
        public int PlayerFacingSourceArtPlacementCount;
        public int GroundCount;
        public int TransitionCount;
        public int BaseCount;
        public int ObstacleCount;
        public int ResourceCount;
        public int PropCount;
        public readonly List<string> Errors = new List<string>();
    }
}
