using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Map
{
    public sealed class TerrainDebugRenderer : MonoBehaviour
    {
        readonly Dictionary<Int2, Renderer> terrainViews = new Dictionary<Int2, Renderer>();
        readonly List<Int2> removeBuffer = new List<Int2>();

        public RtsSimulationDriver driver;
        public BoardCoordinateMapper mapper;
        Transform terrainRoot;
        Material roadMaterial;
        Material roughMaterial;
        Material forestMaterial;
        Material waterMaterial;
        Material cliffMaterial;
        Material oreMaterial;

        public int TerrainCellCount { get; private set; }
        public int HighlightedTerrainCellCount { get; private set; }
        public int ImpassableTerrainCellCount { get; private set; }

        void Update()
        {
            if (driver != null && driver.LatestSnapshot != null)
                RenderSnapshot(driver.LatestSnapshot);
        }

        public void Initialize(RtsSimulationDriver simulationDriver, BoardCoordinateMapper coordinateMapper)
        {
            driver = simulationDriver;
            mapper = coordinateMapper;
            EnsureRoot();
            EnsureMaterials();
        }

        public void RenderSnapshot(WorldSnapshot snapshot)
        {
            TerrainCellCount = 0;
            HighlightedTerrainCellCount = 0;
            ImpassableTerrainCellCount = 0;
            if (snapshot == null || snapshot.Map == null || snapshot.Map.TerrainCells.Count == 0)
                return;
            if (mapper == null)
                mapper = Object.FindFirstObjectByType<BoardCoordinateMapper>();
            if (mapper == null)
                return;

            EnsureRoot();
            EnsureMaterials();
            var seen = new HashSet<Int2>();
            TerrainCellCount = snapshot.Map.TerrainCells.Count;

            for (var i = 0; i < snapshot.Map.TerrainCells.Count; i++)
            {
                var cell = snapshot.Map.TerrainCells[i];
                if (IsImpassable(cell))
                    ImpassableTerrainCellCount++;
                if (!ShouldHighlight(cell))
                    continue;

                HighlightedTerrainCellCount++;
                seen.Add(cell.Cell);
                Renderer view;
                if (!terrainViews.TryGetValue(cell.Cell, out view) || view == null)
                {
                    var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    obj.name = "Terrain Debug Cell " + cell.Cell;
                    obj.transform.SetParent(terrainRoot, false);
                    RemoveCollider(obj);
                    view = obj.GetComponent<Renderer>();
                    terrainViews[cell.Cell] = view;
                }

                view.transform.position = mapper.CellToWorldCenter(cell.Cell) + Vector3.up * 0.028f;
                view.transform.localScale = new Vector3(mapper.CellSizeMeters * 0.92f, 0.018f, mapper.CellSizeMeters * 0.92f);
                view.sharedMaterial = MaterialFor(cell.Kind);
            }

            removeBuffer.Clear();
            foreach (var pair in terrainViews)
                if (!seen.Contains(pair.Key))
                    removeBuffer.Add(pair.Key);

            for (var i = 0; i < removeBuffer.Count; i++)
            {
                var key = removeBuffer[i];
                var view = terrainViews[key];
                terrainViews.Remove(key);
                if (view != null)
                    DestroyObject(view.gameObject);
            }
        }

        static bool ShouldHighlight(TerrainCellSnapshot cell)
        {
            return cell.Kind != "Clear" || cell.IsBlocked || cell.HasBuilding;
        }

        static bool IsImpassable(TerrainCellSnapshot cell)
        {
            return cell.Kind == "Water" || cell.Kind == "Cliff" || cell.IsBlocked;
        }

        Material MaterialFor(string kind)
        {
            if (kind == "Road")
                return roadMaterial;
            if (kind == "Rough")
                return roughMaterial;
            if (kind == "Forest")
                return forestMaterial;
            if (kind == "Water")
                return waterMaterial;
            if (kind == "Cliff")
                return cliffMaterial;
            if (kind == "OreField")
                return oreMaterial;
            return roughMaterial;
        }

        void EnsureRoot()
        {
            if (terrainRoot != null)
                return;
            var root = new GameObject("Terrain Debug Views");
            root.transform.SetParent(transform, false);
            terrainRoot = root.transform;
        }

        void EnsureMaterials()
        {
            roadMaterial = roadMaterial ?? CreateMaterial(new Color(0.46f, 0.48f, 0.42f, 0.64f));
            roughMaterial = roughMaterial ?? CreateMaterial(new Color(0.44f, 0.36f, 0.24f, 0.62f));
            forestMaterial = forestMaterial ?? CreateMaterial(new Color(0.10f, 0.34f, 0.18f, 0.62f));
            waterMaterial = waterMaterial ?? CreateMaterial(new Color(0.10f, 0.27f, 0.48f, 0.70f));
            cliffMaterial = cliffMaterial ?? CreateMaterial(new Color(0.28f, 0.25f, 0.23f, 0.72f));
            oreMaterial = oreMaterial ?? CreateMaterial(new Color(0.76f, 0.57f, 0.16f, 0.70f));
        }

        static Material CreateMaterial(Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.color = color;
            return material;
        }

        static void RemoveCollider(GameObject target)
        {
            var collider = target.GetComponent<Collider>();
            if (collider != null)
                DestroyObject(collider);
        }

        static void DestroyObject(Object target)
        {
            if (Application.isPlaying)
                Destroy(target);
            else
                DestroyImmediate(target);
        }
    }
}
