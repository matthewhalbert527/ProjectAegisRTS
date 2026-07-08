using System;
using System.Collections.Generic;
using ProjectAegisRTS.Maps;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.MapEditor
{
    public static class AegisRuntimeMapDocumentJson
    {
        public static AegisMapDocument FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentException("Aegis map JSON is empty.", "json");

            var dto = JsonUtility.FromJson<AegisRuntimeMapDocumentDto>(json);
            if (dto == null)
                throw new InvalidOperationException("Could not parse Aegis map JSON.");

            return dto.ToDocument();
        }
    }

    [Serializable]
    public sealed class AegisRuntimeMapDocumentDto
    {
        public string formatVersion;
        public string mapId;
        public string displayName;
        public int width;
        public int height;
        public int tileWidth;
        public int tileHeight;
        public string defaultTerrainId;
        public int startingCredits;
        public AegisRuntimeTerrainCellDto[] terrainBase;
        public AegisRuntimeTerrainCellDto[] terrainOverlay;
        public AegisRuntimeBlockerCellDto[] blockers;
        public AegisRuntimeResourcePlacementDto[] resources;
        public AegisRuntimePlayerStartDto[] playerStarts;
        public AegisRuntimeActorPlacementDto[] actorPlacements;
        public AegisRuntimeRegionDto[] regions;
        public AegisRuntimeNavOverrideDto[] navOverrides;

        public AegisMapDocument ToDocument()
        {
            var document = new AegisMapDocument
            {
                FormatVersion = string.IsNullOrEmpty(formatVersion) ? AegisMapDocument.CurrentFormatVersion : formatVersion,
                MapId = mapId ?? string.Empty,
                DisplayName = displayName ?? string.Empty,
                Width = width,
                Height = height,
                TileWidth = tileWidth <= 0 ? 32 : tileWidth,
                TileHeight = tileHeight <= 0 ? 32 : tileHeight,
                DefaultTerrainId = string.IsNullOrEmpty(defaultTerrainId) ? AegisMapTerrainIds.Clear : defaultTerrainId,
                StartingCredits = startingCredits <= 0 ? 5000 : startingCredits,
                TerrainBase = new List<AegisTerrainCell>(),
                TerrainOverlay = new List<AegisTerrainCell>(),
                Blockers = new List<AegisBlockerCell>(),
                Resources = new List<AegisResourcePlacement>(),
                PlayerStarts = new List<AegisPlayerStart>(),
                ActorPlacements = new List<AegisActorPlacement>(),
                Regions = new List<AegisRegion>(),
                NavOverrides = new List<AegisNavOverride>()
            };

            AppendTerrain(document.TerrainBase, terrainBase);
            AppendTerrain(document.TerrainOverlay, terrainOverlay);
            AppendBlockers(document.Blockers, blockers);
            AppendResources(document.Resources, resources);
            AppendPlayerStarts(document.PlayerStarts, playerStarts);
            AppendActorPlacements(document.ActorPlacements, actorPlacements);
            AppendRegions(document.Regions, regions);
            AppendNavOverrides(document.NavOverrides, navOverrides);
            return document;
        }

        static void AppendTerrain(List<AegisTerrainCell> target, AegisRuntimeTerrainCellDto[] source)
        {
            if (source == null)
                return;

            for (var i = 0; i < source.Length; i++)
                if (source[i] != null)
                    target.Add(new AegisTerrainCell(source[i].x, source[i].y, source[i].terrainId));
        }

        static void AppendBlockers(List<AegisBlockerCell> target, AegisRuntimeBlockerCellDto[] source)
        {
            if (source == null)
                return;

            for (var i = 0; i < source.Length; i++)
                if (source[i] != null)
                    target.Add(new AegisBlockerCell(source[i].x, source[i].y, source[i].blocksGround, source[i].reason));
        }

        static void AppendResources(List<AegisResourcePlacement> target, AegisRuntimeResourcePlacementDto[] source)
        {
            if (source == null)
                return;

            for (var i = 0; i < source.Length; i++)
            {
                var cell = source[i];
                if (cell == null)
                    continue;

                target.Add(new AegisResourcePlacement
                {
                    FieldId = cell.fieldId ?? string.Empty,
                    X = cell.x,
                    Y = cell.y,
                    ResourceKind = string.IsNullOrEmpty(cell.resourceKind) ? "ore" : cell.resourceKind,
                    Amount = cell.amount,
                    MaxAmount = cell.maxAmount <= 0 ? cell.amount : cell.maxAmount,
                    Regenerates = cell.regenerates,
                    RegenerationRatePerTick = cell.regenerationRatePerTick,
                    RegenerationDelayTicks = cell.regenerationDelayTicks
                });
            }
        }

        static void AppendPlayerStarts(List<AegisPlayerStart> target, AegisRuntimePlayerStartDto[] source)
        {
            if (source == null)
                return;

            for (var i = 0; i < source.Length; i++)
                if (source[i] != null)
                    target.Add(new AegisPlayerStart(source[i].playerId, source[i].x, source[i].y, source[i].name));
        }

        static void AppendActorPlacements(List<AegisActorPlacement> target, AegisRuntimeActorPlacementDto[] source)
        {
            if (source == null)
                return;

            for (var i = 0; i < source.Length; i++)
                if (source[i] != null)
                    target.Add(new AegisActorPlacement(source[i].typeId, source[i].ownerPlayerId, source[i].x, source[i].y, source[i].facingDegrees, source[i].tag));
        }

        static void AppendRegions(List<AegisRegion> target, AegisRuntimeRegionDto[] source)
        {
            if (source == null)
                return;

            for (var i = 0; i < source.Length; i++)
                if (source[i] != null)
                    target.Add(new AegisRegion(source[i].regionId, source[i].x, source[i].y, source[i].width, source[i].height, source[i].purpose));
        }

        static void AppendNavOverrides(List<AegisNavOverride> target, AegisRuntimeNavOverrideDto[] source)
        {
            if (source == null)
                return;

            for (var i = 0; i < source.Length; i++)
                if (source[i] != null)
                    target.Add(new AegisNavOverride
                    {
                        X = source[i].x,
                        Y = source[i].y,
                        MovementClass = source[i].movementClass ?? string.Empty,
                        Passable = source[i].passable,
                        Cost = source[i].cost
                    });
        }
    }

    [Serializable]
    public sealed class AegisRuntimeTerrainCellDto
    {
        public int x;
        public int y;
        public string terrainId;
    }

    [Serializable]
    public sealed class AegisRuntimeBlockerCellDto
    {
        public int x;
        public int y;
        public bool blocksGround;
        public string reason;
    }

    [Serializable]
    public sealed class AegisRuntimeResourcePlacementDto
    {
        public string fieldId;
        public int x;
        public int y;
        public string resourceKind;
        public int amount;
        public int maxAmount;
        public bool regenerates;
        public int regenerationRatePerTick;
        public int regenerationDelayTicks;
    }

    [Serializable]
    public sealed class AegisRuntimePlayerStartDto
    {
        public int playerId;
        public int x;
        public int y;
        public string name;
    }

    [Serializable]
    public sealed class AegisRuntimeActorPlacementDto
    {
        public string typeId;
        public int ownerPlayerId;
        public int x;
        public int y;
        public int facingDegrees;
        public string tag;
    }

    [Serializable]
    public sealed class AegisRuntimeRegionDto
    {
        public string regionId;
        public int x;
        public int y;
        public int width;
        public int height;
        public string purpose;
    }

    [Serializable]
    public sealed class AegisRuntimeNavOverrideDto
    {
        public int x;
        public int y;
        public string movementClass;
        public bool passable;
        public int cost;
    }
}
