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
            var shoreFeatherMaterial = AegisVisualCompilerPrimitives.Material(context, "river.shoreline_feather");
            var depthEdgeMaterial = AegisVisualCompilerPrimitives.Material(context, "river.depth_edge");
            var shallowEdgeMaterial = AegisVisualCompilerPrimitives.Material(context, "river.shallow_edge");
            var rippleMaterial = AegisVisualCompilerPrimitives.Material(context, "river.ripple");
            var riverPropMaterial = AegisVisualCompilerPrimitives.Material(context, "vegetation.grass");

            var ribbons = CollectWaterRibbons(context, summary);
            EmitWaterRibbonMeshes(context, summary, waterLayer, waterMaterial, ribbons);
            EmitWaterEdgeDetails(context, summary, waterLayer, depthEdgeMaterial, shallowEdgeMaterial, rippleMaterial, ribbons);
            EmitMergedShoreline(context, summary, shoreLayer, shoreMaterial, shoreFeatherMaterial, riverPropMaterial, ribbons);
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

        static void EmitWaterEdgeDetails(AegisMapVisualCompileContext context, AegisVisualLayerSummary summary, Transform waterLayer, Material depthEdgeMaterial, Material shallowEdgeMaterial, Material rippleMaterial, List<List<RowSpan>> ribbons)
        {
            for (var i = 0; i < ribbons.Count; i++)
            {
                var spans = ribbons[i];
                EmitWaterEdgePatches(context, waterLayer, summary, spans, i, true, depthEdgeMaterial, "depth", 0.18f, 0.22f, 0.48f, 1.1f, 3.6f, 0.064f, 6700, 0.64f);
                EmitWaterEdgePatches(context, waterLayer, summary, spans, i, false, depthEdgeMaterial, "depth", 0.18f, 0.22f, 0.48f, 1.1f, 3.6f, 0.064f, 6800, 0.64f);
                EmitWaterEdgePatches(context, waterLayer, summary, spans, i, true, shallowEdgeMaterial, "shallow", 0.62f, 0.16f, 0.30f, 1.6f, 4.8f, 0.067f, 6720, 0.34f);
                EmitWaterEdgePatches(context, waterLayer, summary, spans, i, false, shallowEdgeMaterial, "shallow", 0.62f, 0.16f, 0.30f, 1.6f, 4.8f, 0.067f, 6820, 0.34f);
                EmitWaterRipples(context, waterLayer, summary, spans, i, rippleMaterial);
            }
        }

        static void EmitWaterEdgePatches(AegisMapVisualCompileContext context, Transform waterLayer, AegisVisualLayerSummary summary, List<RowSpan> spans, int ribbonIndex, bool leftBank, Material material, string suffix, float centerOffset, float minWidth, float maxWidth, float minLength, float maxLength, float elevation, int salt, float chance)
        {
            if (spans.Count == 0)
                return;

            var stride = Mathf.Max(3, Mathf.RoundToInt(spans.Count / 18f));
            for (var i = 1; i < spans.Count - 1; i += stride)
            {
                var span = spans[i];
                if (span.Right - span.Left < 2)
                    continue;

                if (context.Hash01(ribbonIndex + i, span.Y, salt) > chance)
                    continue;

                var edge = SmoothEdge(context, spans, i, leftBank, span.Y);
                var side = leftBank ? 1f : -1f;
                var drift = Mathf.Lerp(-0.10f, 0.14f, context.Hash01(ribbonIndex + i, span.Y, salt + 1));
                var center = new Vector2(edge + side * (centerOffset + drift), span.Y + 0.5f + Mathf.Lerp(-0.34f, 0.34f, context.Hash01(ribbonIndex + i, span.Y, salt + 2)));
                var width = Mathf.Lerp(minWidth, maxWidth, context.Hash01(ribbonIndex + i, span.Y, salt + 3));
                var length = Mathf.Lerp(minLength, maxLength, context.Hash01(ribbonIndex + i, span.Y, salt + 4));
                var angle = 90f + Mathf.Lerp(-12f, 12f, context.Hash01(ribbonIndex + i, span.Y, salt + 5));
                AegisVisualCompilerPrimitives.CreateOrganicQuad(waterLayer, "water_edge_" + suffix + "_" + ribbonIndex + "_" + (leftBank ? "left" : "right") + "_" + span.Y, center, width, length, elevation, material, angle, context, ribbonIndex + i, span.Y, salt + 6, Mathf.Min(width, length) * 0.18f);
                summary.ShorelineDetailDecalCount++;
            }
        }

        static void EmitWaterRipples(AegisMapVisualCompileContext context, Transform waterLayer, AegisVisualLayerSummary summary, List<RowSpan> spans, int ribbonIndex, Material rippleMaterial)
        {
            if (spans.Count == 0)
                return;

            var stride = Mathf.Max(5, Mathf.RoundToInt(spans.Count / 16f));
            for (var i = 1; i < spans.Count; i += stride)
            {
                var span = spans[i];
                var width = span.Right - span.Left;
                if (width < 3)
                    continue;

                if (context.Hash01(ribbonIndex, span.Y, 6900) > 0.72f)
                    continue;

                var left = SmoothEdge(context, spans, i, true, span.Y) + 0.8f;
                var right = SmoothEdge(context, spans, i, false, span.Y) - 0.8f;
                if (right <= left)
                    continue;

                var centerX = Mathf.Lerp(left, right, context.Hash01(ribbonIndex + i, span.Y, 6910));
                var centerY = span.Y + 0.5f + (context.Hash01(ribbonIndex + i, span.Y, 6920) - 0.5f) * 0.6f;
                var rippleWidth = Mathf.Min(right - left, Mathf.Lerp(2.4f, 7.8f, context.Hash01(ribbonIndex + i, span.Y, 6930)));
                var rippleHeight = Mathf.Lerp(0.08f, 0.24f, context.Hash01(ribbonIndex + i, span.Y, 6940));
                var angle = Mathf.Lerp(-5f, 5f, context.Hash01(ribbonIndex + i, span.Y, 6950));
                AegisVisualCompilerPrimitives.CreateOrganicQuad(waterLayer, "water_ripple_" + ribbonIndex + "_" + span.Y, new Vector2(centerX, centerY), rippleWidth, rippleHeight, 0.076f, rippleMaterial, angle, context, ribbonIndex, span.Y, 6960, rippleHeight * 0.4f);
                summary.ShorelineDetailDecalCount++;
            }
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

        static void EmitMergedShoreline(AegisMapVisualCompileContext context, AegisVisualLayerSummary summary, Transform shoreLayer, Material shoreMaterial, Material shoreFeatherMaterial, Material riverPropMaterial, List<List<RowSpan>> ribbons)
        {
            for (var i = 0; i < ribbons.Count; i++)
            {
                var spans = ribbons[i];
                EmitBankMesh(context, shoreLayer, shoreMaterial, summary, spans, i, true, 0.48f, 0.046f, "core");
                EmitBankMesh(context, shoreLayer, shoreMaterial, summary, spans, i, false, 0.48f, 0.046f, "core");
                EmitBankFeatherMesh(context, shoreLayer, shoreFeatherMaterial, summary, spans, i, true);
                EmitBankFeatherMesh(context, shoreLayer, shoreFeatherMaterial, summary, spans, i, false);
                EmitCapMesh(context, shoreLayer, shoreMaterial, summary, spans, i, true, 0.50f, "core");
                EmitCapMesh(context, shoreLayer, shoreMaterial, summary, spans, i, false, 0.50f, "core");
                EmitCapMesh(context, shoreLayer, shoreFeatherMaterial, summary, spans, i, true, 1.25f, "feather");
                EmitCapMesh(context, shoreLayer, shoreFeatherMaterial, summary, spans, i, false, 1.25f, "feather");
                EmitRiverBankProps(context, shoreLayer, summary, riverPropMaterial, spans);
            }
        }

        static void EmitBankMesh(AegisMapVisualCompileContext context, Transform shoreLayer, Material shoreMaterial, AegisVisualLayerSummary summary, List<RowSpan> spans, int ribbonIndex, bool leftBank, float widthScale, float elevation, string suffix)
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
                var outer = inner + (leftBank ? -BankWidth(context, rowForNoise, 6100) * widthScale : BankWidth(context, rowForNoise, 6200) * widthScale);
                var low = Mathf.Min(inner, outer);
                var high = Mathf.Max(inner, outer);
                var insetNoise = (context.Hash01(rowForNoise, leftBank ? 6110 : 6210, 6115) - 0.5f) * 0.10f;
                vertices[i * 2] = new Vector3(low + insetNoise, elevation, z);
                vertices[i * 2 + 1] = new Vector3(high + insetNoise, elevation, z);
                uvs[i * 2] = new Vector2(0f, i * 0.31f);
                uvs[i * 2 + 1] = new Vector2(1f, i * 0.31f);
            }

            CreateRibbonMesh(shoreLayer, shoreMaterial, "shoreline_bank_mesh_" + suffix + "_" + ribbonIndex + "_" + (leftBank ? "left" : "right"), vertices, uvs);
            summary.ShorelineEdges += spans.Count;
            summary.ShorelineMeshes++;
        }

        static void EmitBankFeatherMesh(AegisMapVisualCompileContext context, Transform shoreLayer, Material shoreMaterial, AegisVisualLayerSummary summary, List<RowSpan> spans, int ribbonIndex, bool leftBank)
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
                var edge = SmoothEdge(context, spans, i, leftBank, rowForNoise) + (leftBank ? -0.08f : 0.08f);
                var coreOuter = edge + (leftBank ? -BankWidth(context, rowForNoise, 6500) * 0.42f : BankWidth(context, rowForNoise, 6600) * 0.42f);
                var featherOuter = edge + (leftBank ? -BankWidth(context, rowForNoise, 6510) * 1.35f : BankWidth(context, rowForNoise, 6610) * 1.35f);
                var low = Mathf.Min(coreOuter, featherOuter);
                var high = Mathf.Max(coreOuter, featherOuter);
                var noise = (context.Hash01(rowForNoise, leftBank ? 6520 : 6620, 6525) - 0.5f) * 0.18f;
                vertices[i * 2] = new Vector3(low + noise, 0.044f, z);
                vertices[i * 2 + 1] = new Vector3(high + noise, 0.044f, z);
                uvs[i * 2] = new Vector2(0f, i * 0.22f);
                uvs[i * 2 + 1] = new Vector2(1f, i * 0.22f);
            }

            CreateRibbonMesh(shoreLayer, shoreMaterial, "shoreline_bank_mesh_feather_" + ribbonIndex + "_" + (leftBank ? "left" : "right"), vertices, uvs);
            summary.ShorelineEdges += spans.Count;
            summary.ShorelineMeshes++;
        }

        static void EmitCapMesh(AegisMapVisualCompileContext context, Transform shoreLayer, Material shoreMaterial, AegisVisualLayerSummary summary, List<RowSpan> spans, int ribbonIndex, bool south, float widthScale, string suffix)
        {
            if (spans.Count == 0)
                return;

            var sectionIndex = south ? 0 : spans.Count;
            var rowForNoise = Mathf.Clamp(south ? spans[0].Y : spans[spans.Count - 1].Y, 0, context.Height - 1);
            var edgeZ = south ? spans[0].Y : spans[0].Y + spans.Count;
            var innerZ = edgeZ + (south ? 0.04f : -0.04f);
            var outerZ = edgeZ + (south ? -BankWidth(context, rowForNoise, 6300) * widthScale : BankWidth(context, rowForNoise, 6400) * widthScale);
            var zLow = Mathf.Min(innerZ, outerZ);
            var zHigh = Mathf.Max(innerZ, outerZ);
            var left = SmoothEdge(context, spans, sectionIndex, true, rowForNoise) - 0.52f;
            var right = SmoothEdge(context, spans, sectionIndex, false, rowForNoise) + 0.52f;
            var shoulder = 0.22f + context.Hash01(rowForNoise, south ? 6310 : 6410, 6320) * 0.22f;
            var vertices = new[]
            {
                new Vector3(left - shoulder, suffix == "feather" ? 0.043f : 0.045f, zLow),
                new Vector3(right + shoulder, suffix == "feather" ? 0.043f : 0.045f, zLow),
                new Vector3(left, suffix == "feather" ? 0.043f : 0.045f, zHigh),
                new Vector3(right, suffix == "feather" ? 0.043f : 0.045f, zHigh)
            };
            var uvs = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f)
            };

            CreateRibbonMesh(shoreLayer, shoreMaterial, "shoreline_cap_mesh_" + suffix + "_" + ribbonIndex + "_" + (south ? "south" : "north"), vertices, uvs);
            summary.ShorelineEdges++;
            summary.ShorelineMeshes++;
        }

        static void EmitRiverBankProps(AegisMapVisualCompileContext context, Transform shoreLayer, AegisVisualLayerSummary summary, Material riverPropMaterial, List<RowSpan> spans)
        {
            if (spans.Count == 0)
                return;

            var stride = Mathf.Max(3, Mathf.RoundToInt(spans.Count / 16f));
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
            if (context.Hash01(x, y, 4010) >= 0.46f)
                return;

            var propPosition = new Vector3(center.x, 0.08f, center.y);
            var rotation = Quaternion.Euler(0f, context.Hash01(x, y, 4020) * 360f, 0f);
            var scale = Vector3.one * Mathf.Lerp(0.72f, 1.28f, context.Hash01(x, y, 4030));
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
