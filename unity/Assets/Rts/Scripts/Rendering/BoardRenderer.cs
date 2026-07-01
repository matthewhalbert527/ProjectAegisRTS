using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Utilities;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Rendering
{
    public sealed class BoardRenderer : MonoBehaviour
    {
        readonly List<GameObject> placementCells = new List<GameObject>();
        BoardCoordinateMapper mapper;
        Stage1MaterialLibrary materials;
        GameObject hoverCell;
        public int FineGridLineCount { get; private set; }

        public void Initialize(BoardCoordinateMapper coordinateMapper)
        {
            mapper = coordinateMapper;
            materials = Stage1MaterialLibrary.Create();

            ClearGeneratedChildren();
            CreateBoardSurface();
            CreateGridLines();
            hoverCell = CreateFlatCell("Hovered Cell", materials.HoverCell);
            hoverCell.SetActive(false);
        }

        public void UpdateHover(Int2? hovered)
        {
            UpdateHover(hovered, false);
        }

        public void UpdateHover(Int2? hovered, bool placementGridCell)
        {
            if (hoverCell == null || mapper == null)
                return;

            if (!hovered.HasValue)
            {
                hoverCell.SetActive(false);
                return;
            }

            hoverCell.SetActive(true);
            hoverCell.transform.position = (placementGridCell ? mapper.PlacementCellToWorldCenter(hovered.Value) : mapper.CellToWorldCenter(hovered.Value)) + Vector3.up * 0.035f;
            var size = (placementGridCell ? mapper.PlacementCellSizeMeters : mapper.CellSizeMeters) * 0.94f;
            hoverCell.transform.localScale = new Vector3(size, 0.035f, size);
        }

        public void UpdatePlacementPreview(PlacementPreviewSnapshot preview)
        {
            if (preview == null || preview.FootprintCells == null || preview.FootprintCells.Count == 0)
            {
                HidePlacementCells();
                return;
            }

            SetPlacementPreview(preview.FootprintCells, preview.CanPlace);
        }

        public void SetHoveredCell(Int2? hovered)
        {
            UpdateHover(hovered);
        }

        public void SetHoveredCell(Int2? hovered, bool placementGridCell)
        {
            UpdateHover(hovered, placementGridCell);
        }

        public void SetPlacementPreview(IReadOnlyList<Int2> footprintCells, bool isValid)
        {
            if (footprintCells == null || footprintCells.Count == 0)
            {
                HidePlacementCells();
                return;
            }

            while (placementCells.Count < footprintCells.Count)
            {
                var cell = CreateFlatCell("Placement Preview Cell", isValid ? materials.PlacementValid : materials.PlacementInvalid);
                placementCells.Add(cell);
            }

            for (var i = 0; i < placementCells.Count; i++)
            {
                var cellObject = placementCells[i];
                if (i >= footprintCells.Count)
                {
                    cellObject.SetActive(false);
                    continue;
                }

                cellObject.SetActive(true);
                cellObject.transform.position = mapper.PlacementCellToWorldCenter(footprintCells[i]) + Vector3.up * 0.045f;
                var size = mapper.PlacementCellSizeMeters * 0.94f;
                cellObject.transform.localScale = new Vector3(size, 0.035f, size);
                SetMaterial(cellObject, isValid ? materials.PlacementValid : materials.PlacementInvalid);
            }
        }

        public void SetPlacementPreview(string actorTypeId, IReadOnlyList<Int2> footprintCells, Int2 topLeftCell, bool isValid, string message)
        {
            SetPlacementPreview(footprintCells, isValid);
        }

        public void ClearPlacementPreview()
        {
            HidePlacementCells();
        }

        void CreateBoardSurface()
        {
            var surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
            surface.name = "Board Surface";
            surface.transform.SetParent(transform, false);
            surface.transform.localPosition = new Vector3(
                mapper.BoardWidth * mapper.CellSizeMeters * 0.5f,
                -0.035f,
                mapper.BoardHeight * mapper.CellSizeMeters * 0.5f);
            surface.transform.localScale = new Vector3(
                mapper.BoardWidth * mapper.CellSizeMeters,
                0.05f,
                mapper.BoardHeight * mapper.CellSizeMeters);
            RemoveCollider(surface);
            SetMaterial(surface, materials.Board);

            var edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            edge.name = "Board Edge";
            edge.transform.SetParent(transform, false);
            edge.transform.localPosition = surface.transform.localPosition + Vector3.down * 0.06f;
            edge.transform.localScale = new Vector3(
                mapper.BoardWidth * mapper.CellSizeMeters + 0.35f,
                0.08f,
                mapper.BoardHeight * mapper.CellSizeMeters + 0.35f);
            RemoveCollider(edge);
            SetMaterial(edge, materials.BoardEdge);
        }

        void CreateGridLines()
        {
            FineGridLineCount = 0;
            for (var x = 0; x <= mapper.PlacementBoardWidth; x++)
            {
                var isCoarse = x % mapper.PlacementGridScale == 0;
                var height = isCoarse ? 0.026f : 0.02f;
                var width = isCoarse ? 0.018f : 0.004f;
                CreateLine("Grid X " + x, new Vector3(x * mapper.PlacementCellSizeMeters, height, 0f), new Vector3(x * mapper.PlacementCellSizeMeters, height, mapper.BoardHeight * mapper.CellSizeMeters), width, isCoarse ? materials.CoarseGridLine : materials.FineGridLine);
                if (!isCoarse)
                    FineGridLineCount++;
            }

            for (var y = 0; y <= mapper.PlacementBoardHeight; y++)
            {
                var isCoarse = y % mapper.PlacementGridScale == 0;
                var height = isCoarse ? 0.027f : 0.021f;
                var width = isCoarse ? 0.018f : 0.004f;
                CreateLine("Grid Y " + y, new Vector3(0f, height, y * mapper.PlacementCellSizeMeters), new Vector3(mapper.BoardWidth * mapper.CellSizeMeters, height, y * mapper.PlacementCellSizeMeters), width, isCoarse ? materials.CoarseGridLine : materials.FineGridLine);
                if (!isCoarse)
                    FineGridLineCount++;
            }
        }

        void CreateLine(string lineName, Vector3 start, Vector3 end, float width, Material material)
        {
            var lineObject = new GameObject(lineName);
            lineObject.transform.SetParent(transform, false);
            var line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.widthMultiplier = width;
            line.sharedMaterial = material != null ? material : materials.GridLine;
        }

        GameObject CreateFlatCell(string objectName, Material material)
        {
            var cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cell.name = objectName;
            cell.transform.SetParent(transform, false);
            cell.transform.localScale = new Vector3(mapper.CellSizeMeters * 0.94f, 0.035f, mapper.CellSizeMeters * 0.94f);
            RemoveCollider(cell);
            SetMaterial(cell, material);
            return cell;
        }

        void HidePlacementCells()
        {
            for (var i = 0; i < placementCells.Count; i++)
                placementCells[i].SetActive(false);
        }

        void ClearGeneratedChildren()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }

            placementCells.Clear();
        }

        static void RemoveCollider(GameObject target)
        {
            var collider = target.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(collider);
                else
                    Object.DestroyImmediate(collider);
            }
        }

        static void SetMaterial(GameObject target, Material material)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
        }
    }
}
