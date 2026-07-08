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
            new AegisTerrainDetailOverlayCompiler(),
            new AegisWaterAndShorelineCompiler(),
            new AegisRoadVisualCompiler(),
            new AegisCliffTopologyCompiler(),
            new AegisResourceFieldVisualCompiler(),
            new AegisBasePadVisualCompiler(),
            new AegisScatterVisualCompiler()
        };

        public AegisMapVisualCompileResult Compile(AegisVisualMapDocument document, string sourcePath, bool persistAssets, AegisMapVisualTheme theme, int visualSeed)
        {
            return Compile(document, sourcePath, persistAssets, theme, visualSeed, AegisMapVisualCompileSettings.ProductionDefault());
        }

        public AegisMapVisualCompileResult Compile(AegisVisualMapDocument document, string sourcePath, bool persistAssets, AegisMapVisualTheme theme, int visualSeed, AegisMapVisualCompileSettings settings)
        {
            if (document == null)
                throw new ArgumentNullException("document");

            document.Normalize();
            if (theme == null)
                theme = AegisBiomeVisualTheme.Create(FindBiome(document));

            var context = CreateContext(document, sourcePath, persistAssets, theme, visualSeed, settings);
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
            return CompileDocument(document, sourcePath, persistAssets, AegisMapVisualCompileSettings.ProductionDefault());
        }

        public static AegisMapVisualCompileResult CompileDocument(AegisVisualMapDocument document, string sourcePath, bool persistAssets, AegisMapVisualCompileSettings settings)
        {
            var seed = document == null ? 0 : document.ReadSeed();
            var theme = AegisBiomeVisualTheme.Create(FindBiome(document));
            return new AegisMapVisualCompiler().Compile(document, sourcePath, persistAssets, theme, seed, settings);
        }

        public static AegisMapVisualCompileResult CompileDocument(AegisVisualMapDocument document, string sourcePath, bool persistAssets, AegisMapVisualTheme theme, int visualSeed)
        {
            return new AegisMapVisualCompiler().Compile(document, sourcePath, persistAssets, theme, visualSeed, AegisMapVisualCompileSettings.ProductionDefault());
        }

        public static AegisMapVisualCompileResult CompileDocument(AegisVisualMapDocument document, string sourcePath, bool persistAssets, AegisMapVisualTheme theme, int visualSeed, AegisMapVisualCompileSettings settings)
        {
            return new AegisMapVisualCompiler().Compile(document, sourcePath, persistAssets, theme, visualSeed, settings);
        }

        static AegisMapVisualCompileContext CreateContext(AegisVisualMapDocument document, string sourcePath, bool persistAssets, AegisMapVisualTheme theme, int visualSeed, AegisMapVisualCompileSettings settings)
        {
            var safeMapId = AegisVisualCompilerPrimitives.Sanitize(document.mapId);
            var seed = visualSeed == 0 ? document.ReadSeed() : visualSeed;
            var context = new AegisMapVisualCompileContext(document.width, document.height);
            context.MapId = string.IsNullOrEmpty(document.mapId) ? "aegis_map" : document.mapId;
            context.SourceAssetPath = sourcePath;
            context.Seed = seed;
            context.PersistAssets = persistAssets;
            context.Theme = theme;
            context.Settings = settings ?? AegisMapVisualCompileSettings.ProductionDefault();
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
            var transparent = rule != null && rule.Transparent;
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
            ApplyTextureTiling(material, role);
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

        public static GameObject CreateWorldUvQuad(Transform parent, string name, Vector2 center, float width, float height, float elevation, Material material, float startX, float startY, float logicalWidth, float logicalHeight, float uvWorldScale)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(center.x, elevation, center.y);

            var halfWidth = width * 0.5f;
            var halfHeight = height * 0.5f;
            var bleedX = Mathf.Max(0f, (width - logicalWidth) * 0.5f);
            var bleedY = Mathf.Max(0f, (height - logicalHeight) * 0.5f);
            var safeUvScale = Mathf.Max(0.001f, uvWorldScale);
            var u0 = (startX - bleedX) / safeUvScale;
            var v0 = (startY - bleedY) / safeUvScale;
            var u1 = (startX + logicalWidth + bleedX) / safeUvScale;
            var v1 = (startY + logicalHeight + bleedY) / safeUvScale;

            var mesh = new Mesh();
            mesh.name = name + "_world_uv_mesh";
            mesh.vertices = new[]
            {
                new Vector3(-halfWidth, 0f, -halfHeight),
                new Vector3(halfWidth, 0f, -halfHeight),
                new Vector3(-halfWidth, 0f, halfHeight),
                new Vector3(halfWidth, 0f, halfHeight)
            };
            mesh.normals = new[]
            {
                Vector3.up,
                Vector3.up,
                Vector3.up,
                Vector3.up
            };
            mesh.uv = new[]
            {
                new Vector2(u0, v0),
                new Vector2(u1, v0),
                new Vector2(u0, v1),
                new Vector2(u1, v1)
            };
            mesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
            mesh.RecalculateBounds();

            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            return go;
        }

        public static GameObject CreateOrganicQuad(Transform parent, string name, Vector2 center, float width, float height, float elevation, Material material, float angleDegrees, AegisMapVisualCompileContext context, int x, int y, int salt, float edgeJitter, float uvWorldScale = 0f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(center.x, elevation, center.y);
            go.transform.rotation = Quaternion.Euler(0f, angleDegrees, 0f);

            const int columns = 4;
            const int rows = 3;
            var vertices = new Vector3[(columns + 1) * (rows + 1)];
            var normals = new Vector3[vertices.Length];
            var uvs = new Vector2[vertices.Length];

            var safeWidth = Mathf.Max(0.05f, width);
            var safeHeight = Mathf.Max(0.05f, height);
            var jitter = Mathf.Clamp(edgeJitter, 0f, Mathf.Min(safeWidth, safeHeight) * 0.45f);

            for (var j = 0; j <= rows; j++)
            {
                for (var i = 0; i <= columns; i++)
                {
                    var index = OrganicVertexIndex(i, j, columns);
                    var u = i / (float)columns;
                    var v = j / (float)rows;
                    var localX = (u - 0.5f) * safeWidth;
                    var localZ = (v - 0.5f) * safeHeight;
                    var edgeScale = OrganicEdgeScale(i, j, columns, rows);

                    if (edgeScale > 0f && jitter > 0f)
                    {
                        var xNoise = context == null ? 0.5f : context.Hash01(x + i, y + j, salt + 11);
                        var zNoise = context == null ? 0.5f : context.Hash01(x + i, y + j, salt + 23);
                        var sidePush = Mathf.Lerp(0.35f, 1.0f, context == null ? 0.5f : context.Hash01(x + i, y + j, salt + 37)) * jitter * edgeScale;

                        if (i == 0)
                            localX -= sidePush * (0.55f + xNoise * 0.45f);
                        else if (i == columns)
                            localX += sidePush * (0.55f + xNoise * 0.45f);
                        else
                            localX += (xNoise - 0.5f) * jitter * 0.45f;

                        if (j == 0)
                            localZ -= sidePush * (0.55f + zNoise * 0.45f);
                        else if (j == rows)
                            localZ += sidePush * (0.55f + zNoise * 0.45f);
                        else
                            localZ += (zNoise - 0.5f) * jitter * 0.45f;
                    }
                    else
                    {
                        var centerNoiseX = context == null ? 0.5f : context.Hash01(x + i, y + j, salt + 41);
                        var centerNoiseZ = context == null ? 0.5f : context.Hash01(x + i, y + j, salt + 43);
                        localX += (centerNoiseX - 0.5f) * jitter * 0.18f;
                        localZ += (centerNoiseZ - 0.5f) * jitter * 0.18f;
                    }

                    vertices[index] = new Vector3(localX, 0f, localZ);
                    normals[index] = Vector3.up;
                    if (uvWorldScale > 0f)
                    {
                        var safeUvScale = Mathf.Max(0.001f, uvWorldScale);
                        uvs[index] = new Vector2((u - 0.5f) * safeWidth / safeUvScale, (v - 0.5f) * safeHeight / safeUvScale);
                    }
                    else
                    {
                        uvs[index] = new Vector2(u, v);
                    }
                }
            }

            var triangles = new int[columns * rows * 6];
            var triangle = 0;
            for (var j = 0; j < rows; j++)
            {
                for (var i = 0; i < columns; i++)
                {
                    var a = OrganicVertexIndex(i, j, columns);
                    var b = OrganicVertexIndex(i + 1, j, columns);
                    var c = OrganicVertexIndex(i, j + 1, columns);
                    var d = OrganicVertexIndex(i + 1, j + 1, columns);
                    triangles[triangle++] = a;
                    triangles[triangle++] = c;
                    triangles[triangle++] = b;
                    triangles[triangle++] = b;
                    triangles[triangle++] = c;
                    triangles[triangle++] = d;
                }
            }

            var mesh = new Mesh();
            mesh.name = name + "_mesh";
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();

            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
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

        static int OrganicVertexIndex(int i, int j, int columns)
        {
            return j * (columns + 1) + i;
        }

        static float OrganicEdgeScale(int i, int j, int columns, int rows)
        {
            var edgeCount = 0;
            if (i == 0 || i == columns)
                edgeCount++;
            if (j == 0 || j == rows)
                edgeCount++;

            if (edgeCount == 0)
                return 0f;
            return edgeCount == 1 ? 1f : 0.62f;
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

        static void ApplyTextureTiling(Material material, string role)
        {
            if (material == null || material.mainTexture == null)
                return;

            var tiling = Vector2.one;
            if (role == "road.dirt")
                tiling = new Vector2(1.15f, 1.65f);
            else if (role == "road.gravel")
                tiling = new Vector2(1.35f, 1.75f);
            else if (role == "terrain.grass" || role == "terrain.dark_grass")
                tiling = new Vector2(1.35f, 1.35f);
            else if (role == "terrain.dirt" || role == "terrain.mud")
                tiling = new Vector2(1.18f, 1.18f);
            else if (role == "terrain.gravel" || role == "terrain.cliff_ground" || role == "blocker.rock")
                tiling = new Vector2(1.35f, 1.35f);
            else if (role == "river.water" || role == "terrain.shallow_water" || role == "terrain.deep_water")
                tiling = new Vector2(1.02f, 1.18f);
            else if (IsTileableSurfaceRole(role))
                tiling = new Vector2(1.12f, 1.12f);

            material.mainTextureScale = tiling;
            if (material.HasProperty("_BaseMap"))
                material.SetTextureScale("_BaseMap", tiling);
            if (material.HasProperty("_MainTex"))
                material.SetTextureScale("_MainTex", tiling);
        }

        static bool IsTileableSurfaceRole(string role)
        {
            if (string.IsNullOrEmpty(role))
                return false;

            return role == "terrain.grass" ||
                role == "terrain.dark_grass" ||
                role == "terrain.dirt" ||
                role == "terrain.gravel" ||
                role == "terrain.mud" ||
                role == "terrain.shallow_water" ||
                role == "terrain.deep_water" ||
                role == "terrain.cliff_ground" ||
                role == "terrain.ore_stained_soil" ||
                role == "terrain.concrete_base_pad" ||
                role == "road.dirt" ||
                role == "road.gravel" ||
                role == "river.water" ||
                role == "river.shoreline" ||
                role == "cliff.edge.straight" ||
                role == "cliff.edge.corner_inner" ||
                role == "cliff.edge.corner_outer" ||
                role == "cliff.edge.endcap" ||
                role == "blocker.rock" ||
                role == "basepad.panel" ||
                role == "basepad.trim" ||
                role == "basepad.corner";
        }
    }
}
#endif
