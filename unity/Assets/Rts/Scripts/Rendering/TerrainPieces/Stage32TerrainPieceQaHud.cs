using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.TerrainPieces
{
    public sealed class Stage32TerrainPieceQaHud : MonoBehaviour
    {
        public Stage32TerrainSetDressingReviewController reviewController;
        public TerrainPieceLibrary pieceLibrary;
        public TerrainSetDressingRuntimeLayer runtimeLayer;
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
            GUILayout.BeginArea(new Rect(24f, 24f, 470f, 230f), GUI.skin.box);
            GUILayout.Label("STAGE 32 TERRAIN PIECE QA (F10)");
            GUILayout.Label("Catalog pieces: " + (pieceLibrary == null ? 0 : pieceLibrary.Count));
            GUILayout.Label("Review pieces: " + (reviewController == null ? 0 : reviewController.PieceCount));
            GUILayout.Label("Material swatches: " + (reviewController == null ? 0 : reviewController.MaterialSwatchCount));
            GUILayout.Label("Footprint refs: " + (reviewController == null ? 0 : reviewController.FootprintReferenceCount));
            GUILayout.Label("Player-facing rendered pieces: " + (runtimeLayer == null || runtimeLayer.renderer == null ? 0 : runtimeLayer.renderer.RenderedPieceCount));
            if (GUILayout.Button("Refresh Terrain Review"))
                Refresh();
            GUILayout.EndArea();
        }

        public void Refresh()
        {
            EnsureReferences();
            if (reviewController != null)
                reviewController.EnsureReviewScene();
            if (runtimeLayer != null)
                runtimeLayer.EnsureInitialized();
        }

        void EnsureReferences()
        {
            if (reviewController == null)
                reviewController = FindFirstObjectByType<Stage32TerrainSetDressingReviewController>();
            if (runtimeLayer == null)
                runtimeLayer = FindFirstObjectByType<TerrainSetDressingRuntimeLayer>();
            if (pieceLibrary == null && reviewController != null)
                pieceLibrary = reviewController.pieceLibrary;
        }
    }
}
