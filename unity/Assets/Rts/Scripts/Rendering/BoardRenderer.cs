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
            if (hoverCell == null || mapper == null)
                return;

            if (!hovered.HasValue)
            {
                hoverCell.SetActive(false);
                return;
            }

            hoverCell.SetActive(true);
            hoverCell.transform.position = mapper.CellToWorldCenter(hovered.Value) + Vector3.up * 0.035f;
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
                cellObject.transform.position = mapper.CellToWorldCenter(footprintCells[i]) + Vector3.up * 0.045f;
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
            for (var x = 0; x <= mapper.BoardWidth; x++)
                CreateLine("Grid X " + x, new Vector3(x * mapper.CellSizeMeters, 0.02f, 0f), new Vector3(x * mapper.CellSizeMeters, 0.02f, mapper.BoardHeight * mapper.CellSizeMeters));

            for (var y = 0; y <= mapper.BoardHeight; y++)
                CreateLine("Grid Y " + y, new Vector3(0f, 0.021f, y * mapper.CellSizeMeters), new Vector3(mapper.BoardWidth * mapper.CellSizeMeters, 0.021f, y * mapper.CellSizeMeters));
        }

        void CreateLine(string lineName, Vector3 start, Vector3 end)
        {
            var lineObject = new GameObject(lineName);
            lineObject.transform.SetParent(transform, false);
            var line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.widthMultiplier = 0.015f;
            line.sharedMaterial = materials.GridLine;
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
