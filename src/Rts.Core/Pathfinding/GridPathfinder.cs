using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Simulation;

namespace ProjectAegisRTS.Pathfinding
{
    public sealed class GridPathfinder
    {
        static readonly Int2[] Neighbors =
        {
            new Int2(0, -1),
            new Int2(1, 0),
            new Int2(0, 1),
            new Int2(-1, 0)
        };

        public List<Int2> FindPath(GridMap map, Int2 start, Int2 goal)
        {
            if (!map.Contains(start) || !map.Contains(goal) || !map.IsPassableForUnit(goal))
                return new List<Int2>();

            var frontier = new Queue<Int2>();
            var cameFrom = new Dictionary<Int2, Int2>();
            frontier.Enqueue(start);
            cameFrom[start] = start;

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();
                if (current.Equals(goal))
                    break;

                for (var i = 0; i < Neighbors.Length; i++)
                {
                    var next = current + Neighbors[i];
                    if (!map.Contains(next))
                        continue;
                    if (!next.Equals(goal) && !map.IsPassableForUnit(next))
                        continue;
                    if (cameFrom.ContainsKey(next))
                        continue;

                    cameFrom[next] = current;
                    frontier.Enqueue(next);
                }
            }

            if (!cameFrom.ContainsKey(goal))
                return new List<Int2>();

            var path = new List<Int2>();
            var step = goal;
            while (!step.Equals(start))
            {
                path.Add(step);
                step = cameFrom[step];
            }

            path.Reverse();
            return path;
        }
    }
}
