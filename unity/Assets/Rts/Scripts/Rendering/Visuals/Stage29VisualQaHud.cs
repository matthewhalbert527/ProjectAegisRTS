using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Visuals
{
    public sealed class Stage29VisualQaHud : MonoBehaviour
    {
        public Stage29BattlefieldVisualReviewController reviewController;
        public TerrainMaterialProfileLibrary terrainMaterialProfileLibrary;
        public bool visible = true;
        public KeyCode toggleKey = KeyCode.F10;

        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                visible = !visible;
            if (Input.GetKeyDown(KeyCode.F5))
                Refresh();
        }

        void OnGUI()
        {
            if (!visible)
                return;

            EnsureReferences();
            GUILayout.BeginArea(new Rect(24f, 24f, 440f, 240f), GUI.skin.box);
            GUILayout.Label("STAGE 29 VISUAL QA (F10)");
            GUILayout.Label("Terrain profiles: " + (terrainMaterialProfileLibrary == null ? 0 : terrainMaterialProfileLibrary.ProfileCount));
            GUILayout.Label("Terrain tiles: " + (reviewController == null ? 0 : reviewController.TerrainTileCount));
            GUILayout.Label("Fine-grid lines: " + (reviewController == null ? 0 : reviewController.FineGridLineCount));
            GUILayout.Label("MVP proxies: " + (reviewController == null ? 0 : reviewController.ActorProxyCount));
            GUILayout.Label("Material swatches: " + (reviewController == null ? 0 : reviewController.MaterialSwatchCount));
            if (GUILayout.Button("Refresh Review"))
                Refresh();
            GUILayout.EndArea();
        }

        public void Refresh()
        {
            EnsureReferences();
            if (reviewController != null)
                reviewController.EnsureReviewScene();
        }

        void EnsureReferences()
        {
            if (reviewController == null)
                reviewController = FindFirstObjectByType<Stage29BattlefieldVisualReviewController>();
            if (terrainMaterialProfileLibrary == null)
                terrainMaterialProfileLibrary = FindFirstObjectByType<TerrainMaterialProfileLibrary>();
        }
    }
}
