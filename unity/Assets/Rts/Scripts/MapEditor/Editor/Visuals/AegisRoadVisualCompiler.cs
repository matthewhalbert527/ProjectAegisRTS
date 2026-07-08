#if UNITY_EDITOR
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.MapEditor.Visuals;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    sealed class AegisRoadVisualCompiler : IAegisVisualLayerCompiler
    {
        public AegisVisualLayerSummary Compile(AegisMapVisualCompileContext context)
        {
            var summary = new AegisVisualLayerSummary("Roads And Tire Tracks");
            var layer = AegisVisualCompilerPrimitives.CreateLayer(context, "Roads And Tire Tracks");
            var roadMaterial = AegisVisualCompilerPrimitives.Material(context, "road.dirt");
            var rutMaterial = AegisVisualCompilerPrimitives.Material(context, "road.gravel");
            var bridgeDeckMaterial = AegisVisualCompilerPrimitives.Material(context, "basepad.panel");
            var bridgeRailMaterial = AegisVisualCompilerPrimitives.Material(context, "basepad.trim");
            var bridgeShadowMaterial = AegisVisualCompilerPrimitives.Material(context, "decal.scorch");

            for (var i = 0; i < context.RoadSegments.Count; i++)
            {
                var segment = context.RoadSegments[i];
                var runs = SplitByWater(context, segment);
                for (var runIndex = 0; runIndex < runs.Count; runIndex++)
                {
                    var run = runs[runIndex];
                    if (run.IsWater)
                        EmitBridgeRun(context, layer, summary, i, runIndex, run, bridgeDeckMaterial, bridgeRailMaterial, bridgeShadowMaterial);
                    else
                        EmitRoadRun(context, layer, summary, i, runIndex, run, roadMaterial, rutMaterial);
                }
            }

            return summary;
        }

        static List<RoadRun> SplitByWater(AegisMapVisualCompileContext context, AegisVisualPathSegment segment)
        {
            var runs = new List<RoadRun>();
            var length = AegisVisualCompilerPrimitives.SegmentLength(segment.A, segment.B);
            if (length <= 0.1f)
                return runs;

            var steps = Mathf.Max(2, Mathf.CeilToInt(length / 0.75f));
            var runStart = segment.A;
            var previousWater = IsWaterAt(context, segment.A);
            for (var step = 1; step <= steps; step++)
            {
                var t = step / (float)steps;
                var point = Vector2.Lerp(segment.A, segment.B, t);
                var water = IsWaterAt(context, point);
                if (water == previousWater)
                    continue;

                runs.Add(new RoadRun(runStart, point, segment.Width, previousWater));
                runStart = point;
                previousWater = water;
            }

            runs.Add(new RoadRun(runStart, segment.B, segment.Width, previousWater));
            return runs;
        }

        static void EmitRoadRun(AegisMapVisualCompileContext context, Transform layer, AegisVisualLayerSummary summary, int segmentIndex, int runIndex, RoadRun run, Material roadMaterial, Material rutMaterial)
        {
            var length = AegisVisualCompilerPrimitives.SegmentLength(run.A, run.B);
            if (length <= 0.3f)
                return;

            var center = Vector2.Lerp(run.A, run.B, 0.5f);
            var angle = AegisVisualCompilerPrimitives.DirectionAngle(run.A, run.B);
            var direction = (run.B - run.A).normalized;
            var normal = new Vector2(-direction.y, direction.x);
            var width = Mathf.Clamp(run.Width, 1.85f, 3.35f);

            AegisVisualCompilerPrimitives.CreateQuad(layer, "road_body_" + segmentIndex + "_" + runIndex, center, length, width, 0.055f, roadMaterial, angle);
            AegisVisualCompilerPrimitives.CreateQuad(layer, "road_edge_wear_left_" + segmentIndex + "_" + runIndex, center + normal * (width * 0.42f), length * 0.92f, 0.26f, 0.064f, rutMaterial, angle);
            AegisVisualCompilerPrimitives.CreateQuad(layer, "road_edge_wear_right_" + segmentIndex + "_" + runIndex, center - normal * (width * 0.42f), length * 0.92f, 0.26f, 0.064f, rutMaterial, angle);
            if (length > 5f)
            {
                AegisVisualCompilerPrimitives.CreateQuad(layer, "road_tire_rut_left_" + segmentIndex + "_" + runIndex, center + normal * 0.48f, length * 0.76f, 0.11f, 0.071f, rutMaterial, angle);
                AegisVisualCompilerPrimitives.CreateQuad(layer, "road_tire_rut_right_" + segmentIndex + "_" + runIndex, center - normal * 0.48f, length * 0.76f, 0.11f, 0.071f, rutMaterial, angle);
            }

            summary.RoadSegments++;
        }

        static void EmitBridgeRun(AegisMapVisualCompileContext context, Transform layer, AegisVisualLayerSummary summary, int segmentIndex, int runIndex, RoadRun run, Material deckMaterial, Material railMaterial, Material shadowMaterial)
        {
            var length = AegisVisualCompilerPrimitives.SegmentLength(run.A, run.B);
            if (length <= 0.3f)
                return;

            var center = Vector2.Lerp(run.A, run.B, 0.5f);
            var angle = AegisVisualCompilerPrimitives.DirectionAngle(run.A, run.B);
            var direction = (run.B - run.A).normalized;
            var normal = new Vector2(-direction.y, direction.x);
            var deckWidth = Mathf.Clamp(run.Width + 0.62f, 2.5f, 4.2f);

            AegisVisualCompilerPrimitives.CreateQuad(layer, "bridge_prototype_shadow_" + segmentIndex + "_" + runIndex, center, length + 1.2f, deckWidth + 0.8f, 0.081f, shadowMaterial, angle);
            AegisVisualCompilerPrimitives.CreateQuad(layer, "bridge_prototype_deck_" + segmentIndex + "_" + runIndex, center, length + 0.55f, deckWidth, 0.18f, deckMaterial, angle);
            AegisVisualCompilerPrimitives.CreateQuad(layer, "bridge_prototype_rail_left_" + segmentIndex + "_" + runIndex, center + normal * (deckWidth * 0.48f), length + 0.48f, 0.18f, 0.31f, railMaterial, angle);
            AegisVisualCompilerPrimitives.CreateQuad(layer, "bridge_prototype_rail_right_" + segmentIndex + "_" + runIndex, center - normal * (deckWidth * 0.48f), length + 0.48f, 0.18f, 0.31f, railMaterial, angle);
            summary.BridgeCrossings++;
        }

        static bool IsWaterAt(AegisMapVisualCompileContext context, Vector2 point)
        {
            var x = Mathf.Clamp(Mathf.FloorToInt(point.x), 0, context.Width - 1);
            var y = Mathf.Clamp(Mathf.FloorToInt(point.y), 0, context.Height - 1);
            return context.IsWater(x, y);
        }

        struct RoadRun
        {
            public readonly Vector2 A;
            public readonly Vector2 B;
            public readonly float Width;
            public readonly bool IsWater;

            public RoadRun(Vector2 a, Vector2 b, float width, bool isWater)
            {
                A = a;
                B = b;
                Width = width;
                IsWater = isWater;
            }
        }
    }
}
#endif
