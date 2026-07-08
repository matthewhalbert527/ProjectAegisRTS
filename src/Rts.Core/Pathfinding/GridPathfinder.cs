using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Simulation;
using ProjectAegisRTS.Terrain;

namespace ProjectAegisRTS.Pathfinding
{
    public sealed class PathQueryResult
    {
        public bool Success { get; private set; }
        public Int2 StartCell { get; private set; }
        public Int2 GoalCell { get; private set; }
        public MovementClass MovementClass { get; private set; }
        public int TotalCost { get; private set; }
        public int VisitedCellCount { get; private set; }
        public string FailureCode { get; private set; }
        public IReadOnlyList<Int2> Path { get; private set; }

        public PathQueryResult(bool success, Int2 startCell, Int2 goalCell, MovementClass movementClass, int totalCost, int visitedCellCount, string failureCode, IReadOnlyList<Int2> path)
        {
            Success = success;
            StartCell = startCell;
            GoalCell = goalCell;
            MovementClass = movementClass;
            TotalCost = totalCost;
            VisitedCellCount = visitedCellCount;
            FailureCode = failureCode ?? string.Empty;
            Path = path ?? new Int2[0];
        }
    }

    public sealed class GridPathfinder
    {
        const int CardinalCostMultiplier = 10;
        const int DiagonalCostMultiplier = 14;

        static readonly Int2[] Neighbors =
        {
            new Int2(0, -1),
            new Int2(1, -1),
            new Int2(1, 0),
            new Int2(1, 1),
            new Int2(0, 1),
            new Int2(-1, 1),
            new Int2(-1, 0),
            new Int2(-1, -1)
        };

        public List<Int2> FindPath(GridMap map, Int2 start, Int2 goal)
        {
            return new List<Int2>(QueryPath(map, null, start, goal, MovementClass.Wheeled).Path);
        }

        public PathQueryResult QueryPath(GridMap map, RtsRules rules, Int2 start, Int2 goal, MovementClass movementClass)
        {
            if (!map.Contains(start) || !map.Contains(goal))
                return new PathQueryResult(false, start, goal, movementClass, 0, 0, "InvalidStartOrGoal", new Int2[0]);

            if (!map.IsPassableForUnit(start, movementClass, rules))
                return new PathQueryResult(false, start, goal, movementClass, 0, 0, "StartImpassable", new Int2[0]);

            if (!map.IsPassableForUnit(goal, movementClass, rules))
                return new PathQueryResult(false, start, goal, movementClass, 0, 0, "GoalImpassable", new Int2[0]);

            if (start.Equals(goal))
                return new PathQueryResult(true, start, goal, movementClass, 0, 1, string.Empty, new Int2[0]);

            var frontier = new List<Int2>();
            var cameFrom = new Dictionary<Int2, Int2>();
            var costSoFar = new Dictionary<Int2, int>();
            frontier.Add(start);
            cameFrom[start] = start;
            costSoFar[start] = 0;
            var visited = 0;

            while (frontier.Count > 0)
            {
                var currentIndex = FindLowestCostFrontierIndex(frontier, costSoFar);
                var current = frontier[currentIndex];
                frontier.RemoveAt(currentIndex);
                visited++;
                if (current.Equals(goal))
                    break;

                for (var i = 0; i < Neighbors.Length; i++)
                {
                    var offset = Neighbors[i];
                    var next = current + offset;
                    if (!map.Contains(next))
                        continue;
                    if (!map.IsPassableForUnit(next, movementClass, rules))
                        continue;
                    if (IsDiagonal(offset) && !CanMoveDiagonally(map, rules, current, offset, movementClass))
                        continue;

                    var newCost = costSoFar[current] + StepCost(map, rules, next, movementClass, offset);
                    int existingCost;
                    if (costSoFar.TryGetValue(next, out existingCost) && newCost >= existingCost)
                        continue;

                    costSoFar[next] = newCost;
                    cameFrom[next] = current;
                    if (!frontier.Contains(next))
                        frontier.Add(next);
                }
            }

            if (!cameFrom.ContainsKey(goal))
                return new PathQueryResult(false, start, goal, movementClass, 0, visited, "Unreachable", new Int2[0]);

            var path = new List<Int2>();
            var step = goal;
            while (!step.Equals(start))
            {
                path.Add(step);
                step = cameFrom[step];
            }

            path.Reverse();
            return new PathQueryResult(true, start, goal, movementClass, costSoFar[goal], visited, string.Empty, path);
        }

        static int FindLowestCostFrontierIndex(IReadOnlyList<Int2> frontier, Dictionary<Int2, int> costSoFar)
        {
            var bestIndex = 0;
            var bestCell = frontier[0];
            var bestCost = costSoFar[bestCell];
            for (var i = 1; i < frontier.Count; i++)
            {
                var cell = frontier[i];
                var cost = costSoFar[cell];
                if (cost < bestCost ||
                    (cost == bestCost && (cell.Y < bestCell.Y || (cell.Y == bestCell.Y && cell.X < bestCell.X))))
                {
                    bestIndex = i;
                    bestCell = cell;
                    bestCost = cost;
                }
            }

            return bestIndex;
        }

        static int StepCost(GridMap map, RtsRules rules, Int2 cell, MovementClass movementClass, Int2 offset)
        {
            var multiplier = IsDiagonal(offset) ? DiagonalCostMultiplier : CardinalCostMultiplier;
            return map.GetMovementCost(cell, movementClass, rules) * multiplier;
        }

        static bool IsDiagonal(Int2 offset)
        {
            return offset.X != 0 && offset.Y != 0;
        }

        static bool CanMoveDiagonally(GridMap map, RtsRules rules, Int2 current, Int2 offset, MovementClass movementClass)
        {
            var horizontal = new Int2(current.X + offset.X, current.Y);
            var vertical = new Int2(current.X, current.Y + offset.Y);
            return map.Contains(horizontal) &&
                map.Contains(vertical) &&
                map.IsPassableForUnit(horizontal, movementClass, rules) &&
                map.IsPassableForUnit(vertical, movementClass, rules);
        }
    }
}
