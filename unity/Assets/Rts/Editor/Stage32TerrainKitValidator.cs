using System.IO;
using ProjectAegisRTS.UnityClient.Rendering.Terrain;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage32TerrainKitValidator
    {
        private const string PrefabFolder = "Assets/Rts/Art/Prefabs/Terrain/Stage32Generated";
        private const string ScenePath = "Assets/Rts/Scenes/Stage32_TerrainAssetReplacementReview.unity";
        private const string ReportFileName = "STAGE32_TERRAIN_QA_REPORT.md";

        [MenuItem("ProjectAegisRTS/Stage 32/Validate Terrain Kit")]
        public static void ValidateTerrainKitMenu() => ValidateTerrainKitBatch();

        public static void ValidateTerrainKitBatch()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabFolder });
            System.Array.Sort(guids);
            var failures = 0;
            var warnings = 0;
            var reportPath = GetRepoDocsPath(ReportFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
            using (var writer = new StreamWriter(reportPath, false))
            {
                writer.WriteLine("# Stage 32 Terrain QA Report");
                writer.WriteLine();
                writer.WriteLine($"Prefab count: {guids.Length}");
                writer.WriteLine();
                writer.WriteLine("| Prefab | Result | Notes |");
                writer.WriteLine("|---|---|---|");
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    var tag = prefab ? prefab.GetComponent<Stage32TerrainPieceTag>() : null;
                    if (!prefab || tag == null)
                    {
                        failures++;
                        writer.WriteLine($"| `{path}` | FAIL | Missing prefab or Stage32TerrainPieceTag. |");
                        continue;
                    }
                    var renderers = prefab.GetComponentsInChildren<MeshRenderer>();
                    if (renderers.Length == 0)
                    {
                        failures++;
                        writer.WriteLine($"| `{path}` | FAIL | No MeshRenderer children. |");
                        continue;
                    }
                    var lod = prefab.GetComponent<LODGroup>();
                    if (!lod) warnings++;
                    var notes = lod ? "OK" : "Warning: no LODGroup";
                    writer.WriteLine($"| `{tag.terrainId}` | PASS | {notes}; category={tag.category}; fine={tag.fineGridSize.x}x{tag.fineGridSize.y}; meshObjects={renderers.Length}. |");
                }
                if (!File.Exists(ScenePath))
                {
                    warnings++;
                    writer.WriteLine($"| `{ScenePath}` | WARN | Review scene file not found on disk; regenerate kit. |");
                }
                writer.WriteLine();
                writer.WriteLine($"Failures: {failures}");
                writer.WriteLine($"Warnings: {warnings}");
            }
            AssetDatabase.Refresh();
            if (failures > 0) throw new System.Exception($"Stage32 terrain validation failed with {failures} failures. See docs/{ReportFileName}");
            Debug.Log($"Stage32 terrain validation passed with {warnings} warnings. Report: docs/{ReportFileName}");
        }

        private static string GetRepoDocsPath(string fileName)
        {
            var unityRoot = Directory.GetParent(Application.dataPath);
            var repoRoot = unityRoot?.Parent;
            if (repoRoot == null)
                return Path.Combine("docs", fileName);
            return Path.Combine(repoRoot.FullName, "docs", fileName);
        }
    }
}
