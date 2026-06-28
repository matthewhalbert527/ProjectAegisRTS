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
        public float CellSizeMeters { get; private set; }
        public Transform BoardRoot { get; private set; }

        public void Configure(int boardWidth, int boardHeight, float cellSizeMeters, Transform boardRoot)
        {
            BoardWidth = Mathf.Max(1, boardWidth);
            BoardHeight = Mathf.Max(1, boardHeight);
            CellSizeMeters = Mathf.Max(0.1f, cellSizeMeters);
            BoardRoot = boardRoot != null ? boardRoot : transform;
        }

        public bool IsInsideBoard(Int2 cell)
        {
            return cell.X >= 0 && cell.Y >= 0 && cell.X < BoardWidth && cell.Y < BoardHeight;
        }

        public Vector3 CellToWorldCenter(Int2 cell)
        {
            var local = new Vector3(
                (cell.X + 0.5f) * CellSizeMeters,
                0f,
                (cell.Y + 0.5f) * CellSizeMeters);

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
                var local = new Vector3(
                    (actor.CellPosition.X + building.FootprintCells.X * 0.5f) * CellSizeMeters,
                    0f,
                    (actor.CellPosition.Y + building.FootprintCells.Y * 0.5f) * CellSizeMeters);

                return Root.TransformPoint(local);
            }

            return FixedWorldToBoardWorld(actor.FixedWorldPosition);
        }

        public bool TryRayToCell(Ray ray, out Int2 cell, out Vector3 hitPoint)
        {
            cell = Int2.Zero;
            hitPoint = Vector3.zero;

            var plane = new Plane(Root.up, Root.position);
            float distance;
            if (!plane.Raycast(ray, out distance))
                return false;

            hitPoint = ray.GetPoint(distance);
            var local = Root.InverseTransformPoint(hitPoint);
            var x = Mathf.FloorToInt(local.x / CellSizeMeters);
            var y = Mathf.FloorToInt(local.z / CellSizeMeters);
            var candidate = new Int2(x, y);
            if (!IsInsideBoard(candidate))
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
