using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjectAegisRTS.Maps.Tiled
{
    public sealed class AegisTiledMapExporter
    {
        static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public string ExportToJson(AegisMapDocument document)
        {
            var validation = new AegisMapDocumentValidator().Validate(document);
            if (!validation.Success)
                throw new InvalidOperationException("Aegis map document is invalid: " + string.Join("; ", validation.Errors));

            var tiled = ToTiledMap(document);
            return JsonSerializer.Serialize(tiled, JsonOptions);
        }

        public TiledMapDto ToTiledMap(AegisMapDocument document)
        {
            var tileWidth = document.TileWidth <= 0 ? 32 : document.TileWidth;
            var tileHeight = document.TileHeight <= 0 ? 32 : document.TileHeight;
            var map = new TiledMapDto
            {
                Width = document.Width,
                Height = document.Height,
                TileWidth = tileWidth,
                TileHeight = tileHeight,
                Infinite = false,
                NextLayerId = 9,
                NextObjectId = 1
            };

            map.Properties.Add(new TiledPropertyDto("aegisMapId", "string", document.MapId));
            map.Properties.Add(new TiledPropertyDto("displayName", "string", document.DisplayName));
            map.TileSets.Add(CreateInlineTileset(tileWidth, tileHeight));

            var nextObjectId = 1;
            map.Layers.Add(TileLayer(1, AegisMapDocument.TerrainBaseLayerName, document.Width, document.Height, BuildTerrainData(document)));
            map.Layers.Add(TileLayer(2, AegisMapDocument.TerrainOverlayLayerName, document.Width, document.Height, BuildSparseTerrainData(document.Width, document.Height, document.TerrainOverlay)));
            map.Layers.Add(TileLayer(3, AegisMapDocument.BlockersLayerName, document.Width, document.Height, BuildBlockerData(document)));
            map.Layers.Add(TileLayer(4, AegisMapDocument.ResourcesLayerName, document.Width, document.Height, BuildResourceData(document)));
            map.Layers.Add(ObjectLayer(5, AegisMapDocument.PlayerStartsLayerName, BuildPlayerStartObjects(document, tileWidth, tileHeight, ref nextObjectId)));
            map.Layers.Add(ObjectLayer(6, AegisMapDocument.ActorPlacementsLayerName, BuildActorObjects(document, tileWidth, tileHeight, ref nextObjectId)));
            map.Layers.Add(ObjectLayer(7, AegisMapDocument.RegionsLayerName, BuildRegionObjects(document, tileWidth, tileHeight, ref nextObjectId)));
            map.Layers.Add(TileLayer(8, AegisMapDocument.NavOverridesLayerName, document.Width, document.Height, BuildNavData(document)));
            map.NextObjectId = nextObjectId;

            return map;
        }

        static TiledLayerDto TileLayer(int id, string name, int width, int height, int[] data)
        {
            return new TiledLayerDto
            {
                Id = id,
                Name = name,
                Type = "tilelayer",
                Width = width,
                Height = height,
                Data = data
            };
        }

        static TiledLayerDto ObjectLayer(int id, string name, List<TiledObjectDto> objects)
        {
            return new TiledLayerDto
            {
                Id = id,
                Name = name,
                Type = "objectgroup",
                Objects = objects
            };
        }

        static TiledTileSetDto CreateInlineTileset(int tileWidth, int tileHeight)
        {
            var set = new TiledTileSetDto
            {
                FirstGid = 1,
                Name = "aegis_starter_tiles",
                TileWidth = tileWidth,
                TileHeight = tileHeight,
                TileCount = 7,
                Columns = 7
            };

            set.Tiles.Add(Tile(0, AegisMapTerrainIds.Clear));
            set.Tiles.Add(Tile(1, AegisMapTerrainIds.Road));
            set.Tiles.Add(Tile(2, AegisMapTerrainIds.Rough));
            set.Tiles.Add(Tile(3, AegisMapTerrainIds.Forest));
            set.Tiles.Add(Tile(4, AegisMapTerrainIds.Water));
            set.Tiles.Add(Tile(5, AegisMapTerrainIds.Cliff));
            set.Tiles.Add(Tile(6, AegisMapTerrainIds.Ore));
            return set;
        }

        static TiledTileDto Tile(int id, string terrainId)
        {
            var tile = new TiledTileDto { Id = id };
            tile.Properties.Add(new TiledPropertyDto("aegisTerrainId", "string", terrainId));
            return tile;
        }

        static int[] BuildTerrainData(AegisMapDocument document)
        {
            var data = new int[document.Width * document.Height];
            var defaultGid = GidForTerrainId(document.DefaultTerrainId);
            for (var i = 0; i < data.Length; i++)
                data[i] = defaultGid;

            if (document.TerrainBase != null)
                for (var i = 0; i < document.TerrainBase.Count; i++)
                {
                    var cell = document.TerrainBase[i];
                    data[cell.Y * document.Width + cell.X] = GidForTerrainId(cell.TerrainId);
                }

            return data;
        }

        static int[] BuildSparseTerrainData(int width, int height, List<AegisTerrainCell> terrainCells)
        {
            var data = new int[width * height];
            if (terrainCells != null)
                for (var i = 0; i < terrainCells.Count; i++)
                {
                    var cell = terrainCells[i];
                    data[cell.Y * width + cell.X] = GidForTerrainId(cell.TerrainId);
                }
            return data;
        }

        static int[] BuildBlockerData(AegisMapDocument document)
        {
            var data = new int[document.Width * document.Height];
            if (document.Blockers != null)
                for (var i = 0; i < document.Blockers.Count; i++)
                {
                    var blocker = document.Blockers[i];
                    if (blocker.BlocksGround)
                        data[blocker.Y * document.Width + blocker.X] = 1;
                }
            return data;
        }

        static int[] BuildResourceData(AegisMapDocument document)
        {
            var data = new int[document.Width * document.Height];
            if (document.Resources != null)
                for (var i = 0; i < document.Resources.Count; i++)
                {
                    var resource = document.Resources[i];
                    data[resource.Y * document.Width + resource.X] = 7;
                }
            return data;
        }

        static int[] BuildNavData(AegisMapDocument document)
        {
            var data = new int[document.Width * document.Height];
            if (document.NavOverrides != null)
                for (var i = 0; i < document.NavOverrides.Count; i++)
                {
                    var nav = document.NavOverrides[i];
                    data[nav.Y * document.Width + nav.X] = nav.Passable ? 1 : 2;
                }
            return data;
        }

        static List<TiledObjectDto> BuildPlayerStartObjects(AegisMapDocument document, int tileWidth, int tileHeight, ref int nextObjectId)
        {
            var objects = new List<TiledObjectDto>();
            if (document.PlayerStarts == null)
                return objects;
            for (var i = 0; i < document.PlayerStarts.Count; i++)
            {
                var start = document.PlayerStarts[i];
                var obj = PointObject(ref nextObjectId, start.Name, "player_start", start.X, start.Y, tileWidth, tileHeight);
                obj.Properties.Add(new TiledPropertyDto("playerId", "int", start.PlayerId));
                objects.Add(obj);
            }
            return objects;
        }

        static List<TiledObjectDto> BuildActorObjects(AegisMapDocument document, int tileWidth, int tileHeight, ref int nextObjectId)
        {
            var objects = new List<TiledObjectDto>();
            if (document.ActorPlacements == null)
                return objects;
            for (var i = 0; i < document.ActorPlacements.Count; i++)
            {
                var actor = document.ActorPlacements[i];
                var obj = PointObject(ref nextObjectId, actor.Tag, actor.TypeId, actor.X, actor.Y, tileWidth, tileHeight);
                obj.Rotation = actor.FacingDegrees;
                obj.Properties.Add(new TiledPropertyDto("typeId", "string", actor.TypeId));
                obj.Properties.Add(new TiledPropertyDto("ownerPlayerId", "int", actor.OwnerPlayerId));
                obj.Properties.Add(new TiledPropertyDto("facingDegrees", "int", actor.FacingDegrees));
                objects.Add(obj);
            }
            return objects;
        }

        static List<TiledObjectDto> BuildRegionObjects(AegisMapDocument document, int tileWidth, int tileHeight, ref int nextObjectId)
        {
            var objects = new List<TiledObjectDto>();
            if (document.Regions == null)
                return objects;
            for (var i = 0; i < document.Regions.Count; i++)
            {
                var region = document.Regions[i];
                var obj = new TiledObjectDto
                {
                    Id = nextObjectId++,
                    Name = region.RegionId,
                    Type = "region",
                    X = region.X * tileWidth,
                    Y = region.Y * tileHeight,
                    Width = region.Width * tileWidth,
                    Height = region.Height * tileHeight,
                    Visible = true
                };
                obj.Properties.Add(new TiledPropertyDto("regionId", "string", region.RegionId));
                obj.Properties.Add(new TiledPropertyDto("purpose", "string", region.Purpose));
                objects.Add(obj);
            }
            return objects;
        }

        static TiledObjectDto PointObject(ref int nextObjectId, string name, string type, int x, int y, int tileWidth, int tileHeight)
        {
            return new TiledObjectDto
            {
                Id = nextObjectId++,
                Name = name ?? string.Empty,
                Type = type ?? string.Empty,
                X = x * tileWidth,
                Y = y * tileHeight,
                Width = 0,
                Height = 0,
                Point = true,
                Visible = true
            };
        }

        static int GidForTerrainId(string terrainId)
        {
            switch (AegisMapTerrainIds.Normalize(terrainId))
            {
                case AegisMapTerrainIds.Road:
                    return 2;
                case AegisMapTerrainIds.Rough:
                    return 3;
                case AegisMapTerrainIds.Forest:
                    return 4;
                case AegisMapTerrainIds.Water:
                    return 5;
                case AegisMapTerrainIds.Cliff:
                    return 6;
                case AegisMapTerrainIds.Ore:
                case "ore_field":
                    return 7;
                default:
                    return 1;
            }
        }
    }
}
