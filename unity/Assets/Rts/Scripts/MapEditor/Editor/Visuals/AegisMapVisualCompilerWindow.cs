#if UNITY_EDITOR
using ProjectAegisRTS.UnityClient.MapEditor.Visuals;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public sealed class AegisMapVisualCompilerWindow : EditorWindow
    {
        Object selectedMap;
        GameObject currentRoot;
        AegisMapVisualCompileResult lastResult;
        string lastSummary = string.Empty;
        string lastCapturePath = string.Empty;
        int visualSeed;
        ThemeChoice themeChoice = ThemeChoice.ForestPrototype;
        AegisMapVisualRenderMode visualMode = AegisMapVisualRenderMode.ProductionPreview;
        Vector2 scroll;
        bool showTerrainOverlay;
        bool showBlockerOverlay;
        bool showResourceOverlay;
        bool showBuildPadOverlay;
        bool showCliffOverlay;
        bool showPathabilityOverlay;

        enum ThemeChoice
        {
            Debug,
            ForestPrototype,
            DesertPrototype
        }

        [MenuItem("Project Aegis/Map Editor/Visual Compiler")]
        public static void Open()
        {
            var window = GetWindow<AegisMapVisualCompilerWindow>("Aegis Visual Compiler");
            window.minSize = new Vector2(460f, 520f);
            window.Show();
        }

        void OnEnable()
        {
            if (selectedMap == null && UnityEditor.Selection.activeObject != null)
                selectedMap = UnityEditor.Selection.activeObject;
        }

        void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("Map Visual Compiler", EditorStyles.boldLabel);
            selectedMap = EditorGUILayout.ObjectField("Aegis Map", selectedMap, typeof(Object), false);
            themeChoice = (ThemeChoice)EditorGUILayout.EnumPopup("Visual Theme", themeChoice);
            visualMode = (AegisMapVisualRenderMode)EditorGUILayout.EnumPopup("Visual Mode", visualMode);
            visualSeed = EditorGUILayout.IntField("Visual Seed", visualSeed);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug Overlays", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Debug overlays are not production output. ProductionPreview hides helper geometry unless Hybrid mode enables selected overlays.", MessageType.Info);
            showTerrainOverlay = EditorGUILayout.ToggleLeft("Terrain", showTerrainOverlay);
            showBlockerOverlay = EditorGUILayout.ToggleLeft("Blockers", showBlockerOverlay);
            showResourceOverlay = EditorGUILayout.ToggleLeft("Resources", showResourceOverlay);
            showBuildPadOverlay = EditorGUILayout.ToggleLeft("Build Pads", showBuildPadOverlay);
            showCliffOverlay = EditorGUILayout.ToggleLeft("Cliffs", showCliffOverlay);
            showPathabilityOverlay = EditorGUILayout.ToggleLeft("Pathability / Fairness", showPathabilityOverlay);

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Compile Preview", GUILayout.Height(30f)))
                    CompilePreview();
                if (GUILayout.Button("Clear Preview", GUILayout.Height(30f)))
                    ClearPreview();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Use Selected Asset"))
                    selectedMap = UnityEditor.Selection.activeObject;
                if (GUILayout.Button("Capture Screenshot"))
                    CapturePreview();
            }

            if (!string.IsNullOrEmpty(lastCapturePath))
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Last Capture", lastCapturePath, EditorStyles.wordWrappedLabel);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Layer Summary", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(string.IsNullOrEmpty(lastSummary) ? "Compile a .aegismap.json file to see terrain chunk, transition, shoreline, road, cliff, resource, base pad, and scatter summaries." : lastSummary, MessageType.None);
            EditorGUILayout.EndScrollView();
        }

        void CompilePreview()
        {
            var path = AssetDatabase.GetAssetPath(selectedMap);
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".aegismap.json", System.StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog("Visual Compiler", "Select a .aegismap.json asset first.", "OK");
                return;
            }

            var document = AegisVisualMapDocument.Load(path);
            if (document == null)
            {
                EditorUtility.DisplayDialog("Visual Compiler", "The selected map could not be parsed.", "OK");
                return;
            }

            ClearPreview();
            var seed = visualSeed == 0 ? document.ReadSeed() : visualSeed;
            lastResult = AegisMapVisualCompiler.CompileDocument(document, path, true, CreateTheme(), seed, CreateSettings());
            currentRoot = lastResult.Root;
            lastSummary = AddOverlaySummary(lastResult.ToSummaryText());
            UnityEditor.Selection.activeObject = currentRoot;
            EditorGUIUtility.PingObject(currentRoot);
        }

        void ClearPreview()
        {
            if (currentRoot != null)
                DestroyImmediate(currentRoot);

            currentRoot = null;
            lastResult = null;
        }

        void CapturePreview()
        {
            if (currentRoot == null || lastResult == null)
            {
                CompilePreview();
                if (currentRoot == null)
                    return;
            }

            var path = AssetDatabase.GetAssetPath(selectedMap);
            var document = AegisVisualMapDocument.Load(path);
            if (document == null)
                return;

            lastCapturePath = AegisMapPreviewCaptureTool.CapturePreview(currentRoot, document, "aegis_visual_compiler_preview.png", 1600, 1100, 0.24f);
        }

        AegisMapVisualTheme CreateTheme()
        {
            switch (themeChoice)
            {
                case ThemeChoice.Debug:
                    return AegisBiomeVisualTheme.DebugVisualTheme();
                case ThemeChoice.DesertPrototype:
                    return AegisBiomeVisualTheme.DesertPrototypeVisualTheme();
                default:
                    return AegisBiomeVisualTheme.ForestPrototypeVisualTheme();
            }
        }

        AegisMapVisualCompileSettings CreateSettings()
        {
            return new AegisMapVisualCompileSettings
            {
                RenderMode = visualMode,
                Overlays = new AegisMapVisualOverlaySettings
                {
                    Terrain = showTerrainOverlay,
                    Blockers = showBlockerOverlay,
                    Resources = showResourceOverlay,
                    BuildPads = showBuildPadOverlay,
                    Cliffs = showCliffOverlay,
                    Pathability = showPathabilityOverlay
                }
            };
        }

        string AddOverlaySummary(string summary)
        {
            return summary +
                "\n\nVisual mode: " + visualMode +
                "\nDebug overlay toggles:" +
                "\n- Terrain: " + showTerrainOverlay +
                "\n- Blockers: " + showBlockerOverlay +
                "\n- Resources: " + showResourceOverlay +
                "\n- Build pads: " + showBuildPadOverlay +
                "\n- Cliffs: " + showCliffOverlay +
                "\n- Pathability/fairness: " + showPathabilityOverlay;
        }
    }
}
#endif
