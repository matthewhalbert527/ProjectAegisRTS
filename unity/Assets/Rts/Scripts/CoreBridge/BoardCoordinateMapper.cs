using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Snapshots;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.CoreBridge
{
    public sealed class BoardCoordinateMapper : MonoBehaviour
    {
        public int BoardWidth { get; private set; }
        public int BoardHeight { get; private set; }
        public int PlacementGridScale { get; private set; }
        public int PlacementBoardWidth { get { return BoardWidth * PlacementGridScale; } }
        public int PlacementBoardHeight { get { return BoardHeight * PlacementGridScale; } }
        public float CellSizeMeters { get; private set; }
        public float PlacementCellSizeMeters { get { return CellSizeMeters / Mathf.Max(1, PlacementGridScale); } }
        public Transform BoardRoot { get; private set; }

        public void Configure(int boardWidth, int boardHeight, float cellSizeMeters, Transform boardRoot)
        {
            Configure(boardWidth, boardHeight, cellSizeMeters, boardRoot, PlacementGridMetrics.PlacementGridScale);
        }

        public void Configure(int boardWidth, int boardHeight, float cellSizeMeters, Transform boardRoot, int placementGridScale)
        {
            BoardWidth = Mathf.Max(1, boardWidth);
            BoardHeight = Mathf.Max(1, boardHeight);
            PlacementGridScale = Mathf.Max(1, placementGridScale);
            CellSizeMeters = Mathf.Max(0.1f, cellSizeMeters);
            BoardRoot = boardRoot != null ? boardRoot : transform;
        }

        public bool IsInsideBoard(Int2 cell)
        {
            return cell.X >= 0 && cell.Y >= 0 && cell.X < BoardWidth && cell.Y < BoardHeight;
        }

        public bool IsInsidePlacementBoard(Int2 placementCell)
        {
            return placementCell.X >= 0 && placementCell.Y >= 0 && placementCell.X < PlacementBoardWidth && placementCell.Y < PlacementBoardHeight;
        }

        public Vector3 CellToWorldCenter(Int2 cell)
        {
            var local = new Vector3(
                (cell.X + 0.5f) * CellSizeMeters,
                0f,
                (cell.Y + 0.5f) * CellSizeMeters);

            return Root.TransformPoint(local);
        }

        public Vector3 PlacementCellToWorldCenter(Int2 placementCell)
        {
            var placementCellSize = PlacementCellSizeMeters;
            var local = new Vector3(
                (placementCell.X + 0.5f) * placementCellSize,
                0f,
                (placementCell.Y + 0.5f) * placementCellSize);

            return Root.TransformPoint(local);
        }

        public Vector3 FixedWorldToBoardWorld(Int2 fixedWorldPosition)
        {
            var local = new Vector3(
                fixedWorldPosition.X / (float)FixedMath.CellScale * CellSizeMeters,
                0f,
                fixedWorldPosition.Y / (float)FixedMath.CellScale * CellSizeMeters);

            return Root.TransformPoint(local);
        }

        public Vector3 ActorToWorldPosition(ActorSnapshot actor, ActorDefinition definition)
        {
            var building = definition as BuildingDefinition;
            if (building != null)
            {
                var placementFootprint = actor.PlacementFootprintCells.Equals(Int2.Zero) ? building.PlacementFootprintCells : actor.PlacementFootprintCells;
                var placementCellSize = PlacementCellSizeMeters;
                var local = new Vector3(
                    (actor.PlacementTopLeftCell.X + placementFootprint.X * 0.5f) * placementCellSize,
                    0f,
                    (actor.PlacementTopLeftCell.Y + placementFootprint.Y * 0.5f) * placementCellSize);

                return Root.TransformPoint(local);
            }

            return FixedWorldToBoardWorld(actor.FixedWorldPosition);
        }

        public bool TryRayToCell(Ray ray, out Int2 cell, out Vector3 hitPoint)
        {
            return TryRayToGridCell(ray, CellSizeMeters, BoardWidth, BoardHeight, out cell, out hitPoint);
        }

        public bool TryRayToPlacementCell(Ray ray, out Int2 placementCell, out Vector3 hitPoint)
        {
            return TryRayToGridCell(ray, PlacementCellSizeMeters, PlacementBoardWidth, PlacementBoardHeight, out placementCell, out hitPoint);
        }

        bool TryRayToGridCell(Ray ray, float gridCellSizeMeters, int gridWidth, int gridHeight, out Int2 cell, out Vector3 hitPoint)
        {
            cell = Int2.Zero;
            hitPoint = Vector3.zero;

            var plane = new Plane(Root.up, Root.position);
            float distance;
            if (!plane.Raycast(ray, out distance))
                return false;

            hitPoint = ray.GetPoint(distance);
            var local = Root.InverseTransformPoint(hitPoint);
            var x = Mathf.FloorToInt(local.x / gridCellSizeMeters);
            var y = Mathf.FloorToInt(local.z / gridCellSizeMeters);
            var candidate = new Int2(x, y);
            if (candidate.X < 0 || candidate.Y < 0 || candidate.X >= gridWidth || candidate.Y >= gridHeight)
                return false;

            cell = candidate;
            return true;
        }

        public Vector3 BoardCenterWorld
        {
            get
            {
                var local = new Vector3(
                    BoardWidth * CellSizeMeters * 0.5f,
                    0f,
                    BoardHeight * CellSizeMeters * 0.5f);

                return Root.TransformPoint(local);
            }
        }

        public Vector3 BoardSizeWorld
        {
            get { return new Vector3(BoardWidth * CellSizeMeters, 0f, BoardHeight * CellSizeMeters); }
        }

        Transform Root
        {
            get { return BoardRoot != null ? BoardRoot : transform; }
        }
    }
}
