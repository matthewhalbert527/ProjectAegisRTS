using System.Collections.Generic;
using ProjectAegisRTS.Core;

namespace ProjectAegisRTS.Maps.Generation
{
    public sealed class AegisMapBalanceAnalyzer
    {
        public AegisMapBalanceReport Analyze(AegisMapDocument document)
        {
            var warnings = new List<string>();
            var connected = StartsAreConnected(document);
            if (!connected)
                warnings.Add("StartsDisconnected:At least one player start cannot reach another start.");

            return new AegisMapBalanceReport(
                connected,
                document == null || document.Resources == null ? 0 : document.Resources.Count,
                document == null || document.Blockers == null ? 0 : document.Blockers.Count,
                warnings);
        }

        public bool StartsAreConnected(AegisMapDocument document)
        {
            if (document == null || document.PlayerStarts == null || document.PlayerStarts.Count < 2)
                return true;

            for (var i = 1; i < document.PlayerStarts.Count; i++)
                if (!HasPath(document, new Int2(document.PlayerStarts[0].X, document.PlayerStarts[0].Y), new Int2(document.PlayerStarts[i].X, document.PlayerStarts[i].Y)))
                    return false;

            return true;
        }

        bool HasPath(AegisMapDocument document, Int2 start, Int2 goal)
        {
            var blocked = BuildBlockedSet(document);
            var terrain = BuildTerrainMap(document);
            var visited = new bool[document.Width * document.Height];
            var queue = new Queue<Int2>();
            queue.Enqueue(start);
            visited[Key(document.Width, start.X, start.Y)] = true;

            var directions = new[] { new Int2(1, 0), new Int2(-1, 0), new Int2(0, 1), new Int2(0, -1) };
            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                if (cell.Equals(goal))
                    return true;

                for (var i = 0; i < directions.Length; i++)
                {
                    var next = cell + directions[i];
                    if (next.X < 0 || next.Y < 0 || next.X >= document.Width || next.Y >= document.Height)
                        continue;

                    var key = Key(document.Width, next.X, next.Y);
                    if (visited[key] || blocked.Contains(key) || !CanTraverse(document, terrain, next))
                        continue;

                    visited[key] = true;
                    queue.Enqueue(next);
                }
            }

            return false;
        }

        static bool CanTraverse(AegisMapDocument document, Dictionary<int, string> terrain, Int2 cell)
        {
            string terrainId;
            if (!terrain.TryGetValue(Key(document.Width, cell.X, cell.Y), out terrainId))
                terrainId = document.DefaultTerrainId;
            var normalized = AegisMapTerrainIds.Normalize(terrainId);
            return normalized != AegisMapTerrainIds.Cliff && normalized != AegisMapTerrainIds.Water;
        }

        static HashSet<int> BuildBlockedSet(AegisMapDocument document)
        {
            var set = new HashSet<int>();
            if (document == null || document.Blockers == null)
                return set;
            for (var i = 0; i < document.Blockers.Count; i++)
                if (document.Blockers[i].BlocksGround)
                    set.Add(Key(document.Width, document.Blockers[i].X, document.Blockers[i].Y));
            return set;
        }

        static Dictionary<int, string> BuildTerrainMap(AegisMapDocument document)
        {
            var map = new Dictionary<int, string>();
            AddTerrain(map, document.Width, document.TerrainBase);
            AddTerrain(map, document.Width, document.TerrainOverlay);
            return map;
        }

        static void AddTerrain(Dictionary<int, string> map, int width, List<AegisTerrainCell> cells)
        {
            if (cells == null)
                return;
            for (var i = 0; i < cells.Count; i++)
                map[Key(width, cells[i].X, cells[i].Y)] = cells[i].TerrainId;
        }

        static int Key(int width, int x, int y)
        {
            return y * width + x;
        }
    }
}
