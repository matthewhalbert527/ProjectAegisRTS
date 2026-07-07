using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProjectAegisRTS.Maps.Tiled
{
    public sealed class TiledMapDto
    {
        public string Type { get; set; }
        public string Orientation { get; set; }
        public string RenderOrder { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public bool Infinite { get; set; }
        public int NextLayerId { get; set; }
        public int NextObjectId { get; set; }
        public List<TiledLayerDto> Layers { get; set; }
        public List<TiledTileSetDto> TileSets { get; set; }
        public List<TiledPropertyDto> Properties { get; set; }

        public TiledMapDto()
        {
            Type = "map";
            Orientation = "orthogonal";
            RenderOrder = "right-down";
            TileWidth = 32;
            TileHeight = 32;
            NextLayerId = 1;
            NextObjectId = 1;
            Layers = new List<TiledLayerDto>();
            TileSets = new List<TiledTileSetDto>();
            Properties = new List<TiledPropertyDto>();
        }
    }

    public sealed class TiledLayerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Visible { get; set; }
        public float Opacity { get; set; }
        public string Encoding { get; set; }
        public string Compression { get; set; }
        public object Data { get; set; }
        public string DrawOrder { get; set; }
        public List<TiledObjectDto> Objects { get; set; }
        public List<TiledPropertyDto> Properties { get; set; }

        public TiledLayerDto()
        {
            Name = string.Empty;
            Type = "tilelayer";
            Visible = true;
            Opacity = 1f;
            DrawOrder = "topdown";
            Objects = new List<TiledObjectDto>();
            Properties = new List<TiledPropertyDto>();
        }
    }

    public sealed class TiledObjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Rotation { get; set; }
        public bool Point { get; set; }
        public bool Visible { get; set; }
        public List<TiledPropertyDto> Properties { get; set; }

        public TiledObjectDto()
        {
            Name = string.Empty;
            Type = string.Empty;
            Visible = true;
            Properties = new List<TiledPropertyDto>();
        }
    }

    public sealed class TiledTileSetDto
    {
        public int FirstGid { get; set; }
        public string Source { get; set; }
        public string Name { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int TileCount { get; set; }
        public int Columns { get; set; }
        public List<TiledTileDto> Tiles { get; set; }

        public TiledTileSetDto()
        {
            Source = string.Empty;
            Name = string.Empty;
            Tiles = new List<TiledTileDto>();
        }
    }

    public sealed class TiledTileDto
    {
        public int Id { get; set; }
        public List<TiledPropertyDto> Properties { get; set; }

        public TiledTileDto()
        {
            Properties = new List<TiledPropertyDto>();
        }
    }

    public sealed class TiledPropertyDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public object Value { get; set; }

        public TiledPropertyDto()
        {
            Name = string.Empty;
            Type = "string";
        }

        public TiledPropertyDto(string name, string type, object value)
        {
            Name = name;
            Type = type;
            Value = value;
        }
    }
}
