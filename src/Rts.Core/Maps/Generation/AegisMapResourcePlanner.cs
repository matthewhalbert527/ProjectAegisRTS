using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Terrain;

namespace ProjectAegisRTS.Maps.Generation
{
    public sealed class AegisMapResourcePlanner
    {
        public void PlaceResources(
            AegisMapDocument document,
            TerrainKind[] terrain,
            bool[] protectedCells,
            AegisMapGenerationRequest request,
            AegisMapGenerationProfile profile)
        {
            var fieldIndex = 1;
            for (var i = 0; i < document.PlayerStarts.Count; i++)
            {
                var start = document.PlayerStarts[i];
                var directionX = start.X < document.Width / 2 ? 1 : -1;
                var directionY = start.Y < document.Height / 2 ? 1 : -1;
                for (var cluster = 0; cluster < profile.ResourceClustersPerPlayer; cluster++)
                {
                    var offsetY = (cluster % 2 == 0 ? -1 : 1) * (5 + cluster * 2);
                    var offsetX = 12 + cluster * 4;
                    var center = new Int2(start.X + directionX * offsetX, start.Y + directionY * offsetY);
                    PlaceCluster(document, terrain, protectedCells, profile, center, "ore_player_" + start.PlayerId + "_" + cluster, ref fieldIndex);
                }
            }

            var seed = request.ResolveSeed();
            for (var i = 0; i < profile.ExpansionResourceClusters; i++)
            {
                var x = 10 + AegisMapTerrainPlanner.HashToThousand(seed ^ 0x5EED, i + 3, 11) * (document.Width - 20) / 1000;
                var y = 10 + AegisMapTerrainPlanner.HashToThousand(seed ^ 0xA11E, i + 7, 17) * (document.Height - 20) / 1000;
                PlaceCluster(document, terrain, protectedCells, profile, new Int2(x, y), "ore_expansion_" + i, ref fieldIndex);
            }
        }

        void PlaceCluster(
            AegisMapDocument document,
            TerrainKind[] terrain,
            bool[] protectedCells,
            AegisMapGenerationProfile profile,
            Int2 center,
            string fieldPrefix,
            ref int fieldIndex)
        {
            var radius = profile.ResourceClusterRadius;
            for (var y = -radius; y <= radius; y++)
                for (var x = -radius; x <= radius; x++)
                {
                    if (Abs(x) + Abs(y) > radius + 1)
                        continue;

                    var cellX = center.X + x;
                    var cellY = center.Y + y;
                    if (cellX <= 1 || cellY <= 1 || cellX >= document.Width - 2 || cellY >= document.Height - 2)
                        continue;

                    var key = cellY * document.Width + cellX;
                    if (protectedCells[key])
                        continue;
                    if (terrain[key] == TerrainKind.Water || terrain[key] == TerrainKind.Cliff)
                        continue;
                    if (HasResourceAt(document, cellX, cellY))
                        continue;

                    terrain[key] = TerrainKind.OreField;
                    var resource = new AegisResourcePlacement(cellX, cellY, "ore", profile.ResourceAmountPerCell);
                    resource.FieldId = fieldPrefix + "_" + fieldIndex++;
                    resource.MaxAmount = profile.ResourceAmountPerCell;
                    resource.Regenerates = profile.RegenerationRatePerTick > 0;
                    resource.RegenerationRatePerTick = profile.RegenerationRatePerTick;
                    resource.RegenerationDelayTicks = profile.RegenerationDelayTicks;
                    document.Resources.Add(resource);
                }
        }

        static int Abs(int value)
        {
            return value < 0 ? -value : value;
        }

        static bool HasResourceAt(AegisMapDocument document, int x, int y)
        {
            for (var i = 0; i < document.Resources.Count; i++)
                if (document.Resources[i].X == x && document.Resources[i].Y == y)
                    return true;
            return false;
        }
    }

    public sealed class ResourceFieldDefinition
    {
        public string FieldId { get; private set; }
        public string ResourceKind { get; private set; }
        public int MaxAmount { get; private set; }
        public int RegenerationRatePerTick { get; private set; }
        public int RegenerationDelayTicks { get; private set; }

        public ResourceFieldDefinition(string fieldId, string resourceKind, int maxAmount, int regenerationRatePerTick, int regenerationDelayTicks)
        {
            FieldId = fieldId ?? string.Empty;
            ResourceKind = string.IsNullOrEmpty(resourceKind) ? "ore" : resourceKind;
            MaxAmount = maxAmount < 0 ? 0 : maxAmount;
            RegenerationRatePerTick = regenerationRatePerTick < 0 ? 0 : regenerationRatePerTick;
            RegenerationDelayTicks = regenerationDelayTicks < 0 ? 0 : regenerationDelayTicks;
        }
    }

    public sealed class ResourceRegenerationRules
    {
        public int DelayTicks { get; private set; }
        public int RatePerTick { get; private set; }

        public ResourceRegenerationRules(int delayTicks, int ratePerTick)
        {
            DelayTicks = delayTicks < 0 ? 0 : delayTicks;
            RatePerTick = ratePerTick < 0 ? 0 : ratePerTick;
        }
    }

    public sealed class ResourceFieldState
    {
        public string FieldId { get; private set; }
        public Int2 Cell { get; private set; }
        public string ResourceKind { get; private set; }
        public int Amount { get; set; }
        public int MaxAmount { get; private set; }
        public int LastHarvestTick { get; set; }
        public bool Visible { get; set; }

        public bool IsDepleted
        {
            get { return Amount <= 0; }
        }

        public ResourceFieldState(string fieldId, Int2 cell, string resourceKind, int amount, int maxAmount)
        {
            FieldId = fieldId ?? string.Empty;
            Cell = cell;
            ResourceKind = string.IsNullOrEmpty(resourceKind) ? "ore" : resourceKind;
            Amount = amount < 0 ? 0 : amount;
            MaxAmount = maxAmount < Amount ? Amount : maxAmount;
            LastHarvestTick = 0;
            Visible = true;
        }
    }

    public sealed class ResourceHarvestResult
    {
        public bool Success { get; private set; }
        public int HarvestedAmount { get; private set; }
        public string ErrorCode { get; private set; }

        ResourceHarvestResult(bool success, int harvestedAmount, string errorCode)
        {
            Success = success;
            HarvestedAmount = harvestedAmount;
            ErrorCode = errorCode ?? string.Empty;
        }

        public static ResourceHarvestResult Ok(int amount)
        {
            return new ResourceHarvestResult(true, amount, string.Empty);
        }

        public static ResourceHarvestResult Fail(string errorCode)
        {
            return new ResourceHarvestResult(false, 0, errorCode);
        }
    }

    public sealed class AegisResourceSimulation
    {
        readonly Dictionary<string, ResourceFieldState> fields;
        readonly ResourceRegenerationRules rules;

        public int TickNumber { get; private set; }

        public AegisResourceSimulation(ResourceRegenerationRules rules)
        {
            this.rules = rules ?? new ResourceRegenerationRules(0, 0);
            fields = new Dictionary<string, ResourceFieldState>();
            TickNumber = 0;
        }

        public void AddField(ResourceFieldState field)
        {
            fields[field.FieldId] = field;
        }

        public ResourceFieldState GetField(string fieldId)
        {
            return fields[fieldId];
        }

        public ResourceHarvestResult Harvest(string fieldId, int requestedAmount)
        {
            ResourceFieldState field;
            if (!fields.TryGetValue(fieldId, out field))
                return ResourceHarvestResult.Fail("ResourceFieldMissing");
            if (field.IsDepleted)
                return ResourceHarvestResult.Fail("ResourceDepleted");
            if (requestedAmount <= 0)
                return ResourceHarvestResult.Fail("HarvestAmountInvalid");

            var harvested = requestedAmount > field.Amount ? field.Amount : requestedAmount;
            field.Amount -= harvested;
            field.LastHarvestTick = TickNumber;
            field.Visible = !field.IsDepleted;
            return ResourceHarvestResult.Ok(harvested);
        }

        public void Tick()
        {
            TickNumber++;
            foreach (var field in fields.Values)
            {
                if (rules.RatePerTick <= 0 || field.Amount >= field.MaxAmount)
                    continue;
                if (TickNumber - field.LastHarvestTick < rules.DelayTicks)
                    continue;

                field.Amount += rules.RatePerTick;
                if (field.Amount > field.MaxAmount)
                    field.Amount = field.MaxAmount;
                field.Visible = field.Amount > 0;
            }
        }
    }
}
