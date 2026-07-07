#if UNITY_EDITOR
using System;
using System.IO;
using ProjectAegisRTS.UnityClient.MapEditor;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public sealed class AegisMapEditorWindow : EditorWindow
    {
        static readonly string[] SizePresets = { "small", "medium", "large", "custom" };
        static readonly string[] PlayerCounts = { "2", "4", "6", "8" };
        static readonly string[] Biomes = { "grassland", "desert", "tundra", "volcanic", "rocky", "forest", "wasteland" };
        static readonly string[] ResourceDensities = { "very low", "low", "medium", "high", "very high" };
        static readonly string[] CliffDensities = { "none", "low", "medium", "high", "extreme" };
        static readonly string[] RockinessValues = { "none", "low", "medium", "high", "extreme" };
        static readonly string[] WaterAmounts = { "none", "low", "medium", "high" };
        static readonly string[] Symmetries = { "none", "horizontal", "vertical", "rotational", "radial" };
        static readonly string[] GameplayProfiles = { "open", "balanced", "chokepoint", "defensive", "resource-rich", "scarce", "tournament" };

        string prompt = "large rocky map with high cliffs and lots of ore";
        int sizePreset;
        int customWidth = 100;
        int customHeight = 100;
        int playerCountIndex;
        int biome;
        int resourceDensity = 2;
        int cliffDensity = 1;
        int rockiness = 1;
        int waterAmount;
        int symmetry = 1;
        int seed = 1337;
        int gameplayProfile = 1;
        bool oreRegenerationEnabled = true;
        int oreRegenerationRate = 2;
        string lastGeneratedJson = string.Empty;
        string status = string.Empty;
        Vector2 scroll;

        public static void Open()
        {
            var window = GetWindow<AegisMapEditorWindow>("Aegis Map Editor");
            window.minSize = new Vector2(520, 520);
            window.Show();
        }

        void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("AI / Procedural Generate", EditorStyles.boldLabel);
            prompt = EditorGUILayout.TextField("Prompt", prompt);
            sizePreset = EditorGUILayout.Popup("Size Preset", sizePreset, SizePresets);
            if (SizePresets[sizePreset] == "custom")
            {
                customWidth = EditorGUILayout.IntField("Width", customWidth);
                customHeight = EditorGUILayout.IntField("Height", customHeight);
            }
            playerCountIndex = EditorGUILayout.Popup("Player Count", playerCountIndex, PlayerCounts);
            biome = EditorGUILayout.Popup("Biome", biome, Biomes);
            resourceDensity = EditorGUILayout.Popup("Resources", resourceDensity, ResourceDensities);
            cliffDensity = EditorGUILayout.Popup("Cliffs", cliffDensity, CliffDensities);
            rockiness = EditorGUILayout.Popup("Rockiness", rockiness, RockinessValues);
            waterAmount = EditorGUILayout.Popup("Water", waterAmount, WaterAmounts);
            symmetry = EditorGUILayout.Popup("Symmetry", symmetry, Symmetries);
            seed = EditorGUILayout.IntField("Seed", seed);
            gameplayProfile = EditorGUILayout.Popup("Profile", gameplayProfile, GameplayProfiles);
            oreRegenerationEnabled = EditorGUILayout.Toggle("Ore Regeneration", oreRegenerationEnabled);
            oreRegenerationRate = EditorGUILayout.IntSlider("Ore Regen Rate", oreRegenerationRate, 0, 20);

            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Preview"))
                GeneratePreview();
            if (GUILayout.Button("Validate Map"))
                ValidateCurrent();
            if (GUILayout.Button("Save .aegismap.json"))
                SaveAegisMap();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export Tiled JSON"))
                ExportTiledJson();
            if (GUILayout.Button("Randomize Seed"))
                RandomizeSeed();
            if (GUILayout.Button("Generate Prompt Examples"))
                GeneratePromptExamples();
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(status))
                EditorGUILayout.HelpBox(status, status.StartsWith("Error", StringComparison.Ordinal) ? MessageType.Error : MessageType.Info);

            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(string.IsNullOrEmpty(lastGeneratedJson) ? string.Empty : lastGeneratedJson, GUILayout.MinHeight(160));
            EditorGUILayout.EndScrollView();
        }

        void GeneratePreview()
        {
            lastGeneratedJson = BuildCurrentJson();
            status = "Generated deterministic preview for seed " + seed + ".";
        }

        void ValidateCurrent()
        {
            var width = ResolveWidth();
            var height = ResolveHeight();
            if (width < 100 || height < 100 || width > 400 || height > 400)
            {
                status = "Error: map dimensions must be between 100 and 400.";
                return;
            }
            if (string.IsNullOrEmpty(lastGeneratedJson))
                lastGeneratedJson = BuildCurrentJson();
            status = "Map settings validated for " + width + "x" + height + ".";
        }

        void SaveAegisMap()
        {
            ValidateCurrent();
            if (status.StartsWith("Error", StringComparison.Ordinal))
                return;

            var mapId = "procedural_" + ResolveWidth() + "x" + ResolveHeight() + "_seed_" + seed;
            var path = EditorUtility.SaveFilePanelInProject("Save Aegis Map", mapId + ".aegismap", "json", "Save generated Aegis map.", AegisMapEditorPaths.SamplesFolder);
            if (string.IsNullOrEmpty(path))
                return;

            File.WriteAllText(path, BuildCurrentJson());
            AssetDatabase.ImportAsset(path);
            UnityEditor.Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            status = "Saved " + path + ".";
        }

        void ExportTiledJson()
        {
            if (string.IsNullOrEmpty(lastGeneratedJson))
                lastGeneratedJson = BuildCurrentJson();

            var mapId = "procedural_" + ResolveWidth() + "x" + ResolveHeight() + "_seed_" + seed;
            var path = EditorUtility.SaveFilePanelInProject("Export Tiled JSON", mapId + ".tiled", "json", "Save Tiled-compatible JSON shell.", AegisMapEditorPaths.GeneratedMapsFolder);
            if (string.IsNullOrEmpty(path))
                return;

            File.WriteAllText(path, AegisMapEditorFileTemplates.CreateTiledJsonShellFromAegisMap(lastGeneratedJson, mapId));
            AssetDatabase.ImportAsset(path);
            UnityEditor.Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            status = "Exported " + path + ".";
        }

        void RandomizeSeed()
        {
            unchecked
            {
                seed = (int)(DateTime.UtcNow.Ticks & 0x7FFFFFFF);
            }
            status = "Seed randomized to " + seed + ".";
        }

        void GeneratePromptExamples()
        {
            var path = AegisMapEditorPaths.AssetPromptsFolder + "/aegis_procedural_prompt_examples.md";
            File.WriteAllText(path, AegisMapEditorFileTemplates.ProceduralPromptExamplesMarkdown());
            AssetDatabase.ImportAsset(path);
            UnityEditor.Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            status = "Generated prompt examples at " + path + ".";
        }

        string BuildCurrentJson()
        {
            var mapId = "procedural_" + ResolveWidth() + "x" + ResolveHeight() + "_seed_" + seed;
            return AegisMapEditorFileTemplates.CreateProceduralAegisMapJson(
                mapId,
                "Procedural " + SizePresets[sizePreset] + " " + Biomes[biome],
                prompt,
                ResolveWidth(),
                ResolveHeight(),
                int.Parse(PlayerCounts[playerCountIndex]),
                Biomes[biome],
                ResourceDensities[resourceDensity],
                CliffDensities[cliffDensity],
                RockinessValues[rockiness],
                WaterAmounts[waterAmount],
                Symmetries[symmetry],
                seed,
                GameplayProfiles[gameplayProfile],
                oreRegenerationEnabled,
                oreRegenerationRate);
        }

        int ResolveWidth()
        {
            if (SizePresets[sizePreset] == "medium")
                return 200;
            if (SizePresets[sizePreset] == "large")
                return 400;
            if (SizePresets[sizePreset] == "custom")
                return customWidth;
            return 100;
        }

        int ResolveHeight()
        {
            if (SizePresets[sizePreset] == "medium")
                return 200;
            if (SizePresets[sizePreset] == "large")
                return 400;
            if (SizePresets[sizePreset] == "custom")
                return customHeight;
            return 100;
        }
    }
}
#endif
