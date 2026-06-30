using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.Visibility;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering.Visibility
{
    public sealed class FogOverlayRenderer : MonoBehaviour
    {
        readonly Dictionary<Int2, Renderer> overlays = new Dictionary<Int2, Renderer>();
        readonly List<Int2> removeBuffer = new List<Int2>();

        public RtsSimulationDriver driver;
        public BoardCoordinateMapper mapper;
        Transform fogRoot;
        Material unexploredMaterial;
        Material exploredMaterial;

        public int UnexploredCellCount { get; private set; }
        public int ExploredCellCount { get; private set; }
        public int VisibleCellCount { get; private set; }

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
            if (snapshot == null || snapshot.Fog == null || snapshot.Fog.Cells.Count == 0)
                return;
            if (mapper == null)
                mapper = Object.FindFirstObjectByType<BoardCoordinateMapper>();
            if (mapper == null)
                return;

            EnsureRoot();
            EnsureMaterials();
            var seen = new HashSet<Int2>();
            UnexploredCellCount = 0;
            ExploredCellCount = 0;
            VisibleCellCount = 0;

            for (var i = 0; i < snapshot.Fog.Cells.Count; i++)
            {
                var cell = snapshot.Fog.Cells[i];
                if (cell.Visibility == CellVisibility.Visible)
                {
                    VisibleCellCount++;
                    continue;
                }

                if (cell.Visibility == CellVisibility.Explored)
                    ExploredCellCount++;
                else
                    UnexploredCellCount++;

                seen.Add(cell.Cell);
                Renderer view;
                if (!overlays.TryGetValue(cell.Cell, out view) || view == null)
                {
                    var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    obj.name = "Fog Cell " + cell.Cell;
                    obj.transform.SetParent(fogRoot, false);
                    view = obj.GetComponent<Renderer>();
                    overlays[cell.Cell] = view;
                }

                var position = mapper.CellToWorldCenter(cell.Cell);
                view.transform.position = position + Vector3.up * 0.035f;
                view.transform.localScale = new Vector3(mapper.CellSizeMeters * 0.96f, 0.02f, mapper.CellSizeMeters * 0.96f);
                view.sharedMaterial = cell.Visibility == CellVisibility.Explored ? exploredMaterial : unexploredMaterial;
            }

            removeBuffer.Clear();
            foreach (var pair in overlays)
                if (!seen.Contains(pair.Key))
                    removeBuffer.Add(pair.Key);

            for (var i = 0; i < removeBuffer.Count; i++)
            {
                var key = removeBuffer[i];
                var view = overlays[key];
                overlays.Remove(key);
                if (view != null)
                    VisibilityObjectUtility.DestroyObject(view.gameObject);
            }
        }

        void EnsureRoot()
        {
            if (fogRoot != null)
                return;
            var root = new GameObject("Fog Overlay Views");
            root.transform.SetParent(transform, false);
            fogRoot = root.transform;
        }

        void EnsureMaterials()
        {
            if (unexploredMaterial == null)
            {
                unexploredMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                unexploredMaterial.color = new Color(0.01f, 0.015f, 0.02f, 0.86f);
            }

            if (exploredMaterial == null)
            {
                exploredMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                exploredMaterial.color = new Color(0.08f, 0.11f, 0.13f, 0.45f);
            }
        }
    }
}
