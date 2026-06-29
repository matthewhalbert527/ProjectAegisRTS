using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Selection
{
    public static class SelectionResolver
    {
        public static List<LeftHandSelectionCandidate> FindCandidates(
            WorldSnapshot snapshot,
            RtsSimulationDriver driver,
            BoardCoordinateMapper mapper,
            Ray ray,
            Int2? boardCell,
            float rayToleranceMeters,
            int cellRadius)
        {
            var result = new List<LeftHandSelectionCandidate>();
            if (snapshot == null || driver == null || mapper == null)
                return result;

            var normalizedRay = new Ray(ray.origin, ray.direction.normalized);
            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                if (actor.OwnerId != driver.PlayerId)
                    continue;

                ActorDefinition definition;
                if (!driver.TryGetDefinition(actor.TypeId, out definition))
                    continue;

                var worldPosition = mapper.ActorToWorldPosition(actor, definition);
                var distance = DistanceToRay(normalizedRay, worldPosition);
                var cellMatch = !boardCell.HasValue || ActorNearCell(actor, definition, boardCell.Value, cellRadius);
                if (!cellMatch && distance > rayToleranceMeters)
                    continue;

                result.Add(new LeftHandSelectionCandidate
                {
                    ActorId = actor.ActorId,
                    TypeId = actor.TypeId,
                    Category = DetermineCategory(definition),
                    Cell = actor.CellPosition,
                    WorldPosition = worldPosition,
                    DistanceToRayOrCell = boardCell.HasValue && cellMatch ? actor.CellPosition.ManhattanDistanceTo(boardCell.Value) : distance,
                    Priority = Priority(definition, driver.IsActorSelected(actor.ActorId)),
                    DisplayName = definition.DisplayName
                });
            }

            result.Sort(CompareCandidates);
            return result;
        }

        public static List<LeftHandSelectionCandidate> FindCandidatesInRect(
            WorldSnapshot snapshot,
            RtsSimulationDriver driver,
            BoardCoordinateMapper mapper,
            Int2 a,
            Int2 b)
        {
            var result = new List<LeftHandSelectionCandidate>();
            if (snapshot == null || driver == null || mapper == null)
                return result;

            var minX = Mathf.Min(a.X, b.X);
            var maxX = Mathf.Max(a.X, b.X);
            var minY = Mathf.Min(a.Y, b.Y);
            var maxY = Mathf.Max(a.Y, b.Y);

            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                if (actor.OwnerId != driver.PlayerId)
                    continue;

                ActorDefinition definition;
                if (!driver.TryGetDefinition(actor.TypeId, out definition))
                    continue;

                if (!ActorIntersectsRect(actor, definition, minX, maxX, minY, maxY))
                    continue;

                result.Add(new LeftHandSelectionCandidate
                {
                    ActorId = actor.ActorId,
                    TypeId = actor.TypeId,
                    Category = DetermineCategory(definition),
                    Cell = actor.CellPosition,
                    WorldPosition = mapper.ActorToWorldPosition(actor, definition),
                    DistanceToRayOrCell = 0f,
                    Priority = Priority(definition, driver.IsActorSelected(actor.ActorId)),
                    DisplayName = definition.DisplayName
                });
            }

            result.Sort(CompareCandidates);
            return result;
        }

        static bool ActorNearCell(ActorSnapshot actor, ActorDefinition definition, Int2 cell, int radius)
        {
            var building = definition as BuildingDefinition;
            if (building == null)
                return actor.CellPosition.ManhattanDistanceTo(cell) <= radius;

            return cell.X >= actor.CellPosition.X - radius &&
                   cell.Y >= actor.CellPosition.Y - radius &&
                   cell.X < actor.CellPosition.X + building.FootprintCells.X + radius &&
                   cell.Y < actor.CellPosition.Y + building.FootprintCells.Y + radius;
        }

        static bool ActorIntersectsRect(ActorSnapshot actor, ActorDefinition definition, int minX, int maxX, int minY, int maxY)
        {
            var building = definition as BuildingDefinition;
            if (building == null)
                return actor.CellPosition.X >= minX && actor.CellPosition.X <= maxX && actor.CellPosition.Y >= minY && actor.CellPosition.Y <= maxY;

            var actorMaxX = actor.CellPosition.X + building.FootprintCells.X - 1;
            var actorMaxY = actor.CellPosition.Y + building.FootprintCells.Y - 1;
            return actor.CellPosition.X <= maxX && actorMaxX >= minX && actor.CellPosition.Y <= maxY && actorMaxY >= minY;
        }

        static float DistanceToRay(Ray ray, Vector3 point)
        {
            return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
        }

        static int CompareCandidates(LeftHandSelectionCandidate a, LeftHandSelectionCandidate b)
        {
            var priority = b.Priority.CompareTo(a.Priority);
            if (priority != 0)
                return priority;
            return a.DistanceToRayOrCell.CompareTo(b.DistanceToRayOrCell);
        }

        static int Priority(ActorDefinition definition, bool selected)
        {
            var priority = selected ? 20 : 0;
            if (definition is UnitDefinition)
                priority += 30;
            if (definition.TypeId.Contains("infantry"))
                priority += 5;
            if (definition.TypeId.Contains("tower") || definition.TypeId.Contains("turret"))
                priority += 2;
            return priority;
        }

        static string DetermineCategory(ActorDefinition definition)
        {
            if (definition.TypeId.Contains("aircraft"))
                return "aircraft";
            if (definition.TypeId.Contains("infantry") || definition.TypeId == "engineer")
                return "infantry";
            if (definition.TypeId.Contains("tower") || definition.TypeId.Contains("turret"))
                return "defense";
            if (definition is BuildingDefinition)
                return "building";
            return "vehicle";
        }
    }
}
