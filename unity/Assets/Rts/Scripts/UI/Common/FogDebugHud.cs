using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering.Visibility;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class FogDebugHud : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public FogOverlayRenderer fogOverlayRenderer;
        public VisibilityDebugRenderer visibilityDebugRenderer;
        public RadarSnapshotAdapter radarSnapshotAdapter;
        public MinimapRenderSystem minimapRenderSystem;
        public bool visible = true;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F7))
                visible = !visible;
        }

        void OnGUI()
        {
            if (!visible || driver == null || driver.LatestSnapshot == null)
                return;

            var snapshot = driver.LatestSnapshot;
            GUILayout.BeginArea(new Rect(12, 600, 380, 150), GUI.skin.box);
            GUILayout.Label("Stage 11 Fog / Radar");
            GUILayout.Label("Tick: " + snapshot.Tick + "  Player: " + snapshot.Fog.PlayerId);
            GUILayout.Label("Fog cells V/E/U: " +
                (fogOverlayRenderer != null ? fogOverlayRenderer.VisibleCellCount : 0) + " / " +
                (fogOverlayRenderer != null ? fogOverlayRenderer.ExploredCellCount : 0) + " / " +
                (fogOverlayRenderer != null ? fogOverlayRenderer.UnexploredCellCount : 0));
            GUILayout.Label("Radar: " + (radarSnapshotAdapter != null && radarSnapshotAdapter.IsRadarActive ? "active" : "inactive") +
                " provider " + (radarSnapshotAdapter != null ? radarSnapshotAdapter.ProviderActorId : 0));
            GUILayout.Label("Minimap actor dots: " + (minimapRenderSystem != null ? minimapRenderSystem.ActorDotCount : 0));
            if (GUILayout.Button("Reset Fog Demo"))
                driver.TryCreateFogRadarDemoWorld();
            GUILayout.EndArea();
        }
    }
}
