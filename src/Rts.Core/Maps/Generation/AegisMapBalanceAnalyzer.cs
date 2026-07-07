using System.Collections.Generic;
using ProjectAegisRTS.Core;

namespace ProjectAegisRTS.Maps.Generation
{
    public sealed class AegisMapBalanceAnalyzer
    {
        public AegisMapBalanceReport Analyze(AegisMapDocument document)
        {
            var warnings = new List<string>();
            var unreachablePlayerIds = new List<int>();
            var nearbyResources = new List<AegisPlayerResourceMetric>();
            var connectedPairs = 0;
            var totalPairs = 0;
            var minDistance = 0;
            var maxDistance = 0;
            var connected = AnalyzeStartConnectivity(document, unreachablePlayerIds, out connectedPairs, out totalPairs, out minDistance, out maxDistance);
            if (!connected)
                warnings.Add("StartsDisconnected:At least one player start cannot reach another start.");

            AnalyzeNearbyResources(document, nearbyResources, warnings);
            if (totalPairs > 1 && minDistance > 0 && maxDistance > minDistance * 2)
                warnings.Add("StartDistanceSpreadHigh:Start path distances vary by more than 2x.");

            return new AegisMapBalanceReport(
                connected,
                document == null || document.Resources == null ? 0 : document.Resources.Count,
                document == null || document.Blockers == null ? 0 : document.Blockers.Count,
                connectedPairs,
                totalPairs,
                minDistance,
                maxDistance,
                unreachablePlayerIds,
                nearbyResources,
                warnings);
        }

        public bool StartsAreConnected(AegisMapDocument document)
        {
            var unreachablePlayerIds = new List<int>();
            int connectedPairs;
            int totalPairs;
            int minDistance;
            int maxDistance;
            return AnalyzeStartConnectivity(document, unreachablePlayerIds, out connectedPairs, out totalPairs, out minDistance, out maxDistance);
        }

        bool AnalyzeStartConnectivity(
            AegisMapDocument document,
            List<int> unreachablePlayerIds,
            out int connectedPairs,
            out int totalPairs,
            out int minDistance,
            out int maxDistance)
        {
            connectedPairs = 0;
            totalPairs = 0;
            minDistance = 0;
            maxDistance = 0;

            if (document == null || document.PlayerStarts == null || document.PlayerStarts.Count < 2)
                return true;

            var blocked = BuildBlockedSet(document);
            var terrain = BuildTerrainMap(document);
            for (var i = 0; i < document.PlayerStarts.Count; i++)
                for (var j = i + 1; j < document.PlayerStarts.Count; j++)
                {
                    totalPairs++;
                    var a = document.PlayerStarts[i];
                    var b = document.PlayerStarts[j];
                    var distance = PathDistance(document, terrain, blocked, new Int2(a.X, a.Y), new Int2(b.X, b.Y));
                    if (distance < 0)
                    {
                        AddUnique(unreachablePlayerIds, a.PlayerId);
                        AddUnique(unreachablePlayerIds, b.PlayerId);
                        continue;
                    }

                    connectedPairs++;
                    if (minDistance == 0 || distance < minDistance)
                        minDistance = distance;
                    if (distance > maxDistance)
                        maxDistance = distance;
                }

            return connectedPairs == totalPairs;
        }

        int PathDistance(AegisMapDocument document, Dictionary<int, string> terrain, HashSet<int> blocked, Int2 start, Int2 goal)
        {
            var visited = new bool[document.Width * document.Height];
            var distances = new int[document.Width * document.Height];
            var queue = new Queue<Int2>();
            queue.Enqueue(start);
            var startKey = Key(document.Width, start.X, start.Y);
            visited[startKey] = true;
            distances[startKey] = 0;

            var directions = new[] { new Int2(1, 0), new Int2(-1, 0), new Int2(0, 1), new Int2(0, -1) };
            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                var currentDistance = distances[Key(document.Width, cell.X, cell.Y)];
                if (cell.Equals(goal))
                    return currentDistance;

                for (var i = 0; i < directions.Length; i++)
                {
                    var next = cell + directions[i];
                    if (next.X < 0 || next.Y < 0 || next.X >= document.Width || next.Y >= document.Height)
                        continue;

                    var key = Key(document.Width, next.X, next.Y);
                    if (visited[key] || blocked.Contains(key) || !CanTraverse(document, terrain, next))
                        continue;

                    visited[key] = true;
                    distances[key] = currentDistance + 1;
                    queue.Enqueue(next);
                }
            }

            return -1;
        }

        static void AnalyzeNearbyResources(AegisMapDocument document, List<AegisPlayerResourceMetric> metrics, List<string> warnings)
        {
            if (document == null || document.PlayerStarts == null || document.Resources == null)
                return;

            var radius = Clamp(System.Math.Min(document.Width, document.Height) / 4, 20, 64);
            var minAmount = 0;
            var maxAmount = 0;
            for (var i = 0; i < document.PlayerStarts.Count; i++)
            {
                var start = document.PlayerStarts[i];
                var amount = 0;
                var cells = 0;
                for (var r = 0; r < document.Resources.Count; r++)
                {
                    var resource = document.Resources[r];
                    var distance = Abs(resource.X - start.X) + Abs(resource.Y - start.Y);
                    if (distance > radius)
                        continue;
                    amount += resource.Amount;
                    cells++;
                }

                metrics.Add(new AegisPlayerResourceMetric(start.PlayerId, amount, cells));
                if (i == 0 || amount < minAmount)
                    minAmount = amount;
                if (amount > maxAmount)
                    maxAmount = amount;
            }

            if (metrics.Count > 1 && maxAmount > 0 && minAmount * 100 < maxAmount * 65)
                warnings.Add("NearbyResourceImbalance:Nearby resource amount differs by more than 35 percent between starts.");
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

        static void AddUnique(List<int> values, int value)
        {
            for (var i = 0; i < values.Count; i++)
                if (values[i] == value)
                    return;
            values.Add(value);
        }

        static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        static int Abs(int value)
        {
            return value < 0 ? -value : value;
        }
    }
}
