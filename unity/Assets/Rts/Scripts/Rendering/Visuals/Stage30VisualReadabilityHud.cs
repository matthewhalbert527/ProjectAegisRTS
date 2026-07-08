using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Visuals
{
    public sealed class Stage30VisualReadabilityHud : MonoBehaviour
    {
        public Stage29BattlefieldVisualReviewController reviewController;
        public Stage30ReadabilityProfile readabilityProfile;
        public bool visible = true;
        public Rect panelRect = new Rect(18f, 18f, 440f, 210f);

        public void Refresh()
        {
            if (reviewController == null)
                reviewController = FindFirstObjectByType<Stage29BattlefieldVisualReviewController>();
        }

        void OnGUI()
        {
            if (!visible)
                return;

            Refresh();
            GUI.Box(panelRect, GUIContent.none);
            GUILayout.BeginArea(new Rect(panelRect.x + 12f, panelRect.y + 10f, panelRect.width - 24f, panelRect.height - 20f));
            GUILayout.Label("Stage 30 Visual Readability QA");
            GUILayout.Label("Terrain tiles: " + CountOrZero(reviewController == null ? 0 : reviewController.TerrainTileCount));
            GUILayout.Label("MVP proxies: " + CountOrZero(reviewController == null ? 0 : reviewController.ActorProxyCount));
            GUILayout.Label("Fine-grid lines: " + CountOrZero(reviewController == null ? 0 : reviewController.FineGridLineCount));
            if (readabilityProfile != null)
            {
                GUILayout.Label("Actor/terrain luminance delta: " + readabilityProfile.minimumActorTerrainLuminanceDelta.ToString("F2"));
                GUILayout.Label("Fine-grid dominance max: " + readabilityProfile.maximumFineGridDominance.ToString("F2"));
            }
            GUILayout.Label("Debug panels stay hidden in player-facing scenes.");
            GUILayout.EndArea();
        }

        static string CountOrZero(int value)
        {
            return value.ToString();
        }
    }
}
