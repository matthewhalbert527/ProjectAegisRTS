#if UNITY_EDITOR
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.MapEditor.Visuals;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    sealed class AegisWaterAndShorelineCompiler : IAegisVisualLayerCompiler
    {
        public AegisVisualLayerSummary Compile(AegisMapVisualCompileContext context)
        {
            var summary = new AegisVisualLayerSummary("Water And Shoreline");
            var waterLayer = AegisVisualCompilerPrimitives.CreateLayer(context, "Water Surface");
            var shoreLayer = AegisVisualCompilerPrimitives.CreateLayer(context, "Shoreline Mud And Wetness");
            var waterMaterial = AegisVisualCompilerPrimitives.Material(context, "river.water");
            var shoreMaterial = AegisVisualCompilerPrimitives.Material(context, "river.shoreline");
            var riverPropMaterial = AegisVisualCompilerPrimitives.Material(context, "vegetation.grass");

            var ribbons = CollectWaterRibbons(context, summary);
            EmitWaterRibbonMeshes(context, summary, waterLayer, waterMaterial, ribbons);
            EmitMergedShoreline(context, summary, shoreLayer, shoreMaterial, riverPropMaterial, ribbons);
            return summary;
        }

        static List<List<RowSpan>> CollectWaterRibbons(AegisMapVisualCompileContext context, AegisVisualLayerSummary summary)
        {
            var ribbons = new List<List<RowSpan>>();
            var current = new List<RowSpan>();
            for (var y = 0; y < context.Height; y++)
            {
                RowSpan span;
                if (!TryFindDominantWaterSpan(context, y, summary, out span))
                {
                    AddRibbon(ribbons, current);
                    current.Clear();
                    continue;
                }

                current.Add(span);
            }

            AddRibbon(ribbons, current);
            return ribbons;
        }

        static void AddRibbon(List<List<RowSpan>> ribbons, List<RowSpan> current)
        {
            if (current.Count <= 0)
                return;

            ribbons.Add(new List<RowSpan>(current));
        }

        static void EmitWaterRibbonMeshes(AegisMapVisualCompileContext context, AegisVisualLayerSummary summary, Transform waterLayer, Material waterMaterial, List<List<RowSpan>> ribbons)
        {
            for (var i = 0; i < ribbons.Count; i++)
                FlushRibbon(context, waterLayer, waterMaterial, summary, ribbons[i], i);
        }

        static bool TryFindDominantWaterSpan(AegisMapVisualCompileContext context, int y, AegisVisualLayerSummary summary, out RowSpan span)
        {
            span = default(RowSpan);
            var bestStart = -1;
            var bestEnd = -1;
            var bestLength = 0;
            var x = 0;
            while (x < context.Width)
            {
                if (!context.IsWater(x, y))
                {
                    x++;
                    continue;
                }

                var start = x;
                while (x < context.Width && context.IsWater(x, y))
                {
                    summary.WaterCells++;
                    x++;
                }

                var length = x - start;
                if (length > bestLength)
                {
                    bestStart = start;
                    bestEnd = x;
                    bestLength = length;
                }
            }

            if (bestLength <= 0)
                return false;

            span = new RowSpan(y, bestStart, bestEnd);
            return true;
        }

        static void FlushRibbon(AegisMapVisualCompileContext context, Transform waterLayer, Material waterMaterial, AegisVisualLayerSummary summary, List<RowSpan> spans, int ribbonIndex)
        {
            if (spans.Count == 0)
                return;

            var sectionCount = spans.Count + 1;
            var vertices = new Vector3[sectionCount * 2];
            var uvs = new Vector2[sectionCount * 2];
            for (var i = 0; i < sectionCount; i++)
            {
                var z = spans[0].Y + i;
                var rowForNoise = Mathf.Clamp(spans[0].Y + i, 0, context.Height - 1);
                var left = SmoothEdge(context, spans, i, true, rowForNoise) - 0.06f;
                var right = SmoothEdge(context, spans, i, false, rowForNoise) + 0.06f;
                if (right - left < 0.8f)
                {
                    var center = (left + right) * 0.5f;
                    left = center - 0.4f;
                    right = center + 0.4f;
                }

                vertices[i * 2] = new Vector3(left, 0.034f, z);
                vertices[i * 2 + 1] = new Vector3(right, 0.034f, z);
                uvs[i * 2] = new Vector2(0f, i * 0.33f);
                uvs[i * 2 + 1] = new Vector2(1f, i * 0.33f);
            }

            var triangles = new int[(sectionCount - 1) * 6];
            var t = 0;
            for (var i = 0; i < sectionCount - 1; i++)
            {
                var a = i * 2;
                var b = a + 1;
                var c = (i + 1) * 2;
                var d = c + 1;
                triangles[t++] = a;
                triangles[t++] = c;
                triangles[t++] = b;
                triangles[t++] = b;
                triangles[t++] = c;
                triangles[t++] = d;
            }

            var mesh = new Mesh();
            mesh.name = "aegis_water_ribbon_mesh_" + ribbonIndex;
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var go = new GameObject("water_ribbon_mesh_" + ribbonIndex);
            go.transform.SetParent(waterLayer, false);
            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = waterMaterial;
            summary.WaterMeshes++;
            summary.WaterStrips += spans.Count;
        }

        static float SmoothEdge(AegisMapVisualCompileContext context, List<RowSpan> spans, int sectionIndex, bool left, int rowForNoise)
        {
            var currentIndex = Mathf.Clamp(sectionIndex, 0, spans.Count - 1);
            var previousIndex = Mathf.Clamp(sectionIndex - 1, 0, spans.Count - 1);
            var nextIndex = Mathf.Clamp(sectionIndex + 1, 0, spans.Count - 1);
            var current = Edge(spans[currentIndex], left);
            var previous = Edge(spans[previousIndex], left);
            var next = Edge(spans[nextIndex], left);
            var smoothed = current * 0.74f + previous * 0.13f + next * 0.13f;
            var noise = (context.Hash01(left ? 31 : 37, rowForNoise, left ? 5510 : 5520) - 0.5f) * 0.16f;
            return smoothed + noise;
        }

        static float Edge(RowSpan span, bool left)
        {
            return left ? span.Left : span.Right;
        }

        static void EmitMergedShoreline(AegisMapVisualCompileContext context, AegisVisualLayerSummary summary, Transform shoreLayer, Material shoreMaterial, Material riverPropMaterial, List<List<RowSpan>> ribbons)
        {
            for (var i = 0; i < ribbons.Count; i++)
            {
                var spans = ribbons[i];
                EmitBankMesh(context, shoreLayer, shoreMaterial, summary, spans, i, true);
                EmitBankMesh(context, shoreLayer, shoreMaterial, summary, spans, i, false);
                EmitCapMesh(context, shoreLayer, shoreMaterial, summary, spans, i, true);
                EmitCapMesh(context, shoreLayer, shoreMaterial, summary, spans, i, false);
                EmitRiverBankProps(context, shoreLayer, summary, riverPropMaterial, spans);
            }
        }

        static void EmitBankMesh(AegisMapVisualCompileContext context, Transform shoreLayer, Material shoreMaterial, AegisVisualLayerSummary summary, List<RowSpan> spans, int ribbonIndex, bool leftBank)
        {
            if (spans.Count == 0)
                return;

            var sectionCount = spans.Count + 1;
            var vertices = new Vector3[sectionCount * 2];
            var uvs = new Vector2[sectionCount * 2];
            for (var i = 0; i < sectionCount; i++)
            {
                var z = spans[0].Y + i;
                var rowForNoise = Mathf.Clamp(spans[0].Y + i, 0, context.Height - 1);
                var inner = SmoothEdge(context, spans, i, leftBank, rowForNoise) + (leftBank ? -0.08f : 0.08f);
                var outer = inner + (leftBank ? -BankWidth(context, rowForNoise, 6100) : BankWidth(context, rowForNoise, 6200));
                var low = Mathf.Min(inner, outer);
                var high = Mathf.Max(inner, outer);
                var insetNoise = (context.Hash01(rowForNoise, leftBank ? 6110 : 6210, 6115) - 0.5f) * 0.10f;
                vertices[i * 2] = new Vector3(low + insetNoise, 0.046f, z);
                vertices[i * 2 + 1] = new Vector3(high + insetNoise, 0.046f, z);
                uvs[i * 2] = new Vector2(0f, i * 0.31f);
                uvs[i * 2 + 1] = new Vector2(1f, i * 0.31f);
            }

            CreateRibbonMesh(shoreLayer, shoreMaterial, "shoreline_bank_mesh_" + ribbonIndex + "_" + (leftBank ? "left" : "right"), vertices, uvs);
            summary.ShorelineEdges += spans.Count;
            summary.ShorelineMeshes++;
        }

        static void EmitCapMesh(AegisMapVisualCompileContext context, Transform shoreLayer, Material shoreMaterial, AegisVisualLayerSummary summary, List<RowSpan> spans, int ribbonIndex, bool south)
        {
            if (spans.Count == 0)
                return;

            var sectionIndex = south ? 0 : spans.Count;
            var rowForNoise = Mathf.Clamp(south ? spans[0].Y : spans[spans.Count - 1].Y, 0, context.Height - 1);
            var edgeZ = south ? spans[0].Y : spans[0].Y + spans.Count;
            var innerZ = edgeZ + (south ? 0.04f : -0.04f);
            var outerZ = edgeZ + (south ? -BankWidth(context, rowForNoise, 6300) : BankWidth(context, rowForNoise, 6400));
            var zLow = Mathf.Min(innerZ, outerZ);
            var zHigh = Mathf.Max(innerZ, outerZ);
            var left = SmoothEdge(context, spans, sectionIndex, true, rowForNoise) - 0.52f;
            var right = SmoothEdge(context, spans, sectionIndex, false, rowForNoise) + 0.52f;
            var shoulder = 0.22f + context.Hash01(rowForNoise, south ? 6310 : 6410, 6320) * 0.22f;
            var vertices = new[]
            {
                new Vector3(left - shoulder, 0.045f, zLow),
                new Vector3(right + shoulder, 0.045f, zLow),
                new Vector3(left, 0.045f, zHigh),
                new Vector3(right, 0.045f, zHigh)
            };
            var uvs = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f)
            };

            CreateRibbonMesh(shoreLayer, shoreMaterial, "shoreline_cap_mesh_" + ribbonIndex + "_" + (south ? "south" : "north"), vertices, uvs);
            summary.ShorelineEdges++;
            summary.ShorelineMeshes++;
        }

        static void EmitRiverBankProps(AegisMapVisualCompileContext context, Transform shoreLayer, AegisVisualLayerSummary summary, Material riverPropMaterial, List<RowSpan> spans)
        {
            if (spans.Count == 0)
                return;

            var stride = Mathf.Max(4, Mathf.RoundToInt(spans.Count / 10f));
            for (var i = 1; i < spans.Count; i += stride)
            {
                var span = spans[i];
                var left = SmoothEdge(context, spans, i, true, span.Y) - 0.72f;
                var right = SmoothEdge(context, spans, i, false, span.Y) + 0.72f;
                var y = span.Y + 0.5f;
                MaybeEmitRiverProp(context, shoreLayer, summary, riverPropMaterial, Mathf.RoundToInt(left), span.Y, new Vector2(left, y), "left");
                MaybeEmitRiverProp(context, shoreLayer, summary, riverPropMaterial, Mathf.RoundToInt(right), span.Y, new Vector2(right, y), "right");
            }
        }

        static float BankWidth(AegisMapVisualCompileContext context, int row, int salt)
        {
            return 0.62f + context.Hash01(row, salt, salt + 7) * 0.50f;
        }

        static void CreateRibbonMesh(Transform parent, Material material, string name, Vector3[] vertices, Vector2[] uvs)
        {
            var sectionCount = vertices.Length / 2;
            var triangles = new int[(sectionCount - 1) * 6];
            var t = 0;
            for (var i = 0; i < sectionCount - 1; i++)
            {
                var a = i * 2;
                var b = a + 1;
                var c = (i + 1) * 2;
                var d = c + 1;
                triangles[t++] = a;
                triangles[t++] = c;
                triangles[t++] = b;
                triangles[t++] = b;
                triangles[t++] = c;
                triangles[t++] = d;
            }

            var mesh = new Mesh();
            mesh.name = "aegis_" + name;
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
        }

        static void MaybeEmitRiverProp(AegisMapVisualCompileContext context, Transform shoreLayer, AegisVisualLayerSummary summary, Material riverPropMaterial, int x, int y, Vector2 center, string suffix)
        {
            if (context.Hash01(x, y, 4010) >= 0.28f)
                return;

            var propPosition = new Vector3(center.x, 0.08f, center.y);
            var rotation = Quaternion.Euler(0f, context.Hash01(x, y, 4020) * 360f, 0f);
            var scale = Vector3.one * Mathf.Lerp(0.55f, 1.0f, context.Hash01(x, y, 4030));
            var prefabPath = AegisMapArtPack.Pick(AegisMapArtPack.RiverMeshes, context.Seed, x, y);
            if (AegisMapArtPack.TryInstantiatePrefab(shoreLayer, "river_bank_prop_" + suffix + "_" + x + "_" + y, prefabPath, propPosition, rotation, scale, riverPropMaterial))
                summary.ScatterCount++;
            else
                summary.SkippedPlacementCount++;
        }

        struct RowSpan
        {
            public readonly int Y;
            public readonly int Left;
            public readonly int Right;

            public RowSpan(int y, int left, int right)
            {
                Y = y;
                Left = left;
                Right = right;
            }
        }
    }
}
#endif
