using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering.Map;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class MapValidationDebugHud : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public TerrainDebugRenderer terrainDebugRenderer;
        public PathDebugRenderer pathDebugRenderer;
        public MapAuthoringOverlay mapAuthoringOverlay;
        public bool visible = true;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
                visible = !visible;
        }

        void OnGUI()
        {
            if (!visible)
                return;
            if (driver == null)
                driver = Object.FindFirstObjectByType<RtsSimulationDriver>();

            GUILayout.BeginArea(new Rect(850, 570, 430, 215), GUI.skin.box);
            GUILayout.Label("Stage 13 Map / Terrain / Pathing");
            if (driver == null || driver.LatestSnapshot == null)
            {
                GUILayout.Label("No map snapshot.");
                GUILayout.EndArea();
                return;
            }

            var map = driver.LatestSnapshot.Map;
            GUILayout.Label("Map: " + map.Width + "x" + map.Height + " valid=" + map.IsValid + " errors=" + map.ValidationErrors.Count);
            GUILayout.Label("Terrain cells: " + (terrainDebugRenderer != null ? terrainDebugRenderer.TerrainCellCount : map.TerrainCells.Count) +
                " highlighted=" + (terrainDebugRenderer != null ? terrainDebugRenderer.HighlightedTerrainCellCount : 0) +
                " blocked=" + (terrainDebugRenderer != null ? terrainDebugRenderer.ImpassableTerrainCellCount : 0));
            GUILayout.Label("Path queries: " + (pathDebugRenderer != null ? pathDebugRenderer.RecentQueryCount : map.RecentPathQueries.Count) +
                " path cells=" + (pathDebugRenderer != null ? pathDebugRenderer.PathCellCount : 0));
            GUILayout.Label(pathDebugRenderer != null ? pathDebugRenderer.LatestQuerySummary : string.Empty);
            GUILayout.Label(mapAuthoringOverlay != null ? mapAuthoringOverlay.Summary : string.Empty);

            if (GUILayout.Button("Reset Map Demo"))
                driver.TryCreateMapTerrainDemoWorld();
            if (GUILayout.Button("Scout Path To East"))
                MoveFirstOwned("scout_rover", new ProjectAegisRTS.Core.Int2(18, 6));
            if (GUILayout.Button("Infantry Path To Forest"))
                MoveFirstOwned("rifle_infantry", new ProjectAegisRTS.Core.Int2(9, 15));
            GUILayout.EndArea();
        }

        void MoveFirstOwned(string typeId, ProjectAegisRTS.Core.Int2 destination)
        {
            var snapshot = driver.LatestSnapshot;
            if (snapshot == null)
                return;

            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                if (actor.OwnerId == driver.PlayerId && actor.TypeId == typeId && !actor.IsDestroyed)
                {
                    driver.SetSelectedActorIds(new[] { actor.ActorId });
                    driver.TryIssueMoveSelectedToCell(destination);
                    return;
                }
            }
        }
    }
}
