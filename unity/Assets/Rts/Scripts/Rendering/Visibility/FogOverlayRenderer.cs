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
        public Color unexploredColor = new Color(0.02f, 0.03f, 0.04f, 0.18f);
        public Color exploredColor = new Color(0.08f, 0.11f, 0.13f, 0.08f);
        Transform fogRoot;
        Material unexploredMaterial;
        Material exploredMaterial;

        public int UnexploredCellCount { get; private set; }
        public int ExploredCellCount { get; private set; }
        public int VisibleCellCount { get; private set; }
        public bool IsUsingTransparentMaterials { get; private set; }

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
                unexploredMaterial = CreateTransparentMaterial("Stage16 Fog Unexplored");

            if (exploredMaterial == null)
                exploredMaterial = CreateTransparentMaterial("Stage16 Fog Explored");

            ApplyTransparentMaterial(unexploredMaterial, unexploredColor);
            ApplyTransparentMaterial(exploredMaterial, exploredColor);
            IsUsingTransparentMaterials = IsTransparentMaterial(unexploredMaterial) && IsTransparentMaterial(exploredMaterial);
        }

        static Material CreateTransparentMaterial(string materialName)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader);
            material.name = materialName;
            return material;
        }

        static void ApplyTransparentMaterial(Material material, Color color)
        {
            if (material == null)
                return;

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color"))
            {
                material.color = color;
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_Surface"))
                material.SetFloat("_Surface", 1f);
            if (material.HasProperty("_Blend"))
                material.SetFloat("_Blend", 0f);
            if (material.HasProperty("_SrcBlend"))
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (material.HasProperty("_DstBlend"))
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (material.HasProperty("_ZWrite"))
                material.SetFloat("_ZWrite", 0f);

            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        static bool IsTransparentMaterial(Material material)
        {
            if (material == null)
                return false;
            if (material.renderQueue >= (int)UnityEngine.Rendering.RenderQueue.Transparent)
                return true;
            return material.HasProperty("_ZWrite") && Mathf.Approximately(material.GetFloat("_ZWrite"), 0f);
        }
    }
}
