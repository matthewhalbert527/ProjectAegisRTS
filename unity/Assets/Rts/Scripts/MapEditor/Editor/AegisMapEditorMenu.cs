#if UNITY_EDITOR
using System.IO;
using ProjectAegisRTS.UnityClient.MapEditor;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class AegisMapEditorMenu
    {
        [MenuItem("Project Aegis/Map Editor/Open Map Editor")]
        public static void OpenMapEditor()
        {
            EnsureFolders();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(AegisMapEditorPaths.MapEditorRoot);
            EditorUtility.DisplayDialog("Project Aegis Map Editor", "Map-editor folders and samples are ready under Assets/Rts/MapEditor.", "OK");
        }

        [MenuItem("Project Aegis/Map Editor/Import Tiled JSON as Aegis Map")]
        public static void ImportTiledJsonAsAegisMap()
        {
            EnsureFolders();
            var sourcePath = EditorUtility.OpenFilePanel("Import Tiled JSON", Application.dataPath, "json");
            if (string.IsNullOrEmpty(sourcePath))
                return;

            var mapId = Path.GetFileNameWithoutExtension(sourcePath).Replace(".tiled", string.Empty);
            var targetPath = EditorUtility.SaveFilePanelInProject(
                "Save Aegis Map",
                mapId + ".aegismap",
                "json",
                "Choose where to save the runtime Aegis map document.",
                AegisMapEditorPaths.SamplesFolder);
            if (string.IsNullOrEmpty(targetPath))
                return;

            var tiledJson = File.ReadAllText(sourcePath);
            File.WriteAllText(targetPath, AegisMapEditorFileTemplates.CreateAegisMapShellFromTiledJson(tiledJson, mapId));
            AssetDatabase.ImportAsset(targetPath);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(targetPath);
            Debug.Log("Imported Tiled JSON shell to Aegis map: " + targetPath + ". Use Rts.Core importer tests for deterministic conversion.");
        }

        [MenuItem("Project Aegis/Map Editor/Export Selected Aegis Map to Tiled JSON")]
        public static void ExportSelectedAegisMapToTiledJson()
        {
            EnsureFolders();
            var selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(selectedPath) || !selectedPath.EndsWith(".aegismap.json"))
            {
                EditorUtility.DisplayDialog("Export Aegis Map", "Select a .aegismap.json asset first.", "OK");
                return;
            }

            var mapId = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(selectedPath));
            var targetPath = EditorUtility.SaveFilePanelInProject(
                "Export Tiled JSON",
                mapId + ".tiled",
                "json",
                "Choose where to save the Tiled-compatible JSON shell.",
                AegisMapEditorPaths.GeneratedMapsFolder);
            if (string.IsNullOrEmpty(targetPath))
                return;

            var aegisJson = File.ReadAllText(selectedPath);
            File.WriteAllText(targetPath, AegisMapEditorFileTemplates.CreateTiledJsonShellFromAegisMap(aegisJson, mapId));
            AssetDatabase.ImportAsset(targetPath);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(targetPath);
            Debug.Log("Exported selected Aegis map shell to Tiled JSON: " + targetPath);
        }

        [MenuItem("Project Aegis/Map Editor/Create Tiled Starter Tileset")]
        public static void CreateTiledStarterTileset()
        {
            EnsureFolders();
            var path = AegisMapEditorPaths.TiledMapsFolder + "/aegis_starter_tiles.tsx";
            File.WriteAllText(path, AegisMapEditorFileTemplates.StarterTilesetTsx());
            AssetDatabase.ImportAsset(path);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(path);
            Debug.Log("Created Tiled starter tileset: " + path);
        }

        [MenuItem("Project Aegis/Map Editor/Build Proxy Materials and Prefabs")]
        public static void BuildProxyMaterialsAndPrefabs()
        {
            EnsureFolders();
            CreateMaterial("aegis_proxy_clear.mat", new Color(0.29f, 0.42f, 0.27f));
            CreateMaterial("aegis_proxy_road.mat", new Color(0.36f, 0.34f, 0.31f));
            CreateMaterial("aegis_proxy_rough.mat", new Color(0.45f, 0.41f, 0.36f));
            CreateMaterial("aegis_proxy_forest.mat", new Color(0.16f, 0.31f, 0.18f));
            CreateMaterial("aegis_proxy_water.mat", new Color(0.12f, 0.35f, 0.48f));
            CreateMaterial("aegis_proxy_cliff.mat", new Color(0.25f, 0.25f, 0.25f));
            CreateMaterial("aegis_proxy_ore.mat", new Color(0.2f, 0.62f, 0.68f));
            CreatePrefab("aegis_map_blocker_proxy.prefab", new Vector3(1f, 0.2f, 1f));
            AssetDatabase.SaveAssets();
            Debug.Log("Built Aegis map-editor proxy materials and prefabs.");
        }

        [MenuItem("Project Aegis/Map Editor/Export Unity AI Asset Prompts")]
        public static void ExportUnityAiAssetPrompts()
        {
            EnsureFolders();
            var path = AegisMapEditorPaths.AssetPromptsFolder + "/aegis_map_editor_asset_prompts.md";
            File.WriteAllText(path, AegisMapEditorFileTemplates.UnityAiAssetPromptMarkdown());
            AssetDatabase.ImportAsset(path);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(path);
            Debug.Log("Exported Unity AI asset prompts: " + path);
        }

        static void EnsureFolders()
        {
            EnsureFolder("Assets/Rts", "MapEditor");
            EnsureFolder(AegisMapEditorPaths.MapEditorRoot, "Samples");
            EnsureFolder(AegisMapEditorPaths.MapEditorRoot, "AssetPrompts");
            EnsureFolder(AegisMapEditorPaths.MapEditorRoot, "ProxyAssets");
            EnsureFolder("Assets/Rts", "Maps");
            EnsureFolder("Assets/Rts/Maps", "Tiled");
            EnsureFolder("Assets/Rts/Maps", "Generated");
        }

        static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }

        static void CreateMaterial(string fileName, Color color)
        {
            var path = AegisMapEditorPaths.ProxyAssetsFolder + "/" + fileName;
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.color = color;
            EditorUtility.SetDirty(material);
        }

        static void CreatePrefab(string fileName, Vector3 scale)
        {
            var path = AegisMapEditorPaths.ProxyAssetsFolder + "/" + fileName;
            var proxy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            proxy.name = Path.GetFileNameWithoutExtension(fileName);
            proxy.transform.localScale = scale;
            PrefabUtility.SaveAsPrefabAsset(proxy, path);
            Object.DestroyImmediate(proxy);
        }
    }
}
#endif
