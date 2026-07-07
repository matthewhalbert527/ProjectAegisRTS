using System;
using System.Text.RegularExpressions;

namespace ProjectAegisRTS.UnityClient.MapEditor
{
    public static class AegisMapEditorPaths
    {
        public const string MapEditorRoot = "Assets/Rts/MapEditor";
        public const string SamplesFolder = "Assets/Rts/MapEditor/Samples";
        public const string AssetPromptsFolder = "Assets/Rts/MapEditor/AssetPrompts";
        public const string ProxyAssetsFolder = "Assets/Rts/MapEditor/ProxyAssets";
        public const string TiledMapsFolder = "Assets/Rts/Maps/Tiled";
        public const string GeneratedMapsFolder = "Assets/Rts/Maps/Generated";
    }

    public static class AegisMapEditorFileTemplates
    {
        public static string CreateAegisMapShellFromTiledJson(string tiledJson, string mapId)
        {
            var width = ReadInt(tiledJson, "width", 100);
            var height = ReadInt(tiledJson, "height", 100);
            var safeMapId = string.IsNullOrEmpty(mapId) ? "imported_tiled_map" : mapId;

            return "{\n" +
                "  \"formatVersion\": \"aegismap.v1\",\n" +
                "  \"mapId\": \"" + EscapeJson(safeMapId) + "\",\n" +
                "  \"displayName\": \"" + EscapeJson(safeMapId) + "\",\n" +
                "  \"width\": " + width + ",\n" +
                "  \"height\": " + height + ",\n" +
                "  \"tileWidth\": 32,\n" +
                "  \"tileHeight\": 32,\n" +
                "  \"defaultTerrainId\": \"clear\",\n" +
                "  \"startingCredits\": 5000,\n" +
                "  \"terrainBase\": [],\n" +
                "  \"terrainOverlay\": [],\n" +
                "  \"blockers\": [],\n" +
                "  \"resources\": [],\n" +
                "  \"playerStarts\": [],\n" +
                "  \"actorPlacements\": [],\n" +
                "  \"regions\": [],\n" +
                "  \"navOverrides\": [],\n" +
                "  \"properties\": { \"source\": \"unity_editor_shell_import\" }\n" +
                "}\n";
        }

        public static string CreateTiledJsonShellFromAegisMap(string aegisJson, string mapId)
        {
            var width = ReadInt(aegisJson, "width", 100);
            var height = ReadInt(aegisJson, "height", 100);
            var safeMapId = string.IsNullOrEmpty(mapId) ? "exported_aegis_map" : mapId;

            return "{\n" +
                "  \"type\": \"map\",\n" +
                "  \"orientation\": \"orthogonal\",\n" +
                "  \"renderorder\": \"right-down\",\n" +
                "  \"width\": " + width + ",\n" +
                "  \"height\": " + height + ",\n" +
                "  \"tilewidth\": 32,\n" +
                "  \"tileheight\": 32,\n" +
                "  \"infinite\": false,\n" +
                "  \"properties\": [\n" +
                "    { \"name\": \"aegisMapId\", \"type\": \"string\", \"value\": \"" + EscapeJson(safeMapId) + "\" }\n" +
                "  ],\n" +
                "  \"layers\": [],\n" +
                "  \"tilesets\": []\n" +
                "}\n";
        }

        public static string StarterTilesetTsx()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                "<tileset version=\"1.10\" tiledversion=\"1.10.2\" name=\"aegis_starter_tiles\" tilewidth=\"32\" tileheight=\"32\" tilecount=\"7\" columns=\"7\">\n" +
                "  <tile id=\"0\"><properties><property name=\"aegisTerrainId\" value=\"clear\"/></properties></tile>\n" +
                "  <tile id=\"1\"><properties><property name=\"aegisTerrainId\" value=\"road\"/></properties></tile>\n" +
                "  <tile id=\"2\"><properties><property name=\"aegisTerrainId\" value=\"rough\"/></properties></tile>\n" +
                "  <tile id=\"3\"><properties><property name=\"aegisTerrainId\" value=\"forest\"/></properties></tile>\n" +
                "  <tile id=\"4\"><properties><property name=\"aegisTerrainId\" value=\"water\"/></properties></tile>\n" +
                "  <tile id=\"5\"><properties><property name=\"aegisTerrainId\" value=\"cliff\"/></properties></tile>\n" +
                "  <tile id=\"6\"><properties><property name=\"aegisTerrainId\" value=\"ore\"/></properties></tile>\n" +
                "</tileset>\n";
        }

        public static string UnityAiAssetPromptMarkdown()
        {
            return "# Unity AI Asset Prompts: Aegis Map Editor\n\n" +
                "- Terrain clear: readable RTS grass-dirt base tile, neutral palette, top-down orthographic use.\n" +
                "- Terrain road: compact worn service road tile that aligns on a square grid.\n" +
                "- Terrain rough: rocky uneven ground, readable as slower traversal.\n" +
                "- Terrain forest: sparse tactical cover cluster, no protected franchise motifs.\n" +
                "- Terrain water: calm shallow water tile, compatible with naval pathing previews.\n" +
                "- Terrain cliff: hard impassable ridge tile for blockers and pathing tests.\n" +
                "- Ore resource: original crystalline mineral field with Project Aegis styling.\n";
        }

        static int ReadInt(string json, string propertyName, int fallback)
        {
            if (string.IsNullOrEmpty(json))
                return fallback;

            var match = Regex.Match(json, "\\\"" + Regex.Escape(propertyName) + "\\\"\\s*:\\s*(\\d+)", RegexOptions.IgnoreCase);
            if (!match.Success)
                return fallback;

            int value;
            return int.TryParse(match.Groups[1].Value, out value) ? value : fallback;
        }

        static string EscapeJson(string text)
        {
            return (text ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
