using System;
using System.Collections.Generic;
using System.Text;
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

        public static string CreateProceduralAegisMapJson(
            string mapId,
            string displayName,
            string prompt,
            int width,
            int height,
            int playerCount,
            string biome,
            string resourceDensity,
            string cliffDensity,
            string rockiness,
            string waterAmount,
            string symmetry,
            int seed,
            string gameplayProfile,
            bool oreRegenerationEnabled,
            int oreRegenerationRate)
        {
            var safeMapId = string.IsNullOrEmpty(mapId) ? "procedural_aegis_map" : mapId;
            var starts = BuildPlayerStarts(width, height, playerCount);
            var resources = BuildProceduralResources(width, height, starts, resourceDensity, oreRegenerationEnabled, oreRegenerationRate);
            var terrain = BuildProceduralTerrain(width, height, biome, cliffDensity, rockiness, waterAmount, seed);
            var blockers = BuildProceduralBlockers(width, height, terrain);
            RemoveProtectedCells(width, height, starts, terrain, blockers, resources);

            var json = new StringBuilder();
            json.Append("{\n");
            AppendProperty(json, "formatVersion", "aegismap.v1", true);
            AppendProperty(json, "mapId", safeMapId, true);
            AppendProperty(json, "displayName", string.IsNullOrEmpty(displayName) ? safeMapId : displayName, true);
            AppendProperty(json, "width", width, true);
            AppendProperty(json, "height", height, true);
            AppendProperty(json, "tileWidth", 32, true);
            AppendProperty(json, "tileHeight", 32, true);
            AppendProperty(json, "defaultTerrainId", "clear", true);
            AppendProperty(json, "startingCredits", 5000, true);
            AppendTerrainArray(json, "terrainBase", terrain, width, true);
            json.Append("  \"terrainOverlay\": [],\n");
            AppendBlockersArray(json, blockers, width, true);
            AppendResourcesArray(json, resources, true);
            AppendPlayerStartsArray(json, starts, true);
            json.Append("  \"actorPlacements\": [],\n");
            AppendRegionsArray(json, starts, true);
            json.Append("  \"navOverrides\": [],\n");
            json.Append("  \"properties\": {\n");
            json.Append("    \"generatedBy\": \"unity_procedural_prompt_generator\",\n");
            json.Append("    \"prompt\": \"").Append(EscapeJson(prompt)).Append("\",\n");
            json.Append("    \"biome\": \"").Append(EscapeJson(biome)).Append("\",\n");
            json.Append("    \"resourceDensity\": \"").Append(EscapeJson(resourceDensity)).Append("\",\n");
            json.Append("    \"cliffDensity\": \"").Append(EscapeJson(cliffDensity)).Append("\",\n");
            json.Append("    \"rockiness\": \"").Append(EscapeJson(rockiness)).Append("\",\n");
            json.Append("    \"waterAmount\": \"").Append(EscapeJson(waterAmount)).Append("\",\n");
            json.Append("    \"symmetry\": \"").Append(EscapeJson(symmetry)).Append("\",\n");
            json.Append("    \"gameplayProfile\": \"").Append(EscapeJson(gameplayProfile)).Append("\",\n");
            json.Append("    \"generationSeed\": \"").Append(seed).Append("\"\n");
            json.Append("  }\n");
            json.Append("}\n");
            return json.ToString();
        }

        public static string ProceduralPromptExamplesMarkdown()
        {
            return "# Project Aegis Procedural Prompt Examples\n\n" +
                "- small rocky map with lots of ore and high cliffs\n" +
                "- medium 4 player desert map, medium resources, low cliffs\n" +
                "- 200 by 200 forest map with high rockiness and regenerating ore\n" +
                "- large rocky battlefield with high resources, many choke points, cliffs, and regenerating ore fields\n";
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

        static void AppendProperty(StringBuilder json, string name, string value, bool comma)
        {
            json.Append("  \"").Append(name).Append("\": \"").Append(EscapeJson(value)).Append("\"").Append(comma ? "," : string.Empty).Append("\n");
        }

        static void AppendProperty(StringBuilder json, string name, int value, bool comma)
        {
            json.Append("  \"").Append(name).Append("\": ").Append(value).Append(comma ? "," : string.Empty).Append("\n");
        }

        static List<ProceduralStart> BuildPlayerStarts(int width, int height, int playerCount)
        {
            var starts = new List<ProceduralStart>();
            var margin = Math.Max(14, Math.Min(width, height) / 7);
            var midX = width / 2;
            var midY = height / 2;
            playerCount = playerCount <= 2 ? 2 : playerCount <= 4 ? 4 : playerCount <= 6 ? 6 : 8;
            starts.Add(new ProceduralStart(1, margin, midY));
            starts.Add(new ProceduralStart(2, width - margin - 1, midY));
            if (playerCount >= 4)
            {
                starts[0] = new ProceduralStart(1, margin, margin);
                starts[1] = new ProceduralStart(2, width - margin - 1, height - margin - 1);
                starts.Add(new ProceduralStart(3, width - margin - 1, margin));
                starts.Add(new ProceduralStart(4, margin, height - margin - 1));
            }
            if (playerCount >= 6)
            {
                starts.Add(new ProceduralStart(5, midX, margin));
                starts.Add(new ProceduralStart(6, midX, height - margin - 1));
            }
            if (playerCount >= 8)
            {
                starts.Add(new ProceduralStart(7, margin, midY));
                starts.Add(new ProceduralStart(8, width - margin - 1, midY));
            }
            return starts;
        }

        static List<ProceduralResource> BuildProceduralResources(int width, int height, List<ProceduralStart> starts, string density, bool regenerates, int regenerationRate)
        {
            var resources = new List<ProceduralResource>();
            var clusters = density == "very high" ? 4 : density == "high" ? 3 : density == "low" || density == "very low" ? 1 : 2;
            var amount = density == "very high" ? 420 : density == "high" ? 340 : density == "low" || density == "very low" ? 180 : 260;
            for (var s = 0; s < starts.Count; s++)
            {
                var start = starts[s];
                var dx = start.X < width / 2 ? 1 : -1;
                var dy = start.Y < height / 2 ? 1 : -1;
                for (var c = 0; c < clusters; c++)
                {
                    var centerX = start.X + dx * (12 + c * 4);
                    var centerY = start.Y + dy * ((c % 2 == 0 ? -1 : 1) * (5 + c * 2));
                    for (var y = -2; y <= 2; y++)
                        for (var x = -2; x <= 2; x++)
                        {
                            if (Math.Abs(x) + Math.Abs(y) > 3)
                                continue;
                            resources.Add(new ProceduralResource(centerX + x, centerY + y, amount, regenerates, regenerationRate));
                        }
                }
            }
            return resources;
        }

        static Dictionary<int, string> BuildProceduralTerrain(int width, int height, string biome, string cliffDensity, string rockiness, string waterAmount, int seed)
        {
            var terrain = new Dictionary<int, string>();
            var cliffThreshold = cliffDensity == "high" ? 900 : cliffDensity == "extreme" ? 820 : cliffDensity == "none" ? 1001 : 960;
            var rockThreshold = rockiness == "high" ? 880 : rockiness == "extreme" ? 800 : rockiness == "none" ? 1001 : 940;
            var waterThreshold = waterAmount == "high" ? 900 : waterAmount == "medium" ? 945 : waterAmount == "low" ? 980 : 1001;
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    var key = y * width + x;
                    if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                    {
                        terrain[key] = "cliff";
                        continue;
                    }
                    var noise = HashToThousand(seed, x, y);
                    if (noise >= waterThreshold)
                        terrain[key] = "water";
                    else if (noise >= cliffThreshold)
                        terrain[key] = "cliff";
                    else if (noise >= rockThreshold)
                        terrain[key] = "rough";
                    else if (biome == "forest" && noise > 760)
                        terrain[key] = "forest";
                    else if ((biome == "desert" || biome == "rocky" || biome == "wasteland") && noise > 720)
                        terrain[key] = "rough";
                }
            return terrain;
        }

        static List<int> BuildProceduralBlockers(int width, int height, Dictionary<int, string> terrain)
        {
            var blockers = new List<int>();
            foreach (var pair in terrain)
                if (pair.Value == "cliff")
                    blockers.Add(pair.Key);
            return blockers;
        }

        static void RemoveProtectedCells(int width, int height, List<ProceduralStart> starts, Dictionary<int, string> terrain, List<int> blockers, List<ProceduralResource> resources)
        {
            var protectedCells = new HashSet<int>();
            for (var i = 0; i < starts.Count; i++)
                for (var y = -10; y <= 10; y++)
                    for (var x = -10; x <= 10; x++)
                    {
                        var cellX = starts[i].X + x;
                        var cellY = starts[i].Y + y;
                        if (cellX <= 0 || cellY <= 0 || cellX >= width - 1 || cellY >= height - 1)
                            continue;
                        var key = cellY * width + cellX;
                        protectedCells.Add(key);
                        terrain.Remove(key);
                    }

            blockers.RemoveAll(protectedCells.Contains);
            resources.RemoveAll(r => protectedCells.Contains(r.Y * width + r.X));
        }

        static void AppendTerrainArray(StringBuilder json, string name, Dictionary<int, string> terrain, int width, bool comma)
        {
            json.Append("  \"").Append(name).Append("\": [\n");
            var first = true;
            foreach (var pair in terrain)
            {
                if (!first)
                    json.Append(",\n");
                first = false;
                json.Append("    { \"x\": ").Append(pair.Key % width).Append(", \"y\": ").Append(pair.Key / width).Append(", \"terrainId\": \"").Append(pair.Value).Append("\" }");
            }
            json.Append("\n  ]").Append(comma ? "," : string.Empty).Append("\n");
        }

        static void AppendBlockersArray(StringBuilder json, List<int> blockers, int width, bool comma)
        {
            json.Append("  \"blockers\": [\n");
            for (var i = 0; i < blockers.Count; i++)
            {
                if (i != 0)
                    json.Append(",\n");
                json.Append("    { \"x\": ").Append(blockers[i] % width).Append(", \"y\": ").Append(blockers[i] / width).Append(", \"blocksGround\": true, \"reason\": \"unity_generated_cliff\" }");
            }
            json.Append("\n  ]").Append(comma ? "," : string.Empty).Append("\n");
        }

        static void AppendResourcesArray(StringBuilder json, List<ProceduralResource> resources, bool comma)
        {
            json.Append("  \"resources\": [\n");
            for (var i = 0; i < resources.Count; i++)
            {
                var r = resources[i];
                if (i != 0)
                    json.Append(",\n");
                json.Append("    { \"fieldId\": \"unity_ore_").Append(i + 1).Append("\", \"x\": ").Append(r.X).Append(", \"y\": ").Append(r.Y).Append(", \"resourceKind\": \"ore\", \"amount\": ").Append(r.Amount).Append(", \"maxAmount\": ").Append(r.Amount).Append(", \"regenerates\": ").Append(r.Regenerates ? "true" : "false").Append(", \"regenerationRatePerTick\": ").Append(r.RegenerationRate).Append(", \"regenerationDelayTicks\": 60 }");
            }
            json.Append("\n  ]").Append(comma ? "," : string.Empty).Append("\n");
        }

        static void AppendPlayerStartsArray(StringBuilder json, List<ProceduralStart> starts, bool comma)
        {
            json.Append("  \"playerStarts\": [\n");
            for (var i = 0; i < starts.Count; i++)
            {
                if (i != 0)
                    json.Append(",\n");
                json.Append("    { \"playerId\": ").Append(starts[i].PlayerId).Append(", \"x\": ").Append(starts[i].X).Append(", \"y\": ").Append(starts[i].Y).Append(", \"name\": \"Player ").Append(starts[i].PlayerId).Append("\" }");
            }
            json.Append("\n  ]").Append(comma ? "," : string.Empty).Append("\n");
        }

        static void AppendRegionsArray(StringBuilder json, List<ProceduralStart> starts, bool comma)
        {
            json.Append("  \"regions\": [\n");
            for (var i = 0; i < starts.Count; i++)
            {
                if (i != 0)
                    json.Append(",\n");
                json.Append("    { \"regionId\": \"base_area_p").Append(starts[i].PlayerId).Append("\", \"x\": ").Append(starts[i].X - 10).Append(", \"y\": ").Append(starts[i].Y - 10).Append(", \"width\": 21, \"height\": 21, \"purpose\": \"unity_generated_base_area\" }");
            }
            json.Append("\n  ]").Append(comma ? "," : string.Empty).Append("\n");
        }

        static int HashToThousand(int seed, int x, int y)
        {
            unchecked
            {
                var h = (uint)seed;
                h ^= (uint)(x * 374761393);
                h = (h << 13) | (h >> 19);
                h ^= (uint)(y * 668265263);
                h *= 1274126177u;
                h ^= h >> 16;
                return (int)(h % 1000u);
            }
        }

        sealed class ProceduralStart
        {
            public int PlayerId;
            public int X;
            public int Y;

            public ProceduralStart(int playerId, int x, int y)
            {
                PlayerId = playerId;
                X = x;
                Y = y;
            }
        }

        sealed class ProceduralResource
        {
            public int X;
            public int Y;
            public int Amount;
            public bool Regenerates;
            public int RegenerationRate;

            public ProceduralResource(int x, int y, int amount, bool regenerates, int regenerationRate)
            {
                X = x;
                Y = y;
                Amount = amount;
                Regenerates = regenerates;
                RegenerationRate = regenerationRate;
            }
        }
    }
}
