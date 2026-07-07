using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Demo;
using ProjectAegisRTS.Economy;
using ProjectAegisRTS.Simulation;
using ProjectAegisRTS.Terrain;

namespace ProjectAegisRTS.Maps
{
    public sealed class AegisMapDocumentWorldFactory
    {
        readonly AegisMapDocumentValidator validator;

        public AegisMapDocumentWorldFactory()
        {
            validator = new AegisMapDocumentValidator();
        }

        public RtsWorld CreateWorld(AegisMapDocument document)
        {
            return CreateWorld(document, DemoRules.CreateDefaultRules());
        }

        public RtsWorld CreateWorld(AegisMapDocument document, RtsRules rules)
        {
            if (rules == null)
                throw new ArgumentNullException("rules");

            var validation = validator.Validate(document, rules);
            if (!validation.Success)
                throw new InvalidOperationException("Aegis map document is invalid: " + string.Join("; ", validation.Errors));

            var gridMap = CreateGridMap(document);
            var world = new RtsWorld(rules, gridMap);

            foreach (var playerId in CollectPlayerIds(document))
                world.AddPlayer(playerId, "Player " + playerId, document.StartingCredits <= 0 ? 5000 : document.StartingCredits);

            if (document.Resources != null)
                for (var i = 0; i < document.Resources.Count; i++)
                {
                    var resource = document.Resources[i];
                    world.AddResourceCell(new Int2(resource.X, resource.Y), ParseResourceKind(resource.ResourceKind), resource.Amount);
                }

            if (document.ActorPlacements != null)
                for (var i = 0; i < document.ActorPlacements.Count; i++)
                {
                    var actor = document.ActorPlacements[i];
                    var state = world.CreateActor(actor.TypeId, actor.OwnerPlayerId, new Int2(actor.X, actor.Y));
                    state.FacingDegrees = actor.FacingDegrees;
                }

            return world;
        }

        public GridMap CreateGridMap(AegisMapDocument document)
        {
            var validation = validator.Validate(document);
            if (!validation.Success)
                throw new InvalidOperationException("Aegis map document is invalid: " + string.Join("; ", validation.Errors));

            TerrainKind defaultTerrain;
            if (!AegisMapTerrainIds.TryParseTerrainKind(document.DefaultTerrainId, out defaultTerrain))
                defaultTerrain = TerrainKind.Clear;

            var map = new GridMap(document.Width, document.Height);
            for (var y = 0; y < document.Height; y++)
                for (var x = 0; x < document.Width; x++)
                    map.SetTerrainKind(new Int2(x, y), defaultTerrain);

            ApplyTerrainLayer(map, document.TerrainBase);
            ApplyTerrainLayer(map, document.TerrainOverlay);

            if (document.Blockers != null)
                for (var i = 0; i < document.Blockers.Count; i++)
                {
                    var blocker = document.Blockers[i];
                    if (blocker.BlocksGround)
                        map.SetBlocked(new Int2(blocker.X, blocker.Y), true);
                }

            return map;
        }

        static void ApplyTerrainLayer(GridMap map, List<AegisTerrainCell> cells)
        {
            if (cells == null)
                return;

            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                TerrainKind kind;
                if (AegisMapTerrainIds.TryParseTerrainKind(cell.TerrainId, out kind))
                    map.SetTerrainKind(new Int2(cell.X, cell.Y), kind);
            }
        }

        static IReadOnlyList<int> CollectPlayerIds(AegisMapDocument document)
        {
            var ids = new List<int>();
            if (document.PlayerStarts != null)
                for (var i = 0; i < document.PlayerStarts.Count; i++)
                    AddUnique(ids, document.PlayerStarts[i].PlayerId);

            if (document.ActorPlacements != null)
                for (var i = 0; i < document.ActorPlacements.Count; i++)
                    AddUnique(ids, document.ActorPlacements[i].OwnerPlayerId);

            ids.Sort();
            return ids;
        }

        static void AddUnique(List<int> ids, int playerId)
        {
            if (playerId <= 0)
                return;
            if (!ids.Contains(playerId))
                ids.Add(playerId);
        }

        static ResourceKind ParseResourceKind(string resourceKind)
        {
            if (string.Equals(resourceKind, "ore", StringComparison.OrdinalIgnoreCase))
                return ResourceKind.Ore;

            return ResourceKind.Ore;
        }
    }
}
