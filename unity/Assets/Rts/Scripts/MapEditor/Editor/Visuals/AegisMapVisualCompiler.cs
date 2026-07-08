#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.MapEditor;
using ProjectAegisRTS.UnityClient.MapEditor.Visuals;
using UnityEditor;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public sealed class AegisMapVisualCompiler
    {
        const int StartProtectedRadius = 9;

        readonly List<IAegisVisualLayerCompiler> layers = new List<IAegisVisualLayerCompiler>
        {
            new AegisTerrainLayerCompiler(),
            new AegisTerrainTransitionCompiler(),
            new AegisWaterAndShorelineCompiler(),
            new AegisRoadVisualCompiler(),
            new AegisCliffTopologyCompiler(),
            new AegisResourceFieldVisualCompiler(),
            new AegisBasePadVisualCompiler(),
            new AegisScatterVisualCompiler()
        };

        public AegisMapVisualCompileResult Compile(AegisVisualMapDocument document, string sourcePath, bool persistAssets, AegisMapVisualTheme theme, int visualSeed)
        {
            if (document == null)
                throw new ArgumentNullException("document");

            document.Normalize();
            if (theme == null)
                theme = AegisBiomeVisualTheme.Create(FindBiome(document));

            var context = CreateContext(document, sourcePath, persistAssets, theme, visualSeed);
            var result = new AegisMapVisualCompileResult();
            result.Root = context.Root;

            for (var i = 0; i < layers.Count; i++)
            {
                var summary = layers[i].Compile(context);
                if (summary != null)
                    result.Layers.Add(summary);
            }

            var marker = context.Root.GetComponent<AegisMapVisualScene>();
            if (marker != null)
            {
                marker.VisualCompilerVersion = "visual-compiler-v1";
                marker.VisualThemeId = theme.ThemeId;
                marker.VisualCompilerSummary = result.ToSummaryText();
            }

            return result;
        }

        public static AegisMapVisualCompileResult CompileDocument(AegisVisualMapDocument document, string sourcePath, bool persistAssets)
        {
            var seed = document == null ? 0 : document.ReadSeed();
            var theme = AegisBiomeVisualTheme.Create(FindBiome(document));
            return new AegisMapVisualCompiler().Compile(document, sourcePath, persistAssets, theme, seed);
        }

        public static AegisMapVisualCompileResult CompileDocument(AegisVisualMapDocument document, string sourcePath, bool persistAssets, AegisMapVisualTheme theme, int visualSeed)
        {
            return new AegisMapVisualCompiler().Compile(document, sourcePath, persistAssets, theme, visualSeed);
        }

        static AegisMapVisualCompileContext CreateContext(AegisVisualMapDocument document, string sourcePath, bool persistAssets, AegisMapVisualTheme theme, int visualSeed)
        {
            var safeMapId = AegisVisualCompilerPrimitives.Sanitize(document.mapId);
            var seed = visualSeed == 0 ? document.ReadSeed() : visualSeed;
            var context = new AegisMapVisualCompileContext(document.width, document.height);
            context.MapId = string.IsNullOrEmpty(document.mapId) ? "aegis_map" : document.mapId;
            context.SourceAssetPath = sourcePath;
            context.Seed = seed;
            context.PersistAssets = persistAssets;
            context.Theme = theme;
            context.Root = new GameObject("Aegis Visual Map - " + safeMapId);
            context.Root.transform.position = Vector3.zero;

            var marker = context.Root.AddComponent<AegisMapVisualScene>();
            marker.MapId = context.MapId;
            marker.SourceAssetPath = sourcePath;
            marker.Width = document.width;
            marker.Height = document.height;
            marker.Seed = seed;
            marker.Biome = theme.Biome;

            PopulateTerrain(context, document);
            PopulateStarts(context, document);
            PopulateBlockers(context, document);
            PopulateResources(context, document);
            PopulateRoadSegments(context);
            return context;
        }

        static void PopulateTerrain(AegisMapVisualCompileContext context, AegisVisualMapDocument document)
        {
            for (var y = 0; y < context.Height; y++)
                for (var x = 0; x < context.Width; x++)
                    context.SetTerrainRole(x, y, TerrainIdToRole(document.defaultTerrainId));

            if (document.terrainBase == null)
                return;

            for (var i = 0; i < document.terrainBase.Length; i++)
            {
                var cell = document.terrainBase[i];
                if (cell != null)
                    context.SetTerrainRole(cell.x, cell.y, TerrainIdToRole(cell.terrainId));
            }
        }

        static void PopulateStarts(AegisMapVisualCompileContext context, AegisVisualMapDocument document)
        {
            if (document.playerStarts == null)
                return;

            for (var i = 0; i < document.playerStarts.Length; i++)
            {
                var start = document.playerStarts[i];
                if (start == null || !context.InBounds(start.x, start.y))
                    continue;

                context.Starts.Add(new AegisVisualStartModel
                {
                    PlayerId = start.playerId,
                    X = start.x,
                    Y = start.y,
                    Name = start.name,
                    ProtectedRadius = StartProtectedRadius
                });
                context.MarkStartProtected(start.x, start.y, StartProtectedRadius);
            }
        }

        static void PopulateBlockers(AegisMapVisualCompileContext context, AegisVisualMapDocument document)
        {
            if (document.blockers == null)
                return;

            for (var i = 0; i < document.blockers.Length; i++)
            {
                var blocker = document.blockers[i];
                if (blocker == null || !blocker.blocksGround)
                    continue;

                var cliffLike = !string.IsNullOrEmpty(blocker.reason) &&
                    (blocker.reason.IndexOf("cliff", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     blocker.reason.IndexOf("rock", StringComparison.OrdinalIgnoreCase) >= 0);
                context.SetBlocker(blocker.x, blocker.y, cliffLike);
            }
        }

        static void PopulateResources(AegisMapVisualCompileContext context, AegisVisualMapDocument document)
        {
            if (document.resources == null)
                return;

            var fields = new Dictionary<string, AegisVisualResourceFieldModel>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < document.resources.Length; i++)
            {
                var resource = document.resources[i];
                if (resource == null || !context.InBounds(resource.x, resource.y))
                    continue;

                if (context.IsStartProtected(resource.x, resource.y))
                    continue;

                var fieldId = string.IsNullOrEmpty(resource.fieldId) ? "field_" + resource.x + "_" + resource.y : resource.fieldId;
                var kind = string.IsNullOrEmpty(resource.resourceKind) ? "ore" : resource.resourceKind;
                context.SetResource(resource.x, resource.y, fieldId, kind, resource.amount);

                AegisVisualResourceFieldModel field;
                if (!fields.TryGetValue(fieldId, out field))
                {
                    field = new AegisVisualResourceFieldModel
                    {
                        FieldId = fieldId,
                        ResourceKind = kind
                    };
                    fields.Add(fieldId, field);
                    context.ResourceFields.Add(field);
                }

                field.CurrentAmount += Math.Max(0, resource.amount);
                field.MaxAmount += Math.Max(1, resource.amount);
                field.Cells.Add(new AegisVisualResourceCellModel { X = resource.x, Y = resource.y, Amount = resource.amount });
            }
        }

        static void PopulateRoadSegments(AegisMapVisualCompileContext context)
        {
            if (context.Starts.Count == 0)
                return;

            var center = new Vector2(context.Width * 0.5f, context.Height * 0.5f);
            for (var i = 0; i < context.Starts.Count; i++)
            {
                var start = context.Starts[i];
                var startPoint = new Vector2(start.X + 0.5f, start.Y + 0.5f);
                var mid = Vector2.Lerp(startPoint, center, 0.58f);
                mid.x += (context.Hash01(start.X, start.Y, 31) - 0.5f) * 9f;
                mid.y += (context.Hash01(start.X, start.Y, 32) - 0.5f) * 9f;
                context.RoadSegments.Add(new AegisVisualPathSegment(startPoint, mid, 3.6f));
                context.RoadSegments.Add(new AegisVisualPathSegment(mid, center, 3.2f));
            }

            if (context.Starts.Count > 1)
            {
                for (var i = 0; i < context.Starts.Count; i++)
                {
                    var a = context.Starts[i];
                    var b = context.Starts[(i + 1) % context.Starts.Count];
                    if (a.PlayerId >= b.PlayerId && context.Starts.Count > 2)
                        continue;
                    context.RoadSegments.Add(new AegisVisualPathSegment(
                        new Vector2(a.X + 0.5f, a.Y + 0.5f),
                        new Vector2(b.X + 0.5f, b.Y + 0.5f),
                        2.4f));
                }
            }
        }

        static string TerrainIdToRole(string terrainId)
        {
            if (string.IsNullOrEmpty(terrainId))
                return "terrain.grass";

            var id = terrainId.ToLowerInvariant();
            if (id.Contains("water") || id.Contains("river"))
                return "terrain.shallow_water";
            if (id.Contains("deep"))
                return "terrain.deep_water";
            if (id.Contains("forest"))
                return "terrain.dark_grass";
            if (id.Contains("road") || id.Contains("path") || id.Contains("dirt"))
                return "terrain.dirt";
            if (id.Contains("rough") || id.Contains("rock") || id.Contains("gravel"))
                return "terrain.gravel";
            if (id.Contains("mud"))
                return "terrain.mud";
            if (id.Contains("cliff") || id.Contains("block"))
                return "terrain.cliff_ground";
            if (id.Contains("concrete") || id.Contains("base"))
                return "terrain.concrete_base_pad";

            return "terrain.grass";
        }

        static string FindBiome(AegisVisualMapDocument document)
        {
            if (document == null || document.terrainBase == null)
                return "forest";

            var forest = 0;
            var desert = 0;
            var rocky = 0;
            for (var i = 0; i < document.terrainBase.Length; i++)
            {
                var cell = document.terrainBase[i];
                if (cell == null || string.IsNullOrEmpty(cell.terrainId))
                    continue;

                var id = cell.terrainId.ToLowerInvariant();
                if (id.Contains("forest"))
                    forest++;
                if (id.Contains("desert") || id.Contains("sand"))
                    desert++;
                if (id.Contains("rock") || id.Contains("rough") || id.Contains("cliff"))
                    rocky++;
            }

            if (desert > forest && desert > rocky)
                return "desert";
            if (rocky > forest && rocky > desert)
                return "rocky";
            return "forest";
        }
    }

    interface IAegisVisualLayerCompiler
    {
        AegisVisualLayerSummary Compile(AegisMapVisualCompileContext context);
    }

    static class AegisVisualCompilerPrimitives
    {
        public static Transform CreateLayer(AegisMapVisualCompileContext context, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(context.Root.transform, false);
            return go.transform;
        }

        public static Material Material(AegisMapVisualCompileContext context, string role)
        {
            Material material;
            if (context.MaterialCache.TryGetValue(role, out material))
                return material;

            var rule = context.Theme == null ? null : context.Theme.RuleFor(role);
            var color = rule == null ? Color.white : rule.Color;
            // Prototype compiler layers use opaque materials to avoid stacked transparent
            // URP sorting artifacts while the final shader/material-layer path is pending.
            var transparent = false;
            var fileName = "aegis_visual_compiler_" + Sanitize(role) + ".mat";
            material = AegisMapArtPack.Material(
                fileName,
                color,
                false,
                context.PersistAssets,
                transparent,
                rule == null ? null : rule.AlbedoPath,
                rule == null ? null : rule.NormalPath,
                rule == null ? null : rule.MaskPath);
            context.MaterialCache[role] = material;
            return material;
        }

        public static GameObject CreateQuad(Transform parent, string name, Vector2 center, float width, float height, float elevation, Material material, float angleDegrees)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(center.x, elevation, center.y);
            go.transform.rotation = Quaternion.Euler(90f, 0f, angleDegrees);
            go.transform.localScale = new Vector3(width, height, 1f);
            AssignMaterialAndStripCollider(go, material);
            return go;
        }

        public static GameObject CreateCube(Transform parent, string name, Vector3 position, Vector3 scale, Quaternion rotation, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.rotation = rotation;
            go.transform.localScale = scale;
            AssignMaterialAndStripCollider(go, material);
            return go;
        }

        public static GameObject CreateCylinder(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = scale;
            AssignMaterialAndStripCollider(go, material);
            return go;
        }

        public static float DirectionAngle(Vector2 a, Vector2 b)
        {
            var delta = b - a;
            if (delta.sqrMagnitude < 0.0001f)
                return 0f;

            return Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        }

        public static float SegmentLength(Vector2 a, Vector2 b)
        {
            return Vector2.Distance(a, b);
        }

        public static string Sanitize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "aegis";

            var chars = text.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '_' && chars[i] != '-')
                    chars[i] = '_';
            }

            return new string(chars);
        }

        public static bool IsRoadNear(AegisMapVisualCompileContext context, int x, int y, float maxDistance)
        {
            var point = new Vector2(x + 0.5f, y + 0.5f);
            for (var i = 0; i < context.RoadSegments.Count; i++)
            {
                var segment = context.RoadSegments[i];
                if (DistanceToSegment(point, segment.A, segment.B) <= maxDistance)
                    return true;
            }

            return false;
        }

        public static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            var length = ab.sqrMagnitude;
            if (length <= 0.0001f)
                return Vector2.Distance(point, a);

            var t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / length);
            return Vector2.Distance(point, a + ab * t);
        }

        static void AssignMaterialAndStripCollider(GameObject go, Material material)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;

            var collider = go.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);
        }
    }
}
#endif
