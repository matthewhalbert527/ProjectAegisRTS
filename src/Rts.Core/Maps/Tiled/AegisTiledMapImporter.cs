using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using ProjectAegisRTS.Terrain;

namespace ProjectAegisRTS.Maps.Tiled
{
    public sealed class AegisTiledMapImporter
    {
        static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public AegisTiledImportResult ImportFile(string path, AegisTiledImportOptions options)
        {
            return ImportJson(File.ReadAllText(path), options);
        }

        public AegisTiledImportResult ImportJson(string json, AegisTiledImportOptions options)
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            var importOptions = options ?? new AegisTiledImportOptions();

            TiledMapDto tiled;
            try
            {
                tiled = JsonSerializer.Deserialize<TiledMapDto>(json, JsonOptions);
            }
            catch (Exception ex)
            {
                errors.Add("TiledJsonInvalid:" + ex.Message);
                return AegisTiledImportResult.Fail(errors, warnings);
            }

            if (tiled == null)
            {
                errors.Add("TiledMapMissing:The Tiled JSON map is empty.");
                return AegisTiledImportResult.Fail(errors, warnings);
            }

            ValidateMapHeader(tiled, errors);
            if (errors.Count != 0)
                return AegisTiledImportResult.Fail(errors, warnings);

            var document = AegisMapDocument.CreateEmpty(
                tiled.Width,
                tiled.Height,
                GetStringProperty(tiled.Properties, "aegisMapId", importOptions.DefaultMapId));
            document.DisplayName = GetStringProperty(tiled.Properties, "displayName", importOptions.DefaultDisplayName);
            document.TileWidth = tiled.TileWidth <= 0 ? 32 : tiled.TileWidth;
            document.TileHeight = tiled.TileHeight <= 0 ? 32 : tiled.TileHeight;
            document.DefaultTerrainId = importOptions.DefaultTerrainId;

            foreach (var property in SafeProperties(tiled.Properties))
                document.Properties[property.Name] = PropertyValueToString(property.Value);

            var gidTerrainIds = BuildTerrainGidMap(tiled.TileSets);
            var layers = tiled.Layers ?? new List<TiledLayerDto>();
            for (var i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];
                if (layer == null)
                {
                    errors.Add("TiledLayerNull:" + i);
                    continue;
                }

                var layerName = layer.Name ?? string.Empty;
                if (string.Equals(layer.Type, "tilelayer", StringComparison.OrdinalIgnoreCase))
                    ImportTileLayer(tiled, layer, layerName, gidTerrainIds, importOptions, document, errors, warnings);
                else if (string.Equals(layer.Type, "objectgroup", StringComparison.OrdinalIgnoreCase))
                    ImportObjectLayer(tiled, layer, layerName, importOptions, document, warnings);
                else
                    warnings.Add("UnsupportedLayerType:" + layerName + ":" + layer.Type);
            }

            if (errors.Count != 0)
                return AegisTiledImportResult.Fail(errors, warnings);

            var validation = new AegisMapDocumentValidator().Validate(document);
            if (!validation.Success)
            {
                foreach (var error in validation.Errors)
                    errors.Add("AegisMapInvalid:" + error);
                foreach (var warning in validation.Warnings)
                    warnings.Add("AegisMapWarning:" + warning);
                return AegisTiledImportResult.Fail(errors, warnings);
            }

            foreach (var warning in validation.Warnings)
                warnings.Add("AegisMapWarning:" + warning);

            return AegisTiledImportResult.Ok(document, warnings);
        }

        static void ValidateMapHeader(TiledMapDto tiled, List<string> errors)
        {
            if (tiled.Infinite)
                errors.Add("TiledInfiniteMapUnsupported:Project Aegis imports finite Tiled maps only.");
            if (!string.Equals(tiled.Orientation, "orthogonal", StringComparison.OrdinalIgnoreCase))
                errors.Add("TiledOrientationUnsupported:Only orthogonal maps are supported.");
            if (tiled.Width < AegisMapDocument.MinWidth || tiled.Height < AegisMapDocument.MinHeight)
                errors.Add("TiledMapTooSmall:Maps must be at least 100x100.");
            if (tiled.Width > AegisMapDocument.MaxWidth || tiled.Height > AegisMapDocument.MaxHeight)
                errors.Add("TiledMapTooLarge:Maps must be at most 400x400.");
        }

        static void ImportTileLayer(
            TiledMapDto tiled,
            TiledLayerDto layer,
            string layerName,
            Dictionary<int, string> gidTerrainIds,
            AegisTiledImportOptions options,
            AegisMapDocument document,
            List<string> errors,
            List<string> warnings)
        {
            var data = DecodeTileLayer(tiled, layer, layerName, errors);
            if (data == null)
                return;

            if (string.Equals(layerName, AegisMapDocument.TerrainBaseLayerName, StringComparison.Ordinal))
            {
                ImportTerrainCells(data, document.Width, document.Height, document.DefaultTerrainId, gidTerrainIds, document.TerrainBase);
                return;
            }
            if (string.Equals(layerName, AegisMapDocument.TerrainOverlayLayerName, StringComparison.Ordinal))
            {
                ImportTerrainCells(data, document.Width, document.Height, string.Empty, gidTerrainIds, document.TerrainOverlay);
                return;
            }
            if (string.Equals(layerName, AegisMapDocument.BlockersLayerName, StringComparison.Ordinal))
            {
                for (var i = 0; i < data.Count; i++)
                    if (data[i] != 0)
                        document.Blockers.Add(new AegisBlockerCell(i % document.Width, i / document.Width, true, "tiled_blocker"));
                return;
            }
            if (string.Equals(layerName, AegisMapDocument.ResourcesLayerName, StringComparison.Ordinal))
            {
                for (var i = 0; i < data.Count; i++)
                    if (data[i] != 0)
                        document.Resources.Add(new AegisResourcePlacement(i % document.Width, i / document.Width, options.DefaultResourceKind, options.DefaultResourceAmount));
                return;
            }
            if (string.Equals(layerName, AegisMapDocument.NavOverridesLayerName, StringComparison.Ordinal))
            {
                for (var i = 0; i < data.Count; i++)
                    if (data[i] != 0)
                        document.NavOverrides.Add(new AegisNavOverride { X = i % document.Width, Y = i / document.Width, MovementClass = "ground", Passable = false, Cost = 0 });
                return;
            }

            warnings.Add("UnknownTileLayerIgnored:" + layerName);
        }

        static void ImportTerrainCells(List<int> data, int width, int height, string defaultTerrainId, Dictionary<int, string> gidTerrainIds, List<AegisTerrainCell> target)
        {
            for (var i = 0; i < data.Count; i++)
            {
                var gid = data[i];
                if (gid == 0)
                    continue;

                var terrainId = TerrainIdForGid(gidTerrainIds, gid);
                if (!string.IsNullOrEmpty(defaultTerrainId) && string.Equals(terrainId, defaultTerrainId, StringComparison.OrdinalIgnoreCase))
                    continue;

                target.Add(new AegisTerrainCell(i % width, i / width, terrainId));
            }
        }

        static void ImportObjectLayer(
            TiledMapDto tiled,
            TiledLayerDto layer,
            string layerName,
            AegisTiledImportOptions options,
            AegisMapDocument document,
            List<string> warnings)
        {
            var objects = layer.Objects ?? new List<TiledObjectDto>();
            for (var i = 0; i < objects.Count; i++)
            {
                var obj = objects[i];
                if (obj == null)
                    continue;

                if (string.Equals(layerName, AegisMapDocument.PlayerStartsLayerName, StringComparison.Ordinal))
                {
                    var playerId = GetIntProperty(obj.Properties, "playerId", GuessPlayerId(obj, i + 1));
                    var cell = ObjectCell(tiled, obj);
                    document.PlayerStarts.Add(new AegisPlayerStart(playerId, cell.X, cell.Y, obj.Name));
                }
                else if (string.Equals(layerName, AegisMapDocument.ActorPlacementsLayerName, StringComparison.Ordinal))
                {
                    var typeId = GetStringProperty(obj.Properties, "typeId", !string.IsNullOrEmpty(obj.Type) ? obj.Type : obj.Name);
                    var owner = GetIntProperty(obj.Properties, "ownerPlayerId", GetIntProperty(obj.Properties, "playerId", 1));
                    var facing = GetIntProperty(obj.Properties, "facingDegrees", (int)obj.Rotation);
                    var cell = ObjectCell(tiled, obj);
                    document.ActorPlacements.Add(new AegisActorPlacement(typeId, owner, cell.X, cell.Y, facing, obj.Name));
                }
                else if (string.Equals(layerName, AegisMapDocument.ResourcesLayerName, StringComparison.Ordinal))
                {
                    var kind = GetStringProperty(obj.Properties, "resourceKind", options.DefaultResourceKind);
                    var amount = GetIntProperty(obj.Properties, "amount", options.DefaultResourceAmount);
                    foreach (var cell in ObjectCells(tiled, obj))
                        document.Resources.Add(new AegisResourcePlacement(cell.X, cell.Y, kind, amount));
                }
                else if (string.Equals(layerName, AegisMapDocument.RegionsLayerName, StringComparison.Ordinal))
                {
                    var cell = ObjectCell(tiled, obj);
                    var width = Math.Max(1, CeilingCells(obj.Width, tiled.TileWidth));
                    var height = Math.Max(1, CeilingCells(obj.Height, tiled.TileHeight));
                    var id = GetStringProperty(obj.Properties, "regionId", string.IsNullOrEmpty(obj.Name) ? "region_" + obj.Id : obj.Name);
                    var purpose = GetStringProperty(obj.Properties, "purpose", obj.Type);
                    document.Regions.Add(new AegisRegion(id, cell.X, cell.Y, width, height, purpose));
                }
                else if (string.Equals(layerName, AegisMapDocument.NavOverridesLayerName, StringComparison.Ordinal))
                {
                    var cell = ObjectCell(tiled, obj);
                    document.NavOverrides.Add(new AegisNavOverride
                    {
                        X = cell.X,
                        Y = cell.Y,
                        MovementClass = GetStringProperty(obj.Properties, "movementClass", "ground"),
                        Passable = GetBoolProperty(obj.Properties, "passable", true),
                        Cost = GetIntProperty(obj.Properties, "cost", 1)
                    });
                }
                else if (string.Equals(layerName, AegisMapDocument.BlockersLayerName, StringComparison.Ordinal))
                {
                    foreach (var cell in ObjectCells(tiled, obj))
                        document.Blockers.Add(new AegisBlockerCell(cell.X, cell.Y, true, obj.Name));
                }
                else
                    warnings.Add("UnknownObjectLayerIgnored:" + layerName);
            }
        }

        static List<int> DecodeTileLayer(TiledMapDto map, TiledLayerDto layer, string layerName, List<string> errors)
        {
            if (!string.IsNullOrEmpty(layer.Compression))
            {
                errors.Add("TiledCompressionUnsupported:" + layerName + ":" + layer.Compression);
                return null;
            }

            if (!string.IsNullOrEmpty(layer.Encoding) && !string.Equals(layer.Encoding, "csv", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("TiledEncodingUnsupported:" + layerName + ":" + layer.Encoding);
                return null;
            }

            var width = layer.Width == 0 ? map.Width : layer.Width;
            var height = layer.Height == 0 ? map.Height : layer.Height;
            if (width != map.Width || height != map.Height || layer.X != 0 || layer.Y != 0)
            {
                errors.Add("TiledLayerBoundsUnsupported:" + layerName + ":Tile layers must cover the full finite map.");
                return null;
            }

            var data = DecodeData(layer.Data, layer.Encoding, layerName, errors);
            if (data == null)
                return null;

            var expected = map.Width * map.Height;
            if (data.Count != expected)
                errors.Add("TiledLayerDataSizeMismatch:" + layerName + ":Expected " + expected + " values, got " + data.Count + ".");

            return data;
        }

        static List<int> DecodeData(object dataValue, string encoding, string layerName, List<string> errors)
        {
            var result = new List<int>();
            if (dataValue == null)
            {
                errors.Add("TiledLayerDataMissing:" + layerName);
                return null;
            }

            if (dataValue is JsonElement)
            {
                var element = (JsonElement)dataValue;
                if (element.ValueKind == JsonValueKind.Array)
                {
                    foreach (var value in element.EnumerateArray())
                    {
                        if (value.ValueKind != JsonValueKind.Number)
                        {
                            errors.Add("TiledLayerDataInvalid:" + layerName + ":Array contains a non-number value.");
                            return null;
                        }
                        result.Add(value.GetInt32());
                    }
                    return result;
                }

                if (element.ValueKind == JsonValueKind.String && string.Equals(encoding, "csv", StringComparison.OrdinalIgnoreCase))
                    return DecodeCsv(element.GetString(), layerName, errors);

                errors.Add("TiledLayerDataUnsupported:" + layerName);
                return null;
            }

            if (dataValue is string)
                return DecodeCsv((string)dataValue, layerName, errors);

            errors.Add("TiledLayerDataUnsupported:" + layerName);
            return null;
        }

        static List<int> DecodeCsv(string csv, string layerName, List<string> errors)
        {
            var result = new List<int>();
            var parts = (csv ?? string.Empty).Split(new[] { ',', '\n', '\r', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < parts.Length; i++)
            {
                int value;
                if (!int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                {
                    errors.Add("TiledLayerCsvInvalid:" + layerName + ":" + parts[i]);
                    return null;
                }
                result.Add(value);
            }

            return result;
        }

        static Dictionary<int, string> BuildTerrainGidMap(List<TiledTileSetDto> tileSets)
        {
            var map = new Dictionary<int, string>();
            var sets = tileSets ?? new List<TiledTileSetDto>();
            for (var i = 0; i < sets.Count; i++)
            {
                var set = sets[i];
                if (set == null)
                    continue;
                var tiles = set.Tiles ?? new List<TiledTileDto>();
                for (var t = 0; t < tiles.Count; t++)
                {
                    var tile = tiles[t];
                    if (tile == null)
                        continue;
                    var terrainId = GetStringProperty(tile.Properties, "aegisTerrainId", GetStringProperty(tile.Properties, "terrainId", string.Empty));
                    if (!string.IsNullOrEmpty(terrainId))
                        map[set.FirstGid + tile.Id] = terrainId;
                }
            }

            return map;
        }

        static string TerrainIdForGid(Dictionary<int, string> mappedTerrainIds, int gid)
        {
            string terrainId;
            if (mappedTerrainIds != null && mappedTerrainIds.TryGetValue(gid, out terrainId))
                return AegisMapTerrainIds.Normalize(terrainId);

            switch (gid)
            {
                case 2:
                    return AegisMapTerrainIds.Road;
                case 3:
                    return AegisMapTerrainIds.Rough;
                case 4:
                    return AegisMapTerrainIds.Forest;
                case 5:
                    return AegisMapTerrainIds.Water;
                case 6:
                    return AegisMapTerrainIds.Cliff;
                case 7:
                    return AegisMapTerrainIds.Ore;
                default:
                    return AegisMapTerrainIds.Clear;
            }
        }

        static AegisCell ObjectCell(TiledMapDto map, TiledObjectDto obj)
        {
            return new AegisCell(
                Clamp((int)Math.Floor(obj.X / SafeTileWidth(map)), 0, map.Width - 1),
                Clamp((int)Math.Floor(obj.Y / SafeTileHeight(map)), 0, map.Height - 1));
        }

        static IEnumerable<AegisCell> ObjectCells(TiledMapDto map, TiledObjectDto obj)
        {
            var start = ObjectCell(map, obj);
            var width = Math.Max(1, CeilingCells(obj.Width, SafeTileWidth(map)));
            var height = Math.Max(1, CeilingCells(obj.Height, SafeTileHeight(map)));
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    var cell = new AegisCell(start.X + x, start.Y + y);
                    if (cell.X >= 0 && cell.Y >= 0 && cell.X < map.Width && cell.Y < map.Height)
                        yield return cell;
                }
        }

        static int CeilingCells(double pixels, int tileSize)
        {
            if (pixels <= 0)
                return 1;
            return (int)Math.Ceiling(pixels / tileSize);
        }

        static int SafeTileWidth(TiledMapDto map)
        {
            return map.TileWidth <= 0 ? 32 : map.TileWidth;
        }

        static int SafeTileHeight(TiledMapDto map)
        {
            return map.TileHeight <= 0 ? 32 : map.TileHeight;
        }

        static int GuessPlayerId(TiledObjectDto obj, int fallback)
        {
            var candidate = !string.IsNullOrEmpty(obj.Name) ? obj.Name : obj.Type;
            if (candidate != null)
            {
                var digits = string.Empty;
                for (var i = 0; i < candidate.Length; i++)
                    if (char.IsDigit(candidate[i]))
                        digits += candidate[i];
                int parsed;
                if (digits.Length != 0 && int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
                    return parsed;
            }

            return fallback;
        }

        static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        static IEnumerable<TiledPropertyDto> SafeProperties(List<TiledPropertyDto> properties)
        {
            return properties ?? new List<TiledPropertyDto>();
        }

        static string GetStringProperty(List<TiledPropertyDto> properties, string name, string fallback)
        {
            var property = FindProperty(properties, name);
            if (property == null)
                return fallback;
            var value = PropertyValueToString(property.Value);
            return string.IsNullOrEmpty(value) ? fallback : value;
        }

        static int GetIntProperty(List<TiledPropertyDto> properties, string name, int fallback)
        {
            var property = FindProperty(properties, name);
            if (property == null)
                return fallback;
            var text = PropertyValueToString(property.Value);
            int parsed;
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) ? parsed : fallback;
        }

        static bool GetBoolProperty(List<TiledPropertyDto> properties, string name, bool fallback)
        {
            var property = FindProperty(properties, name);
            if (property == null)
                return fallback;
            var text = PropertyValueToString(property.Value);
            bool parsed;
            return bool.TryParse(text, out parsed) ? parsed : fallback;
        }

        static TiledPropertyDto FindProperty(List<TiledPropertyDto> properties, string name)
        {
            if (properties == null)
                return null;
            for (var i = 0; i < properties.Count; i++)
                if (properties[i] != null && string.Equals(properties[i].Name, name, StringComparison.Ordinal))
                    return properties[i];
            return null;
        }

        static string PropertyValueToString(object value)
        {
            if (value == null)
                return string.Empty;
            if (value is JsonElement)
            {
                var element = (JsonElement)value;
                switch (element.ValueKind)
                {
                    case JsonValueKind.String:
                        return element.GetString();
                    case JsonValueKind.Number:
                        return element.ToString();
                    case JsonValueKind.True:
                        return "true";
                    case JsonValueKind.False:
                        return "false";
                    default:
                        return element.ToString();
                }
            }
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        struct AegisCell
        {
            public readonly int X;
            public readonly int Y;

            public AegisCell(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
    }
}
