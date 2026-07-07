using System;
using System.Collections.Generic;
using ProjectAegisRTS.Terrain;

namespace ProjectAegisRTS.Maps
{
    public sealed class AegisMapDocument
    {
        public const string CurrentFormatVersion = "aegismap.v1";
        public const int MinWidth = 100;
        public const int MinHeight = 100;
        public const int MaxWidth = 400;
        public const int MaxHeight = 400;
        public const int SmallMapWidth = 100;
        public const int SmallMapHeight = 100;
        public const int MediumMapWidth = 200;
        public const int MediumMapHeight = 200;
        public const int LargeMapWidth = 400;
        public const int LargeMapHeight = 400;

        public const string TerrainBaseLayerName = "terrain_base";
        public const string TerrainOverlayLayerName = "terrain_overlay";
        public const string BlockersLayerName = "blockers";
        public const string ResourcesLayerName = "resources";
        public const string PlayerStartsLayerName = "player_starts";
        public const string ActorPlacementsLayerName = "actor_placements";
        public const string RegionsLayerName = "regions";
        public const string NavOverridesLayerName = "nav_overrides";

        public string FormatVersion { get; set; }
        public string MapId { get; set; }
        public string DisplayName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public string DefaultTerrainId { get; set; }
        public int StartingCredits { get; set; }
        public List<AegisTerrainCell> TerrainBase { get; set; }
        public List<AegisTerrainCell> TerrainOverlay { get; set; }
        public List<AegisBlockerCell> Blockers { get; set; }
        public List<AegisResourcePlacement> Resources { get; set; }
        public List<AegisPlayerStart> PlayerStarts { get; set; }
        public List<AegisActorPlacement> ActorPlacements { get; set; }
        public List<AegisRegion> Regions { get; set; }
        public List<AegisNavOverride> NavOverrides { get; set; }
        public Dictionary<string, string> Properties { get; set; }

        public AegisMapDocument()
        {
            FormatVersion = CurrentFormatVersion;
            MapId = string.Empty;
            DisplayName = string.Empty;
            TileWidth = 32;
            TileHeight = 32;
            DefaultTerrainId = AegisMapTerrainIds.Clear;
            StartingCredits = 5000;
            TerrainBase = new List<AegisTerrainCell>();
            TerrainOverlay = new List<AegisTerrainCell>();
            Blockers = new List<AegisBlockerCell>();
            Resources = new List<AegisResourcePlacement>();
            PlayerStarts = new List<AegisPlayerStart>();
            ActorPlacements = new List<AegisActorPlacement>();
            Regions = new List<AegisRegion>();
            NavOverrides = new List<AegisNavOverride>();
            Properties = new Dictionary<string, string>(StringComparer.Ordinal);
        }

        public static AegisMapDocument CreateEmpty(int width, int height, string mapId)
        {
            return new AegisMapDocument
            {
                MapId = string.IsNullOrEmpty(mapId) ? "aegis_map" : mapId,
                DisplayName = string.IsNullOrEmpty(mapId) ? "Aegis Map" : mapId,
                Width = width,
                Height = height
            };
        }
    }

    public sealed class AegisTerrainCell
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string TerrainId { get; set; }

        public AegisTerrainCell()
        {
            TerrainId = AegisMapTerrainIds.Clear;
        }

        public AegisTerrainCell(int x, int y, string terrainId)
        {
            X = x;
            Y = y;
            TerrainId = string.IsNullOrEmpty(terrainId) ? AegisMapTerrainIds.Clear : terrainId;
        }
    }

    public sealed class AegisBlockerCell
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool BlocksGround { get; set; }
        public string Reason { get; set; }

        public AegisBlockerCell()
        {
            BlocksGround = true;
            Reason = string.Empty;
        }

        public AegisBlockerCell(int x, int y, bool blocksGround, string reason)
        {
            X = x;
            Y = y;
            BlocksGround = blocksGround;
            Reason = reason ?? string.Empty;
        }
    }

    public sealed class AegisResourcePlacement
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string ResourceKind { get; set; }
        public int Amount { get; set; }

        public AegisResourcePlacement()
        {
            ResourceKind = "ore";
            Amount = 500;
        }

        public AegisResourcePlacement(int x, int y, string resourceKind, int amount)
        {
            X = x;
            Y = y;
            ResourceKind = string.IsNullOrEmpty(resourceKind) ? "ore" : resourceKind;
            Amount = amount;
        }
    }

    public sealed class AegisPlayerStart
    {
        public int PlayerId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; set; }

        public AegisPlayerStart()
        {
            Name = string.Empty;
        }

        public AegisPlayerStart(int playerId, int x, int y, string name)
        {
            PlayerId = playerId;
            X = x;
            Y = y;
            Name = name ?? string.Empty;
        }
    }

    public sealed class AegisActorPlacement
    {
        public string TypeId { get; set; }
        public int OwnerPlayerId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int FacingDegrees { get; set; }
        public string Tag { get; set; }

        public AegisActorPlacement()
        {
            TypeId = string.Empty;
            Tag = string.Empty;
        }

        public AegisActorPlacement(string typeId, int ownerPlayerId, int x, int y, int facingDegrees, string tag)
        {
            TypeId = typeId ?? string.Empty;
            OwnerPlayerId = ownerPlayerId;
            X = x;
            Y = y;
            FacingDegrees = facingDegrees;
            Tag = tag ?? string.Empty;
        }
    }

    public sealed class AegisRegion
    {
        public string RegionId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Purpose { get; set; }

        public AegisRegion()
        {
            RegionId = string.Empty;
            Purpose = string.Empty;
        }

        public AegisRegion(string regionId, int x, int y, int width, int height, string purpose)
        {
            RegionId = regionId ?? string.Empty;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Purpose = purpose ?? string.Empty;
        }
    }

    public sealed class AegisNavOverride
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string MovementClass { get; set; }
        public bool Passable { get; set; }
        public int Cost { get; set; }

        public AegisNavOverride()
        {
            MovementClass = string.Empty;
            Passable = true;
            Cost = 1;
        }
    }

    public static class AegisMapTerrainIds
    {
        public const string Clear = "clear";
        public const string Road = "road";
        public const string Rough = "rough";
        public const string Forest = "forest";
        public const string Water = "water";
        public const string Cliff = "cliff";
        public const string Ore = "ore";

        public static bool TryParseTerrainKind(string terrainId, out TerrainKind kind)
        {
            var normalized = Normalize(terrainId);
            if (normalized == Clear)
            {
                kind = TerrainKind.Clear;
                return true;
            }
            if (normalized == Road)
            {
                kind = TerrainKind.Road;
                return true;
            }
            if (normalized == Rough)
            {
                kind = TerrainKind.Rough;
                return true;
            }
            if (normalized == Forest)
            {
                kind = TerrainKind.Forest;
                return true;
            }
            if (normalized == Water)
            {
                kind = TerrainKind.Water;
                return true;
            }
            if (normalized == Cliff)
            {
                kind = TerrainKind.Cliff;
                return true;
            }
            if (normalized == Ore || normalized == "ore_field" || normalized == "orefield")
            {
                kind = TerrainKind.OreField;
                return true;
            }

            kind = TerrainKind.Clear;
            return false;
        }

        public static string ToTerrainId(TerrainKind kind)
        {
            switch (kind)
            {
                case TerrainKind.Road:
                    return Road;
                case TerrainKind.Rough:
                    return Rough;
                case TerrainKind.Forest:
                    return Forest;
                case TerrainKind.Water:
                    return Water;
                case TerrainKind.Cliff:
                    return Cliff;
                case TerrainKind.OreField:
                    return Ore;
                default:
                    return Clear;
            }
        }

        public static string Normalize(string terrainId)
        {
            return string.IsNullOrEmpty(terrainId) ? Clear : terrainId.Trim().ToLowerInvariant();
        }
    }
}
