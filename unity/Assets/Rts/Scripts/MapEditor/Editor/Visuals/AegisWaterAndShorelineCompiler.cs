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

            EmitWaterRibbonMeshes(context, summary, waterLayer, waterMaterial);
            EmitMergedShoreline(context, summary, shoreLayer, shoreMaterial, riverPropMaterial);
            return summary;
        }

        static void EmitWaterRibbonMeshes(AegisMapVisualCompileContext context, AegisVisualLayerSummary summary, Transform waterLayer, Material waterMaterial)
        {
            var current = new List<RowSpan>();
            var ribbonIndex = 0;
            for (var y = 0; y < context.Height; y++)
            {
                RowSpan span;
                if (!TryFindDominantWaterSpan(context, y, summary, out span))
                {
                    FlushRibbon(context, waterLayer, waterMaterial, summary, current, ribbonIndex);
                    if (current.Count > 0)
                        ribbonIndex++;
                    current.Clear();
                    continue;
                }

                current.Add(span);
            }

            FlushRibbon(context, waterLayer, waterMaterial, summary, current, ribbonIndex);
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

            if (spans.Count == 1)
            {
                var only = spans[0];
                var center = new Vector2((only.Left + only.Right) * 0.5f, only.Y + 0.5f);
                AegisVisualCompilerPrimitives.CreateQuad(waterLayer, "water_strip_" + only.Left + "_" + only.Y + "_" + (only.Right - only.Left), center, only.Right - only.Left + 0.34f, 1.34f, 0.034f, waterMaterial, 0f);
                summary.WaterStrips++;
                return;
            }

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

        static void EmitMergedShoreline(AegisMapVisualCompileContext context, AegisVisualLayerSummary summary, Transform shoreLayer, Material shoreMaterial, Material riverPropMaterial)
        {
            EmitHorizontalShoreline(context, summary, shoreLayer, shoreMaterial, riverPropMaterial, true);
            EmitHorizontalShoreline(context, summary, shoreLayer, shoreMaterial, riverPropMaterial, false);
            EmitVerticalShoreline(context, summary, shoreLayer, shoreMaterial, riverPropMaterial, true);
            EmitVerticalShoreline(context, summary, shoreLayer, shoreMaterial, riverPropMaterial, false);
        }

        static void EmitHorizontalShoreline(AegisMapVisualCompileContext context, AegisVisualLayerSummary summary, Transform shoreLayer, Material shoreMaterial, Material riverPropMaterial, bool north)
        {
            for (var y = 0; y < context.Height; y++)
            {
                var x = 0;
                while (x < context.Width)
                {
                    if (!IsHorizontalShore(context, x, y, north))
                    {
                        x++;
                        continue;
                    }

                    var startX = x;
                    while (x < context.Width && IsHorizontalShore(context, x, y, north))
                        x++;

                    var width = x - startX;
                    var centerY = north ? y + 1.03f : y - 0.03f;
                    var center = new Vector2(startX + width * 0.5f, centerY);
                    AegisVisualCompilerPrimitives.CreateQuad(shoreLayer, "shoreline_h_" + startX + "_" + y + "_" + north, center, width + 1.1f, 0.86f, 0.044f, shoreMaterial, 0f);
                    summary.ShorelineEdges++;
                    MaybeEmitRiverProp(context, shoreLayer, summary, riverPropMaterial, Mathf.RoundToInt(center.x), y, center);
                }
            }
        }

        static void EmitVerticalShoreline(AegisMapVisualCompileContext context, AegisVisualLayerSummary summary, Transform shoreLayer, Material shoreMaterial, Material riverPropMaterial, bool east)
        {
            for (var x = 0; x < context.Width; x++)
            {
                var y = 0;
                while (y < context.Height)
                {
                    if (!IsVerticalShore(context, x, y, east))
                    {
                        y++;
                        continue;
                    }

                    var startY = y;
                    while (y < context.Height && IsVerticalShore(context, x, y, east))
                        y++;

                    var height = y - startY;
                    var centerX = east ? x + 1.03f : x - 0.03f;
                    var center = new Vector2(centerX, startY + height * 0.5f);
                    AegisVisualCompilerPrimitives.CreateQuad(shoreLayer, "shoreline_v_" + x + "_" + startY + "_" + east, center, 0.86f, height + 1.1f, 0.045f, shoreMaterial, 0f);
                    summary.ShorelineEdges++;
                    MaybeEmitRiverProp(context, shoreLayer, summary, riverPropMaterial, x, Mathf.RoundToInt(center.y), center);
                }
            }
        }

        static bool IsHorizontalShore(AegisMapVisualCompileContext context, int x, int y, bool north)
        {
            if (!context.IsWater(x, y))
                return false;
            var ny = north ? y + 1 : y - 1;
            return !context.InBounds(x, ny) || !context.IsWater(x, ny);
        }

        static bool IsVerticalShore(AegisMapVisualCompileContext context, int x, int y, bool east)
        {
            if (!context.IsWater(x, y))
                return false;
            var nx = east ? x + 1 : x - 1;
            return !context.InBounds(nx, y) || !context.IsWater(nx, y);
        }

        static void MaybeEmitRiverProp(AegisMapVisualCompileContext context, Transform shoreLayer, AegisVisualLayerSummary summary, Material riverPropMaterial, int x, int y, Vector2 center)
        {
            if (context.Hash01(x, y, 4010) >= 0.12f)
                return;

            var propPosition = new Vector3(center.x, 0.08f, center.y);
            var rotation = Quaternion.Euler(0f, context.Hash01(x, y, 4020) * 360f, 0f);
            var scale = Vector3.one * Mathf.Lerp(0.55f, 1.0f, context.Hash01(x, y, 4030));
            var prefabPath = AegisMapArtPack.Pick(AegisMapArtPack.RiverMeshes, context.Seed, x, y);
            if (AegisMapArtPack.TryInstantiatePrefab(shoreLayer, "river_bank_prop_" + x + "_" + y, prefabPath, propPosition, rotation, scale, riverPropMaterial))
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
